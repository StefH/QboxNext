import { Injectable, Inject } from '@angular/core';
import { Router } from '@angular/router';

import { Auth0DecodedHash, Auth0Error, WebAuth } from 'auth0-js';

import { LoginModel } from '../common/models';
import { SessionStorageService } from '../common/services/session-storage.service';
import { WINDOW } from '../common';


@Injectable({
    providedIn: 'root'
})
export class AuthenticationService {
    private LOGINMODEL_KEY = 'login';

    private readonly auth0: WebAuth;

    constructor(private sessionStorageService: SessionStorageService, public router: Router, @Inject(WINDOW) private window: Window) {
        this.auth0 = new WebAuth({
            clientID: 'zGwuLd2ot4Q1o4F2Z81jKQFS1c3FNswu',
            domain: 'stef-heyenrath.eu.auth0.com',
            responseType: 'token id_token',
            audience: 'https://qboxnext.web.nl',
            redirectUri: `${window.location.href}callback`,
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
                this.router.navigate(['/']);
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

        this.setLoginModel(model);
    }

    public logout(): void {
        // Remove from sessionStorage
        this.clearLoginModel();

        // Go back to the home route
        this.router.navigate(['']);
    }

    public getLoginModel(): LoginModel | null {
        return this.sessionStorageService.get(this.LOGINMODEL_KEY, LoginModel);
    }

    public clearLoginModel(): void {
        this.sessionStorageService.clear(this.LOGINMODEL_KEY);
    }

    public isAuthenticated(): boolean {
        const model = this.getLoginModel();
        return model !== null && !model.isExpired();
    }

    private setLoginModel(loginModel: LoginModel): void {
        this.sessionStorageService.set(this.LOGINMODEL_KEY, loginModel);
    }
}
