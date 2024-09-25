import { Component, ViewEncapsulation } from '@angular/core';
import { Location } from '@angular/common';
import { ActivatedRoute, NavigationEnd, Router } from '@angular/router';
import { filter } from 'rxjs';
@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css'],
  encapsulation: ViewEncapsulation.None
})
export class DashboardComponent {
  selectedDbName: string;
  selectedCollectionName: string;

  searchParam = {limit: 10, selectedKey: '', searchKeyword: '', currentPage: 1};
  downloadDatabaseInformation = { totalCollections: 0, processedCollections: 0, progress: 0, guid: '' };

  constructor(private activatedRoute: ActivatedRoute, private location: Location, private router: Router) { }

  ngOnInit() {
    this.activatedRoute.paramMap.subscribe(params => {
      const dbName = params.get('dbName');
      const collectionName = params.get('collectionName');
      if (dbName) this.selectedDbName = dbName;
      if (collectionName) this.selectedCollectionName = collectionName;
    });

    this.activatedRoute.queryParamMap.subscribe(params => {
      this.searchParam = {
        limit: params.get('limit') ? Number(params.get('limit')) : 10,
        selectedKey: params.get('selectedKey') ? params.get('selectedKey') : '',
        searchKeyword: params.get('searchKeyword') ? params.get('searchKeyword') : '',
        currentPage: params.get('currentPage') ? Number(params.get('currentPage')) : 1
      };
    });

    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe((event: NavigationEnd) => {
      if (event.url === '/' || event.urlAfterRedirects === '/') {
        this.resetToDefaultState();
      }
    });
  }

  resetToDefaultState() {
    this.selectedDbName = undefined;
    this.selectedCollectionName = undefined;
  }


  handleDatabaseClick(dbName: string) {
    this.selectedDbName = dbName;
    this.location.replaceState(`/db/${dbName}`);
  }

  handleCollectionClick(click: any) {
    this.selectedCollectionName = click.collectionName;
    this.selectedDbName = click.dbName;
    this.location.replaceState(`/db/${click.dbName}/${click.collectionName}`);
  }

  handleDocumentSearch(search: any) {
    const queryParams = new URLSearchParams({
      currentPage: search.currentPage.toString(),
      limit: search.limit.toString(),
      selectedKey: search.selectedKey,
      searchKeyword: search.searchKeyword
    }).toString();
    const newPath = `/db/${this.selectedDbName}/${this.selectedCollectionName}?${queryParams}`;
    this.location.replaceState(newPath);
  }


  checkIfDbIsSet() {
    return this.selectedDbName !== undefined;
  }

  checkIfCollectionIsSet() {
    return this.selectedDbName !== undefined && this.selectedCollectionName !== undefined;
  }
}
