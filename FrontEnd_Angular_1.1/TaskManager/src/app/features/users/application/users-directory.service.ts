import { Injectable, computed, inject, signal } from '@angular/core';
import { EMPTY, catchError, expand, finalize, of, reduce } from 'rxjs';
import { FIRST_PAGE, MAX_PAGE_SIZE } from '../../../shared/models/paged-result';
import { User } from '../domain/models/user';
import { UsersApiService } from '../infrastructure/users-api.service';

/**
 * Directorio completo de usuarios para el panel del equipo (como en el prototipo). A diferencia del listado
 * paginado, aquí se necesitan todos: se recorren las páginas de la API con `expand` y se juntan con `reduce`.
 */
@Injectable({ providedIn: 'root' })
export class UsersDirectoryService {
  private readonly api = inject(UsersApiService);
  private readonly _users = signal<readonly User[]>([]);
  private readonly _loading = signal(false);
  private loaded = false;

  readonly users = this._users.asReadonly();
  readonly loading = this._loading.asReadonly();
  private readonly byId = computed(() => new Map(this._users().map((user) => [user.id, user])));

  load(force = false): void {
    if ((this.loaded && !force) || this._loading()) {
      return;
    }

    const fetchPage = (page: number) =>
      this.api.getUsers({ search: null, page, pageSize: MAX_PAGE_SIZE });

    this._loading.set(true);
    fetchPage(FIRST_PAGE)
      .pipe(
        expand((result) => (result.hasNextPage ? fetchPage(result.page + 1) : EMPTY)),
        reduce((users, result) => [...users, ...result.items], [] as User[]),
        catchError(() => of(null)),
        finalize(() => this._loading.set(false)),
      )
      .subscribe((users) => {
        if (users) {
          this._users.set(users);
          this.loaded = true;
        }
      });
  }

  findById(id: string | null): User | null {
    return id ? (this.byId().get(id) ?? null) : null;
  }

  readonly photoUrlOf = (userId: string | null): string | null => this.api.photoUrl(this.findById(userId));
}
