import { Injectable, inject } from '@angular/core';
import { Observable, of, tap } from 'rxjs';
import { NotificationService } from '../../../core/notifications/notification.service';
import { DEFAULT_PAGE_SIZE, FIRST_PAGE, PagedResult } from '../../../shared/models/paged-result';
import { PagedListState } from '../../../shared/state/paged-list.state';
import { CreateUserRequest, UpdateUserRequest, User, UserPhotoChange, UserQuery } from '../domain/models/user';
import { UsersApiService } from '../infrastructure/users-api.service';

/** Estado del listado de usuarios (búsqueda y paginación) y casos de uso de creación y edición. */
@Injectable({ providedIn: 'root' })
export class UsersStateService extends PagedListState<User, UserQuery> {
  private readonly api = inject(UsersApiService);
  private readonly notifications = inject(NotificationService);

  constructor() {
    super({ page: FIRST_PAGE, pageSize: DEFAULT_PAGE_SIZE, search: null });
  }

  create(request: CreateUserRequest): Observable<User> {
    return this.api.create(request).pipe(
      tap((user) => {
        this.notifications.success(`Usuario "${user.name}" creado.`);
        this.refresh();
      }),
    );
  }

  update(userId: string, request: UpdateUserRequest): Observable<User> {
    return this.api.update(userId, request).pipe(
      tap((user) => {
        this.notifications.success(`Usuario "${user.name}" actualizado.`);
        this.refresh();
      }),
    );
  }

  changePhoto(user: User, change: UserPhotoChange): Observable<User> {
    if (change.kind === 'keep') {
      return of(user);
    }

    const request = change.kind === 'replace' ? this.api.uploadPhoto(user.id, change.file) : this.api.deletePhoto(user.id);
    return request.pipe(tap(() => this.refresh()));
  }

  protected override fetch(query: UserQuery): Observable<PagedResult<User>> {
    return this.api.getUsers(query);
  }
}
