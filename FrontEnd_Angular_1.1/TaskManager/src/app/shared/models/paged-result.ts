export interface PagedResult<T> {
  readonly items: readonly T[];
  readonly page: number;
  readonly pageSize: number;
  readonly totalCount: number;
  readonly totalPages: number;
  readonly hasPreviousPage: boolean;
  readonly hasNextPage: boolean;
}

export interface PageQuery {
  readonly page: number;
  readonly pageSize: number;
}

export const FIRST_PAGE = 1;
export const DEFAULT_PAGE_SIZE = 10;
export const MAX_PAGE_SIZE = 100;
export const PAGE_SIZE_OPTIONS: readonly number[] = [10, 20, 30];
