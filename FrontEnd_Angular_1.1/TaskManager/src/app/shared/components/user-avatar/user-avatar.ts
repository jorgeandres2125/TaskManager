import { ChangeDetectionStrategy, Component, computed, effect, input, signal } from '@angular/core';

export const DEFAULT_AVATAR_URL = 'default-avatar.svg';

const SIZE_CLASSES = {
  sm: 'h-7 w-7',
  md: 'h-9 w-9',
  lg: 'h-12 w-12',
  xl: 'h-20 w-20',
} as const;

/** Avatar de una persona: su foto si tiene (`photoUrl`) y, si no, o si la foto no carga, una imagen por defecto. */
@Component({
  selector: 'app-user-avatar',
  template: `
    <img
      class="inline-block shrink-0 rounded-full bg-elevated object-cover ring-1 ring-white/10"
      [class]="sizeClasses()"
      [src]="src()"
      [attr.title]="name()"
      alt=""
      (error)="photoFailed.set(true)"
    />
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'inline-flex shrink-0' },
})
export class UserAvatar {
  readonly name = input.required<string>();
  readonly size = input<keyof typeof SIZE_CLASSES>('sm');
  readonly photoUrl = input<string | null>(null);

  protected readonly photoFailed = signal(false);

  protected readonly src = computed(() => (this.photoUrl() && !this.photoFailed() ? this.photoUrl() : DEFAULT_AVATAR_URL));
  protected readonly sizeClasses = computed(() => SIZE_CLASSES[this.size()]);

  constructor() {
    effect(() => {
      this.photoUrl();
      this.photoFailed.set(false);
    });
  }
}
