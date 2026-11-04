import { Routes } from '@angular/router';

export const TASKS_ROUTES: Routes = [
  {
    path: '',
    title: 'Tareas · TaskManager',
    loadComponent: () =>
      import('./presentation/pages/task-list-page/task-list-page').then((m) => m.TaskListPage),
  },
];
