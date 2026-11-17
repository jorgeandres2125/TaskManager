import { HttpErrorResponse } from '@angular/common/http';
import { apiErrorMessage, toApiError } from './api-error';

describe('toApiError', () => {
  it('convierte una respuesta ProblemDetails de validación', () => {
    const error = toApiError(
      new HttpErrorResponse({
        status: 400,
        error: {
          title: 'Uno o más datos no son válidos.',
          errors: { title: ["'Title' es obligatorio."] },
          traceId: 'abc',
        },
      }),
    );

    expect(error.status).toBe(400);
    expect(error.title).toBe('Uno o más datos no son válidos.');
    expect(error.fieldErrors['title']).toEqual(["'Title' es obligatorio."]);
    expect(error.traceId).toBe('abc');
  });

  it('usa el detalle como mensaje cuando existe', () => {
    const error = toApiError(
      new HttpErrorResponse({ status: 409, error: { title: 'Conflicto', detail: 'Ya existe un usuario.' } }),
    );

    expect(apiErrorMessage(error)).toBe('Ya existe un usuario.');
  });

  it('identifica la falta de conexión con el servidor (estado 0)', () => {
    const error = toApiError(new HttpErrorResponse({ status: 0 }));

    expect(error.status).toBe(0);
    expect(error.title).toContain('No se pudo conectar');
  });

  it('usa un título genérico si la respuesta no es ProblemDetails', () => {
    const error = toApiError(new HttpErrorResponse({ status: 502, error: '<html>Bad gateway</html>' }));

    expect(error.title).toBe('Ocurrió un error inesperado.');
    expect(error.fieldErrors).toEqual({});
  });
});
