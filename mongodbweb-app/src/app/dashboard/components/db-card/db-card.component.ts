import { Component, HostListener, ElementRef, Input, ViewChild, Output, EventEmitter } from '@angular/core';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { DatabaseService } from '../../../shared/service/database.service';
import { ToastrService } from 'ngx-toastr';
import { v4 as uuidv4 } from 'uuid';

@Component({
  selector: 'app-db-card',
  templateUrl: './db-card.component.html',
  styleUrl: './db-card.component.css'
})
export class DbCardComponent {
  @Input() db: any;
  @Output() downloadRequest = new EventEmitter<{ dbName: string, guid: string }>();
  statsKeys: string[] = [];
  typedDbName: string = '';
  progress: number = 0; 

  static currentlyOpen: DbCardComponent | null = null;

  public isDropdownOpen = false;

  @ViewChild('dropdownMenu', { static: false }) dropdownMenu: ElementRef;

  constructor(private toastr: ToastrService, private databaseService: DatabaseService, private eRef: ElementRef, private modalService: NgbModal) { }

  ngOnInit() {
    if (this.db && this.db.stats) {
      this.statsKeys = Object.keys(this.db.stats);
    }
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

  deleteDatabase() {
    this.modalService.dismissAll();
    this.databaseService.deleteDatabase(this.db.name).subscribe(response => {
      if (response.success) {
        console.log('Database deleted successfully', response.message);
        this.toastr.info('Database deleted successfully', this.db.name);
      } else {
        console.error('There was an error deleting the database', response.message);
      }
    });
  }

  isCriticalDatabase(dbName) {
    return dbName === 'admin' || dbName === 'config' || dbName === 'local';
  }

  openModal(content) {
    this.typedDbName = '';
    this.modalService.open(content, { centered: true })
  }

  downloadDatabase(dbName: string) {
    const guid = uuidv4();
    this.downloadRequest.emit({ dbName, guid });
  }

}
