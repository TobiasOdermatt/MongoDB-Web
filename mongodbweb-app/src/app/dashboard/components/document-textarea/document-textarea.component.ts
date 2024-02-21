import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';

@Component({
  selector: 'app-document-textarea',
  templateUrl: './document-textarea.component.html',
  styleUrls: ['./document-textarea.component.css']
})
export class DocumentTextareaComponent implements OnChanges {
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

  getTextareaRows(text: string): number {
    const maxRows = 10;
    const lines = text.split('\n').length;
    const rows = Math.min(lines, maxRows);
    return rows;
  }
}
