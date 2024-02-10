import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { DatabaseService } from '../shared/service/database.service';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { ToastrService } from 'ngx-toastr';
import { ProgressService } from '../shared/service/progress.service';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {
  numberOfDBs: number = 0;
  databaseList: any[] = [];
  newDbName: string = '';
  newCollectionName: string = '';
  currentDownloadGuid: string = '';
  downloadInformation: DownloadProgressInformationInterface = {
    totalCollections: 0,
    processedCollections: 0,
    progress: 0,
    guid: ''
  };

  constructor(private progressService: ProgressService, private toastr: ToastrService, private modalService: NgbModal, private databaseService: DatabaseService) { }
  @ViewChild('downloadModal', { static: false }) downloadModal: ElementRef;


  ngOnInit() {
    this.loadDatabaseList();
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
      if (progress === 100) {
        setTimeout(() => {
          this.modalService.dismissAll();
        }, 400);
      }
    }
  }

  loadDatabaseList() {
    this.databaseService.getListDb().subscribe({
      next: (data) => {
        if (data && data.databases) {
          this.databaseList = data.databases;
          this.numberOfDBs = data.databases.length;
          this.loadDatabaseStatistics();
        }
      },
      error: (error) => {
        console.error('There was an error fetching the database list', error);
      }
    });
  }

  loadDatabaseStatistics() {
    this.databaseList.forEach((db, index) => {
      if (db.name) {
        this.databaseService.getDatabaseStatistics(db.name).subscribe({
          next: (stats) => {
            if (stats) {
              this.databaseList[index] = { ...db, stats: stats };
            }
          },
          error: (error) => {
            console.error(`There was an error fetching statistics for database ${db.name}`, error);
          }
        });
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
      console.error('Database name and collection name are required.');
      return;
    }

    this.databaseService.createCollection(this.newDbName, this.newCollectionName).subscribe({
      next: (response) => {
        if (response && response.success) {
          this.toastr.info('Collection created successfully', 'Success!')
          this.loadDatabaseList();
        } else {
          console.error(`Failed to create collection '${this.newCollectionName}' in database '${this.newDbName}': ${response.message}`);
        }
      },
      error: (error) => {
        console.error(`Error creating collection '${this.newCollectionName}' in database '${this.newDbName}':`, error);
      }
    });
  }

  handleDownloadRequest(event: { dbName: string, guid: string }) {
    this.currentDownloadGuid = event.guid;
    this.modalService.open(this.downloadModal, { centered: true });
    this.databaseService.prepareDatabaseDownload(event.dbName, event.guid).subscribe({
      next: (response) => {
        if (response && response.success) {
          this.toastr.info('Database download initiated successfully', 'Success!');
        } else {
          console.error(`Failed to initiate download for database '${event.dbName}': ${response.message}`);
          this.toastr.error('Failed to initiate download', 'Error!');
        }
      },
      error: (error) => {
        console.error(`Error initiating download for database '${event.dbName}':`, error);
        this.toastr.error('Failed to initiate download', 'Error!');
      }
    });
  }
}

interface DownloadProgressInformationInterface {
  totalCollections: number;
  processedCollections: number;
  progress: number;
  guid: string;
}
