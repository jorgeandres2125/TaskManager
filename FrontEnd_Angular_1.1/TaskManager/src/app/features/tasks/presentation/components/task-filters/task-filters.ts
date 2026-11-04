import { ChangeDetectionStrategy, Component, OnInit, inject, input, output } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NonNullableFormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { debounceTime, distinctUntilChanged, map, merge } from 'rxjs';
import { SearchField } from '../../../../../shared/components/search-field/search-field';
import { TaskPriorityEnum } from '../../../domain/enums/task-priority.enum';
import { TaskStatusEnum } from '../../../domain/enums/task-status.enum';
import { TaskFilters as TaskFiltersModel } from '../../../domain/models/task';
import { TASK_PRIORITY_OPTIONS, TASK_STATUS_DOT_CLASSES, TASK_STATUS_LABELS } from '../../task-display';
import { TaskPriority } from '../task-priority/task-priority';

const TAG_DEBOUNCE_MS = 400;

/**
 * Filtros del tablero de tareas: estado (muestra solo esa columna del Kanban), prioridad y etiqueta.
 * Solo emite cambios; no conoce el estado de la aplicación ni la API.
 */
@Component({
  selector: 'app-task-filters',
  imports: [ReactiveFormsModule, MatIconModule, SearchField, TaskPriority],
  templateUrl: './task-filters.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TaskFilters implements OnInit {
  readonly filters = input.required<TaskFiltersModel>();
  readonly filtersChange = output<Partial<TaskFiltersModel>>();

  protected readonly priorityOptions = TASK_PRIORITY_OPTIONS;
  protected readonly statusOptions = Object.values(TaskStatusEnum);
  protected readonly statusLabels = TASK_STATUS_LABELS;
  protected readonly statusDots = TASK_STATUS_DOT_CLASSES;

  protected readonly form = inject(NonNullableFormBuilder).group({
    status: [null as TaskStatusEnum | null],
    priority: [null as TaskPriorityEnum | null],
    tag: [''],
  });

  constructor() {
    const { status, priority, tag } = this.form.controls;

    merge(
      status.valueChanges.pipe(map((value) => ({ status: value }))),
      priority.valueChanges.pipe(map((value) => ({ priority: value }))),
      tag.valueChanges.pipe(
        debounceTime(TAG_DEBOUNCE_MS),
        map((value) => value.trim().replace(/^#+/, '').trim()),
        distinctUntilChanged(),
        map((value) => ({ tag: value || null })),
      ),
    )
      .pipe(takeUntilDestroyed())
      .subscribe((changes) => this.filtersChange.emit(changes));
  }

  ngOnInit(): void {
    const { status, priority, tag } = this.filters();
    this.form.setValue({ status, priority, tag: tag ?? '' }, { emitEvent: false });
  }

  protected selectStatus(status: TaskStatusEnum | null): void {
    if (this.form.controls.status.value !== status) {
      this.form.controls.status.setValue(status);
    }
  }

  protected selectPriority(priority: TaskPriorityEnum | null): void {
    if (this.form.controls.priority.value !== priority) {
      this.form.controls.priority.setValue(priority);
    }
  }

  protected clear(): void {
    this.form.reset({ status: null, priority: null, tag: '' }, { emitEvent: false });
    this.filtersChange.emit({ status: null, priority: null, tag: null });
  }

  protected get hasFilters(): boolean {
    const { status, priority, tag } = this.form.getRawValue();
    return status !== null || priority !== null || tag.trim() !== '';
  }
}
