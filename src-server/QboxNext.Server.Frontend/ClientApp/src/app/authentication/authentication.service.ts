import { Injectable } from '@angular/core';
import { Router } from '@angular/router';

import { AppInsightsService } from '@markpieszak/ng-application-insights';
import { Auth0DecodedHash, Auth0Error, WebAuth } from 'auth0-js';

import { environment } from '../../environments/environment';
import { LoginModel } from '../common/models';
import { SessionStorageService } from '../common/services/session-storage.service';

@Injectable({
    providedIn: 'root'
})
export class AuthenticationService {
    private auth0: WebAuth;

    constructor(private sessionStorageService: SessionStorageService, private router: Router, private appInsightsService: AppInsightsService) {
        this.auth0 = new WebAuth({
            clientID: environment.clientId,
            domain: 'stef-heyenrath.eu.auth0.com',
            responseType: 'token id_token',
            audience: 'https://qboxnext.web.nl',
            redirectUri: `${location.origin}/callback`,
            scope: 'openid'
        });
    }

    public login(): void {
        this.auth0.authorize();
    }

    public handleAuthentication(): void {
        this.auth0.parseHash((err: Auth0Error | null, authResult: Auth0DecodedHash) => {
            if (authResult && authResult.accessToken && authResult.idToken) {
                this.setSession(authResult);
                this.router.navigate(['/electricity']);
            } else if (err) {
                this.router.navigate(['/']);
                console.log(err);
                alert(`Error: ${err.error}. Check the console for further details.`);
            }
        });
    }

    private setSession(authResult: Auth0DecodedHash): void {
        const model = new LoginModel({
            accessToken: authResult.accessToken,
            expiresAt: new Date().getTime() + (authResult.expiresIn || 0) * 1000,
            serialNumber: authResult.idTokenPayload['https://qboxnext.web.nl/SerialNumber']
        });

        this.appInsightsService.setAuthenticatedUserContext(authResult.idTokenPayload['sub']);

        this.setLoginModel(model);
    }

    public logout(): void {
        // Remove from sessionStorage
        this.clearLoginModel();

        // Go back to the home route
        this.router.navigate(['']);
    }

    public getLoginModel(): LoginModel | null {
        return this.sessionStorageService.get('LoginModel', LoginModel);
    }

    public clearLoginModel(): void {
        this.sessionStorageService.clear('LoginModel');
    }

    public isAuthenticated(): boolean {
        const model = this.getLoginModel();
        return model !== null && !model.isExpired();
    }

    private setLoginModel(loginModel: LoginModel): void {
        this.sessionStorageService.set('LoginModel', loginModel);
    }
}
