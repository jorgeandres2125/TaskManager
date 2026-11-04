import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideZonelessChangeDetection } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../../environments/environment';
import { TasksApiService } from './tasks-api.service';

describe('TasksApiService (información adicional)', () => {
  let api: TasksApiService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideZonelessChangeDetection(), provideHttpClient(), provideHttpClientTesting()],
    });
    api = TestBed.inject(TasksApiService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('envía solo los cambios con PATCH a /additional-info', () => {
    const request = { setMetadata: { ticket: 'DB-42' }, removeMetadata: ['sprint'] };

    api.patchAdditionalInfo('t1', request).subscribe();

    const call = http.expectOne(`${environment.apiBaseUrl}/tasks/t1/additional-info`);
    expect(call.request.method).toBe('PATCH');
    expect(call.request.body).toEqual(request);
    call.flush({});
  });
});
