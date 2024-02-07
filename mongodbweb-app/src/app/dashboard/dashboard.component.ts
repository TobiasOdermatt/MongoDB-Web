import { Component, OnInit } from '@angular/core';
import { DatabaseService } from '../shared/service/database.service';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {
  numberOfDBs: number = 0;
  databaseList: any[] = [];
  databaseStats: any[] = [];

  constructor(private databaseService: DatabaseService) { }

  ngOnInit() {
    this.loadDatabaseList();
  }

  loadDatabaseList() {
    this.databaseService.getListDb().subscribe({
      next: (data) => {
        if (data && data.databases) {
          this.databaseList = data.databases;
          this.numberOfDBs = data.databases.length;
          console.log('Database list fetched', data);
          this.loadDatabaseStatistics();
        }
      },
      error: (error) => {
        console.error('There was an error fetching the database list', error);
      }
    });
  }

  loadDatabaseStatistics() {
    this.databaseList.forEach(db => {
      if (db.name) {
        this.databaseService.getDatabaseStatistics(db.name).subscribe({
          next: (stats) => {
            if (stats) {

              this.databaseStats.push({ dbName: db.name, stats: stats });
            }
          },
          error: (error) => {
            console.error(`There was an error fetching statistics for database ${db.name}`, error);
          }
        });
      }
    });
  }

  getStatsForDb(dbName: string): any {
    const stats = this.databaseStats.find(stat => stat.dbName === dbName);
    return stats ? stats.stats : null;
  }
}
