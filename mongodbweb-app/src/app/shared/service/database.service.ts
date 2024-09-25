import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';

@Injectable({
  providedIn: 'root'
})
export class DatabaseService {
  constructor(private http: HttpClient, private toastr: ToastrService) { }

  getListDb(): Observable<any> {
    return this.http.get<any>(`api/Db/listDB`)
      .pipe(
        catchError((error: HttpErrorResponse) => {
          console.error('Error fetching database list', error);
          this.toastr.error('Error fetching database list');
          return of({ databases: [] });
        })
      );
  }
  
  getDatabaseStatistics(dbName: string): Observable<any> {
    if (!dbName) {
      console.error('Database name is required.');
      return of(null);
    }
    return this.http.get<any>(`api/Db/databaseStatistics/${dbName}`)
      .pipe(
        catchError((error: HttpErrorResponse) => {
          console.error(`Error fetching statistics for database '${dbName}'`, error);
          this.toastr.error(`Error fetching statistics for database '${dbName}'`);
          return of(null);
        })
      );
  }

  getGlobalStatistics(): Observable<string> {
    return this.http.get(`api/Db/globalStatistics/`, { responseType: 'text' })
      .pipe(
        catchError((error: HttpErrorResponse) => {
          console.error(`Error fetching global statistic'`, error);
          this.toastr.error(`Error fetching global statistic'`);
          return of('');
        })
      );
  }

  getListCollections(dbName: string): Observable<any> {
    if (!dbName) {
      console.error('Database name is required for listing collections.');
      return of(null);
    }
    return this.http.get<any>(`api/Db/listCollections/${dbName}`)
      .pipe(
        catchError((error: HttpErrorResponse) => {
          console.error(`Error fetching collections for database '${dbName}'`, error);
          this.toastr.error(`Error fetching collections for database '${dbName}'`);
          return of({ collections: [] });
        })
      );
  }

  deleteDatabase(dbName: string): Observable<any> {
    if (!dbName) {
      console.error('Database name is required for deletion.');
      return of({ success: false, message: 'Database name is required.', statusCode: 400 });
    }
    return this.http.delete(`api/Db/deleteDatabase/${dbName}`, { observe: 'response', responseType: 'text' })
      .pipe(
        map(response => {
          return { success: true, message: response.body, statusCode: response.status };
        }),
        catchError((error: HttpErrorResponse) => {
          console.error(`Error deleting database '${dbName}'`, error);
          this.toastr.error(`Error deleting database '${dbName}'`);
          return of({ success: false, message: `Error deleting database '${dbName}': ${error.message}`, statusCode: error.status });
        })
      );
  }

  deleteCollection(dbName: string, collectionName: string): Observable<any> {
    if (!dbName || !collectionName) {
      console.error('Database name and Collection name is required for deletion.');
      return of({ success: false, message: 'Database name and collectionName is required.', statusCode: 400 });
    }
    return this.http.delete(`api/Db/deleteCollection/${dbName}/${collectionName}`, { observe: 'response', responseType: 'text' })
      .pipe(
        map(response => {
          return { success: true, message: response.body, statusCode: response.status };
        }),
        catchError((error: HttpErrorResponse) => {
          console.error(`Error deleting collection '${collectionName}'`, error);
          this.toastr.error(`Error deleting collection '${collectionName}'`);
          return of({ success: false, message: `Error deleting collection '${collectionName}': ${error.message}`, statusCode: error.status });
        })
      );
  }


  getCollectionStatistics(dbName: string, collectionName: string): Observable<any> {
    if (!dbName || !collectionName) {
      console.error('Database name and collection name are required.');
      return of({}); 
    }
    return this.http.get(`api/Db/collectionStatistics/${dbName}/${collectionName}`)
      .pipe(
        catchError((error) => {
          console.error(`Error fetching statistics for collection '${collectionName}' in database '${dbName}'`, error);
          this.toastr.error(`Error fetching statistics for collection '${collectionName}' in database '${dbName}'`);
          return of({}); 
        })
      );
  }

