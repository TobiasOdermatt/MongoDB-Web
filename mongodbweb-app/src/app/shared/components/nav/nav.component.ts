import { Component, HostListener, OnInit } from '@angular/core';
import { EnvService } from "../../service/env.service";
import { IconService } from '../../service/icon.services';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {
  showNav = true;
  isAdmin = false;
  isAuthorized = false;

  constructor(
    private envService: EnvService, public iconService: IconService,
  ) {}

  ngOnInit() {
    this.checkAuthorization();
    this.updateNavDisplay();
  }

  checkAuthorization() {
    this.envService.isAuthorized$.subscribe((isAuthorized: any) => {
      this.isAuthorized = isAuthorized;
      this.envService.isAdmin().subscribe(isAdmin => {
        this.isAdmin = isAdmin;
      });
    });
  }

  @HostListener('window:resize', ['$event'])
  onResize(event) {
    this.updateNavDisplay();
  }

  updateNavDisplay() {
    this.showNav = window.innerWidth > 641;
  }


  Collapse() {
    this.showNav = !this.showNav;
  }
}
