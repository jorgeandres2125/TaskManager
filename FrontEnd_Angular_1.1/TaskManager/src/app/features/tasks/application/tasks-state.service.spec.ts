import { provideZonelessChangeDetection } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Subject, of, throwError } from 'rxjs';
import { NotificationService } from '../../../core/notifications/notification.service';
import { TaskPriorityEnum } from '../domain/enums/task-priority.enum';
import { TaskStatusEnum } from '../domain/enums/task-status.enum';
import { Task } from '../domain/models/task';
import { TasksApiService } from '../infrastructure/tasks-api.service';
import { TasksStateService } from './tasks-state.service';

const task: Task = {
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

describe('TasksStateService', () => {
  let api: jasmine.SpyObj<TasksApiService>;
  let notifications: jasmine.SpyObj<NotificationService>;
  let state: TasksStateService;

  beforeEach(() => {
    api = jasmine.createSpyObj<TasksApiService>('TasksApiService', ['getTasks', 'create', 'update', 'changeStatus', 'patchAdditionalInfo']);
    notifications = jasmine.createSpyObj<NotificationService>('NotificationService', ['success', 'error']);

    TestBed.configureTestingModule({
      providers: [
        provideZonelessChangeDetection(),
        { provide: TasksApiService, useValue: api },
        { provide: NotificationService, useValue: notifications },
      ],
    });
    state = TestBed.inject(TasksStateService);
  });

  it('no consulta tareas por sí mismo (las carga el tablero)', () => {
    expect(api.getTasks).not.toHaveBeenCalled();
  });

  it('combina los cambios de filtros con los actuales', () => {
    state.updateFilters({ priority: TaskPriorityEnum.High });
    state.updateFilters({ userId: 'u1' });

    expect(state.filters()).toEqual(jasmine.objectContaining({ priority: TaskPriorityEnum.High, userId: 'u1' }));
  });

  it('edita la tarea con la API y avisa al terminar', () => {
    const request = { title: 'Nuevo título', userId: 'u2', additionalInfo: null };
    api.update.and.returnValue(of({ ...task, ...request, userName: 'Luis' }));

    state.update('t1', request).subscribe();

    expect(api.update).toHaveBeenCalledWith('t1', request);
    expect(notifications.success).toHaveBeenCalled();
  });

  it('marca la tarea mientras se guarda el estado y avisa al terminar', () => {
    const response = new Subject<Task>();
    api.changeStatus.and.returnValue(response);

    state.changeStatus(task, TaskStatusEnum.InProgress).subscribe();
    expect(state.updatingTaskId()).toBe('t1');

    response.next({ ...task, status: TaskStatusEnum.InProgress });
    response.complete();

    expect(api.changeStatus).toHaveBeenCalledWith('t1', TaskStatusEnum.InProgress);
    expect(state.updatingTaskId()).toBeNull();
    expect(notifications.success).toHaveBeenCalled();
  });

  it('libera la tarea si la API rechaza el cambio', () => {
    api.changeStatus.and.returnValue(throwError(() => new Error('409')));

    state.changeStatus(task, TaskStatusEnum.InProgress).subscribe({ error: () => undefined });

    expect(state.updatingTaskId()).toBeNull();
    expect(notifications.success).not.toHaveBeenCalled();
  });

  it('no llama a la API hasta que alguien se suscribe', () => {
    state.changeStatus(task, TaskStatusEnum.InProgress);

    expect(api.changeStatus).not.toHaveBeenCalled();
    expect(state.updatingTaskId()).toBeNull();
  });

  it('envía el cambio parcial de la información adicional, marca la tarea y avisa al terminar', () => {
    const response = new Subject<Task>();
    const request = { priority: TaskPriorityEnum.High, addTags: ['urgente'] };
    api.patchAdditionalInfo.and.returnValue(response);

    state.patchAdditionalInfo(task, request).subscribe();
    expect(state.updatingTaskId()).toBe('t1');

    response.next(task);
    response.complete();

    expect(api.patchAdditionalInfo).toHaveBeenCalledWith('t1', request);
    expect(state.updatingTaskId()).toBeNull();
    expect(notifications.success).toHaveBeenCalled();
  });
});
