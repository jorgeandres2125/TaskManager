import { COMMA, ENTER } from '@angular/cdk/keycodes';
import { ChangeDetectionStrategy, Component, computed, effect, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatChipInputEvent, MatChipsModule } from '@angular/material/chips';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MAT_DIALOG_DATA, MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Observable } from 'rxjs';
import { FieldErrorPipe } from '../../../../../shared/pipes/field-error.pipe';
import { fromIsoDate, toIsoDate } from '../../../../../shared/utils/date';
import { notBlankValidator } from '../../../../../shared/validators/custom-validators';
import { TasksBoardStateService } from '../../../application/tasks-board-state.service';
import { TaskPriorityEnum } from '../../../domain/enums/task-priority.enum';
import {
  PatchTaskAdditionalInfoRequest,
  TASK_MAX_METADATA_ENTRIES,
  TASK_MAX_TAGS,
  TASK_METADATA_KEY_MAX_LENGTH,
  TASK_METADATA_KEY_PATTERN,
  TASK_METADATA_VALUE_MAX_LENGTH,
  TASK_TAG_MAX_LENGTH,
  Task,
} from '../../../domain/models/task';
import { TASK_PRIORITY_OPTIONS } from '../../task-display';
import { TaskPriority } from '../task-priority/task-priority';

export interface TaskAdditionalInfoDialogData {
  readonly task: Task;
}

export function openTaskAdditionalInfoDialog(dialog: MatDialog, task: Task): Observable<Task | undefined> {
  return dialog
    .open<TaskAdditionalInfoDialog, TaskAdditionalInfoDialogData, Task>(TaskAdditionalInfoDialog, {
      data: { task },
      width: '40rem',
      maxWidth: 'calc(100vw - 2rem)',
    })
    .afterClosed();
}

/**
 * Edición rápida de la información adicional de una tarea: prioridad, fecha estimada, etiquetas y metadatos.
 * Cada cambio se guarda al momento con `PATCH /api/tasks/{id}/additional-info`, que solo toca ese campo del JSON
 * (JSON_MODIFY en SQL Server). La tarjeta del tablero se actualiza con la respuesta.
 */
@Component({
  selector: 'app-task-additional-info-dialog',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatChipsModule,
    MatDatepickerModule,
    MatDialogModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatSelectModule,
    MatTooltipModule,
    FieldErrorPipe,
    TaskPriority,
  ],
  templateUrl: './task-additional-info-dialog.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TaskAdditionalInfoDialog {
  private readonly board = inject(TasksBoardStateService);
  private readonly formBuilder = inject(NonNullableFormBuilder);
  private readonly data = inject<TaskAdditionalInfoDialogData>(MAT_DIALOG_DATA);

  protected readonly task = signal(this.data.task);
  protected readonly busy = computed(() => this.board.updatingTaskId() !== null);

  protected readonly priorityOptions = TASK_PRIORITY_OPTIONS;
  protected readonly maxTags = TASK_MAX_TAGS;
  protected readonly maxMetadataEntries = TASK_MAX_METADATA_ENTRIES;
  protected readonly metadataKeyMaxLength = TASK_METADATA_KEY_MAX_LENGTH;
  protected readonly metadataValueMaxLength = TASK_METADATA_VALUE_MAX_LENGTH;
  protected readonly tagSeparatorKeys = [ENTER, COMMA] as const;

  protected readonly tags = computed(() => this.task().additionalInfo?.tags ?? []);
  protected readonly metadata = computed(() =>
    Object.entries(this.task().additionalInfo?.metadata ?? {}).sort(([left], [right]) => left.localeCompare(right)),
  );
  protected readonly tagsFull = computed(() => this.tags().length >= TASK_MAX_TAGS);
  protected readonly tagError = signal<string | null>(null);

  protected readonly priorityControl = this.formBuilder.control<TaskPriorityEnum | null>(null);
  protected readonly dateControl = this.formBuilder.control<Date | null>(null);
  protected readonly metadataForm = this.formBuilder.group({
    key: [
      '',
      [
        Validators.required,
        Validators.maxLength(TASK_METADATA_KEY_MAX_LENGTH),
        Validators.pattern(TASK_METADATA_KEY_PATTERN),
      ],
    ],
    value: ['', [Validators.required, notBlankValidator, Validators.maxLength(TASK_METADATA_VALUE_MAX_LENGTH)]],
  });

  constructor() {
    this.syncControls();

    effect(() => {
      const options = { emitEvent: false };
      if (this.busy()) {
        this.priorityControl.disable(options);
        this.dateControl.disable(options);
      } else {
        this.priorityControl.enable(options);
        this.dateControl.enable(options);
      }
    });
  }

  protected isNewMetadataKey(): boolean {
    const key = this.metadataForm.controls.key.value.trim();
    return !this.metadata().some(([existing]) => existing === key);
  }

  protected setPriority(priority: TaskPriorityEnum | null): void {
    this.patch(priority === null ? { clearPriority: true } : { priority });
  }

  protected setDate(date: Date | null): void {
    if (date) {
      this.patch({ estimatedCompletionDate: toIsoDate(date) });
    }
  }

  protected clearDate(): void {
    this.patch({ clearEstimatedCompletionDate: true });
  }

  protected addTag(event: MatChipInputEvent): void {
    const tag = event.value.trim();
    this.tagError.set(null);

    if (!tag) {
      return;
    }
    if (tag.length > TASK_TAG_MAX_LENGTH) {
      this.tagError.set(`Cada etiqueta admite como máximo ${TASK_TAG_MAX_LENGTH} caracteres.`);
      return;
    }
    if (this.tags().some((existing) => existing.toLowerCase() === tag.toLowerCase())) {
      event.chipInput.clear();
      return;
    }

    this.patch({ addTags: [tag] }, () => event.chipInput.clear());
  }

  protected removeTag(tag: string): void {
    this.patch({ removeTags: [tag] });
  }

  protected editMetadata(key: string, value: string): void {
    this.metadataForm.setValue({ key, value });
  }

  protected removeMetadata(key: string): void {
    this.patch({ removeMetadata: [key] });
  }

  protected saveMetadata(): void {
    if (this.metadataForm.invalid) {
      this.metadataForm.markAllAsTouched();
      return;
    }

    const { key, value } = this.metadataForm.getRawValue();
    this.patch({ setMetadata: { [key.trim()]: value.trim() } }, () => this.metadataForm.reset());
  }

  private patch(request: PatchTaskAdditionalInfoRequest, onSaved?: () => void): void {
    this.board.patchAdditionalInfo(this.task(), request).subscribe({
      next: (updated) => {
        this.task.set(updated);
        this.syncControls();
        onSaved?.();
      },
      error: () => this.syncControls(),
    });
  }

  private syncControls(): void {
    const info = this.task().additionalInfo;
    const options = { emitEvent: false };
    this.priorityControl.setValue(info?.priority ?? null, options);
    this.dateControl.setValue(info?.estimatedCompletionDate ? fromIsoDate(info.estimatedCompletionDate) : null, options);
  }
}
