import { COMMA, ENTER } from '@angular/cdk/keycodes';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatChipInputEvent, MatChipsModule } from '@angular/material/chips';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MAT_DIALOG_DATA, MatDialog, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { Observable, finalize } from 'rxjs';
import { ApiError } from '../../../../../core/errors/api-error';
import { FieldErrorPipe } from '../../../../../shared/pipes/field-error.pipe';
import { fromIsoDate, toIsoDate } from '../../../../../shared/utils/date';
import { applyApiFieldErrors } from '../../../../../shared/utils/form-errors';
import { maxItemsValidator, notBlankValidator } from '../../../../../shared/validators/custom-validators';
import { UserReference } from '../../../../users/domain/models/user';
import { UserSelect } from '../../../../users/presentation/components/user-select/user-select';
import { TasksStateService } from '../../../application/tasks-state.service';
import { TaskPriorityEnum } from '../../../domain/enums/task-priority.enum';
import {
  CreateTaskRequest,
  TASK_MAX_TAGS,
  TASK_TAG_MAX_LENGTH,
  TASK_TITLE_MAX_LENGTH,
  Task,
} from '../../../domain/models/task';
import { TASK_PRIORITY_OPTIONS } from '../../task-display';
import { TaskPriority } from '../task-priority/task-priority';

export interface TaskFormDialogData {
  readonly task: Task | null;
}

export function openTaskFormDialog(dialog: MatDialog, task: Task | null = null): Observable<Task | undefined> {
  return dialog
    .open<TaskFormDialog, TaskFormDialogData, Task>(TaskFormDialog, {
      data: { task },
      width: '40rem',
      maxWidth: 'calc(100vw - 2rem)',
    })
    .afterClosed();
}

/**
 * Formulario reactivo de alta y edición de tareas. La estructura del formulario coincide con el cuerpo
 * de la API (`additionalInfo.*`), así los errores 400 se asignan a cada campo sin traducir rutas.
 */
@Component({
  selector: 'app-task-form-dialog',
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
    FieldErrorPipe,
    TaskPriority,
    UserSelect,
  ],
  templateUrl: './task-form-dialog.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TaskFormDialog {
  private readonly state = inject(TasksStateService);
  private readonly dialogRef = inject<MatDialogRef<TaskFormDialog, Task>>(MatDialogRef);
  private readonly formBuilder = inject(NonNullableFormBuilder);
  protected readonly data = inject<TaskFormDialogData>(MAT_DIALOG_DATA);

  protected readonly isEdit = this.data.task !== null;
  protected readonly initialUser: UserReference | null = this.data.task
    ? { id: this.data.task.userId, name: this.data.task.userName }
    : null;

  protected readonly titleMaxLength = TASK_TITLE_MAX_LENGTH;
  protected readonly priorityOptions = TASK_PRIORITY_OPTIONS;
  protected readonly tagSeparatorKeys = [ENTER, COMMA] as const;

  protected readonly saving = signal(false);
  protected readonly tagError = signal<string | null>(null);

  protected readonly form = this.createForm(this.data.task);

  protected get tagsControl() {
    return this.form.controls.additionalInfo.controls.tags;
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

    const tags = this.tagsControl.value;
    if (!tags.some((existing) => existing.toLowerCase() === tag.toLowerCase())) {
      this.tagsControl.setValue([...tags, tag]);
      this.tagsControl.markAsTouched();
    }
    event.chipInput.clear();
  }

  protected removeTag(tag: string): void {
    this.tagsControl.setValue(this.tagsControl.value.filter((existing) => existing !== tag));
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const request = this.toRequest();
    const task = this.data.task;

    this.saving.set(true);
    this.dialogRef.disableClose = true;
    (task ? this.state.update(task.id, request) : this.state.create(request))
      .pipe(
        finalize(() => {
          this.saving.set(false);
          this.dialogRef.disableClose = false;
        }),
      )
      .subscribe({
        next: (saved) => this.dialogRef.close(saved),
        error: (error: ApiError) => applyApiFieldErrors(this.form, error),
      });
  }

  private createForm(task: Task | null) {
    const info = task?.additionalInfo;
    return this.formBuilder.group({
      title: [task?.title ?? '', [Validators.required, notBlankValidator, Validators.maxLength(TASK_TITLE_MAX_LENGTH)]],
      userId: this.formBuilder.control<string | null>(task?.userId ?? null, Validators.required),
      additionalInfo: this.formBuilder.group({
        priority: this.formBuilder.control<TaskPriorityEnum | null>(info?.priority ?? null),
        estimatedCompletionDate: this.formBuilder.control<Date | null>(
          info?.estimatedCompletionDate ? fromIsoDate(info.estimatedCompletionDate) : null,
        ),
        tags: this.formBuilder.control<string[]>([...(info?.tags ?? [])], maxItemsValidator(TASK_MAX_TAGS)),
      }),
    });
  }

  private toRequest(): CreateTaskRequest {
    const { title, userId, additionalInfo } = this.form.getRawValue();
    const metadata = this.data.task?.additionalInfo?.metadata ?? null;
    const info = {
      priority: additionalInfo.priority,
      estimatedCompletionDate: additionalInfo.estimatedCompletionDate
        ? toIsoDate(additionalInfo.estimatedCompletionDate)
        : null,
      tags: additionalInfo.tags,
      metadata,
    };
    const hasAdditionalInfo =
      info.priority !== null ||
      info.estimatedCompletionDate !== null ||
      info.tags.length > 0 ||
      Object.keys(metadata ?? {}).length > 0;

    return {
      title: title.trim(),
      userId: userId ?? '',
      additionalInfo: hasAdditionalInfo ? info : null,
    };
  }
}
