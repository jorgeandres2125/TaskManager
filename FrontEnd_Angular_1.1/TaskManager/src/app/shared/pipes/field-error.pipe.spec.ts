import { FormControl, Validators } from '@angular/forms';
import { FieldErrorPipe } from './field-error.pipe';

describe('FieldErrorPipe', () => {
  const pipe = new FieldErrorPipe();

  it('devuelve null si el control es válido', () => {
    expect(pipe.transform(new FormControl('ok', Validators.required))).toBeNull();
  });

  it('prioriza el mensaje de la API sobre los de validación local', () => {
    const control = new FormControl('', Validators.required);
    control.setErrors({ required: true, api: 'El usuario asignado no existe.' });

    expect(pipe.transform(control)).toBe('El usuario asignado no existe.');
  });

  it('traduce los errores de validación de Angular', () => {
    expect(pipe.transform(new FormControl('', Validators.required))).toBe('Este campo es obligatorio.');
    expect(pipe.transform(new FormControl('abcd', Validators.maxLength(3)))).toBe('Máximo 3 caracteres.');
    expect(pipe.transform(new FormControl('no-es-correo', Validators.email))).toBe(
      'El correo no tiene un formato válido.',
    );
  });
});
