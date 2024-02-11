import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { DatabaseService } from '../shared/service/database.service';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { ToastrService } from 'ngx-toastr';
import { firstValueFrom } from 'rxjs';
import { ProgressService } from '../shared/service/progress.service';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {
  databaseList: any[] = [];

  @ViewChild('downloadModal', { static: false }) downloadModal: ElementRef;
  currentDownloadGuid: string = '';
  downloadInformation = { totalCollections: 0, processedCollections: 0, progress: 0, guid: '' };

  newDbName: string = '';
  newCollectionName: string = '';

  constructor(
    private dbService: DatabaseService,
    private progressService: ProgressService,
    private toastr: ToastrService,
    private modalService: NgbModal
  ) { }


  ngOnInit() {
    this.updateDatabaseListAndStats();
    this.progressService.startConnection().then(() => {
      this.progressService.listenForDatabaseProgress(this.updateDownloadProgress.bind(this));
    });
  }

  ngOnDestroy() {
    this.progressService.stopConnection();
  }

  updateDownloadProgress(totalCollections: number, processedCollections: number, progress: number, guid: string, messageType: string) {
    if (this.currentDownloadGuid === guid && messageType === 'download') {
      this.downloadInformation = { totalCollections, processedCollections, progress, guid };
      if (progress === 100) setTimeout(() => this.modalService.dismissAll(), 400);
    }
  }

  updateDatabaseListAndStats() {
    this.dbService.getListDb().subscribe({
      next: (data) => {
        if (data && data.databases) {
          const updatedDbList = data.databases;
          const updatedDbNames = updatedDbList.map(db => db.name);

          updatedDbList.forEach(db => {
            this.dbService.getDatabaseStatistics(db.name).subscribe({
              next: (stats) => {
                const dbIndex = this.databaseList.findIndex(d => d.name === db.name);
                if (dbIndex !== -1) {
                  this.databaseList[dbIndex] = { ...db, stats: stats };
                } else {
                  this.databaseList.push({ ...db, stats: stats });
                }
              },
              error: (error) => {
                this.logAndToastError(`There was an error fetching statistics for database ${db.name}`, error);
              }
            });
          });

          this.databaseList = this.databaseList.filter(db => updatedDbNames.includes(db.name));
        }
      },
      error: (error) => {
        this.logAndToastError('There was an error fetching the database list', error);
      }
    });
  }

  openModal(content) {
    this.newDbName = '';
    this.newCollectionName = '';
    this.modalService.open(content, { centered: true });
  }

  createCollection() {
    this.modalService.dismissAll();
    if (!this.newDbName || !this.newCollectionName) {
      this.logAndToastError('Database name and collection name are required.');
      return;
    }

    this.dbService.createCollection(this.newDbName, this.newCollectionName).subscribe({
      next: (response) => {
        if (response && response.success) {
          this.toastr.info('Collection created successfully')
          this.updateDatabaseListAndStats();
        } else {
          this.logAndToastError(`Failed to create collection '${this.newCollectionName}' in database '${this.newDbName}': ${response.message}`);
        }
      },
      error: (error) => {
        this.logAndToastError(`Error creating collection '${this.newCollectionName}' in database '${this.newDbName}'`, error);
      }
    });
  }

  async handleDownloadRequest(event: { dbName: string, guid: string }) {
    this.resetDownloadInfo(event.guid);
    this.currentDownloadGuid = event.guid;
    this.modalService.open(this.downloadModal, { centered: true });

    try {
      const response = await firstValueFrom(this.dbService.prepareDatabaseDownload(event.dbName, event.guid));
      if (response && response.success) {
        this.toastr.info('Database download initiated successfully');
        this.startDownload(response);
      } else {
        this.logAndToastError(`Failed to initiate download for database '${event.dbName}': ${response.message}`);
      }
    } catch (error) {
      this.logAndToastError(`Error initiating download for database '${event.dbName}':`, error);
    }
  }

  startDownload(response) {
    if (response.fileName) {
      const downloadUrl = `/api/file/downloadFile/${response.fileName}`;
      const a = document.createElement('a');
      a.href = downloadUrl;
      a.download = '';
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
    }
  }

  resetDownloadInfo(guid: string) {
    this.downloadInformation = { totalCollections: 0, processedCollections: 0, progress: 0, guid };
  }

  logAndToastError(message: string, error?: any) {
    console.error(message, error);
    this.toastr.error('Failed to initiate download');
  }
}
