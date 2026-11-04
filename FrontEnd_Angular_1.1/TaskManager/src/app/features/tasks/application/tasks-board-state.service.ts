import { DestroyRef, Injectable, inject } from '@angular/core';
import { Observable, defer, finalize, tap } from 'rxjs';
import { TaskStatusEnum } from '../domain/enums/task-status.enum';
import { PatchTaskAdditionalInfoRequest, Task, TaskFilters } from '../domain/models/task';
import { TasksApiService } from '../infrastructure/tasks-api.service';
import { BoardColumnState } from './board-column.state';
import { TasksStateService } from './tasks-state.service';

/**
 * Estado del tablero Kanban: una columna por cada estado del enunciado (Pending, InProgress, Done).
 * Los contadores salen del `totalCount` de la consulta paginada de cada columna; no hace falta un endpoint nuevo.
 * Con el filtro de estado solo se consulta la columna elegida (parámetro `status` de la API).
 * Los cambios de información adicional sustituyen la tarjeta por la que devuelve la API; si hay filtro de prioridad
 * o de etiqueta, la columna se recarga, porque la tarea puede haber dejado de cumplirlo.
 */
@Injectable({ providedIn: 'root' })
export class TasksBoardStateService {
  private readonly api = inject(TasksApiService);
  private readonly tasksState = inject(TasksStateService);
  private readonly destroyRef = inject(DestroyRef);

  readonly columns: readonly BoardColumnState[] = Object.values(TaskStatusEnum).map(
    (status) => new BoardColumnState(status, (query) => this.api.getTasks(query), this.destroyRef),
  );

  readonly updatingTaskId = this.tasksState.updatingTaskId;

  load(filters: TaskFilters): void {
    for (const column of this.columns) {
      if (filters.status === null || filters.status === column.status) {
        column.reload(filters);
      }
    }
  }

  loadMore(status: TaskStatusEnum): void {
    this.column(status).loadMore();
  }

  move(task: Task, status: TaskStatusEnum): Observable<Task> {
    const source = this.column(task.status);
    const target = this.column(status);

    return defer(() => {
      source.remove(task.id);
      target.add({ ...task, status, allowedTransitions: [] });
      return this.tasksState.changeStatus(task, status);
    }).pipe(
      finalize(() => {
        source.reload();
        target.reload();
      }),
    );
  }

  patchAdditionalInfo(task: Task, request: PatchTaskAdditionalInfoRequest): Observable<Task> {
    return this.tasksState.patchAdditionalInfo(task, request).pipe(
      tap((updated) => {
        const column = this.column(updated.status);
        const { priority, tag } = this.tasksState.filters();
        if (priority !== null || tag !== null) {
          column.reload();
        } else {
          column.replace(updated);
        }
      }),
    );
  }

  private column(status: TaskStatusEnum): BoardColumnState {
    return this.columns.find((column) => column.status === status)!;
  }
}
