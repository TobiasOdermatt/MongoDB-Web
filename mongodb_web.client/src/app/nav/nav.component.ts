import { Component } from '@angular/core';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent {
  showNav = true;

  constructor() {
  }

  Collapse() {
    this.showNav = !this.showNav;
  }

}
