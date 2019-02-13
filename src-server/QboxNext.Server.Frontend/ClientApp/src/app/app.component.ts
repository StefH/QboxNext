import { Component, OnInit } from '@angular/core';

import { AppInsightsService } from '@markpieszak/ng-application-insights';
import * as moment from 'moment';

import { environment } from '../environments/environment';
import { AuthenticationService } from './authentication';
import { LoginModel } from './common/models';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  public title = 'QboxNext.Server.Frontend';

  constructor(private authenticationService: AuthenticationService, appInsightsService: AppInsightsService) {
    if (environment.instrumentationKey.length > 0) {
      appInsightsService.config = {
        instrumentationKey: environment.instrumentationKey
      };

      appInsightsService.init();
    }

    this.authenticationService.handleAuthentication();
  }

  ngOnInit() {
    moment.locale('nl');
  }

  public get loginModel(): LoginModel | null {
    return this.authenticationService.getLoginModel();
  }

  public login(): void {
    this.authenticationService.login();
  }

  public get isAuthenticated(): boolean {
    return this.authenticationService.isAuthenticated();
  }
}
