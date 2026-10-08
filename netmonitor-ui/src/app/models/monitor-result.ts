export interface MonitorResult {
  id: number;
  monitorTargetId: number;
  isSuccess: boolean;
  responseTimeMs: number;
  errorMessage: string | null;
  checkedAt: string;
}
