import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { ProgressService } from './shared/service/progress.service';
@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {
  title = 'MongoDB Web';

  constructor(private progressService: ProgressService) { }

  ngOnInit() {
    const savedTheme = this.getCookie('theme');
    if (savedTheme) {
      this.setTheme(savedTheme);
    }
    this.progressService.startConnection().then(() => {
    });
  }
  onThemeChange(event: Event) {
    const selectedTheme = (event.target as HTMLSelectElement).value;
    this.setTheme(selectedTheme);
    this.setCookie('theme', selectedTheme, 365);
  }

  private setTheme(theme: string) {
    document.documentElement.className = theme === 'Dark' ? 'dark-mode' : 'light-mode';
  }

  private setCookie(name: string, value: string, days: number) {
    const expires = new Date();
    expires.setTime(expires.getTime() + days * 24 * 60 * 60 * 1000);
    document.cookie = `${name}=${value};expires=${expires.toUTCString()};path=/`;
  }

  private getCookie(name: string): string | null {
    const match = document.cookie.match(new RegExp(`(^| )${name}=([^;]+)`));
    return match ? match[2] : null;
  }

  isDarkMode(): boolean {
    return document.documentElement.classList.contains('dark-mode') || !this.isLightMode();
  }

  isLightMode(): boolean {
    return document.documentElement.classList.contains('light-mode');
  }
}
