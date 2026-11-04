import { CdkDrag, CdkDragDrop, CdkDropList, DragDropModule } from '@angular/cdk/drag-drop';
import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  effect,
  input,
  output,
  signal,
  untracked,
  viewChildren,
} from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { InViewport } from '../../../../../shared/directives/in-viewport.directive';
import { TaskStatusEnum } from '../../../domain/enums/task-status.enum';
import { Task, canMoveTo } from '../../../domain/models/task';
import { TASK_STATUS_COUNT_CLASSES, TASK_STATUS_DOT_CLASSES, TASK_STATUS_LABELS } from '../../task-display';
import { TaskCard } from '../task-card/task-card';
import { TaskStatusChange } from '../task-status-menu/task-status-menu';

export interface BoardColumnView {
  readonly status: TaskStatusEnum;
  readonly items: readonly Task[];
  readonly totalCount: number;
  readonly loading: boolean;
  readonly loaded: boolean;
  readonly hasMore: boolean;
}

/**
 * Tablero Kanban (solo presentación): una columna por estado. Arrastrar una tarjeta a otra columna
 * cambia su estado; las columnas que no están en `allowedTransitions` no aceptan la tarjeta
 * (por ejemplo, Pending → Done). El menú de cada tarjeta ofrece lo mismo sin arrastrar.
 */
@Component({
  selector: 'app-task-board',
  imports: [DragDropModule, InViewport, MatButtonModule, MatIconModule, MatProgressBarModule, TaskCard],
  templateUrl: './task-board.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TaskBoard {
  readonly columns = input.required<readonly BoardColumnView[]>();
  readonly updatingTaskId = input<string | null>(null);
  readonly scrollResetKey = input<unknown>(null);
  readonly photoUrlOf = input<(userId: string) => string | null>(() => null);

  private readonly columnLists = viewChildren<ElementRef<HTMLElement>>('columnList');

  readonly move = output<TaskStatusChange>();
  readonly loadMore = output<TaskStatusEnum>();
  readonly edit = output<Task>();

  protected readonly draggingTask = signal<Task | null>(null);

  protected readonly labels = TASK_STATUS_LABELS;
  protected readonly dotClasses = TASK_STATUS_DOT_CLASSES;
  protected readonly gridColumns: Readonly<Partial<Record<number, string>>> = {
    1: 'lg:grid-cols-1',
    2: 'lg:grid-cols-2',
    3: 'lg:grid-cols-3',
  };
  protected readonly countClasses = TASK_STATUS_COUNT_CLASSES;
  protected readonly skeletonCards = [1, 2, 3];

  protected readonly emptyIcons: Readonly<Record<TaskStatusEnum, string>> = {
    [TaskStatusEnum.Pending]: 'inbox',
    [TaskStatusEnum.InProgress]: 'timelapse',
    [TaskStatusEnum.Done]: 'task_alt',
  };
  protected readonly emptyTitles: Readonly<Record<TaskStatusEnum, string>> = {
    [TaskStatusEnum.Pending]: 'No hay tareas pendientes',
    [TaskStatusEnum.InProgress]: 'Nada en progreso',
    [TaskStatusEnum.Done]: 'Aún no hay tareas completadas',
  };
  protected readonly emptyHints: Readonly<Record<TaskStatusEnum, string>> = {
    [TaskStatusEnum.Pending]: 'Cree una con «Nueva tarea» o cambie los filtros.',
    [TaskStatusEnum.InProgress]: 'Arrastre aquí una tarea pendiente para empezarla.',
    [TaskStatusEnum.Done]: 'Arrastre aquí una tarea en progreso al terminarla.',
  };

  protected readonly dragStartDelay = { touch: 300, mouse: 0 };

  protected readonly canEnter = (drag: CdkDrag<Task>, drop: CdkDropList<TaskStatusEnum>): boolean =>
    canMoveTo(drag.data, drop.data);

  constructor() {
    effect(() => {
      this.scrollResetKey();
      untracked(() => this.columnLists().forEach((list) => list.nativeElement.scrollTo({ top: 0 })));
    });
  }

  protected isBlocked(status: TaskStatusEnum): boolean {
    const task = this.draggingTask();
    return task !== null && !canMoveTo(task, status);
  }

  protected isTarget(status: TaskStatusEnum): boolean {
    const task = this.draggingTask();
    return task !== null && task.status !== status && canMoveTo(task, status);
  }

  protected onDrop(event: CdkDragDrop<TaskStatusEnum, TaskStatusEnum, Task>): void {
    this.draggingTask.set(null);
    if (event.previousContainer !== event.container && event.isPointerOverContainer) {
      this.move.emit({ task: event.item.data, status: event.container.data });
    }
  }
}
