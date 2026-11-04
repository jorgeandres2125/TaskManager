import { PageQuery } from '../../../../shared/models/paged-result';

export interface User {
  readonly id: string;
  readonly name: string;
  readonly email: string;
  readonly createdAt: string;
  readonly photoUpdatedAt: string | null;
}

export interface UserQuery extends PageQuery {
  readonly search: string | null;
}

export type UserReference = Pick<User, 'id' | 'name'>;

export type UserPhotoReference = Pick<User, 'id' | 'photoUpdatedAt'>;

export interface CreateUserRequest {
  readonly name: string;
  readonly email: string;
}

export type UpdateUserRequest = CreateUserRequest;

export const USER_NAME_MAX_LENGTH = 100;
export const USER_EMAIL_MAX_LENGTH = 256;

export const USER_PHOTO_MAX_BYTES = 2 * 1024 * 1024;
export const USER_PHOTO_TYPES: readonly string[] = ['image/jpeg', 'image/png', 'image/webp'];
export const USER_PHOTO_ACCEPT = '.jpg,.jpeg,.png,.webp';

export type UserPhotoChange =
  | { readonly kind: 'keep' }
  | { readonly kind: 'replace'; readonly file: File }
  | { readonly kind: 'remove' };
