import { DestroyRef, computed, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { EMPTY, Observable, Subject, catchError, finalize, map, switchMap } from 'rxjs';
import { FIRST_PAGE, PagedResult } from '../../../shared/models/paged-result';
import { TaskStatusEnum } from '../domain/enums/task-status.enum';
import { Task, TaskFilters, TaskQuery } from '../domain/models/task';

export const BOARD_PAGE_SIZE = 20;

interface ColumnRequest {
  readonly filters: TaskFilters;
  readonly page: number;
}

export class BoardColumnState {
  private readonly _items = signal<readonly Task[]>([]);
  private readonly _totalCount = signal(0);
  private readonly _loading = signal(false);
  private readonly _loaded = signal(false);
  private readonly requests = new Subject<ColumnRequest>();
  private filters: TaskFilters | null = null;
  private currentPage = 0;

  readonly items = this._items.asReadonly();
  readonly totalCount = this._totalCount.asReadonly();
  readonly loading = this._loading.asReadonly();
  readonly loaded = this._loaded.asReadonly();
  readonly hasMore = computed(() => this._items().length < this._totalCount());

  constructor(
    readonly status: TaskStatusEnum,
    fetch: (query: TaskQuery) => Observable<PagedResult<Task>>,
    destroyRef: DestroyRef,
  ) {
    this.requests
      .pipe(
        switchMap(({ filters, page }) => {
          this._loading.set(true);
          return fetch({ ...filters, status, page, pageSize: BOARD_PAGE_SIZE }).pipe(
            map((result) => ({ result, page })),
            catchError(() => EMPTY),
            finalize(() => this._loading.set(false)),
          );
        }),
        takeUntilDestroyed(destroyRef),
      )
      .subscribe(({ result, page }) => {
        this.currentPage = page;
        this._totalCount.set(result.totalCount);
        this._items.update((items) => (page === FIRST_PAGE ? result.items : appendNew(items, result.items)));
        this._loaded.set(true);
      });
  }

  reload(filters?: TaskFilters): void {
    this.filters = filters ?? this.filters;
    if (this.filters) {
      this.requests.next({ filters: this.filters, page: FIRST_PAGE });
    }
  }

  loadMore(): void {
    if (this.filters && !this._loading() && this.hasMore()) {
      this.requests.next({ filters: this.filters, page: this.currentPage + 1 });
    }
  }

  remove(taskId: string): void {
    this._items.update((items) => items.filter((task) => task.id !== taskId));
    this._totalCount.update((count) => Math.max(0, count - 1));
  }

  add(task: Task): void {
    this._items.update((items) => [task, ...items]);
    this._totalCount.update((count) => count + 1);
  }
}

function appendNew(current: readonly Task[], next: readonly Task[]): readonly Task[] {
  const ids = new Set(current.map((task) => task.id));
  return [...current, ...next.filter((task) => !ids.has(task.id))];
}
