import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-delete-modal',
  templateUrl: './delete-modal.component.html',
  styleUrl: './delete-modal.component.css'
})
export class DeleteModalComponent {
  @Input() entityValue: string;
  @Input() entityType: string;
  @Output() closeModal: EventEmitter<any> = new EventEmitter();
  @Output() deleteConfirmed: EventEmitter<any> = new EventEmitter();

  typedEntityValueConfirmation: string = '';

  constructor() { }

  onConfirmDelete() {
    this.deleteConfirmed.emit(this.entityValue);
  }

  onCloseModal() {
    this.closeModal.emit()
  }
}
