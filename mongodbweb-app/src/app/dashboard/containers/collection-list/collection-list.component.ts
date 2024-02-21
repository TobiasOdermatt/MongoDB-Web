import { Component, EventEmitter, Input, Output } from '@angular/core';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { DatabaseService } from '../../../shared/service/database.service';
import { ToastrService } from 'ngx-toastr';
import { DeleteModalComponent } from '../../../shared/components/delete-modal/delete-modal.component';
import { InfoJsonModalComponent } from '../../components/info-json-modal/info-json-modal.component';
import { ImportModalComponent } from '../../components/import-modal/import-modal.component';
import { v4 as uuidv4 } from 'uuid';
import { DownloadModalComponent } from '../../../shared/components/download-modal/download-modal.component';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-collection-list',
  templateUrl: './collection-list.component.html',
  styleUrl: './collection-list.component.css'
})
export class CollectionListComponent {
  @Input() dbName: string;
  @Output() clickCollection = new EventEmitter<{ dbName: string, collectionName: string }>();
  collectionList: any[] = []


  constructor(
    private modalService: NgbModal,
    private dbService: DatabaseService,
    private toastr: ToastrService,
    private databaseService: DatabaseService
  ) { }

  onCollectionClick(collectionName: string) {
    this.clickCollection.emit({ dbName: this.dbName, collectionName: collectionName });
  }

  ngOnInit() {
    this.updateCollectionList();
  }

  private subscribeToCloseModal(modalRef: any) {
    modalRef.componentInstance.closeModal.subscribe(() => {
      modalRef.dismiss();
    });
  }

  updateCollectionList() {
    this.dbService.getListCollections(this.dbName).subscribe({
      next: (data) => {
        if (data && data.collections) {
          this.collectionList = data.collections;
        }
      }
    });
  }

  openDeleteCollection(collectionName: any) {
    const modalRef = this.modalService.open(DeleteModalComponent, { centered: true });
    modalRef.componentInstance.entityValue = collectionName;
    modalRef.componentInstance.entityType = 'collection';

    modalRef.componentInstance.deleteConfirmed.subscribe((entityValue: string) => {
      this.dbService.deleteCollection(this.dbName, entityValue).subscribe(response => {
        if (response.success) {
          this.toastr.info('Database deleted successfully', this.dbName);
          this.updateCollectionList();
        }
      });
      modalRef.close();
    });

    this.subscribeToCloseModal(modalRef);
  }

  openDetailCollection(collectionName: string) {
    this.databaseService.getCollectionStatistics(this.dbName, collectionName).subscribe(response => {
      const detailCollection = this.collectionList.find(collection => collection.name === collectionName);
      if (detailCollection) {
        detailCollection.stats = response.collectionStatistics;

        const modalRef = this.modalService.open(InfoJsonModalComponent, { centered: true });
        if (detailCollection.stats) {
          modalRef.componentInstance.json = detailCollection.stats;
          modalRef.componentInstance.title = "Details of " + collectionName;
          modalRef.componentInstance.jsonKeys = Object.keys(detailCollection.stats);
        }
        this.subscribeToCloseModal(modalRef);
      }
    });
  }


  async openDownloadCollection(collectionName: string) {
    const guid = uuidv4();
    const modalRef = this.modalService.open(DownloadModalComponent, { centered: true });
    const downloadComponent: DownloadModalComponent = modalRef.componentInstance as DownloadModalComponent;
    downloadComponent.currentDownloadGuid = guid;
    const response = await firstValueFrom(this.dbService.prepareCollectionDownload(this.dbName, collectionName, guid));
    if (response && response.success) {
      this.toastr.info('Collection download initiated successfully');
      downloadComponent.startDownload(response.fileName);
      setTimeout(() => this.modalService.dismissAll(), 400);
    }
  }

  openImport() {
    const guid = uuidv4();
    const modalRef = this.modalService.open(ImportModalComponent, { centered: true });
    const importComponent: ImportModalComponent = modalRef.componentInstance as ImportModalComponent;
    importComponent.currentImportGuid = guid;
    importComponent.updatDbList.subscribe((event) => {
      this.updateCollectionList();
      this.modalService.dismissAll();
    });
    this.subscribeToCloseModal(modalRef);
  }
}
