import { ChangeDetectionStrategy, Component, inject, input, output } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { ViewportService } from '../../../core/layout/viewport.service';
import { PAGE_SIZE_OPTIONS } from '../../models/paged-result';

export interface PageChange {
  readonly page: number;
  readonly pageSize: number;
}

/**
 * Contenedor común de las listas paginadas: barra de carga, error con reintento, mensaje de lista vacía
 * y paginador. La tabla o las tarjetas se proyectan como contenido.
 */
@Component({
  selector: 'app-paged-list-panel',
  imports: [MatIconModule, MatPaginatorModule, MatProgressBarModule],
  templateUrl: './paged-list-panel.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'block min-w-0' },
})
export class PagedListPanel {
  protected readonly viewport = inject(ViewportService);
  protected readonly pageSizeOptions = PAGE_SIZE_OPTIONS;
  protected readonly skeletonItems = [1, 2, 3, 4, 5, 6, 7, 8];

  readonly loading = input(false);
  readonly loaded = input(true);
  readonly failed = input(false);
  readonly empty = input(false);
  readonly errorMessage = input('No se pudo cargar la lista.');
  readonly emptyMessage = input('No hay elementos para mostrar.');
  readonly totalCount = input.required<number>();
  readonly page = input.required<number>();
  readonly pageSize = input.required<number>();

  readonly pageChange = output<PageChange>();
  readonly retry = output<void>();

  protected onPage(event: PageEvent): void {
    this.pageChange.emit({ page: event.pageIndex + 1, pageSize: event.pageSize });
  }
}
