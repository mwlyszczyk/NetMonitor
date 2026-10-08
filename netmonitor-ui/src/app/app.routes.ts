import { Routes } from '@angular/router';

import { Dashboard } from './pages/dashboard/dashboard';
import { MonitorDetail } from './pages/monitor-detail/monitor-detail';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'dashboard',
    pathMatch: 'full'
  },
  {
    path: 'dashboard',
    component: Dashboard
  },
  {
    path: 'monitors/:id',
    component: MonitorDetail
  }
];
