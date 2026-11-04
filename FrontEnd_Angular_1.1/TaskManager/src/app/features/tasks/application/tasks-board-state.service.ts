import { DestroyRef, Injectable, inject } from '@angular/core';
import { Observable, defer, finalize } from 'rxjs';
import { TaskStatusEnum } from '../domain/enums/task-status.enum';
import { Task, TaskFilters } from '../domain/models/task';
import { TasksApiService } from '../infrastructure/tasks-api.service';
import { BoardColumnState } from './board-column.state';
import { TasksStateService } from './tasks-state.service';

/**
 * Estado del tablero Kanban: una columna por cada estado del enunciado (Pending, InProgress, Done).
 * Los contadores salen del `totalCount` de la consulta paginada de cada columna; no hace falta un endpoint nuevo.
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
      column.reload(filters);
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

  private column(status: TaskStatusEnum): BoardColumnState {
    return this.columns.find((column) => column.status === status)!;
  }
}
