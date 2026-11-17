import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { UserAvatar } from '../../../../../shared/components/user-avatar/user-avatar';
import { toIsoDate } from '../../../../../shared/utils/date';
import { TaskPriorityEnum } from '../../../domain/enums/task-priority.enum';
import { TaskStatusEnum } from '../../../domain/enums/task-status.enum';
import { Task, TaskAdditionalInfoChange } from '../../../domain/models/task';
import { TASK_PRIORITY_OPTIONS } from '../../task-display';
import { TagPill } from '../tag-pill/tag-pill';
import { TaskPriority } from '../task-priority/task-priority';
import { TaskStatusChange, TaskStatusMenu } from '../task-status-menu/task-status-menu';

/**
 * Tarjeta de una tarea en el tablero Kanban (la columna ya indica el estado). Permite cambios rápidos de la
 * información adicional: la prioridad desde su badge y quitar etiquetas; el resto, en la ventana «Información adicional».
 */
@Component({
  selector: 'app-task-card',
  imports: [
    DatePipe,
    MatButtonModule,
    MatIconModule,
    MatMenuModule,
    MatTooltipModule,
    TagPill,
    TaskPriority,
    TaskStatusMenu,
    UserAvatar,
  ],
  templateUrl: './task-card.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TaskCard {
  readonly task = input.required<Task>();
  readonly updating = input(false);
  readonly userPhotoUrl = input<string | null>(null);
  readonly actionsDisabled = input(false);

  readonly statusChange = output<TaskStatusChange>();
  readonly edit = output<Task>();
  readonly editAdditionalInfo = output<Task>();
  readonly additionalInfoChange = output<TaskAdditionalInfoChange>();

  protected readonly priorityOptions = TASK_PRIORITY_OPTIONS;

  protected readonly priority = computed(() => this.task().additionalInfo?.priority ?? null);
  protected readonly tags = computed(() => this.task().additionalInfo?.tags ?? []);
  protected readonly estimatedDate = computed(() => this.task().additionalInfo?.estimatedCompletionDate ?? null);

  protected readonly isOverdue = computed(() => {
    const date = this.estimatedDate();
    return date !== null && this.task().status !== TaskStatusEnum.Done && date < toIsoDate(new Date());
  });

  protected setPriority(priority: TaskPriorityEnum | null): void {
    if (priority === this.priority()) {
      return;
    }
    this.additionalInfoChange.emit({
      task: this.task(),
      request: priority === null ? { clearPriority: true } : { priority },
    });
  }

  protected removeTag(tag: string): void {
    this.additionalInfoChange.emit({ task: this.task(), request: { removeTags: [tag] } });
  }
}
