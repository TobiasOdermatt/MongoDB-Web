import { Component, EventEmitter, Input, Output } from '@angular/core';
import { IconService } from '../../../shared/service/icon.services';
import { DatabaseService } from '../../../shared/service/database.service';
import { catchError, forkJoin, of, switchMap, tap } from 'rxjs';

@Component({
  selector: 'app-document-list',
  templateUrl: './document-list.component.html',
  styleUrl: './document-list.component.css'
})
export class DocumentListComponent {
  @Input() dbName: string;
  @Input() collectionName: string;
  @Input() itemsPerPage: number = 10;
  @Input() currentPage: number = 1;
  @Input() selectedKey: string = '';
  @Input() searchKeyword: string = '';

  @Output() searchDocuments = new EventEmitter<{ limit: number, selectedKey: string, searchKeyword: string, currentPage: number }>();

  documentsList: any[];
  attributeKeyList: any[] = [];
  totalPages: number = 1;
  documentsTotalCount: number = 0;
  hideObjectId: boolean = true;
  pageExist = true;

  constructor(public iconService: IconService, private databaseService: DatabaseService) { }

  onSearchDocuments() {
    this.searchDocuments.emit({ limit: this.itemsPerPage, selectedKey: this.selectedKey, searchKeyword: this.searchKeyword, currentPage: this.currentPage });
  }

  ngOnInit() {
    this.loadDocuments();
  }

  isPreviousButtonDisabled() {
    return this.currentPage === 1;
  }

  isNextButtonDisabled() {
    return this.currentPage >= this.totalPages;
  }

  search() {
    this.currentPage = 1;
    this.loadDocuments();
  }

  gotoPreviousPage() {
    if (this.currentPage > 1) {
      this.currentPage = this.currentPage - 1;
      this.loadDocuments();
    }
  }

  goToNextPage() {
    if (this.currentPage < this.totalPages) {
      this.currentPage = this.currentPage + 1;
      this.loadDocuments();
    }
  }

  goToPage(page: number) {
    if (page < 1 || page > this.totalPages) return;
    this.currentPage = page;
    this.loadDocuments();
  }

  get pagesToShow(): number[] {
    let pages: number[];
    if (this.totalPages <= 5) {
      pages = Array.from({ length: this.totalPages }, (_, i) => i + 1);
    } else if (this.currentPage <= 3) {
      pages = [1, 2, 3, 4, 5];
    } else if (this.currentPage > this.totalPages - 3) {
      pages = Array.from({ length: 5 }, (_, i) => this.totalPages - 4 + i);
    } else {
      pages = Array.from({ length: 5 }, (_, i) => this.currentPage - 2 + i);
    }
    return pages;
  }


  loadDocuments(): void {
    this.onSearchDocuments();
    this.itemsPerPage = this.itemsPerPage || 10;
    let skip = (this.currentPage - 1) * this.itemsPerPage;
    skip = skip == -10 ? 0 : skip;
    this.databaseService.getPaginatedCollection(this.dbName, this.collectionName, skip, this.itemsPerPage, this.selectedKey, this.searchKeyword)
      .pipe(
        tap(response => {
          this.documentsList = response.documents || [];
        }),
        switchMap(() =>
          forkJoin({
            totalCount: this.databaseService.getTotalCount(this.dbName, this.collectionName, this.selectedKey, this.searchKeyword),
            attributeKeys: this.databaseService.getCollectionAttributes(this.dbName, this.collectionName)
          })
        ),
        catchError(error => {
          console.error('Error loading data:', error);
          return of({ totalCount: { totalCount: 0 }, attributeKeys: [] });
        })
      )
      .subscribe({
        next: ({ totalCount, attributeKeys }) => {
          this.documentsTotalCount = totalCount.totalCount;
          this.totalPages = Math.ceil(this.documentsTotalCount / this.itemsPerPage);
          this.attributeKeyList = attributeKeys.attributes || [];
        },
        error: (error) => console.error('Failed to fetch data:', error)
      });
  }
}
