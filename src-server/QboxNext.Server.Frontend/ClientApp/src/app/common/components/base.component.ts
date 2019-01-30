import { OnDestroy } from '@angular/core';

import { Subscription } from 'rxjs';

import { DataLoadStatus } from '../enums';

export abstract class BaseComponent implements OnDestroy {
    protected subscription: Subscription = new Subscription();

    // The DataLoadStatus enumeration is exposed as a public property so it can be used in the html-views.
    public DataLoadStatus: any = DataLoadStatus;

    // This enumeration defines the DataLoadStatus.
    public loadingStatus: DataLoadStatus = DataLoadStatus.None;

    // constructor(protected authenticationService: AuthenticationService) {
    // }

    // tslint:disable-next-line:no-unused
    protected error(message: any): void {
        this.loadingStatus = DataLoadStatus.Error;
    }

    // public get loginModel(): LoginModel | null {
    //     return this.authenticationService.getLoginModel();
    // }

    public ngOnDestroy() {
        this.subscription.unsubscribe();
    }
}
