import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { tagColorClass } from '../../task-display';

/**
 * Etiqueta de una tarea como píldora de color ("backend" → "Backend"); cada etiqueta tiene siempre su color.
 * Con `removable`, muestra un botón para quitarla.
 */
@Component({
  selector: 'app-tag-pill',
  imports: [MatIconModule],
  template: `
    <span class="inline-flex h-[22px] items-center rounded-full px-2 text-xs font-medium ring-1 ring-inset" [class]="colorClass()">
      {{ label() }}
      @if (removable()) {
        <button
          type="button"
          class="-mr-1 ml-1 inline-flex h-4 w-4 cursor-pointer items-center justify-center rounded-full opacity-70 transition-opacity hover:opacity-100 focus-visible:opacity-100 focus-visible:outline focus-visible:outline-2 focus-visible:outline-current disabled:cursor-default disabled:opacity-40"
          [disabled]="disabled()"
          [attr.aria-label]="'Quitar etiqueta ' + label()"
          [title]="'Quitar etiqueta ' + label()"
          (click)="$event.stopPropagation(); remove.emit(tag())"
        >
          <mat-icon class="!h-3.5 !w-3.5 !text-sm" aria-hidden="true">close</mat-icon>
        </button>
      }
    </span>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TagPill {
  readonly tag = input.required<string>();
  readonly removable = input(false);
  readonly disabled = input(false);

  readonly remove = output<string>();

  protected readonly label = computed(() => {
    const tag = this.tag().trim();
    return tag.charAt(0).toUpperCase() + tag.slice(1);
  });
  protected readonly colorClass = computed(() => tagColorClass(this.tag()));
}
