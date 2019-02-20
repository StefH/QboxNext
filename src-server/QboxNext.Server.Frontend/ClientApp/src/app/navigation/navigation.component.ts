import { Component } from '@angular/core';

import { AuthenticationService } from '../authentication';

@Component({
  selector: 'app-navigation',
  templateUrl: './navigation.component.html'
})
export class NavigationComponent {
  public navbarOpen = false;

  constructor(private authenticationService: AuthenticationService) {
    this.authenticationService.handleAuthentication();
  }

  public get isAuthenticated(): boolean {
    return this.authenticationService.isAuthenticated();
  }

  public login(): void {
    this.authenticationService.login();
  }

  public logout(): void {
    this.authenticationService.logout();
  }

  public toggleNavbar() {
    this.navbarOpen = !this.navbarOpen;
  }
}
