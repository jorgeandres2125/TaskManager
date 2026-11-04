import { Injectable, inject } from '@angular/core';
import { Observable, catchError, debounceTime, distinctUntilChanged, map, of, switchMap } from 'rxjs';
import { FIRST_PAGE } from '../../../shared/models/paged-result';
import { User } from '../domain/models/user';
import { UsersApiService } from '../infrastructure/users-api.service';

const SEARCH_DEBOUNCE_MS = 300;
const SEARCH_PAGE_SIZE = 20;

/** Búsqueda de usuarios mientras se escribe (selector de usuario). */
@Injectable({ providedIn: 'root' })
export class UserSearchService {
  private readonly api = inject(UsersApiService);

  search(terms: Observable<string>): Observable<readonly User[]> {
    return terms.pipe(
      debounceTime(SEARCH_DEBOUNCE_MS),
      map((term) => term.trim()),
      distinctUntilChanged(),
      switchMap((term) =>
        this.api.getUsers({ search: term || null, page: FIRST_PAGE, pageSize: SEARCH_PAGE_SIZE }).pipe(
          map((result) => result.items),
          catchError(() => of([])),
        ),
      ),
    );
  }
}
