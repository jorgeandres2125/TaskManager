import { DestroyRef, provideZonelessChangeDetection } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Observable, of } from 'rxjs';
import { SortDirectionEnum } from '../../../shared/enums/sort-direction.enum';
import { PagedResult } from '../../../shared/models/paged-result';
import { TaskStatusEnum } from '../domain/enums/task-status.enum';
import { Task, TaskFilters, TaskQuery } from '../domain/models/task';
import { BoardColumnState } from './board-column.state';

function task(id: string): Task {
  return {
    id,
    number: 1,
    title: id,
    status: TaskStatusEnum.Pending,
    allowedTransitions: [TaskStatusEnum.InProgress],
    userId: 'u1',
    userName: 'Ana',
    createdAt: '2026-11-03T12:00:00Z',
    updatedAt: null,
    additionalInfo: null,
  };
}

function page(items: Task[], totalCount: number): PagedResult<Task> {
  return { items, page: 1, pageSize: 20, totalCount, totalPages: 1, hasPreviousPage: false, hasNextPage: false };
}

const filters: TaskFilters = {
  status: null,
  priority: null,
  tag: null,
  userId: null,
  sortDirection: SortDirectionEnum.Desc,
};

describe('BoardColumnState', () => {
  let queries: TaskQuery[];
  let respond: (query: TaskQuery) => Observable<PagedResult<Task>>;
  let column: BoardColumnState;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideZonelessChangeDetection()] });
    queries = [];
    respond = (query) => (query.page === 1 ? of(page([task('a'), task('b')], 3)) : of(page([task('b'), task('c')], 3)));
    column = new BoardColumnState(
      TaskStatusEnum.Pending,
      (query) => {
        queries.push(query);
        return respond(query);
      },
      TestBed.inject(DestroyRef),
    );
  });

  it('consulta su propio estado con los filtros del tablero', () => {
    column.reload(filters);

    expect(queries[0].status).toBe(TaskStatusEnum.Pending);
    expect(queries[0].page).toBe(1);
    expect(column.items().map((t) => t.id)).toEqual(['a', 'b']);
    expect(column.totalCount()).toBe(3);
    expect(column.hasMore()).toBeTrue();
  });

  it('"Cargar más" añade la página siguiente sin repetir tareas', () => {
    column.reload(filters);

    column.loadMore();

    expect(queries[1].page).toBe(2);
    expect(column.items().map((t) => t.id)).toEqual(['a', 'b', 'c']);
    expect(column.hasMore()).toBeFalse();
  });

  it('al recargar vuelve a la primera página', () => {
    column.reload(filters);
    column.loadMore();

    column.reload();

    expect(queries[2].page).toBe(1);
    expect(column.items().map((t) => t.id)).toEqual(['a', 'b']);
  });

  it('quita y añade tareas de forma optimista actualizando el contador', () => {
    column.reload(filters);

    column.remove('a');
    expect(column.items().map((t) => t.id)).toEqual(['b']);
    expect(column.totalCount()).toBe(2);

    column.add(task('z'));
    expect(column.items()[0].id).toBe('z');
    expect(column.totalCount()).toBe(3);
  });
});
