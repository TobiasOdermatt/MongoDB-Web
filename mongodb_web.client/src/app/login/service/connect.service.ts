import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ConnectService {
  private baseUrl: string = "https://localhost:7282";

  constructor(private http: HttpClient) {}

  createOTP(authCookieKey: string, randData: string): Observable<any> {
    const url = `${this.baseUrl}/api/Auth/CreateOTP`;
    const data = { 'AuthCookieKey': authCookieKey, 'RandData': randData };
    return this.http.post(url, data);
  }
}
