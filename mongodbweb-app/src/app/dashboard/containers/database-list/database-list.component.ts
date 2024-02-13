import { Component } from '@angular/core';
import { DatabaseService } from '../../../shared/service/database.service';
import { ToastrService } from 'ngx-toastr';
import { DeleteModalComponent } from '../../../shared/components/delete-modal/delete-modal.component';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { InfoJsonModalComponent } from '../../components/info-json-modal/info-json-modal.component';
import { CreateCollectionModalComponent } from '../../components/create-collection-modal/create-collection-modal.component';
import { DownloadModalComponent } from '../../../shared/components/download-modal/download-modal.component';
import { v4 as uuidv4 } from 'uuid';
import { firstValueFrom } from 'rxjs';
import { ImportModalComponent } from '../../components/import-modal/import-modal.component';

@Component({
  selector: 'app-database-list',
  templateUrl: './database-list.component.html',
  styleUrl: './database-list.component.css'
})
export class DatabaseListComponent {
  databaseList: any[] = [];

  constructor(
    private modalService: NgbModal,
    private dbService: DatabaseService,
    private toastr: ToastrService
  ) { }
  ngOnInit() {
    this.updateDatabaseListAndStats();
  }

  updateDatabaseListAndStats() {
    this.dbService.getListDb().subscribe({
      next: (data) => {
        if (data && data.databases) {
          const updatedDbList = data.databases;
          const updatedDbNames = updatedDbList.map(db => db.name);

          updatedDbList.forEach(db => {
            this.dbService.getDatabaseStatistics(db.name).subscribe({
              next: (response) => {
                const dbIndex = this.databaseList.findIndex(d => d.name === db.name);
                if (dbIndex !== -1) {
                  this.databaseList[dbIndex] = { ...db, stats: response.databaseStatistics };
                } else {
                  this.databaseList.push({ ...db, stats: response.databaseStatistics });
                }
                            }
            });
          });

          this.databaseList = this.databaseList.filter(db => updatedDbNames.includes(db.name));
        }
            }
    });
  }

  private subscribeToCloseModal(modalRef: any) {
    modalRef.componentInstance.closeModal.subscribe(() => {
      modalRef.dismiss();
    });
  }

  openDeleteDatabase(dbName: string) {
    const modalRef = this.modalService.open(DeleteModalComponent, { centered: true });
    modalRef.componentInstance.entityValue = dbName;
    modalRef.componentInstance.entityType = 'database';
  
    modalRef.componentInstance.deleteConfirmed.subscribe((entityValue: string) => {
      this.dbService.deleteDatabase(entityValue).subscribe(response => {
        if (response.success) {
          this.toastr.info('Database deleted successfully', dbName);
          this.updateDatabaseListAndStats();
        }
      });
      modalRef.close();
    });
  
    this.subscribeToCloseModal(modalRef);
  }

  openDetailDatabase(dbName: string) {
    const modalRef = this.modalService.open(InfoJsonModalComponent, { centered: true });
    const detailDb = this.databaseList.find(db => db.name === dbName);
    if (detailDb && detailDb.stats) {
      modalRef.componentInstance.json = detailDb.stats;
      modalRef.componentInstance.title = "Details of " + dbName;
      modalRef.componentInstance.jsonKeys = Object.keys(detailDb.stats);
    }
    this.subscribeToCloseModal(modalRef);
  }

  openCreateCollection() {
  const modalRef = this.modalService.open(CreateCollectionModalComponent, { centered: true });
  modalRef.componentInstance.createCollection.subscribe((event) => {
    this.dbService.createCollection(event.newDbName, event.newCollectionName).subscribe({
      next: (response) => {
        if (response && response.success) {
          this.toastr.info('Collection created successfully');
          this.updateDatabaseListAndStats();
        this.modalService.dismissAll();
          }
        }
      });
    });
  this.subscribeToCloseModal(modalRef);
  }

  
  openImport() {
    const modalRef = this.modalService.open(ImportModalComponent, { centered: true })
  }

  async openDownloadDatabase(dbName: string) {
    const guid = uuidv4();
    const modalRef = this.modalService.open(DownloadModalComponent, { centered: true });
    const downloadComponent: DownloadModalComponent = modalRef.componentInstance as DownloadModalComponent;
    downloadComponent.currentDownloadGuid = guid;
    const response = await firstValueFrom(this.dbService.prepareDatabaseDownload(dbName, guid));
    if (response && response.success) {
      this.toastr.info('Database download initiated successfully');
      downloadComponent.startDownload(response.fileName);
      setTimeout(() => this.modalService.dismissAll(), 400);
    }
}
}
