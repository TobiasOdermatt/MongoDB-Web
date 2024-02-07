import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { DatabaseListResponse } from '../models/DatabaseListResponse.interface';

@Injectable({
  providedIn: 'root'
})
export class DatabaseListService {
  constructor(private http: HttpClient) { }

  getListDb(): Observable<DatabaseListResponse> {
    return this.http.get<DatabaseListResponse>(`api/Db/listDB`)
      .pipe(
        catchError((error: HttpErrorResponse) => {
          console.error('Error fetching database list', error);
          return of({ databases: [] });
        })
      );
  }
  
}
