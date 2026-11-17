import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../../../core/config/api.config';
import { PagedResult } from '../../../shared/models/paged-result';
import { toHttpParams } from '../../../shared/utils/http-params';
import { CreateUserRequest, UpdateUserRequest, User, UserPhotoReference, UserQuery } from '../domain/models/user';

/** Acceso HTTP a `/api/users`. No guarda estado. */
@Injectable({ providedIn: 'root' })
export class UsersApiService {
  private readonly http = inject(HttpClient);
  private readonly url = `${inject(API_BASE_URL)}/users`;

  getUsers(query: UserQuery): Observable<PagedResult<User>> {
    return this.http.get<PagedResult<User>>(this.url, { params: toHttpParams(query) });
  }

  create(request: CreateUserRequest): Observable<User> {
    return this.http.post<User>(this.url, request);
  }

  update(userId: string, request: UpdateUserRequest): Observable<User> {
    return this.http.put<User>(`${this.url}/${userId}`, request);
  }

  uploadPhoto(userId: string, file: File): Observable<User> {
    const body = new FormData();
    body.append('file', file, file.name);
    return this.http.put<User>(`${this.url}/${userId}/photo`, body);
  }

  deletePhoto(userId: string): Observable<User> {
    return this.http.delete<User>(`${this.url}/${userId}/photo`);
  }

  photoUrl(user: UserPhotoReference | null | undefined): string | null {
    return user?.photoUpdatedAt ? `${this.url}/${user.id}/photo?v=${encodeURIComponent(user.photoUpdatedAt)}` : null;
  }
}
