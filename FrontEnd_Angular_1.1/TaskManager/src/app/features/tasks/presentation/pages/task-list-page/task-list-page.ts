import { ChangeDetectionStrategy, Component, OnInit, computed, effect, inject, untracked } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { PageHeader, PageStat } from '../../../../../shared/components/page-header/page-header';
import { UserAvatar } from '../../../../../shared/components/user-avatar/user-avatar';
import { UsersDirectoryService } from '../../../../users/application/users-directory.service';
import { UserRail } from '../../../../users/presentation/components/user-rail/user-rail';
import { TasksBoardStateService } from '../../../application/tasks-board-state.service';
import { TasksStateService } from '../../../application/tasks-state.service';
import { TaskStatusEnum } from '../../../domain/enums/task-status.enum';
import { Task, TaskAdditionalInfoChange, TaskFilters as TaskFiltersModel } from '../../../domain/models/task';
import { BoardColumnView, TaskBoard } from '../../components/task-board/task-board';
import { TaskFilters } from '../../components/task-filters/task-filters';
import { openTaskAdditionalInfoDialog } from '../../components/task-additional-info-dialog/task-additional-info-dialog';
import { openTaskFormDialog } from '../../components/task-form-dialog/task-form-dialog';
import { TaskStatusChange } from '../../components/task-status-menu/task-status-menu';
import { TASK_STATUS_DOT_CLASSES, TASK_STATUS_LABELS } from '../../task-display';

const STAT_LABELS: Readonly<Record<TaskStatusEnum, string>> = {
  [TaskStatusEnum.Pending]: 'pendientes',
  [TaskStatusEnum.InProgress]: 'en progreso',
  [TaskStatusEnum.Done]: 'completadas',
};

/**
 * Listado de tareas como tablero Kanban: una columna por estado, filtros por estado (muestra solo esa columna),
 * prioridad y etiqueta, y el panel del equipo para filtrar por responsable.
 * Crear y editar tareas se hace en una ventana modal; la información adicional también se cambia desde la tarjeta.
 */
@Component({
  selector: 'app-task-list-page',
  imports: [MatIconModule, PageHeader, TaskBoard, TaskFilters, UserAvatar, UserRail],
  templateUrl: './task-list-page.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TaskListPage implements OnInit {
  protected readonly state = inject(TasksStateService);
  protected readonly board = inject(TasksBoardStateService);
  protected readonly directory = inject(UsersDirectoryService);
  private readonly dialog = inject(MatDialog);

  protected readonly filters = this.state.filters;

  protected readonly selectedUserId = computed(() => this.filters().userId);
  protected readonly selectedUser = computed(() => this.directory.findById(this.selectedUserId()));

  protected readonly boardColumns = computed<readonly BoardColumnView[]>(() => {
    const status = this.filters().status;
    return this.board.columns
      .filter((column) => status === null || column.status === status)
      .map((column) => ({
      status: column.status,
      items: column.items(),
      totalCount: column.totalCount(),
      loading: column.loading(),
      loaded: column.loaded(),
      hasMore: column.hasMore(),
    }));
  });

  protected readonly subtitle = computed(() => {
    const user = this.selectedUser();
    const { status, priority, tag } = this.filters();
    if (user) {
      return `Tareas de ${user.name}`;
    }
    if (status) {
      return `Solo tareas en «${TASK_STATUS_LABELS[status]}»`;
    }
    return priority || tag ? 'Tablero filtrado' : 'Tablero del equipo';
  });

  protected readonly stats = computed<readonly PageStat[]>(() => {
    const columns = this.boardColumns();
    if (!columns.every((column) => column.loaded)) {
      return [{ value: '…', label: 'cargando' }];
    }
    const total: PageStat = { value: columns.reduce((sum, column) => sum + column.totalCount, 0), label: 'tareas' };
    if (columns.length === 1) {
      return [total];
    }
    return [
      total,
      ...columns.map((column) => ({
        value: column.totalCount,
        label: STAT_LABELS[column.status],
        dotClass: TASK_STATUS_DOT_CLASSES[column.status],
      })),
    ];
  });

  constructor() {
    effect(() => {
      const filters = this.filters();
      untracked(() => this.board.load(filters));
    });
  }

  ngOnInit(): void {
    this.directory.load(true);
  }

  protected selectUser(userId: string | null): void {
    this.state.updateFilters({ userId });
  }

  protected onFiltersChange(changes: Partial<TaskFiltersModel>): void {
    this.state.updateFilters(changes);
  }

  protected onMove({ task, status }: TaskStatusChange): void {
    this.board.move(task, status).subscribe({ error: () => undefined });
  }

  protected onLoadMore(status: TaskStatusEnum): void {
    this.board.loadMore(status);
  }

  protected onAdditionalInfoChange({ task, request }: TaskAdditionalInfoChange): void {
    this.board.patchAdditionalInfo(task, request).subscribe({ error: () => undefined });
  }

  protected openAdditionalInfo(task: Task): void {
    openTaskAdditionalInfoDialog(this.dialog, task).subscribe();
  }

  protected openForm(task: Task | null = null): void {
    openTaskFormDialog(this.dialog, task).subscribe((saved) => {
      if (saved) {
        this.board.load(this.filters());
      }
    });
  }
}
