import { ChangeDetectionStrategy, Component } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

/**
 * Estructura común de todas las pantallas: barra lateral fija (280 px) con la navegación en pantallas grandes
 * y barra superior compacta en móvil y tableta. Solo enlaza las secciones que existen (Tareas y Usuarios).
 */
@Component({
  selector: 'app-shell',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, MatIconModule],
  templateUrl: './shell.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Shell {
  protected readonly links = [
    { path: '/tasks', label: 'Tareas', icon: 'view_kanban' },
    { path: '/users', label: 'Usuarios', icon: 'group' },
  ] as const;
}
