import { Component, OnInit } from '@angular/core';
import { AuthenticationService } from '../authentication';
import { LoginModel } from '../common';

@Component({
  selector: 'app-navigation',
  templateUrl: './navigation.component.html'
})
export class NavigationComponent implements OnInit {
  public navbarOpen = false;

  constructor(private authenticationService: AuthenticationService) {
    this.authenticationService.handleAuthentication();
  }

  ngOnInit() {
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
