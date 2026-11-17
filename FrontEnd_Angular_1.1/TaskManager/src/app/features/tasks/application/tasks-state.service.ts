import { Injectable, inject, signal } from '@angular/core';
import { Observable, defer, finalize, tap } from 'rxjs';
import { NotificationService } from '../../../core/notifications/notification.service';
import { SortDirectionEnum } from '../../../shared/enums/sort-direction.enum';
import { TaskStatusEnum } from '../domain/enums/task-status.enum';
import { CreateTaskRequest, PatchTaskAdditionalInfoRequest, Task, TaskFilters, UpdateTaskRequest } from '../domain/models/task';
import { TasksApiService } from '../infrastructure/tasks-api.service';

const INITIAL_FILTERS: TaskFilters = {
  status: null,
  priority: null,
  tag: null,
  userId: null,
  sortDirection: SortDirectionEnum.Desc,
};

/**
 * Filtros de las tareas (se conservan al volver a la pantalla) y casos de uso de creación, edición, cambio de estado
 * y cambios parciales de la información adicional (PATCH, que la API aplica con JSON_MODIFY).
 * Las tareas se muestran en el tablero Kanban, que carga cada columna con estos filtros.
 */
@Injectable({ providedIn: 'root' })
export class TasksStateService {
  private readonly api = inject(TasksApiService);
  private readonly notifications = inject(NotificationService);
  private readonly _filters = signal<TaskFilters>(INITIAL_FILTERS);
  private readonly _updatingTaskId = signal<string | null>(null);

  readonly filters = this._filters.asReadonly();
  readonly updatingTaskId = this._updatingTaskId.asReadonly();

  updateFilters(changes: Partial<TaskFilters>): void {
    this._filters.update((filters) => ({ ...filters, ...changes }));
  }

  create(request: CreateTaskRequest): Observable<Task> {
    return this.api
      .create(request)
      .pipe(tap((task) => this.notifications.success(`Tarea #${task.number} "${task.title}" creada.`)));
  }

  update(taskId: string, request: UpdateTaskRequest): Observable<Task> {
    return this.api
      .update(taskId, request)
      .pipe(tap((task) => this.notifications.success(`Tarea #${task.number} "${task.title}" actualizada.`)));
  }

  patchAdditionalInfo(task: Task, request: PatchTaskAdditionalInfoRequest): Observable<Task> {
    return defer(() => {
      this._updatingTaskId.set(task.id);
      return this.api.patchAdditionalInfo(task.id, request);
    }).pipe(
      tap((updated) => this.notifications.success(`Información adicional de la tarea #${updated.number} actualizada.`)),
      finalize(() => this._updatingTaskId.set(null)),
    );
  }

  changeStatus(task: Task, status: TaskStatusEnum): Observable<Task> {
    return defer(() => {
      this._updatingTaskId.set(task.id);
      return this.api.changeStatus(task.id, status);
    }).pipe(
      tap((updated) => this.notifications.success(`Estado de la tarea #${updated.number} actualizado.`)),
      finalize(() => this._updatingTaskId.set(null)),
    );
  }
}
