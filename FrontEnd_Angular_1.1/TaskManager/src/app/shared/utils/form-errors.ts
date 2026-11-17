import { AbstractControl } from '@angular/forms';
import { ApiError } from '../../core/errors/api-error';

export function applyApiFieldErrors(form: AbstractControl, error: ApiError): boolean {
  let applied = false;
  for (const [path, messages] of Object.entries(error.fieldErrors)) {
    const control = form.get(path) ?? form.get(path.replace(/\[\d+\]/g, ''));
    if (control && messages.length > 0) {
      control.setErrors({ ...control.errors, api: messages[0] });
      control.markAsTouched();
      applied = true;
    }
  }
  return applied;
}

export function applyApiError(control: AbstractControl, message: string): void {
  control.setErrors({ ...control.errors, api: message });
  control.markAsTouched();
}
