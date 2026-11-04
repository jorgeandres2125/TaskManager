import { HttpErrorResponse } from '@angular/common/http';

export interface ApiError {
  readonly status: number;
  readonly title: string;
  readonly detail: string | null;
  readonly fieldErrors: Readonly<Record<string, readonly string[]>>;
  readonly traceId: string | null;
}

interface ProblemDetails {
  readonly title?: string;
  readonly detail?: string;
  readonly errors?: Record<string, string[]>;
  readonly traceId?: string;
}

const CONNECTION_ERROR: ApiError = {
  status: 0,
  title: 'No se pudo conectar con el servidor.',
  detail: 'Compruebe que la API esté en ejecución e intente de nuevo.',
  fieldErrors: {},
  traceId: null,
};

export function toApiError(error: HttpErrorResponse): ApiError {
  if (error.status === 0) {
    return CONNECTION_ERROR;
  }

  const problem: ProblemDetails = isProblemDetails(error.error) ? error.error : {};
  return {
    status: error.status,
    title: problem.title ?? 'Ocurrió un error inesperado.',
    detail: problem.detail ?? null,
    fieldErrors: problem.errors ?? {},
    traceId: problem.traceId ?? null,
  };
}

export function apiErrorMessage(error: ApiError): string {
  return error.detail ?? error.title;
}

function isProblemDetails(body: unknown): body is ProblemDetails {
  return typeof body === 'object' && body !== null && ('title' in body || 'errors' in body);
}
