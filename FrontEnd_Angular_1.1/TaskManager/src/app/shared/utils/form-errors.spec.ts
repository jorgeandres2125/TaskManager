import { FormControl, FormGroup } from '@angular/forms';
import { ApiError } from '../../core/errors/api-error';
import { applyApiFieldErrors } from './form-errors';

function validationError(fieldErrors: Record<string, string[]>): ApiError {
  return { status: 400, title: 'Datos no válidos', detail: null, fieldErrors, traceId: null };
}

describe('applyApiFieldErrors', () => {
  const createForm = () =>
    new FormGroup({
      title: new FormControl(''),
      additionalInfo: new FormGroup({
        priority: new FormControl<string | null>(null),
        tags: new FormControl<string[]>([]),
      }),
    });

  it('asigna cada error a su control, incluidos los anidados', () => {
    const form = createForm();

    const applied = applyApiFieldErrors(
      form,
      validationError({
        title: ['Título obligatorio.'],
        'additionalInfo.priority': ['Prioridad no válida.'],
      }),
    );

    expect(applied).toBeTrue();
    expect(form.get('title')?.errors?.['api']).toBe('Título obligatorio.');
    expect(form.get('additionalInfo.priority')?.errors?.['api']).toBe('Prioridad no válida.');
    expect(form.get('title')?.touched).toBeTrue();
  });

  it('asigna los errores de un elemento de colección al control de la colección', () => {
    const form = createForm();

    applyApiFieldErrors(form, validationError({ 'additionalInfo.tags[0]': ['Etiqueta demasiado larga.'] }));

    expect(form.get('additionalInfo.tags')?.errors?.['api']).toBe('Etiqueta demasiado larga.');
  });

  it('ignora campos que no existen en el formulario', () => {
    expect(applyApiFieldErrors(createForm(), validationError({ unknown: ['x'] }))).toBeFalse();
  });
});
