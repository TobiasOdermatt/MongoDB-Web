import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-info-json-modal',
  templateUrl: './info-json-modal.component.html',
  styleUrl: './info-json-modal.component.css'
})
export class InfoJsonModalComponent {
  @Output() closeModal: EventEmitter<any> = new EventEmitter();
  @Input() json: any;
  @Input() jsonKeys: string;
  @Input() title: string;

  onCloseModal() {
    this.closeModal.emit()
  }

  skipKey(key: string) {
    let skipList = ["wiredTiger", "indexDetails", "indexSizes", "indexBuilds"];
    return skipList.includes(key);
  }
}
