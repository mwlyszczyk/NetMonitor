import {
  Component,
  OnDestroy,
  OnInit,
  inject,
  signal,
  ChangeDetectionStrategy,
} from '@angular/core';
import { Subscription, interval } from 'rxjs';

import { MonitorService } from '../../services/monitor';
import { Monitor } from '../../models/monitor';
import { MonitorStatistics } from '../../models/monitor-statistics';
import { MonitorCard } from '../../components/monitor-card/monitor-card';

@Component({
  selector: 'app-dashboard',
  imports: [MonitorCard],
  templateUrl: './dashboard.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './dashboard.css',
})
export class Dashboard implements OnInit, OnDestroy {
  private monitorService = inject(MonitorService);

  private refreshSubscription?: Subscription;

  monitors = signal<Monitor[]>([]);
  statistics = signal<Record<number, MonitorStatistics>>({});
  loading = signal(true);
  error = signal('');

  ngOnInit(): void {
    this.loadMonitors();
    this.startAutoRefresh();
  }

  ngOnDestroy(): void {
    this.refreshSubscription?.unsubscribe();
  }

  loadMonitors(): void {
    this.monitorService.getMonitors().subscribe({
      next: (monitors: Monitor[]) => {
        this.monitors.set(monitors);
        this.loading.set(false);

        this.loadStatistics(monitors);
      },

      error: (error: unknown) => {
        console.error('Failed to load monitors:', error);

        this.error.set('Unable to load monitors.');
        this.loading.set(false);
      },
    });
  }

  loadStatistics(monitors: Monitor[]): void {
    for (const monitor of monitors) {
      this.monitorService.getStatistics(monitor.id).subscribe({
        next: (stats: MonitorStatistics) => {
          this.statistics.update((current) => ({
            ...current,
            [monitor.id]: stats,
          }));
        },

        error: (error: unknown) => {
          console.error(`Failed to load statistics for monitor ${monitor.id}:`, error);
        },
      });
    }
  }

  startAutoRefresh(): void {
    this.refreshSubscription = interval(30000).subscribe(() => {
      console.log('Refreshing monitor statistics...');

      this.loadStatistics(this.monitors());
    });
  }

  getStatistics(monitorId: number): MonitorStatistics | null {
    return this.statistics()[monitorId] ?? null;
  }
}
