import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { AuthenticationService } from './authentication.service';

@Injectable()
export class AuthenticationGuard implements CanActivate {

    constructor(public authenticationService: AuthenticationService, public router: Router) {
    }

    public canActivate(): Observable<boolean> | Promise<boolean> | boolean {
        if (!this.authenticationService.isAuthenticated()) {
            // this.router.navigate(['/']);
            // this.authenticationService.login();
            return false;
        }

        return true;
    }
}
