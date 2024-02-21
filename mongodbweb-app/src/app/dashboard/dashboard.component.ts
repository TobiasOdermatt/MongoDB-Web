import { Component, ViewEncapsulation } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
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

constructor(private activatedRoute: ActivatedRoute, private router: Router) { }

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
  }

  handleDatabaseClick(dbName: string) {
    this.selectedDbName = dbName;
    this.router.navigate(['/db', dbName]);
  }

  handleCollectionClick(click: any) {
    this.selectedCollectionName = click.collectionName;
    this.selectedDbName = click.dbName;
    this.router.navigate(['/db', click.dbName, click.collectionName]);
  }

  handleDocumentSearch(search: any) {
    const queryParams = {
      currentPage: search.currentPage,
      limit: search.limit,
      selectedKey: search.selectedKey,
      searchKeyword: search.searchKeyword
    };
    this.router.navigate(['/db', this.selectedDbName, this.selectedCollectionName], { queryParams });
  }

  checkIfDbIsSet() {
    return this.selectedDbName !== undefined;
  }

  checkIfCollectionIsSet() {
    return this.selectedDbName !== undefined && this.selectedCollectionName !== undefined;
  }
}
