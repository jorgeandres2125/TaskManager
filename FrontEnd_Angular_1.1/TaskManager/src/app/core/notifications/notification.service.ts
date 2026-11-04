import { Injectable, inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

const SUCCESS_DURATION_MS = 3000;
const ERROR_DURATION_MS = 6000;

/** Avisos al usuario. Los componentes dependen de este service, no directamente de MatSnackBar. */
@Injectable({ providedIn: 'root' })
export class NotificationService {
  private readonly snackBar = inject(MatSnackBar);

  success(message: string): void {
    this.open(message, 'notification-success', SUCCESS_DURATION_MS);
  }

  error(message: string): void {
    this.open(message, 'notification-error', ERROR_DURATION_MS);
  }

  private open(message: string, panelClass: string, duration: number): void {
    this.snackBar.open(message, 'Cerrar', {
      duration,
      panelClass,
      horizontalPosition: 'end',
      verticalPosition: 'bottom',
    });
  }
}
