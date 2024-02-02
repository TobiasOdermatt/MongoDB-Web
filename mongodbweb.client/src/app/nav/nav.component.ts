import { Component, OnInit } from '@angular/core';
import {EnvService} from "../shared/service/env.service";

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {
  showNav = true;
  isAdmin = false;
  isAuthorized = false;
  isLoadingAdmin = true;
  isLoadingAuthorized = true;
  constructor(private envService: EnvService){}

  ngOnInit() {
    this.envService.isAdmin().subscribe(isAdmin => {
      this.isAdmin = isAdmin;
      this.isLoadingAdmin = false;
    });
    this.envService.isAuthorized().subscribe(isAuthorized => {
      this.isAuthorized = isAuthorized;
      this.isLoadingAuthorized = false;
    });
    console.log(this.isAdmin);
  }
  Collapse() {
    this.showNav = !this.showNav;
  }

}
