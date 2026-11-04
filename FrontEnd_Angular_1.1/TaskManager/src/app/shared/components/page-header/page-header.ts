import { ChangeDetectionStrategy, Component, input } from '@angular/core';

export interface PageStat {
  readonly value: number | string;
  readonly label: string;
  readonly dotClass?: string;
}

/**
 * Encabezado común de las páginas: título, subtítulo contextual, cifras rápidas y acciones.
 * Las acciones se proyectan con el atributo `pageAction`; en móvil ocupan todo el ancho.
 */
@Component({
  selector: 'app-page-header',
  template: `
    <header class="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
      <div class="min-w-0">
        <h1 class="text-[1.625rem] leading-tight font-semibold tracking-tight text-fg">{{ title() }}</h1>
        @if (subtitle()) {
          <p class="mt-1 text-sm text-fg-muted">{{ subtitle() }}</p>
        }
        @if (stats().length) {
          <dl class="mt-3 flex flex-wrap items-center gap-x-5 gap-y-2 text-sm" aria-label="Resumen">
            @for (stat of stats(); track stat.label) {
              <div class="flex items-center gap-2">
                @if (stat.dotClass) {
                  <span class="h-2 w-2 rounded-full" [class]="stat.dotClass" aria-hidden="true"></span>
                }
                <dt class="sr-only">{{ stat.label }}</dt>
                <dd class="flex items-baseline gap-1.5">
                  <span class="font-semibold tabular-nums text-fg">{{ stat.value }}</span>
                  <span class="text-fg-muted" aria-hidden="true">{{ stat.label }}</span>
                </dd>
              </div>
            }
          </dl>
        }
      </div>
      <div class="flex shrink-0 flex-col gap-2 sm:flex-row sm:items-center">
        <ng-content select="[pageAction]" />
      </div>
    </header>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PageHeader {
  readonly title = input.required<string>();
  readonly subtitle = input<string | null>(null);
  readonly stats = input<readonly PageStat[]>([]);
}
