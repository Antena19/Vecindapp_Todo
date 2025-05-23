import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { MiPerfilComponent } from './mi-perfil.component';

describe('MiPerfilComponent', () => {
  let component: MiPerfilComponent;
  let fixture: ComponentFixture<MiPerfilComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      imports: [MiPerfilComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(MiPerfilComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }));

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
