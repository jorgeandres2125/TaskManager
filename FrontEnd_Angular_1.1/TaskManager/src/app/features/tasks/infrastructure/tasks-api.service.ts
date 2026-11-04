import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../../../core/config/api.config';
import { PagedResult } from '../../../shared/models/paged-result';
import { toHttpParams } from '../../../shared/utils/http-params';
import { TaskStatusEnum } from '../domain/enums/task-status.enum';
import { CreateTaskRequest, PatchTaskAdditionalInfoRequest, Task, TaskQuery, UpdateTaskRequest } from '../domain/models/task';

/** Acceso HTTP a `/api/tasks`. No guarda estado. */
@Injectable({ providedIn: 'root' })
export class TasksApiService {
  private readonly http = inject(HttpClient);
  private readonly url = `${inject(API_BASE_URL)}/tasks`;

  getTasks(query: TaskQuery): Observable<PagedResult<Task>> {
    return this.http.get<PagedResult<Task>>(this.url, { params: toHttpParams(query) });
  }

  create(request: CreateTaskRequest): Observable<Task> {
    return this.http.post<Task>(this.url, request);
  }

  update(taskId: string, request: UpdateTaskRequest): Observable<Task> {
    return this.http.put<Task>(`${this.url}/${taskId}`, request);
  }

  patchAdditionalInfo(taskId: string, request: PatchTaskAdditionalInfoRequest): Observable<Task> {
    return this.http.patch<Task>(`${this.url}/${taskId}/additional-info`, request);
  }

  changeStatus(taskId: string, status: TaskStatusEnum): Observable<Task> {
    return this.http.put<Task>(`${this.url}/${taskId}/status`, { status });
  }
}
