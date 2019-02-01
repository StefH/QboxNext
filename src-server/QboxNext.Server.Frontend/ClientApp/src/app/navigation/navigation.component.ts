import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-navigation',
  templateUrl: './navigation.component.html'
})
export class NavigationComponent implements OnInit {
  public navbarOpen = false;

  ngOnInit() {
  }

  public toggleNavbar() {
    this.navbarOpen = !this.navbarOpen;
  }
}
