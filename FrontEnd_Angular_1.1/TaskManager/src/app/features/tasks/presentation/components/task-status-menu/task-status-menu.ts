import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TaskStatusEnum } from '../../../domain/enums/task-status.enum';
import { Task } from '../../../domain/models/task';
import { TASK_STATUS_LABELS } from '../../task-display';

export interface TaskStatusChange {
  readonly task: Task;
  readonly status: TaskStatusEnum;
}

/**
 * Menú para cambiar el estado de una tarea (tabla, tarjetas y Kanban). Solo ofrece `allowedTransitions`,
 * que calcula la API. También es la alternativa accesible a arrastrar en el Kanban.
 */
@Component({
  selector: 'app-task-status-menu',
  imports: [MatButtonModule, MatIconModule, MatMenuModule, MatProgressSpinnerModule, MatTooltipModule],
  template: `
    @if (updating()) {
      <mat-spinner diameter="20" class="m-2" />
    } @else {
      <button
        mat-icon-button
        type="button"
        [matMenuTriggerFor]="statusMenu"
        [disabled]="disabled() || task().allowedTransitions.length === 0"
        matTooltip="Cambiar estado"
        aria-label="Cambiar estado"
      >
        <mat-icon class="!text-[20px]">swap_horiz</mat-icon>
      </button>
      <mat-menu #statusMenu="matMenu" xPosition="before">
        @for (status of task().allowedTransitions; track status) {
          <button mat-menu-item type="button" (click)="statusChange.emit({ task: task(), status })">
            Mover a «{{ labels[status] }}»
          </button>
        }
      </mat-menu>
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TaskStatusMenu {
  readonly task = input.required<Task>();
  readonly updating = input(false);
  readonly disabled = input(false);
  readonly statusChange = output<TaskStatusChange>();

  protected readonly labels = TASK_STATUS_LABELS;
}
