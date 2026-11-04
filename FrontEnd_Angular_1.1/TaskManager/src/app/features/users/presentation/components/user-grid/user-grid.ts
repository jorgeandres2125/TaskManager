import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { UserAvatar } from '../../../../../shared/components/user-avatar/user-avatar';
import { User } from '../../../domain/models/user';
import { UserPhotoUrlPipe } from '../../pipes/user-photo-url.pipe';

/**
 * Usuarios como tarjetas (solo presentación): 1 columna en móvil, 2 en tableta y 4 en escritorio.
 * Toda la tarjeta es el botón de editar, así el área táctil es grande.
 */
@Component({
  selector: 'app-user-grid',
  imports: [DatePipe, MatIconModule, MatTooltipModule, UserAvatar, UserPhotoUrlPipe],
  template: `
    <ul class="grid grid-cols-1 gap-3 md:grid-cols-2 xl:grid-cols-4" aria-label="Usuarios">
      @for (user of users(); track user.id) {
        <li>
          <button
            type="button"
            class="group/user flex h-full w-full flex-col gap-3 rounded-lg border border-line bg-elevated p-4 text-left shadow-low transition-[transform,box-shadow,border-color] duration-200 ease-snappy hover:-translate-y-0.5 hover:border-line-strong hover:shadow-medium focus-visible:border-accent-text/60 active:translate-y-0 motion-reduce:hover:translate-y-0"
            [attr.aria-label]="'Editar a ' + user.name"
            (click)="edit.emit(user)"
          >
            <span class="flex items-start justify-between">
              <app-user-avatar [name]="user.name" size="lg" [photoUrl]="user | userPhotoUrl" />
              <span
                class="-mr-1 -mt-1 flex h-8 w-8 items-center justify-center rounded-md text-fg-subtle opacity-0 transition-opacity duration-150 group-hover/user:opacity-100 group-focus-visible/user:opacity-100 max-lg:opacity-100"
                aria-hidden="true"
              >
                <mat-icon class="!h-[18px] !w-[18px] !text-lg">edit</mat-icon>
              </span>
            </span>
            <span class="flex min-w-0 flex-col gap-1">
              <span class="truncate text-[0.9375rem] font-medium text-fg">{{ user.name }}</span>
              <span class="flex min-w-0 items-center gap-1.5 text-[0.8125rem] text-fg-muted">
                <mat-icon class="!h-4 !w-4 shrink-0 !text-base" aria-hidden="true">mail</mat-icon>
                <span class="truncate" [title]="user.email">{{ user.email }}</span>
              </span>
            </span>
            <span class="mt-auto flex items-center gap-1.5 border-t border-line pt-3 text-xs text-fg-subtle">
              <mat-icon class="!h-3.5 !w-3.5 shrink-0 !text-sm" aria-hidden="true">calendar_today</mat-icon>
              Desde {{ user.createdAt | date: 'd MMM y' }}
            </span>
          </button>
        </li>
      }
    </ul>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UserGrid {
  readonly users = input.required<readonly User[]>();
  readonly edit = output<User>();
}
