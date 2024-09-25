import { Component, EventEmitter, Output } from '@angular/core';
import { ProgressService } from '../../service/progress.service';

@Component({
  selector: 'app-download-modal',
  templateUrl: './download-modal.component.html',
  styleUrl: './download-modal.component.css'
})
export class DownloadModalComponent {
  @Output() closeModal: EventEmitter<any> = new EventEmitter();

  downloadDatabaseInformation = { totalCollections: 0, processedCollections: 0, progress: 0, guid: '' };
  currentDownloadGuid: string;

  constructor(private progressService: ProgressService) { }

  ngOnInit() {
    this.initializeProgressListener();
  }


  initializeProgressListener() {
    this.progressService.listenForDatabaseProgress((totalCollections, processedCollections, progress, guid, messageType) => {
      this.updateDownloadProgress(totalCollections, processedCollections, progress, guid, messageType);
    });
  }

  updateDownloadProgress(totalCollections: number, processedCollections: number, progress: number, guid: string, messageType: string) {
    if (this.currentDownloadGuid === guid && messageType === 'download') {
      this.downloadDatabaseInformation = { totalCollections, processedCollections, progress, guid };
    }
  }

  onCloseModal() {
    this.closeModal.emit()
  }

  startDownload(fileName: string) {
    if (fileName) {
      const downloadUrl = `/api/file/downloadFile/${fileName}`;
      const a = document.createElement('a');
      a.href = downloadUrl;
      a.download = '';
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
    }
  }
}
