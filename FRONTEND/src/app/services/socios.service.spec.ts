import { TestBed } from '@angular/core/testing';

import { SociosService } from './socios.service';

describe('SociosService', () => {
  let service: SociosService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(SociosService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
