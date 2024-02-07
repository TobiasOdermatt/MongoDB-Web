import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ConnectService {
  constructor(private http: HttpClient) {}
  createOTP(Username: string, Password: string): Observable<any> {
    const data = { 'Username': Username, 'Password': Password };
    return this.http.post('api/auth/CreateOTP', data);
  }
}
