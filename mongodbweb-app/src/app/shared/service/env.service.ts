import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders , HttpErrorResponse} from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class EnvService {
  private isAuthorizedSource = new BehaviorSubject<boolean>(false);
  isAuthorized$ = this.isAuthorizedSource.asObservable();
  constructor(private http: HttpClient) {}

  getCookie(name: string): string | null {
    const match = document.cookie.match(new RegExp('(^| )' + name + '=([^;]+)'));
    return match ? match[2] : null;
  }

  isAdmin(): Observable<boolean> {
    return this.http.get<boolean>('api/Env/isAdmin', {
    }).pipe(
      catchError((error: HttpErrorResponse) => {
        return of(false);
      })
    );
  }

  isAuthorized(): Observable<boolean> {
    return this.http.get<boolean>('api/Env/isAuthorized', {
    }).pipe(
      catchError((error: HttpErrorResponse) => {
        return of(false);
      })
    );
  }

  isFirstStart(): Observable<boolean> {
    return this.http.get<boolean>('api/Env/isFirstStart', {
    }).pipe(
      catchError((error: HttpErrorResponse) => {
        return of(false);
      })
    );
  }

  useAuthorization(): Observable<boolean> {
    return this.http.get<boolean>('api/Env/isAuthorizationEnabled', {
    }).pipe(
      catchError((error: HttpErrorResponse) => {
        return of(false);
      })
    );
  }

  isCookieSet(): boolean {
    return this.getCookie('UUID') !== null;
  }

  updateAuthorizationStatus(isAuthorized: boolean) {
    this.isAuthorizedSource.next(isAuthorized);
  }

  setCookie(name: string, value: string, days: number) {
    let expires = "";
    if (days) {
      let date = new Date();
      date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
      expires = "; expires=" + date.toUTCString();
    }
    document.cookie = name + "=" + (value || "") + expires + "; path=/";
  }
}
