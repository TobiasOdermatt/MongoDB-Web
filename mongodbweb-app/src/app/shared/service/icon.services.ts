import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';

@Injectable({
  providedIn: 'root'
})
export class IconService {
  svgIcons: { [key: string]: SafeHtml } = {};

  constructor(
    private sanitizer: DomSanitizer,
    private http: HttpClient
  ) {
    this.loadIcons();
  }

  loadIcons() {
    const menuIcons = ['home', 'query', 'pie_chart', 'user_control', 'security', 'settings', 'logout'];
    const otherIcons = ['insert', 'open', 'search'];

    menuIcons.forEach(icon => {
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

    this.loadSvgOtherIcons(otherIcons);
  }

  loadSvgOtherIcons(icons: string[]) {
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
