import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { tagColorClass } from '../../task-display';

/** Etiqueta de una tarea como píldora de color ("backend" → "Backend"); cada etiqueta tiene siempre su color. */
@Component({
  selector: 'app-tag-pill',
  template: `
    <span class="inline-flex h-[22px] items-center rounded-full px-2 text-xs font-medium ring-1 ring-inset" [class]="colorClass()">
      {{ label() }}
    </span>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TagPill {
  readonly tag = input.required<string>();

  protected readonly label = computed(() => {
    const tag = this.tag().trim();
    return tag.charAt(0).toUpperCase() + tag.slice(1);
  });
  protected readonly colorClass = computed(() => tagColorClass(this.tag()));
}
