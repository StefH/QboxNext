import { Injectable } from '@angular/core';
import { CanActivate } from '@angular/router';
import { Observable } from 'rxjs';
import { AuthenticationService } from './authentication.service';

@Injectable()
export class AuthenticationGuard implements CanActivate {

    constructor(private authenticationService: AuthenticationService) {
    }

    public canActivate(): Observable<boolean> | Promise<boolean> | boolean {
        if (!this.authenticationService.isAuthenticated()) {
            this.authenticationService.login();
            return false;
        }

        return true;
    }
}
