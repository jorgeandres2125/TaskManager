import { ScrollingModule } from '@angular/cdk/scrolling';
import { NgTemplateOutlet } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, effect, inject, input, output, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ViewportService } from '../../../../../core/layout/viewport.service';
import { SearchField } from '../../../../../shared/components/search-field/search-field';
import { UserAvatar } from '../../../../../shared/components/user-avatar/user-avatar';
import { User } from '../../../domain/models/user';
import { UserPhotoUrlPipe } from '../../pipes/user-photo-url.pipe';

const COLLAPSED_STORAGE_KEY = 'taskmanager.user-rail.collapsed';

export const RAIL_ROW_HEIGHT = 44;
export const RAIL_STRIP_ITEM_WIDTH = 48;

/**
 * Panel del equipo (inspirado en Slack y Linear): elegir un usuario filtra las tareas por ese responsable;
 * volver a pulsarlo, o pulsar "Todos", quita el filtro.
 *
 * - Lista virtualizada (CDK): solo se pintan las filas visibles, aunque haya cientos de usuarios.
 * - Modo plegado: solo avatares (el nombre sale en el tooltip), para dar más ancho al tablero.
 * - Filtro rápido por nombre, en el navegador (el panel ya tiene todos los usuarios cargados).
 * - En pantallas compactas es una franja horizontal de avatares, también virtualizada.
 */
@Component({
  selector: 'app-user-rail',
  imports: [NgTemplateOutlet, ReactiveFormsModule, ScrollingModule, MatIconModule, MatTooltipModule, SearchField, UserAvatar, UserPhotoUrlPipe],
  templateUrl: './user-rail.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'block' },
})
export class UserRail {
  protected readonly viewport = inject(ViewportService);

  readonly users = input.required<readonly User[]>();
  readonly selectedUserId = input<string | null>(null);
  readonly selectedUserIdChange = output<string | null>();

  protected readonly rowHeight = RAIL_ROW_HEIGHT;
  protected readonly stripItemWidth = RAIL_STRIP_ITEM_WIDTH;

  protected readonly collapsed = signal(readCollapsed());
  protected readonly isCollapsed = computed(() => this.collapsed() && !this.viewport.isCompact());

  protected readonly searchControl = new FormControl('', { nonNullable: true });
  private readonly search = toSignal(this.searchControl.valueChanges, { initialValue: '' });

  protected readonly visibleUsers = computed(() => {
    const term = normalize(this.search());
    return term ? this.users().filter((user) => normalize(user.name).includes(term)) : this.users();
  });

  constructor() {
    effect(() => writeCollapsed(this.collapsed()));
  }

  protected toggleCollapsed(): void {
    this.collapsed.update((collapsed) => !collapsed);
  }

  protected toggle(userId: string): void {
    this.selectedUserIdChange.emit(this.selectedUserId() === userId ? null : userId);
  }

  protected readonly trackById = (_: number, user: User): string => user.id;
}

function normalize(text: string): string {
  return text.normalize('NFD').replace(/\p{Diacritic}/gu, '').toLowerCase().trim();
}

function readCollapsed(): boolean {
  try {
    return localStorage.getItem(COLLAPSED_STORAGE_KEY) === 'true';
  } catch {
    return false;
  }
}

function writeCollapsed(collapsed: boolean): void {
  try {
    localStorage.setItem(COLLAPSED_STORAGE_KEY, String(collapsed));
  } catch {
  }
}
