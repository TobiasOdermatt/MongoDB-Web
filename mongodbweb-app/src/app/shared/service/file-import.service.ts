import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { EventEmitter, Injectable } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { Observable, catchError, of } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class FileImportService {
  public uploadProgress: EventEmitter<any> = new EventEmitter();

  constructor(private toastr: ToastrService, private http: HttpClient) {}

  async uploadChunks(file: File): Promise<string | void> {
    const chunkSize = 20 * 1024 * 1024; // 20 MB
    let start = 0;
    let end = chunkSize;
    let chunkIndex = 0;
    const totalChunks = Math.ceil(file.size / chunkSize);
    const maxRetries = 3;

    while (start < file.size) {
      const chunk = file.slice(start, end);
      const formData = new FormData();
      formData.append('file', chunk, file.name);
      formData.append('chunkIndex', chunkIndex.toString());
      formData.append('totalChunks', totalChunks.toString());

      let progress = Math.round(((chunkIndex + 1) / totalChunks) * 100);
      const processedMB = Math.round((end / (1024 * 1024)) * 100) / 100;
      this.uploadProgress.emit({ totalChunks, chunkIndex, progress, processedMB });

      let response;
      let attempts = 0;

      while (attempts < maxRetries) {
        response = await fetch('/api/File/uploadFile', {
          method: 'POST',
          body: formData,
        });

        if (response.status === 200) {
          break;
        } else if (response.status === 204) {
          break;
        } else {
          attempts++;
          console.error(`Retry attempt ${attempts} for chunk ${chunkIndex}: ${response.statusText}`);
          if (attempts >= maxRetries) {
            this.toastr.error(`Error uploading chunk after ${maxRetries} attempts`);
            throw new Error(`Error uploading chunk ${chunkIndex} after ${maxRetries} attempts: ${response.statusText}`);
          }
        }
      }

      if (response.status === 200 && chunkIndex === totalChunks - 1) {
        console.log('Upload complete');
        return file.name;
      }

      start += chunkSize;
      end = Math.min(file.size, start + chunkSize);
      chunkIndex++;
    }
    return file.name;
  }


  processDataforImport(fileName: string, processGuid: string): Observable<any> {
    const url = `api/FileProcess/processImportAsync/${fileName}/${processGuid}`;
    return this.http.get<any>(url)
      .pipe(
        catchError((error: HttpErrorResponse) => {
          console.error('Error starting import process', error);
          this.toastr.error('Error starting import process');
          return of(null);
        })
      );
  }

  importData(dbName: string, checkedCollectionNames: string[], collectionNameChanges: { [key: string]: string }, adoptOid: boolean, fileName: string, guid: string): Observable<any> {
    const url = `/api/FileProcess/import`;
    const requestBody = {
      dbName,
      checkedCollectionNames,
      collectionNameChanges,
      adoptOid,
      fileName,
      guid
    };

    return this.http.post<any>(url, requestBody).pipe(
      catchError((error: HttpErrorResponse) => {
        console.error('Error during import', error);
        this.toastr.error('Error during import');
        return of(null);
      })
    );
  }
}
