import { ChangeDetectionStrategy, Component, OnInit, computed, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { debounceTime, distinctUntilChanged, map } from 'rxjs';
import { PageHeader, PageStat } from '../../../../../shared/components/page-header/page-header';
import { PagedListPanel } from '../../../../../shared/components/paged-list-panel/paged-list-panel';
import { SearchField } from '../../../../../shared/components/search-field/search-field';
import { UsersStateService } from '../../../application/users-state.service';
import { User } from '../../../domain/models/user';
import { openUserFormDialog } from '../../components/user-form-dialog/user-form-dialog';
import { UserGrid } from '../../components/user-grid/user-grid';

const SEARCH_DEBOUNCE_MS = 400;

/** Usuarios como tarjetas, con búsqueda y paginación. Crear y editar usuarios se hace en una ventana modal. */
@Component({
  selector: 'app-user-list-page',
  imports: [ReactiveFormsModule, MatIconModule, PageHeader, PagedListPanel, SearchField, UserGrid],
  templateUrl: './user-list-page.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UserListPage implements OnInit {
  protected readonly state = inject(UsersStateService);
  private readonly dialog = inject(MatDialog);
  protected readonly searchControl = new FormControl(this.state.query().search ?? '', { nonNullable: true });

  protected readonly subtitle = computed(() => {
    const search = this.state.query().search;
    return search ? `Resultados para «${search}»` : 'Personas a las que se pueden asignar tareas';
  });

  protected readonly stats = computed<readonly PageStat[]>(() =>
    this.state.loaded()
      ? [{ value: this.state.totalCount(), label: this.state.query().search ? 'coincidencias' : 'usuarios' }]
      : [{ value: '…', label: 'cargando' }],
  );

  constructor() {
    this.searchControl.valueChanges
      .pipe(
        debounceTime(SEARCH_DEBOUNCE_MS),
        map((search) => search.trim()),
        distinctUntilChanged(),
        takeUntilDestroyed(),
      )
      .subscribe((search) => this.state.updateFilters({ search: search || null }));
  }

  ngOnInit(): void {
    this.state.refresh();
  }

  protected openForm(user: User | null = null): void {
    openUserFormDialog(this.dialog, user).subscribe();
  }
}
