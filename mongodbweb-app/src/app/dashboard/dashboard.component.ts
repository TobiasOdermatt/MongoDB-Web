import { Component, OnInit } from '@angular/core';
import { DatabaseListService } from '../shared/service/database-list.service';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {
  constructor(private databaseListService: DatabaseListService) { }

  ngOnInit() {
    this.databaseListService.getListDb().subscribe({
      next: (data) => {
        console.log(data);
      },
      error: (error) => {
        console.error('There was an error fetching the database list', error);
      }
    });
  }
}
