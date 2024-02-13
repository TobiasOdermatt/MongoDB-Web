import { Component, EventEmitter, Output } from '@angular/core';

@Component({
  selector: 'app-import-modal',
  templateUrl: './import-modal.component.html',
  styleUrl: './import-modal.component.css'
})
export class ImportModalComponent {
  @Output() closeModal: EventEmitter<any> = new EventEmitter();
  fileName: string = null;
  processFile(file: File) {
    const fileSize = (file.size / 1024 / 1024).toFixed(2);
    this.fileName = `${file.name} (${fileSize} MB)`;
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

  triggerFileInput(fileInput: HTMLInputElement) {
    fileInput.click();
  }

  onFileSelected(event: Event) {
    const target = event.target as HTMLInputElement;
    if (target.files && target.files.length > 0) {
      const file = target.files[0];
      this.processFile(file);
    }
  }
}
