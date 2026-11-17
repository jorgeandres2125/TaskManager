import { TaskStatusEnum } from '../enums/task-status.enum';
import { Task, canMoveTo } from './task';

function taskWith(status: TaskStatusEnum, allowedTransitions: TaskStatusEnum[]): Task {
  return {
    id: 't1',
    number: 1,
    title: 'Tarea',
    status,
    allowedTransitions,
    userId: 'u1',
    userName: 'Ana',
    createdAt: '2026-11-03T12:00:00Z',
    updatedAt: null,
    additionalInfo: null,
  };
}

describe('canMoveTo', () => {
  const pending = taskWith(TaskStatusEnum.Pending, [TaskStatusEnum.InProgress]);

  it('permite las transiciones que indica la API', () => {
    expect(canMoveTo(pending, TaskStatusEnum.InProgress)).toBeTrue();
  });

  it('no permite Pending → Done', () => {
    expect(canMoveTo(pending, TaskStatusEnum.Done)).toBeFalse();
  });

  it('permite soltar la tarea en su propia columna', () => {
    expect(canMoveTo(pending, TaskStatusEnum.Pending)).toBeTrue();
  });
});
