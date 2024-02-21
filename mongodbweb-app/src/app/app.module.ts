import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import {DashboardComponent} from "./dashboard/dashboard.component";
import {LoginComponent} from "./login/login.component";
import { NavComponent } from './shared/components/nav/nav.component';
import { DbCardComponent } from './dashboard/components/db-card/db-card.component';
import { GloablStatisticComponent } from './gloabl-statistic/gloabl-statistic.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ToastrModule } from 'ngx-toastr';
import { DatabaseListComponent } from './dashboard/containers/database-list/database-list.component';
import { DeleteModalComponent } from './shared/components/delete-modal/delete-modal.component';
import { DownloadModalComponent } from './shared/components/download-modal/download-modal.component';
import { ImportModalComponent } from './dashboard/components/import-modal/import-modal.component';
import { CollectionListComponent } from './dashboard/containers/collection-list/collection-list.component';
import { CreateCollectionModalComponent } from './dashboard/components/create-collection-modal/create-collection-modal.component';
import { InfoJsonModalComponent } from './dashboard/components/info-json-modal/info-json-modal.component';
import { CollectionCardComponent } from './dashboard/components/collection-card/collection-card.component';
import { DocumentListComponent } from './dashboard/containers/document-list/document-list.component';
import { DocumentTextareaComponent } from './dashboard/components/document-textarea/document-textarea.component';
import { DocumentContentComponent } from './dashboard/components/document-content/document-content.component';

@NgModule({
  declarations: [
    AppComponent,
    LoginComponent,
    NavComponent,
    DashboardComponent,
    CollectionCardComponent,
    DbCardComponent,
    GloablStatisticComponent,
    DatabaseListComponent,
    CollectionListComponent,
    DocumentListComponent,
    DeleteModalComponent,
    DownloadModalComponent,
    ImportModalComponent,
    CreateCollectionModalComponent,
    InfoJsonModalComponent,
    DocumentListComponent,
    DocumentTextareaComponent,
    DocumentContentComponent
  ],
  imports: [
    BrowserModule, HttpClientModule, NgbModule, FormsModule, AppRoutingModule, BrowserAnimationsModule,
    ToastrModule.forRoot({
      timeOut: 2000,
      positionClass: 'toast-top-right',

    }),

  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
