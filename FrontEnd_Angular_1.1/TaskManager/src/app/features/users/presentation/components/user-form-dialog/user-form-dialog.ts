import { ChangeDetectionStrategy, Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialog, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { Observable, finalize, switchMap, tap } from 'rxjs';
import { ApiError } from '../../../../../core/errors/api-error';
import { UserAvatar } from '../../../../../shared/components/user-avatar/user-avatar';
import { FieldErrorPipe } from '../../../../../shared/pipes/field-error.pipe';
import { applyApiError, applyApiFieldErrors } from '../../../../../shared/utils/form-errors';
import { notBlankValidator } from '../../../../../shared/validators/custom-validators';
import { UsersStateService } from '../../../application/users-state.service';
import {
  USER_EMAIL_MAX_LENGTH,
  USER_NAME_MAX_LENGTH,
  USER_PHOTO_ACCEPT,
  USER_PHOTO_MAX_BYTES,
  USER_PHOTO_TYPES,
  User,
  UserPhotoChange,
} from '../../../domain/models/user';
import { UsersApiService } from '../../../infrastructure/users-api.service';

const HTTP_CONFLICT = 409;

export interface UserFormDialogData {
  readonly user: User | null;
}

export function openUserFormDialog(dialog: MatDialog, user: User | null = null): Observable<User | undefined> {
  return dialog
    .open<UserFormDialog, UserFormDialogData, User>(UserFormDialog, {
      data: { user },
      width: '32rem',
      maxWidth: 'calc(100vw - 2rem)',
    })
    .afterClosed();
}

/**
 * Formulario reactivo de alta y edición de usuarios, con su foto opcional.
 * Primero se guardan nombre y correo y después la foto (la API la recibe aparte, como multipart).
 */
@Component({
  selector: 'app-user-form-dialog',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    FieldErrorPipe,
    UserAvatar,
  ],
  templateUrl: './user-form-dialog.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UserFormDialog {
  private readonly state = inject(UsersStateService);
  private readonly api = inject(UsersApiService);
  private readonly dialogRef = inject<MatDialogRef<UserFormDialog, User>>(MatDialogRef);
  private readonly data = inject<UserFormDialogData>(MAT_DIALOG_DATA);

  private readonly user = signal<User | null>(this.data.user);

  protected readonly isEdit = this.data.user !== null;
  protected readonly saving = signal(false);
  protected readonly photoAccept = USER_PHOTO_ACCEPT;

  protected readonly photoChange = signal<UserPhotoChange>({ kind: 'keep' });
  protected readonly photoError = signal<string | null>(null);
  private readonly previewUrl = signal<string | null>(null);

  protected readonly form = inject(NonNullableFormBuilder).group({
    name: [
      this.data.user?.name ?? '',
      [Validators.required, notBlankValidator, Validators.maxLength(USER_NAME_MAX_LENGTH)],
    ],
    email: [
      this.data.user?.email ?? '',
      [Validators.required, Validators.email, Validators.maxLength(USER_EMAIL_MAX_LENGTH)],
    ],
  });

  protected readonly photoUrl = computed(() => {
    const change = this.photoChange();
    if (change.kind === 'replace') {
      return this.previewUrl();
    }
    return change.kind === 'remove' ? null : this.api.photoUrl(this.user());
  });

  protected readonly hasPhoto = computed(() => this.photoUrl() !== null);

  protected readonly selectedFileName = computed(() => {
    const change = this.photoChange();
    return change.kind === 'replace' ? change.file.name : null;
  });

  protected readonly nameValue = toSignal(this.form.controls.name.valueChanges, {
    initialValue: this.form.controls.name.value,
  });

  constructor() {
    inject(DestroyRef).onDestroy(() => this.revokePreview());
  }

  protected onPhotoSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';
    if (!file) {
      return;
    }

    if (!USER_PHOTO_TYPES.includes(file.type)) {
      this.photoError.set('La foto debe ser JPG, PNG o WebP.');
      return;
    }
    if (file.size > USER_PHOTO_MAX_BYTES) {
      this.photoError.set(`La foto no puede superar ${USER_PHOTO_MAX_BYTES / (1024 * 1024)} MB.`);
      return;
    }

    this.photoError.set(null);
    this.revokePreview();
    this.previewUrl.set(URL.createObjectURL(file));
    this.photoChange.set({ kind: 'replace', file });
  }

  protected removePhoto(): void {
    this.photoError.set(null);
    this.revokePreview();
    this.photoChange.set(this.api.photoUrl(this.user()) ? { kind: 'remove' } : { kind: 'keep' });
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const userId = this.user()?.id ?? null;
    const request = this.form.getRawValue();

    this.saving.set(true);
    this.dialogRef.disableClose = true;
    (userId ? this.state.update(userId, request) : this.state.create(request))
      .pipe(
        tap((user) => this.user.set(user)),
        switchMap((user) => this.state.changePhoto(user, this.photoChange())),
        finalize(() => {
          this.saving.set(false);
          this.dialogRef.disableClose = false;
        }),
      )
      .subscribe({
        next: (saved) => this.dialogRef.close(saved),
        error: (error: ApiError) => this.showError(error),
      });
  }

  private showError(error: ApiError): void {
    const photoMessage = error.fieldErrors['fileName']?.[0] ?? error.fieldErrors['content']?.[0] ?? error.fieldErrors['file']?.[0];
    if (photoMessage) {
      this.photoError.set(photoMessage);
      return;
    }

    if (error.status === HTTP_CONFLICT && error.detail) {
      applyApiError(this.form.controls.email, error.detail);
    } else {
      applyApiFieldErrors(this.form, error);
    }
  }

  private revokePreview(): void {
    const url = this.previewUrl();
    if (url) {
      URL.revokeObjectURL(url);
      this.previewUrl.set(null);
    }
  }
}
