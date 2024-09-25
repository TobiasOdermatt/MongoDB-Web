import { Component, EventEmitter, Output } from '@angular/core';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-create-collection-modal',
  templateUrl: './create-collection-modal.component.html',
  styleUrl: './create-collection-modal.component.css'
})
export class CreateCollectionModalComponent {
  @Output() closeModal: EventEmitter<any> = new EventEmitter();
  @Output() createCollection: EventEmitter<any> = new EventEmitter();
  newDbName: string = '';
  newCollectionName: string = '';

  constructor(private toastr: ToastrService) { }

  isNameValid() {
    if (!this.newDbName.trim() || this.newDbName.length >= 64 || !this.newCollectionName.trim() || this.newCollectionName.length >= 64) {
      this.toastr.info('Database and collection names must be between 1 and 63 characters.');
      return false;
    }
    const invalidChars = /[\/\."$*<>:|?\x00 ]/; 
    if (invalidChars.test(this.newDbName) || invalidChars.test(this.newCollectionName)) {
      this.toastr.info('Names cannot contain /\\. "$*<>:|? or the null character.');
      return false;
    }
    return true;
  }

  onCloseModal() {
    this.closeModal.emit()
  }

  create(){
    if (this.isNameValid()) {
      this.createCollection.emit({newDbName: this.newDbName, newCollectionName: this.newCollectionName});
    }
  }
}
