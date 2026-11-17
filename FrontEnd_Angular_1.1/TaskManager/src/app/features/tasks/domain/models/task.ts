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

export type TaskFilters = Omit<TaskQuery, keyof PageQuery>;

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

export interface PatchTaskAdditionalInfoRequest {
  readonly priority?: TaskPriorityEnum;
  readonly clearPriority?: boolean;
  readonly estimatedCompletionDate?: string;
  readonly clearEstimatedCompletionDate?: boolean;
  readonly addTags?: readonly string[];
  readonly removeTags?: readonly string[];
  readonly setMetadata?: Readonly<Record<string, string>>;
  readonly removeMetadata?: readonly string[];
}

export interface TaskAdditionalInfoChange {
  readonly task: Task;
  readonly request: PatchTaskAdditionalInfoRequest;
}

export const TASK_TITLE_MAX_LENGTH = 200;
export const TASK_MAX_TAGS = 10;
export const TASK_TAG_MAX_LENGTH = 50;
export const TASK_MAX_METADATA_ENTRIES = 20;
export const TASK_METADATA_KEY_MAX_LENGTH = 50;
export const TASK_METADATA_VALUE_MAX_LENGTH = 500;
export const TASK_METADATA_KEY_PATTERN = /^[A-Za-z0-9_-]+$/;
