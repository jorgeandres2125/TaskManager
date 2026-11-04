import { ChangeDetectionStrategy, Component, ElementRef, forwardRef, input, signal, viewChild } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';

const SIZE_CLASSES = {
  lg: 'h-12 rounded-lg pl-11 pr-11 text-[0.9375rem]',
  sm: 'h-9 rounded-md pl-9 pr-9 text-sm',
} as const;

/**
 * Campo de búsqueda al estilo Raycast: icono integrado, botón para borrar y Esc para vaciarlo.
 * Es un control de formulario (ControlValueAccessor), así que se usa con `[formControl]` como un input normal.
 */
@Component({
  selector: 'app-search-field',
  imports: [MatIconModule],
  providers: [{ provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => SearchField), multi: true }],
  template: `
    <div class="group relative">
      <mat-icon
        class="pointer-events-none absolute top-1/2 -translate-y-1/2 !text-fg-subtle transition-colors duration-150 group-focus-within:!text-accent-text"
        [class]="size() === 'lg' ? 'left-3.5 !h-5 !w-5 !text-xl' : 'left-2.5 !h-[18px] !w-[18px] !text-lg'"
        aria-hidden="true"
        >search</mat-icon
      >
      <input
        #input
        type="search"
        autocomplete="off"
        spellcheck="false"
        class="w-full border border-line bg-surface text-fg shadow-low outline-none transition-[border-color,box-shadow,background-color] duration-150 placeholder:text-fg-subtle hover:border-line-strong focus:border-accent-text/70 focus:bg-panel focus:shadow-[0_0_0_4px_rgb(59_99_245/0.18)] disabled:opacity-50 [&::-webkit-search-cancel-button]:hidden"
        [class]="sizeClasses[size()]"
        [placeholder]="placeholder()"
        [attr.aria-label]="label()"
        [value]="value()"
        [disabled]="disabled()"
        (input)="onInput($event)"
        (blur)="onTouched()"
        (keydown.escape)="clear()"
      />
      @if (value()) {
        <button
          type="button"
          class="absolute top-1/2 flex -translate-y-1/2 items-center justify-center rounded-sm text-fg-subtle transition-colors duration-150 hover:bg-hover hover:text-fg"
          [class]="size() === 'lg' ? 'right-2.5 h-7 w-7' : 'right-1.5 h-6 w-6'"
          aria-label="Borrar búsqueda"
          (click)="clear(); input.focus()"
        >
          <mat-icon class="!h-[18px] !w-[18px] !text-lg">close</mat-icon>
        </button>
      }
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'block' },
})
export class SearchField implements ControlValueAccessor {
  readonly placeholder = input('Buscar…');
  readonly label = input.required<string>();
  readonly size = input<keyof typeof SIZE_CLASSES>('lg');

  protected readonly sizeClasses = SIZE_CLASSES;
  protected readonly value = signal('');
  protected readonly disabled = signal(false);
  private readonly inputElement = viewChild.required<ElementRef<HTMLInputElement>>('input');

  private onChange: (value: string) => void = () => undefined;
  protected onTouched: () => void = () => undefined;

  writeValue(value: string | null): void {
    this.value.set(value ?? '');
  }

  registerOnChange(fn: (value: string) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(disabled: boolean): void {
    this.disabled.set(disabled);
  }

  focus(): void {
    this.inputElement().nativeElement.focus();
  }

  protected onInput(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.value.set(value);
    this.onChange(value);
  }

  protected clear(): void {
    if (this.value()) {
      this.value.set('');
      this.onChange('');
    }
  }
}
