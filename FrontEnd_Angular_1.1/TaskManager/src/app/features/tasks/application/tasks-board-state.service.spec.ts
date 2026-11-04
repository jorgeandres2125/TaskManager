import { provideZonelessChangeDetection } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Subject, of, throwError } from 'rxjs';
import { NotificationService } from '../../../core/notifications/notification.service';
import { SortDirectionEnum } from '../../../shared/enums/sort-direction.enum';
import { PagedResult } from '../../../shared/models/paged-result';
import { TaskStatusEnum } from '../domain/enums/task-status.enum';
import { Task, TaskQuery } from '../domain/models/task';
import { TasksApiService } from '../infrastructure/tasks-api.service';
import { TasksBoardStateService } from './tasks-board-state.service';

const pendingTask: Task = {
  id: 't1',
  number: 24,
  title: 'Revisar contrato',
  status: TaskStatusEnum.Pending,
  allowedTransitions: [TaskStatusEnum.InProgress],
  userId: 'u1',
  userName: 'Ana',
  createdAt: '2026-11-03T12:00:00Z',
  updatedAt: null,
  additionalInfo: null,
};

function pageFor(query: TaskQuery): PagedResult<Task> {
  const items = query.status === TaskStatusEnum.Pending ? [pendingTask] : [];
  return {
    items,
    page: 1,
    pageSize: 20,
    totalCount: items.length,
    totalPages: 1,
    hasPreviousPage: false,
    hasNextPage: false,
  };
}

describe('TasksBoardStateService', () => {
  let api: jasmine.SpyObj<TasksApiService>;
  let board: TasksBoardStateService;

  const column = (status: TaskStatusEnum) => board.columns.find((c) => c.status === status)!;
  const statusesRequested = () => api.getTasks.calls.allArgs().map(([query]) => query.status);

  beforeEach(() => {
    jasmine.clock().install();
    api = jasmine.createSpyObj<TasksApiService>('TasksApiService', ['getTasks', 'create', 'changeStatus']);
    api.getTasks.and.callFake((query) => of(pageFor(query)));

    TestBed.configureTestingModule({
      providers: [
        provideZonelessChangeDetection(),
        { provide: TasksApiService, useValue: api },
        { provide: NotificationService, useValue: jasmine.createSpyObj('NotificationService', ['success', 'error']) },
      ],
    });
    board = TestBed.inject(TasksBoardStateService);
    board.load({ status: null, priority: null, tag: null, userId: null, sortDirection: SortDirectionEnum.Desc });
  });

  afterEach(() => jasmine.clock().uninstall());

  it('carga una columna por cada estado del enunciado', () => {
    expect(board.columns.map((c) => c.status)).toEqual([
      TaskStatusEnum.Pending,
      TaskStatusEnum.InProgress,
      TaskStatusEnum.Done,
    ]);
    expect(column(TaskStatusEnum.Pending).items()).toEqual([pendingTask]);
  });

  it('mueve la tarjeta al instante y recarga ambas columnas al guardar', () => {
    const response = new Subject<Task>();
    api.changeStatus.and.returnValue(response);
    api.getTasks.calls.reset();

    board.move(pendingTask, TaskStatusEnum.InProgress).subscribe();

    expect(column(TaskStatusEnum.Pending).items()).toEqual([]);
    expect(column(TaskStatusEnum.InProgress).items()[0].id).toBe('t1');
    expect(board.updatingTaskId()).toBe('t1');

    response.next({ ...pendingTask, status: TaskStatusEnum.InProgress });
    response.complete();

    expect(board.updatingTaskId()).toBeNull();
    expect(statusesRequested()).toEqual(jasmine.arrayContaining([TaskStatusEnum.Pending, TaskStatusEnum.InProgress]));
  });

  it('si la API rechaza el cambio, las columnas se recargan y la tarjeta vuelve a su sitio', () => {
    api.changeStatus.and.returnValue(throwError(() => new Error('409')));

    board.move(pendingTask, TaskStatusEnum.InProgress).subscribe({ error: () => undefined });

    expect(column(TaskStatusEnum.Pending).items()).toEqual([pendingTask]);
    expect(column(TaskStatusEnum.InProgress).items()).toEqual([]);
  });

  it('con el filtro de estado solo consulta la columna de ese estado', () => {
    api.getTasks.calls.reset();

    board.load({ status: TaskStatusEnum.InProgress, priority: null, tag: null, userId: null, sortDirection: SortDirectionEnum.Desc });

    expect(statusesRequested()).toEqual([TaskStatusEnum.InProgress]);
  });
});
