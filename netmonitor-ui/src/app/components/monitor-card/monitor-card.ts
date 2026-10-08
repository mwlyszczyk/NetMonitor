import { Component, Input } from '@angular/core';
import { RouterLink } from '@angular/router';

import { Monitor } from '../../models/monitor';
import { MonitorStatistics } from '../../models/monitor-statistics';

@Component({
  selector: 'app-monitor-card',
  imports: [RouterLink],
  templateUrl: './monitor-card.html',
  styleUrl: './monitor-card.css'
})
export class MonitorCard {
  @Input({ required: true }) monitor!: Monitor;
  @Input() statistics: MonitorStatistics | null = null;
}
