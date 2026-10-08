import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MonitorDetail } from './monitor-detail';

describe('MonitorDetail', () => {
  let component: MonitorDetail;
  let fixture: ComponentFixture<MonitorDetail>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MonitorDetail],
    }).compileComponents();

    fixture = TestBed.createComponent(MonitorDetail);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
