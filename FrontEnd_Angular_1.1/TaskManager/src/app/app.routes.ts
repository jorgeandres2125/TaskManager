import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'tasks' },
  { path: 'tasks', loadChildren: () => import('./features/tasks/tasks.routes').then((m) => m.TASKS_ROUTES) },
  { path: 'users', loadChildren: () => import('./features/users/users.routes').then((m) => m.USERS_ROUTES) },
  { path: '**', redirectTo: 'tasks' },
];
