import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import {DashboardComponent} from "./dashboard/dashboard.component";
import {LoginComponent} from "./login/login.component";
import { AuthGuard } from './shared/service/auth-guard.service';
import { GloablStatisticComponent } from './gloabl-statistic/gloabl-statistic.component';
import { CollectionListComponent } from './dashboard/components/containers/collection-list/collection-list.component';

const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: '', component: DashboardComponent, canActivate: [AuthGuard] },
  { path: 'statistic', component: GloablStatisticComponent, canActivate: [AuthGuard] },
  { path: 'db/:dbName', component: CollectionListComponent, canActivate: [AuthGuard] },
  { path: '**', component: DashboardComponent, canActivate: [AuthGuard] }, 
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
