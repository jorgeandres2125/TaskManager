import { BreakpointObserver } from '@angular/cdk/layout';
import { Injectable, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { map } from 'rxjs';

const COMPACT_MEDIA_QUERY = '(max-width: 1023.98px)';

/**
 * Único punto donde se decide el diseño según el tamaño de pantalla. En modo compacto,
 * las listas se muestran como tarjetas en lugar de tablas.
 */
@Injectable({ providedIn: 'root' })
export class ViewportService {
  readonly isCompact = toSignal(
    inject(BreakpointObserver)
      .observe(COMPACT_MEDIA_QUERY)
      .pipe(map((state) => state.matches)),
    { initialValue: false },
  );
}
