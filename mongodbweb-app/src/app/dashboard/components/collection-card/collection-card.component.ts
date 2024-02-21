import { Component, HostListener, ElementRef, Input, ViewChild, Output, EventEmitter } from '@angular/core';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';

@Component({
  selector: 'app-collection-card',
  templateUrl: './collection-card.component.html',
  styleUrl: './collection-card.component.css'
})
export class CollectionCardComponent {
  @Input() collection: any;
  @Input() dbName: string;
  @Output() deleteCollection: EventEmitter<string> = new EventEmitter();
  @Output() downloadCollection = new EventEmitter<string>();
  @Output() detailCollection = new EventEmitter<string>();
  @Output() clickCollection = new EventEmitter<{ dbName: string, collectionName: string }>();

  statsKeys: string[] = [];

  static currentlyOpen: CollectionCardComponent | null = null;
  public isDropdownOpen = false;
  @ViewChild('dropdownMenu', { static: false }) dropdownMenu: ElementRef;

  constructor(private eRef: ElementRef, private modalService: NgbModal) { }

  @HostListener('document:click', ['$event'])
  clickOutside(event) {
    if (!this.eRef.nativeElement.contains(event.target)) {
      this.closeDropdown();
    } else if (this.dropdownMenu && !this.dropdownMenu.nativeElement.contains(event.target)) {
      this.closeDropdown();
    }
  }

  toggleDropdown(event: MouseEvent) {
    event.stopPropagation();
    if (CollectionCardComponent.currentlyOpen && CollectionCardComponent.currentlyOpen !== this) {
      CollectionCardComponent.currentlyOpen.closeDropdown();
    }
    this.isDropdownOpen = !this.isDropdownOpen;
    if (this.isDropdownOpen) {
      CollectionCardComponent.currentlyOpen = this;
    } else {
      CollectionCardComponent.currentlyOpen = null;
    }
  }

  closeDropdown() {
    if (this.isDropdownOpen) {
      this.isDropdownOpen = false;
    }
  }
}
