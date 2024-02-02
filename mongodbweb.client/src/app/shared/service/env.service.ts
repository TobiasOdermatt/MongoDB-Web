import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders , HttpErrorResponse} from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class EnvService {
  constructor(private http: HttpClient) {}

  getHeaders(): HttpHeaders {
    let headers = new HttpHeaders();
    const uuid = this.getCookie('UUID');
    const token = this.getCookie('Token');
    if (uuid) {
      headers = headers.set('UUID', uuid);
    }
    if (token) {
      headers = headers.set('Token', token);
    }
    console.log(headers)
    return headers;
  }

  getCookie(name: string): string | null {
    const match = document.cookie.match(new RegExp('(^| )' + name + '=([^;]+)'));
    return match ? match[2] : null;
  }

  isAdmin(): Observable<boolean> {
    return this.http.get<boolean>('api/Env/IsAdmin', {
      headers: this.getHeaders(),
      withCredentials: true
    }).pipe(
      catchError((error: HttpErrorResponse) => {
        return of(false);
      })
    );
  }

  isAuthorized(): Observable<boolean> {
    return this.http.get<boolean>('api/Env/IsAuthorized', {
      headers: this.getHeaders(),
      withCredentials: true
    }).pipe(
      catchError((error: HttpErrorResponse) => {
        return of(false);
      })
    );
  }
}
