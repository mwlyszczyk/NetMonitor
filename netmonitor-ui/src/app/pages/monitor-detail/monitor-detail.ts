import { DatePipe } from '@angular/common';
import { Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { BaseChartDirective } from 'ng2-charts';
import { ChartData, ChartOptions } from 'chart.js';

import { MonitorService } from '../../services/monitor';
import { Monitor } from '../../models/monitor';
import { MonitorResult } from '../../models/monitor-result';
import { MonitorStatistics } from '../../models/monitor-statistics';

import { Subscription, forkJoin, timer, EMPTY, catchError, switchMap } from 'rxjs';

@Component({
  selector: 'app-monitor-detail',
  imports: [DatePipe, RouterLink, BaseChartDirective],
  templateUrl: './monitor-detail.html',
  styleUrl: './monitor-detail.css'
})
export class MonitorDetail implements OnInit, OnDestroy {
  private route = inject(ActivatedRoute);
  private monitorService = inject(MonitorService);
  private refreshSubscription?: Subscription;

  monitor = signal<Monitor | null>(null);
  statistics = signal<MonitorStatistics | null>(null);
  results = signal<MonitorResult[]>([]);
  loading = signal(true);
  error = signal('');

  chartData = signal<ChartData<'line'>>({
    labels: [],
    datasets: [{
      label: 'Response time (ms)',
      data: [],
      borderColor: '#2563eb',
      backgroundColor: 'rgba(37, 99, 235, 0.12)',
      fill: true,
      tension: 0.3,
      pointRadius: 3
    }]
  });

  chartOptions: ChartOptions<'line'> = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: { display: false }
    },
    scales: {
      y: {
        beginAtZero: true,
        title: {
          display: true,
          text: 'Response time (ms)'
        }
      },
      x: {
        title: {
          display: true,
          text: 'Check time'
        }
      }
    }
  };
  
  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));

    if (!Number.isInteger(id) || id <= 0) {
      this.error.set('Invalid monitor ID.');
      this.loading.set(false);
      return;
    }

    // Load the monitor's basic information once.
    this.monitorService.getMonitor(id).subscribe({
      next: monitor => {
        this.monitor.set(monitor);
        this.loading.set(false);
      },
      error: err => {
        console.error('Failed to load monitor:', err);
        this.error.set('Unable to load this monitor.');
        this.loading.set(false);
      }
    });

    // Refresh statistics and history immediately, then every 30 seconds.
    this.startAnalyticsRefresh(id);
  }

  private startAnalyticsRefresh(id: number): void {
    this.refreshSubscription = timer(0, 30000)
      .pipe(
        switchMap(() =>
          forkJoin({
            statistics: this.monitorService.getStatistics(id),
            results: this.monitorService.getResults(id, 50)
          }).pipe(
            catchError((error: unknown) => {
              console.error('Failed to refresh monitor analytics:', error);
              return EMPTY;
            })
          )
        )
      )
      .subscribe(({ statistics, results }) => {
        this.statistics.set(statistics);
        this.results.set(results);
        this.updateChart(results);

        console.log('Monitor analytics refreshed:', new Date().toLocaleTimeString());
      });
  }

  private updateChart(results: MonitorResult[]): void {
    // The API returns newest first; reverse a copy for the chart.
    const chronological = [...results].reverse();

    this.chartData.set({
      labels: chronological.map(result =>
        new Date(result.checkedAt).toLocaleTimeString()
      ),
      datasets: [{
        label: 'Response time (ms)',
        data: chronological.map(result => result.responseTimeMs),
        borderColor: '#2563eb',
        backgroundColor: 'rgba(37, 99, 235, 0.12)',
        fill: true,
        tension: 0.3,
        pointRadius: 3
      }]
    });
  }

  ngOnDestroy(): void {
    this.refreshSubscription?.unsubscribe();
  }
}
