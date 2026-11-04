import { ChangeDetectionStrategy, Component, OnInit, inject, input } from '@angular/core';
import { takeUntilDestroyed, toObservable, toSignal } from '@angular/core/rxjs-interop';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { ErrorStateMatcher } from '@angular/material/core';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { filter, map, startWith, switchMap } from 'rxjs';
import { FieldErrorPipe } from '../../../../../shared/pipes/field-error.pipe';
import { UserSearchService } from '../../../application/user-search.service';
import { UserReference } from '../../../domain/models/user';

/**
 * Selector de usuario con búsqueda en el servidor (el listado de usuarios está paginado).
 * Escribe el Id del usuario elegido en `control`; si se vuelve a escribir, el valor se borra
 * hasta elegir otro usuario de la lista. Con `initialUser` (edición) el campo empieza con ese usuario.
 */
@Component({
  selector: 'app-user-select',
  imports: [ReactiveFormsModule, MatFormFieldModule, MatInputModule, MatAutocompleteModule, FieldErrorPipe],
  templateUrl: './user-select.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UserSelect implements OnInit {
  readonly control = input.required<FormControl<string | null>>();
  readonly label = input('Usuario asignado');
  readonly initialUser = input<UserReference | null>(null);

  protected readonly searchControl = new FormControl<string | UserReference>('', { nonNullable: true });

  protected readonly showError = toSignal(
    toObservable(this.control).pipe(
      switchMap((control) =>
        control.events.pipe(
          startWith(null),
          map(() => control.invalid && control.touched),
        ),
      ),
    ),
    { initialValue: false },
  );

  protected readonly errorStateMatcher: ErrorStateMatcher = {
    isErrorState: () => this.showError(),
  };

  protected readonly users = toSignal(
    inject(UserSearchService).search(
      this.searchControl.valueChanges.pipe(
        startWith(''),
        map((value) => (typeof value === 'string' ? value : value.name)),
      ),
    ),
    { initialValue: [] },
  );

  constructor() {
    this.searchControl.valueChanges
      .pipe(
        filter((value) => typeof value === 'string'),
        takeUntilDestroyed(),
      )
      .subscribe(() => this.control().setValue(null));
  }

  ngOnInit(): void {
    const user = this.initialUser();
    if (user) {
      this.searchControl.setValue(user, { emitEvent: false });
    }
  }

  protected select(user: UserReference): void {
    this.control().setValue(user.id);
    this.control().markAsDirty();
  }

  protected readonly displayUser = (value: UserReference | string | null): string =>
    typeof value === 'string' ? value : (value?.name ?? '');
}
