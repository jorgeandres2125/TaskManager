import { Signal, WritableSignal, computed, signal } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { EMPTY, Observable, catchError, debounceTime, finalize, switchMap } from 'rxjs';
import { ApiError } from '../../core/errors/api-error';
import { FIRST_PAGE, PageQuery, PagedResult } from '../models/paged-result';

export abstract class PagedListState<TItem, TQuery extends PageQuery> {
  private readonly _query: WritableSignal<TQuery>;
  private readonly _refreshCount = signal(0);
  private readonly _loading = signal(false);
  private readonly _error = signal<ApiError | null>(null);
  private readonly result: Signal<PagedResult<TItem> | null>;

  readonly query: Signal<TQuery>;
  readonly loading = this._loading.asReadonly();
  readonly error = this._error.asReadonly();
  readonly items = computed(() => this.result()?.items ?? []);
  readonly totalCount = computed(() => this.result()?.totalCount ?? 0);
  readonly loaded = computed(() => this.result() !== null);
  readonly isEmpty = computed(() => this.result() !== null && this.items().length === 0);

  protected constructor(initialQuery: TQuery) {
    this._query = signal(initialQuery);
    this.query = this._query.asReadonly();

    const request = computed(() => ({ query: this._query(), refresh: this._refreshCount() }));

    this.result = toSignal(
      toObservable(request).pipe(
        debounceTime(0),
        switchMap(({ query }) => {
          this._loading.set(true);
          this._error.set(null);
          return this.fetch(query).pipe(
            catchError((error: ApiError) => {
              this._error.set(error);
              return EMPTY;
            }),
            finalize(() => this._loading.set(false)),
          );
        }),
      ),
      { initialValue: null },
    );
  }

  updateFilters(changes: Partial<Omit<TQuery, keyof PageQuery>>): void {
    this._query.update((query) => ({ ...query, ...changes, page: FIRST_PAGE }) as TQuery);
  }

  changePage(page: number, pageSize: number): void {
    this._query.update((query) => ({ ...query, page, pageSize }) as TQuery);
  }

  refresh(): void {
    this._refreshCount.update((count) => count + 1);
  }

  protected abstract fetch(query: TQuery): Observable<PagedResult<TItem>>;
}
