import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { RecuperarPasswordComponent } from './recuperar-password.component';

describe('RecuperarPasswordComponent', () => {
  let component: RecuperarPasswordComponent;
  let fixture: ComponentFixture<RecuperarPasswordComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      imports: [RecuperarPasswordComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(RecuperarPasswordComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
