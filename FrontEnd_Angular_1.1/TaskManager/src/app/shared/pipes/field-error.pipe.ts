import { Pipe, PipeTransform } from '@angular/core';
import { AbstractControl } from '@angular/forms';

@Pipe({ name: 'fieldError', pure: false })
export class FieldErrorPipe implements PipeTransform {
  transform(control: AbstractControl | null | undefined): string | null {
    const errors = control?.errors;
    if (!errors) {
      return null;
    }

    if (errors['api']) {
      return errors['api'] as string;
    }
    if (errors['required'] || errors['blank']) {
      return 'Este campo es obligatorio.';
    }
    if (errors['maxlength']) {
      return `Máximo ${errors['maxlength'].requiredLength} caracteres.`;
    }
    if (errors['email']) {
      return 'El correo no tiene un formato válido.';
    }
    if (errors['maxItems']) {
      return `Máximo ${errors['maxItems'].max} elementos.`;
    }
    if (errors['pattern']) {
      return 'El formato no es válido.';
    }
    if (errors['matDatepickerParse']) {
      return 'La fecha no es válida.';
    }
    return 'El valor no es válido.';
  }
}
