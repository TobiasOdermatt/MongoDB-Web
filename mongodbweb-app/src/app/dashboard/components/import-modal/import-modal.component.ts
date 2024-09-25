import { Component, EventEmitter, Output, ViewChild, ElementRef } from '@angular/core';
import { FileImportService } from '../../../shared/service/file-import.service';
import { ToastrService } from 'ngx-toastr';
import { ProgressService } from '../../../shared/service/progress.service';
import { DatabaseService } from '../../../shared/service/database.service';

@Component({
  selector: 'app-import-modal',
  templateUrl: './import-modal.component.html',
  styleUrls: ['./import-modal.component.css']
})
export class ImportModalComponent {
  @Output() closeModal: EventEmitter<any> = new EventEmitter();
  @Output() updatDbList: EventEmitter<void> = new EventEmitter();

  @ViewChild('fileInput', { static: false }) fileInput: ElementRef;
  fileName: string = null;
  progressInformation = { totalMB: 0, processedMb: 0, progress: 0, guid: '', sizeType: 'MB', progressType: '' };
  currentImportGuid: string;

  showProgressBar = false;
  showUploadSection = true;
  showUserSelectionSection = false;
  databaseExistWarning: boolean = false;

  adoptOid: boolean;
  selectAll: boolean = true;

  databaseName: string;
  fileSize: string ='0';
  collectionsNames: string[] = [];
  checkedCollectionsNames: string[] = [];
  collectionNameChanges: Record<string, string> = {};

  constructor(private fileImportService: FileImportService, private toastr: ToastrService, private progressService: ProgressService, private databaseService: DatabaseService) {
    this.showProgressBar = false;
    this.showUploadSection = true;
    this.showUserSelectionSection = false;
    this.fileImportService.uploadProgress.subscribe(event => {
      this.progressInformation.progress = event.progress;
      this.progressInformation.progressType = 'upload';
      this.progressInformation.processedMb = event.processedMB;
    });
    this.initializeProgressListener();
  }

  initializeProgressListener() {
    this.progressService.listenForProgress((totalMB, processedMb, progress, guid, sizeType, messageType) => {
      this.updateProgress(totalMB, processedMb, progress, guid, sizeType, messageType);
    });
  }

  updateProgress(totalMB: number, processedMb: number, progress: number, guid: string, sizeType: string, progressType: string) {
    if (this.currentImportGuid === guid) {
      this.progressInformation = { totalMB, processedMb, progress, guid, sizeType, progressType };
    }
  }

  toggleSelectAll(): void {
    if (this.selectAll) {
      this.checkedCollectionsNames = [...this.collectionsNames];
    } else {
      this.checkedCollectionsNames = [];
    }
  }

  isChecked(collectionName: string): boolean {
    return this.checkedCollectionsNames.includes(collectionName);
  }

  handleDatabaseNameChange(newValue: string): void {
    if(this.isNameValid(newValue)){
      this.databaseName = newValue;
      this.showExistsWarning(this.databaseName);
    }
  }

  handleCheckboxChange(collectionName: string, event: any): void {
    if (event.target.checked) {
      this.checkedCollectionsNames.push(collectionName);
    } else {
      this.checkedCollectionsNames = this.checkedCollectionsNames.filter(name => name !== collectionName);
    }
  }
  
  processFile(file: File) {
    this.fileSize = (file.size / 1024 / 1024).toFixed(2);
    this.fileName = file.name;
  }

  onDragOver(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
  }

  handleFileDrop(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    if (event.dataTransfer.files.length > 0) {
      const file = event.dataTransfer.files[0];
      this.processFile(file);
    }
  }

  triggerFileInput() {
    this.fileInput.nativeElement.click();
  }

  onFileSelected(event: Event) {
    const files = (event.target as HTMLInputElement).files;
    if (files && files.length > 0) {
      const file = files[0];
      this.processFile(file);
    }
  }

  trackByFn(index, item) {
    return index;
  }

  async upload() {
    const file = this.fileInput.nativeElement.files[0];
    if (file) {
      this.showUploadSection = false;
      this.showProgressBar = true;
      try {
        let fileName = await this.fileImportService.uploadChunks(file);
        if (typeof fileName !== 'string') {
          throw new Error('Filename was not returned from upload.');
        }
        this.toastr.info("File uploaded " + fileName);

        this.fileImportService.processDataforImport(fileName, this.currentImportGuid).toPromise().then((processResult) => {
          this.showProgressBar = false;
          this.toastr.info("Data processing complete");
          this.showUserSelectionSection = true;
          this.databaseName = processResult.databaseName;
          this.showExistsWarning(this.databaseName);
          let collectionNamesString = JSON.stringify(processResult.collectionsNames);
          this.collectionsNames = collectionNamesString ? JSON.parse(collectionNamesString) : [];
          this.checkedCollectionsNames = collectionNamesString ? JSON.parse(collectionNamesString) : [];
          this.collectionsNames.forEach(collectionName => {
            this.collectionNameChanges[collectionName] = collectionName;
          });
        });
      } catch (error) {
        console.error('Upload or processing failed', error);
        this.toastr.error('Upload failed: ' + error.message);
      }
    }
  }

  async import() {
    if (!this.fileName || !this.currentImportGuid) {
      console.error('FileName or Import GUID is missing');
      this.toastr.error('Import failed: Missing fileName or GUID');
      return;
    }
    this.showUserSelectionSection = false;
    this.showProgressBar = true;
    this.fileImportService.importData(this.databaseName, this.checkedCollectionsNames, this.collectionNameChanges, this.adoptOid, this.fileName, this.currentImportGuid)
      .subscribe({
        next: (response) => {
          console.log('Import successful', response);
          this.toastr.info('Data import successful');
          this.updatDbList.emit();
        },
      });
}

showExistsWarning(dbName: string): void {
  this.databaseService.checkDatabaseExistence(dbName.trim()).subscribe({
    next: (response) => {
      if (response.exists) {
        this.databaseExistWarning = true;
      } else {
        this.databaseExistWarning = false;
      }
    },
    error: (error) => {
      console.error('There was an error checking the database existence', error);
      this.toastr.error('There was an error checking the database existence');
    }
  });
}

isNameValid(name) {
  if (!name.trim() || name.length >= 64 || !name.trim() ||name.length >= 64) {
    this.toastr.info('Database and collection names must be between 1 and 63 characters.');
    return false;
  }
  const invalidChars = /[\/\."$*<>:|?\x00 ]/; 
  if (invalidChars.test(name)) {
    this.toastr.info('Names cannot contain /\\. "$*<>:|? or the null character.');
    return false;
  }
  return true;
}
}
