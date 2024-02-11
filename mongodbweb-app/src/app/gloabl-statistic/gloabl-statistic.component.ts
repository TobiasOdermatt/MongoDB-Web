import { Component, OnInit } from '@angular/core';
import { DatabaseService } from '../shared/service/database.service';

@Component({
  selector: 'app-gloabl-statistic',
  templateUrl: './gloabl-statistic.component.html',
  styleUrls: ['./gloabl-statistic.component.css']
})
export class GloablStatisticComponent implements OnInit {
  stats: any = null;

  constructor(private databaseService: DatabaseService) { }

  ngOnInit() {
    this.databaseService.getGlobalStatistics().subscribe({
      next: (data: string) => {
        try {
          this.stats = this.databaseService.safeMongoDBJsonParse(data);
        } catch (error) {
          console.error('Error parsing global statistics with custom logic', error);
          this.stats = null;
        }
      },
      error: (error) => {
        console.error('Failed to fetch global statistics', error);
        this.stats = null;
      }
    });
  }

  displayElement(elementValue: any): string {
    if (Array.isArray(elementValue)) {
      return `[ ${elementValue.map(e => this.displayElement(e)).join(', ')} ]`;
    } else if (elementValue !== null && typeof elementValue === 'object') {
      let elements = [];
      for (const [key, value] of Object.entries(elementValue)) {
        elements.push(`${key}: ${this.displayElement(value)}`);
      }
      return `<ul>${elements.map(e => `<li>${e}</li>`).join('')}</ul>`;
    } else {
      return elementValue.toString();
    }
  }
}
