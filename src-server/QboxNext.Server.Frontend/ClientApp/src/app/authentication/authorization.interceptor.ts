import { HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest, HttpResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';

import { Observable, throwError } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';

import { HttpStatusCodes } from '../common/constants';
import { AuthenticationService } from './authentication.service';

@Injectable()
export class AuthorizationInterceptor implements HttpInterceptor {

    constructor(private router: Router, private authenticationService: AuthenticationService) { }

    intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        const loginModel = this.authenticationService.getLoginModel();

        if (loginModel) {
            request = request.clone({
                setHeaders: {
                    Authorization: loginModel.getAuthenticationHeader(),
                    'Content-Type': 'application/json'
                }
            });
        }

        return next.handle(request)
            .pipe(
                tap((event: HttpEvent<any>) => {
                    if (event instanceof HttpResponse) {
                        // do stuff with response if you want
                    }
                }
                ),
                catchError(err => this.handleError(err, request))
            );
    }

    // tslint:disable-next-line:no-unused
    private handleError(errorResponse: HttpErrorResponse | any, request: any): Observable<any> {
        if (errorResponse instanceof HttpErrorResponse) {
            if (errorResponse.status === HttpStatusCodes.UNAUTHORIZED) {
                // if user is already logged in, reset status
                this.authenticationService.clearLoginModel();

                // redirect to the login route
                this.authenticationService.login();
            }
        }

        return throwError(errorResponse);
    }
}
