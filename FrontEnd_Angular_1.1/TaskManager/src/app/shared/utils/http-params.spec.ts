import { toHttpParams } from './http-params';

describe('toHttpParams', () => {
  it('incluye los valores con contenido y omite null, undefined y texto vacío', () => {
    const params = toHttpParams({ page: 2, status: 'Pending', tag: '', userId: null, priority: undefined });

    expect(params.keys()).toEqual(['page', 'status']);
    expect(params.get('page')).toBe('2');
    expect(params.get('status')).toBe('Pending');
  });
});
