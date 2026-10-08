import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MonitorCard } from './monitor-card';

describe('MonitorCard', () => {
  let component: MonitorCard;
  let fixture: ComponentFixture<MonitorCard>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MonitorCard],
    }).compileComponents();

    fixture = TestBed.createComponent(MonitorCard);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
