import { Injectable, provideZonelessChangeDetection } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Observable, Subject, of, throwError } from 'rxjs';
import { ApiError } from '../../core/errors/api-error';
import { PageQuery, PagedResult } from '../models/paged-result';
import { PagedListState } from './paged-list.state';

interface TestQuery extends PageQuery {
  readonly search: string | null;
}

/** Estado de prueba: cada petición la responde la función `respond`, que el test puede cambiar. */
@Injectable()
class TestListState extends PagedListState<string, TestQuery> {
  readonly requests: TestQuery[] = [];
  respond: (query: TestQuery) => Observable<PagedResult<string>> = () => of(page(['a', 'b']));

  constructor() {
    super({ page: 1, pageSize: 20, search: null });
  }

  protected override fetch(query: TestQuery): Observable<PagedResult<string>> {
    this.requests.push(query);
    return this.respond(query);
  }
}

function page(items: string[], totalCount = items.length): PagedResult<string> {
  return { items, page: 1, pageSize: 20, totalCount, totalPages: 1, hasPreviousPage: false, hasNextPage: false };
}

describe('PagedListState', () => {
  let state: TestListState;

  function flush(): void {
    TestBed.tick();
    jasmine.clock().tick(1);
  }

  beforeEach(() => {
    jasmine.clock().install();
    TestBed.configureTestingModule({ providers: [provideZonelessChangeDetection(), TestListState] });
    state = TestBed.inject(TestListState);
  });

  afterEach(() => jasmine.clock().uninstall());

  it('carga la primera página al crearse', () => {
    expect(state.loaded()).toBeFalse();

    flush();

    expect(state.items()).toEqual(['a', 'b']);
    expect(state.totalCount()).toBe(2);
    expect(state.loaded()).toBeTrue();
    expect(state.loading()).toBeFalse();
  });

  it('agrupa cambios seguidos en una sola petición', () => {
    state.refresh();
    state.updateFilters({ search: 'ana' });

    flush();

    expect(state.requests.length).toBe(1);
    expect(state.requests[0].search).toBe('ana');
  });

  it('al cambiar filtros vuelve a la primera página', () => {
    flush();
    state.changePage(3, 20);
    flush();

    state.updateFilters({ search: 'ana' });
    flush();

    expect(state.query().page).toBe(1);
  });

  it('cancela la petición anterior: una respuesta tardía no sobrescribe la más reciente', () => {
    const slow = new Subject<PagedResult<string>>();
    state.respond = () => slow;
    flush();

    state.respond = () => of(page(['reciente']));
    state.updateFilters({ search: 'nuevo' });
    flush();
    slow.next(page(['antigua']));

    expect(state.items()).toEqual(['reciente']);
    expect(slow.observed).toBeFalse();
  });

  it('ante un error conserva los últimos datos y sigue respondiendo a nuevos filtros', () => {
    flush();
    const apiError: ApiError = { status: 500, title: 'Error', detail: null, fieldErrors: {}, traceId: null };
    state.respond = () => throwError(() => apiError);

    state.refresh();
    flush();

    expect(state.error()).toBe(apiError);
    expect(state.items()).toEqual(['a', 'b']);

    state.respond = () => of(page(['otra']));
    state.updateFilters({ search: 'x' });
    flush();

    expect(state.error()).toBeNull();
    expect(state.items()).toEqual(['otra']);
  });

  it('distingue una lista vacía de una que aún no cargó', () => {
    state.respond = () => of(page([]));
    expect(state.isEmpty()).toBeFalse();

    flush();

    expect(state.isEmpty()).toBeTrue();
  });
});
