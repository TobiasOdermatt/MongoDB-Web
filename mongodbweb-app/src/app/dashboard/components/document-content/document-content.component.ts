import { Component, Input, SimpleChanges, ViewChild, ElementRef, AfterViewInit, OnDestroy } from '@angular/core';
import { JsonEditorService } from '../../../shared/service/json-editor.services';
import { isEqual } from 'lodash';

@Component({
  selector: 'app-document-content',
  templateUrl: './document-content.component.html',
  styleUrls: ['./document-content.component.css']
})
export class DocumentContentComponent implements AfterViewInit, OnDestroy {
  @Input() document: string;
  @ViewChild('jsonEditor') jsonEditorContainer: ElementRef;
  private editor: any;
  private initialDocument: any;
  showSaveButton: boolean = false;
  @Input() dbName;
  @Input() collectionName;
  @Input() hideObjectId: boolean;

  constructor(private jsonEditorService: JsonEditorService) { }

  ngAfterViewInit(): void {
    const options = {
      mode: 'code',
      modes: ['code', 'form', 'text', 'tree', 'view'],
      onChange: () => {
        const errors = this.editor.validate().__zone_symbol__value;
        if (errors.length === 0) {
          this.detectChanges();
        } else {
          console.error('JSON is invalid:', errors);
        }
      }
    };
    this.editor = this.jsonEditorService.createEditor(this.jsonEditorContainer.nativeElement, options);
    this.updateDocument();
  }


  ngOnChanges(changes: SimpleChanges): void {
    if (changes['document']) {
      this.updateDocument();
    }

    if (changes['hideObjectId']) {
      this.updateDocument();
    }
  }

  private updateDocument(): void {
    if (this.document && this.editor) {
      const docObj = JSON.parse(this.document);
      if (this.hideObjectId && docObj._id) {
        delete docObj._id;
      }
      this.initialDocument = JSON.parse(JSON.stringify(docObj));
      this.editor.set(docObj);
      this.showSaveButton = false;
    }
  }

  private detectChanges(): void {
    const currentDoc = this.editor.get();
    if (!isEqual(this.initialDocument, currentDoc)) {
      this.showSaveButton = true;
      console.log('Real change detected', currentDoc);
    } else {
      this.showSaveButton = false;
    }
  }

  ngOnDestroy(): void {
    if (this.editor) {
      this.editor.destroy();
    }
  }
}
