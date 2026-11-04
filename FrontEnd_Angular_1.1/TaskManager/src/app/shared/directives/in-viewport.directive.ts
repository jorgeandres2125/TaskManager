import { DestroyRef, Directive, ElementRef, inject, output } from '@angular/core';

@Directive({ selector: '[appInViewport]' })
export class InViewport {
  readonly appInViewport = output<void>();

  constructor() {
    const observer = new IntersectionObserver(
      (entries) => {
        if (entries.some((entry) => entry.isIntersecting)) {
          this.appInViewport.emit();
        }
      },
      { rootMargin: '300px' },
    );
    observer.observe(inject(ElementRef<HTMLElement>).nativeElement);
    inject(DestroyRef).onDestroy(() => observer.disconnect());
  }
}
