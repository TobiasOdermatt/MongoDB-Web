import { Component, Input, SimpleChanges } from '@angular/core';

@Component({
  selector: 'app-document-content',
  templateUrl: './document-content.component.html',
  styleUrl: './document-content.component.css'
})
export class DocumentContentComponent {
  @Input() document: string;
  @Input() dbName;
  @Input() collectionName;
  @Input() hideObjectId: boolean;

  documentToDisplay;
  objectId: string;
  success;

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['document']) {
      this.updateDocumentToDisplay();
    }

    if (changes['hideObjectId']) {
      this.updateDocumentToDisplay();
    }
  }

  private updateDocumentToDisplay(): void {
    if (this.document) {
      const docObj = JSON.parse(this.document);
      console.log(docObj)
      if (docObj._id) {
        this.objectId = docObj._id;
        if (this.hideObjectId) {
          delete docObj._id;
        }
      }
      this.documentToDisplay = JSON.stringify(docObj, null, 2);
    }
  }
}
