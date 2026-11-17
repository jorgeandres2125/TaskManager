import { TaskPriorityEnum } from '../domain/enums/task-priority.enum';
import { TaskStatusEnum } from '../domain/enums/task-status.enum';

export const TASK_STATUS_LABELS: Readonly<Record<TaskStatusEnum, string>> = {
  [TaskStatusEnum.Pending]: 'Pendiente',
  [TaskStatusEnum.InProgress]: 'En progreso',
  [TaskStatusEnum.Done]: 'Completada',
};

export const TASK_STATUS_DOT_CLASSES: Readonly<Record<TaskStatusEnum, string>> = {
  [TaskStatusEnum.Pending]: 'bg-slate-400',
  [TaskStatusEnum.InProgress]: 'bg-sky-400',
  [TaskStatusEnum.Done]: 'bg-emerald-400',
};

export const TASK_STATUS_COUNT_CLASSES: Readonly<Record<TaskStatusEnum, string>> = {
  [TaskStatusEnum.Pending]: 'bg-slate-400/12 text-slate-300 ring-slate-400/20',
  [TaskStatusEnum.InProgress]: 'bg-sky-400/12 text-sky-300 ring-sky-400/20',
  [TaskStatusEnum.Done]: 'bg-emerald-400/12 text-emerald-300 ring-emerald-400/20',
};

export const TASK_PRIORITY_LABELS: Readonly<Record<TaskPriorityEnum, string>> = {
  [TaskPriorityEnum.Low]: 'Baja',
  [TaskPriorityEnum.Medium]: 'Media',
  [TaskPriorityEnum.High]: 'Alta',
};

export const TASK_PRIORITY_ICONS: Readonly<Record<TaskPriorityEnum, string>> = {
  [TaskPriorityEnum.Low]: 'keyboard_arrow_down',
  [TaskPriorityEnum.Medium]: 'drag_handle',
  [TaskPriorityEnum.High]: 'keyboard_double_arrow_up',
};

export const TASK_PRIORITY_CLASSES: Readonly<Record<TaskPriorityEnum, string>> = {
  [TaskPriorityEnum.Low]: 'bg-slate-400/10 text-slate-300 ring-slate-400/20',
  [TaskPriorityEnum.Medium]: 'bg-amber-400/10 text-amber-300 ring-amber-400/25',
  [TaskPriorityEnum.High]: 'bg-red-400/10 text-red-300 ring-red-400/25',
};

export const TASK_PRIORITY_OPTIONS: readonly TaskPriorityEnum[] = Object.values(TaskPriorityEnum);

export const TAG_COLOR_CLASSES: readonly string[] = [
  'bg-sky-400/10 text-sky-300 ring-sky-400/20',
  'bg-violet-400/10 text-violet-300 ring-violet-400/20',
  'bg-emerald-400/10 text-emerald-300 ring-emerald-400/20',
  'bg-amber-400/10 text-amber-300 ring-amber-400/20',
  'bg-rose-400/10 text-rose-300 ring-rose-400/20',
  'bg-teal-400/10 text-teal-300 ring-teal-400/20',
  'bg-indigo-400/10 text-indigo-300 ring-indigo-400/20',
  'bg-fuchsia-400/10 text-fuchsia-300 ring-fuchsia-400/20',
];

export function tagColorClass(tag: string): string {
  let hash = 0;
  for (const char of tag.toLowerCase()) {
    hash = (hash * 31 + char.charCodeAt(0)) >>> 0;
  }
  return TAG_COLOR_CLASSES[hash % TAG_COLOR_CLASSES.length];
}
