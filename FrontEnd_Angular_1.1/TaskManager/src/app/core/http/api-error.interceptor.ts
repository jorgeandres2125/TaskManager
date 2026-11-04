import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { apiErrorMessage, toApiError } from '../errors/api-error';
import { NotificationService } from '../notifications/notification.service';

export const apiErrorInterceptor: HttpInterceptorFn = (request, next) => {
  const notifications = inject(NotificationService);

  return next(request).pipe(
    catchError((error: unknown) => {
      if (!(error instanceof HttpErrorResponse)) {
        return throwError(() => error);
      }

      const apiError = toApiError(error);
      notifications.error(apiErrorMessage(apiError));
      return throwError(() => apiError);
    }),
  );
};
