import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { TaskPriorityEnum } from '../../../domain/enums/task-priority.enum';
import { TASK_PRIORITY_CLASSES, TASK_PRIORITY_ICONS, TASK_PRIORITY_LABELS } from '../../task-display';

/**
 * Badge de prioridad (icono + "Alta", "Media" o "Baja"), sin repetir la palabra "Prioridad".
 * El mismo en la tarjeta, el filtro y el formulario. El texto accesible sí la incluye.
 */
@Component({
  selector: 'app-task-priority',
  imports: [MatIconModule],
  template: `
    <span
      class="inline-flex h-6 items-center gap-1 rounded-full px-2 text-xs font-medium ring-1 ring-inset align-middle"
      [class]="classes()"
      [attr.title]="'Prioridad ' + label().toLowerCase()"
    >
      <mat-icon class="!h-3.5 !w-3.5 shrink-0 !text-sm" aria-hidden="true">{{ icon() }}</mat-icon>
      <span class="sr-only">Prioridad </span>{{ label() }}
    </span>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TaskPriority {
  readonly priority = input.required<TaskPriorityEnum>();

  protected readonly icon = computed(() => TASK_PRIORITY_ICONS[this.priority()]);
  protected readonly label = computed(() => TASK_PRIORITY_LABELS[this.priority()]);
  protected readonly classes = computed(() => TASK_PRIORITY_CLASSES[this.priority()]);
}
