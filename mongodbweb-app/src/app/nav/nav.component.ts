import { Component, OnInit } from '@angular/core';
import { EnvService } from "../shared/service/env.service";
import { HttpClient } from '@angular/common/http';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {
  showNav = true;
  isAdmin = false;
  isAuthorized = false;
  isCookieSet = false;

  svgIcons: { [key: string]: SafeHtml } = {};

  constructor(
    private envService: EnvService,
    private http: HttpClient,
    private sanitizer: DomSanitizer
  ) {}

  ngOnInit() {
    this.checkAuthorization();
    this.loadSvgIcons();
  }

  checkAuthorization() {
    this.isCookieSet = this.envService.isCookieSet();
    this.envService.isAuthorized$.subscribe((isAuthorized: any) => {
      this.isAuthorized = isAuthorized;
      this.envService.isAdmin().subscribe(isAdmin => {
        this.isAdmin = isAdmin;
      });
    });
  }

  Collapse() {
    this.showNav = !this.showNav;
  }

  loadSvgIcons() {
    const icons = ['home', 'query', 'pie_chart', 'user_control', 'security', 'settings', 'logout'];
    icons.forEach(icon => {
      const cachedIcon = localStorage.getItem(`svg-${icon}`);
      if (cachedIcon) {
        this.svgIcons[icon] = this.sanitizer.bypassSecurityTrustHtml(cachedIcon);
      } else {
        this.http.get(`assets/icons/${icon}.svg`, { responseType: 'text' })
          .subscribe(svgContent => {
            localStorage.setItem(`svg-${icon}`, svgContent);
            this.svgIcons[icon] = this.sanitizer.bypassSecurityTrustHtml(svgContent);
          });
      }
    });
  }
}
