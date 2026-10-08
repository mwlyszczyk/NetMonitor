import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { Monitor } from '../models/monitor';
import { MonitorResult } from '../models/monitor-result';
import { MonitorStatistics } from '../models/monitor-statistics';

@Injectable({
  providedIn: 'root'
})
export class MonitorService {

  private http = inject(HttpClient);

  private apiUrl = 'http://localhost:8080/api/monitors';

  getMonitors(): Observable<Monitor[]> {
    return this.http.get<Monitor[]>(this.apiUrl);
  }

  getMonitor(id: number): Observable<Monitor> {
    return this.http.get<Monitor>(
      `${this.apiUrl}/${id}`
    );
  }

  getResults(
    id: number,
    limit = 50
  ): Observable<MonitorResult[]> {
    return this.http.get<MonitorResult[]>(
      `${this.apiUrl}/${id}/results?limit=${limit}`
    );
  }

  getStatistics(
    id: number
  ): Observable<MonitorStatistics> {
    return this.http.get<MonitorStatistics>(
      `${this.apiUrl}/${id}/statistics`
    );
  }
}
