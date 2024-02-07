import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { DatabaseListResponse } from '../models/DatabaseListResponse.interface';

@Injectable({
  providedIn: 'root'
})
export class DatabaseService {
  constructor(private http: HttpClient) { }

  getListDb(): Observable<any> {
    return this.http.get<any>(`api/Db/listDB`)
      .pipe(
        catchError((error: HttpErrorResponse) => {
          console.error('Error fetching database list', error);
          return of({ databases: [] });
        })
      );
  }
  
  getDatabaseStatistics(dbName: string): Observable<any> {
    if (!dbName) {
      console.error('Database name is required.');
      return of(null);
    }
    return this.http.get(`api/Db/databaseStatistics/${dbName}`)
      .pipe(
        catchError((error: HttpErrorResponse) => {
          console.error(`Error fetching statistics for database '${dbName}'`, error);
          return of(null);
        })
      );
  }
}
