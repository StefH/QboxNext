import { Component, OnInit } from '@angular/core';

import * as moment from 'moment';
import { LoginModel } from './common/models';
import { AuthenticationService } from './authentication';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  public title = 'QboxNext.Server.Frontend';

  constructor(private authenticationService: AuthenticationService) {
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
