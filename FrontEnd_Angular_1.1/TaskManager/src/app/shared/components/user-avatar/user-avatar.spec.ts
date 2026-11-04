import { Component, provideZonelessChangeDetection, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { DEFAULT_AVATAR_URL, UserAvatar } from './user-avatar';

@Component({
  imports: [UserAvatar],
  template: `<app-user-avatar name="Ana" [photoUrl]="photoUrl()" />`,
})
class Host {
  readonly photoUrl = signal<string | null>(null);
}

describe('UserAvatar', () => {
  function setup() {
    TestBed.configureTestingModule({ providers: [provideZonelessChangeDetection()] });
    const fixture = TestBed.createComponent(Host);
    const image = () => fixture.nativeElement.querySelector('img') as HTMLImageElement;
    return { fixture, host: fixture.componentInstance, image };
  }

  it('muestra la imagen por defecto cuando el usuario no tiene foto', async () => {
    const { fixture, image } = setup();
    await fixture.whenStable();

    expect(image().getAttribute('src')).toBe(DEFAULT_AVATAR_URL);
  });

  it('muestra la foto del usuario cuando la tiene', async () => {
    const { fixture, host, image } = setup();
    host.photoUrl.set('/api/users/u1/photo?v=1');
    await fixture.whenStable();

    expect(image().getAttribute('src')).toBe('/api/users/u1/photo?v=1');
  });

  it('vuelve a la imagen por defecto si la foto no carga', async () => {
    const { fixture, host, image } = setup();
    host.photoUrl.set('/api/users/u1/photo?v=1');
    await fixture.whenStable();

    image().dispatchEvent(new Event('error'));
    await fixture.whenStable();

    expect(image().getAttribute('src')).toBe(DEFAULT_AVATAR_URL);
  });

  it('reintenta con una foto nueva después de un fallo', async () => {
    const { fixture, host, image } = setup();
    host.photoUrl.set('/api/users/u1/photo?v=1');
    await fixture.whenStable();
    image().dispatchEvent(new Event('error'));
    await fixture.whenStable();

    host.photoUrl.set('/api/users/u1/photo?v=2');
    await fixture.whenStable();

    expect(image().getAttribute('src')).toBe('/api/users/u1/photo?v=2');
  });
});
