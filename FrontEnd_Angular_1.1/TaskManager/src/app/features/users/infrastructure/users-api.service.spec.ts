import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideZonelessChangeDetection } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../../environments/environment';
import { UsersApiService } from './users-api.service';

describe('UsersApiService (foto)', () => {
  let api: UsersApiService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideZonelessChangeDetection(), provideHttpClient(), provideHttpClientTesting()],
    });
    api = TestBed.inject(UsersApiService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('usa la URL base de la API del ambiente', () => {
    expect(environment.apiBaseUrl).toMatch(/^https?:\/\/.+\/api$/);
  });

  it('incluye la versión en la URL de la foto, para no mostrar una copia anterior', () => {
    expect(api.photoUrl({ id: 'u1', photoUpdatedAt: '2026-11-04T12:00:00Z' })).toBe(
      `${environment.apiBaseUrl}/users/u1/photo?v=2026-11-04T12%3A00%3A00Z`,
    );
  });

  it('no devuelve URL si el usuario no tiene foto', () => {
    expect(api.photoUrl({ id: 'u1', photoUpdatedAt: null })).toBeNull();
    expect(api.photoUrl(null)).toBeNull();
  });

  it('sube la foto como multipart en el campo "file" con su nombre de archivo', () => {
    const file = new File([new Uint8Array([0xff, 0xd8, 0xff])], 'ana.jpg', { type: 'image/jpeg' });

    api.uploadPhoto('u1', file).subscribe();

    const request = http.expectOne(`${environment.apiBaseUrl}/users/u1/photo`);
    expect(request.request.method).toBe('PUT');
    const body = request.request.body as FormData;
    expect((body.get('file') as File).name).toBe('ana.jpg');
    request.flush({});
  });
});