  createCollection(dbName: string, collectionName: string): Observable<any> {
    if (!dbName || !collectionName) {
      console.error('Database name and collection name are required.');
      this.toastr.error('Database name and collection name are required.');
      return of({ success: false, message: 'Database name and collection name are required.' });
    }

    return this.http.post(`api/Db/createCollection/${dbName}/${collectionName}`, {}).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status === 200) {
          console.log(`Collection '${collectionName}' created successfully in database '${dbName}'.`);
          return of({ success: true, message: `Collection '${collectionName}' created successfully in database '${dbName}'.` });
        } else {
          console.error(`Error creating collection '${collectionName}' in database '${dbName}'`, error);
          this.toastr.error(`Error creating collection '${collectionName}' in database '${dbName}'`);
          return of({ success: false, message: `Error creating collection '${collectionName}' in database '${dbName}': ${error.message}`, statusCode: error.status });
        }
      })
    );
  }

  prepareDatabaseDownload(dbName: string, downloadGuid: string): Observable<any> {
    if (!dbName || !downloadGuid) {
      console.error('Database name and download GUID are required.');
      this.toastr.error('Database name and download GUID are required.');
      return of(null);
    }

    const url = `api/Db/prepareDatabaseDownload/${dbName}/${downloadGuid}`;

    return this.http.get(url, { responseType: 'text' })
      .pipe(
        map(fileName => {
          return { success: true, fileName: fileName };
        }),
        catchError((error: HttpErrorResponse) => {
          console.error(`Error preparing download for database '${dbName}'`, error);
          this.toastr.error(`Error preparing download for database '${dbName}'`);
          return of({ success: false, message: error.message, statusCode: error.status });
        })
      );
  }


  prepareCollectionDownload(dbName: string, collectionName: string, downloadGuid: string): Observable<any> {
    if (!dbName || !downloadGuid) {
      console.error('Database name, collection name and download GUID are required.');
      this.toastr.error('Database name, collection name and download GUID are required.');
      return of(null);
    }

    const url = `api/Db/prepareCollectionDownload/${dbName}/${collectionName}/${downloadGuid}`;

    return this.http.get(url, { responseType: 'text' })
      .pipe(
        map(fileName => {
          return { success: true, fileName: fileName };
        }),
        catchError((error: HttpErrorResponse) => {
          console.error(`Error preparing download for collection '${collectionName}'`, error);
          this.toastr.error(`Error preparing download for collection '${collectionName}'`);
          return of({ success: false, message: error.message, statusCode: error.status });
        })
      );
  }

  checkDatabaseExistence(dbName: string): Observable<any> {
    if (!dbName) {
      console.error('Database name is required.');
      this.toastr.error('Database name is required.');
      return of({ success: false, message: 'Database name is required.' });
    }
    return this.http.get<any>(`api/Db/checkDatabaseExistence/${dbName}`)
      .pipe(
        catchError((error: HttpErrorResponse) => {
          console.error(`Error checking existence for database '${dbName}'`, error);
          this.toastr.error(`Error checking existence for database '${dbName}'`);
          return of({ exists: false, message: error.message, statusCode: error.status });
        })
      );
  }

  getTotalCount(dbName: string, collectionName: string, selectedKey?: string, searchValue?: string): Observable<any> {
    let queryParams = new HttpParams();
    if (selectedKey)
        queryParams = queryParams.append('selectedKey', selectedKey);
    if (searchValue)
        queryParams = queryParams.append('searchValue', searchValue);

    return this.http.get<any>(`api/Db/totalCount/${dbName}/${collectionName}`, { params: queryParams })
      .pipe(
        catchError((error: HttpErrorResponse) => {
          console.error(`Error fetching total count for collection '${collectionName}'`, error);
          this.toastr.error(`Error fetching total count for collection '${collectionName}'`);
          return of(null);
        })
      );
  }

  getPaginatedCollection(dbName: string, collectionName: string, skip: number, limit: number, selectedKey?: string, searchValue?: string): Observable<any> {
    let queryParams = new HttpParams()
      .append('skip', skip)
      .append('limit', limit);
    if (selectedKey) queryParams = queryParams.append('selectedKey', selectedKey);
    if (searchValue) queryParams = queryParams.append('searchValue', searchValue);

    return this.http.get<any>(`api/Db/getPaginatedCollection/${dbName}/${collectionName}`, { params: queryParams })
      .pipe(
        catchError((error: HttpErrorResponse) => {
          console.error(`Error fetching paginated collection data for '${collectionName}'`, error);
          this.toastr.error(`Error fetching paginated collection data for '${collectionName}'`);
          return of([]);
        })
      );
  }

  getCollectionCount(dbName: string, collectionName: string, selectedKey?: string, searchValue?: string): Observable<any> {
    let queryParams = new HttpParams();
    if (selectedKey) queryParams = queryParams.append('selectedKey', selectedKey);
    if (searchValue) queryParams = queryParams.append('searchValue', searchValue);

    return this.http.get<any>(`api/Db/collectionCount/${dbName}/${collectionName}`, { params: queryParams })
      .pipe(
        catchError((error: HttpErrorResponse) => {
          console.error(`Error fetching count for collection '${collectionName}'`, error);
          this.toastr.error(`Error fetching count for collection '${collectionName}'`);
          return of(null);
        })
      );
  }

  getCollectionAttributes(dbName: string, collectionName: string): Observable<any> {
    if (!dbName || !collectionName) {
      console.error('Database name and collection name are required.');
      this.toastr.error('Database name and collection name are required.');
      return of({ success: false, message: 'Database name and collection name are required.' });
    }

    return this.http.get<any>(`api/Db/collectionAttributes/${dbName}/${collectionName}`)
      .pipe(
        catchError((error: HttpErrorResponse) => {
          console.error(`Error fetching attributes for collection '${collectionName}' in database '${dbName}'`, error);
          this.toastr.error(`Error fetching attributes for collection '${collectionName}' in database '${dbName}'`);
          return of({ success: false, message: error.message, statusCode: error.status });
        })
      );
  }


  safeMongoDBJsonParse(data: string): any {
    try {
      let modifiedData = data
        .replace(/NumberLong\((\"?\d+\"?)\)/g, '$1')
        .replace(/ISODate\((\"?[^"]+\"?)\)/g, '$1')
        .replace(/Timestamp\(([^)]+)\)/g, (match, p1) => {

          const date = new Date();
          return `"${date.toISOString()}"`;
        });

      return JSON.parse(modifiedData);
    } catch (error) {
      console.error('Failed to safely parse MongoDB JSON', error);
      return null;
    }
  }

}
