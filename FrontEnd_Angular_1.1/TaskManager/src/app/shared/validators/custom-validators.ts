import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export function notBlankValidator(control: AbstractControl<string | null>): ValidationErrors | null {
  const value = control.value;
  return typeof value === 'string' && value.length > 0 && value.trim().length === 0 ? { blank: true } : null;
}

export function maxItemsValidator(max: number): ValidatorFn {
  return (control: AbstractControl<readonly unknown[] | null>): ValidationErrors | null => {
    const length = control.value?.length ?? 0;
    return length > max ? { maxItems: { max, actual: length } } : null;
  };
}
