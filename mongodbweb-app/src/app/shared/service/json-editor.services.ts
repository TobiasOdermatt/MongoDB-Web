import { Injectable } from '@angular/core';
import JSONEditor from 'jsoneditor';
import 'jsoneditor/dist/jsoneditor.min.css';

@Injectable({
  providedIn: 'root'
})
export class JsonEditorService {
  createEditor(element: HTMLElement, options: any): JSONEditor {
    return new JSONEditor(element, options);
  }
}
