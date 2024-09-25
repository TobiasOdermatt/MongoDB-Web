import { Component, HostListener, ElementRef, Input, ViewChild, Output, EventEmitter } from '@angular/core';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';

@Component({
  selector: 'app-db-card',
  templateUrl: './db-card.component.html',
  styleUrl: './db-card.component.css'
})
export class DbCardComponent {
  @Input() db: any;
  @Output() deleteDatabase: EventEmitter<string> = new EventEmitter();
  @Output() downloadDatabase = new EventEmitter<string>();
  @Output() detailDatabase = new EventEmitter<string>();
  @Output() databaseClick = new EventEmitter<string>();

  statsKeys: string[] = [];

  static currentlyOpen: DbCardComponent | null = null;
  public isDropdownOpen = false;
  @ViewChild('dropdownMenu', { static: false }) dropdownMenu: ElementRef;

  constructor(private eRef: ElementRef, private modalService: NgbModal) { }

  ngOnInit() {
  }

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
    if (DbCardComponent.currentlyOpen && DbCardComponent.currentlyOpen !== this) {
      DbCardComponent.currentlyOpen.closeDropdown();
    }
    this.isDropdownOpen = !this.isDropdownOpen;
    if (this.isDropdownOpen) {
      DbCardComponent.currentlyOpen = this;
    } else {
      DbCardComponent.currentlyOpen = null;
    }
  }

  closeDropdown() {
    if (this.isDropdownOpen) {
      this.isDropdownOpen = false;
    }
  }

  isCriticalDatabase() {
    let dbName = this.db['name'];
    return dbName === 'admin' || dbName === 'config' || dbName === 'local';
  }
}
