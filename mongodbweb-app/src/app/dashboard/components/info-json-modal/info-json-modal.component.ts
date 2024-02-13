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

  ngOnInit() {
  console.log(this.json)
  console.log(this.jsonKeys)
  }

  onCloseModal() {
    this.closeModal.emit()
  }
}
