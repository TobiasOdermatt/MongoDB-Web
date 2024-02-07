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
    return this.http.get<boolean>('api/Env/IsAdmin', {
    }).pipe(
      catchError((error: HttpErrorResponse) => {
        return of(false);
      })
    );
  }

  isAuthorized(): Observable<boolean> {
    return this.http.get<boolean>('api/Env/IsAuthorized', {
    }).pipe(
      catchError((error: HttpErrorResponse) => {
        return of(false);
      })
    );
  }

  isCookieSet(): boolean {
    return this.getCookie('UUID') !== null && this.getCookie('Token') !== null;
  }

  updateAuthorizationStatus(isAuthorized: boolean) {
    this.isAuthorizedSource.next(isAuthorized);
  }
}
