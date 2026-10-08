export interface MonitorStatistics {
  uptimePercentage: number;
  averageResponseTimeMs: number;
  successfulChecks: number;
  failedChecks: number;
  totalChecks: number;
  lastStatus: string;
}
