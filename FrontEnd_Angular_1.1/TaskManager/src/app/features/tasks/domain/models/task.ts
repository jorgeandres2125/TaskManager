import { SortDirectionEnum } from '../../../../shared/enums/sort-direction.enum';
import { PageQuery } from '../../../../shared/models/paged-result';
import { TaskPriorityEnum } from '../enums/task-priority.enum';
import { TaskStatusEnum } from '../enums/task-status.enum';

export interface TaskAdditionalInfo {
  readonly priority: TaskPriorityEnum | null;
  readonly estimatedCompletionDate: string | null;
  readonly tags: readonly string[] | null;
  readonly metadata: Readonly<Record<string, string>> | null;
}

export interface Task {
  readonly id: string;
  readonly number: number;
  readonly title: string;
  readonly status: TaskStatusEnum;
  readonly allowedTransitions: readonly TaskStatusEnum[];
  readonly userId: string;
  readonly userName: string;
  readonly createdAt: string;
  readonly updatedAt: string | null;
  readonly additionalInfo: TaskAdditionalInfo | null;
}

export interface TaskQuery extends PageQuery {
  readonly status: TaskStatusEnum | null;
  readonly priority: TaskPriorityEnum | null;
  readonly tag: string | null;
  readonly userId: string | null;
  readonly sortDirection: SortDirectionEnum;
}

export type TaskFilters = Omit<TaskQuery, keyof PageQuery | 'status'>;

export function canMoveTo(task: Task, status: TaskStatusEnum): boolean {
  return task.status === status || task.allowedTransitions.includes(status);
}

export interface CreateTaskRequest {
  readonly title: string;
  readonly userId: string;
  readonly additionalInfo: {
    readonly priority: TaskPriorityEnum | null;
    readonly estimatedCompletionDate: string | null;
    readonly tags: readonly string[];
    readonly metadata?: Readonly<Record<string, string>> | null;
  } | null;
}

export type UpdateTaskRequest = CreateTaskRequest;

export const TASK_TITLE_MAX_LENGTH = 200;
export const TASK_MAX_TAGS = 10;
export const TASK_TAG_MAX_LENGTH = 50;
