import { provideZonelessChangeDetection } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { MAX_PAGE_SIZE, PagedResult } from '../../../shared/models/paged-result';
import { User, UserQuery } from '../domain/models/user';
import { UsersApiService } from '../infrastructure/users-api.service';
import { UsersDirectoryService } from './users-directory.service';

const user = (id: string): User => ({ id, name: `Usuario ${id}`, email: `${id}@acme.com`, createdAt: '', photoUpdatedAt: null });

function pageFor(query: UserQuery): PagedResult<User> {
  const pages = [[user('a'), user('b')], [user('c'), user('d')], [user('e')]];
  return {
    items: pages[query.page - 1],
    page: query.page,
    pageSize: query.pageSize,
    totalCount: 5,
    totalPages: 3,
    hasPreviousPage: query.page > 1,
    hasNextPage: query.page < 3,
  };
}

describe('UsersDirectoryService', () => {
  let api: jasmine.SpyObj<UsersApiService>;
  let directory: UsersDirectoryService;

  beforeEach(() => {
    api = jasmine.createSpyObj<UsersApiService>('UsersApiService', ['getUsers', 'create']);
    api.getUsers.and.callFake((query) => of(pageFor(query)));
    TestBed.configureTestingModule({
      providers: [provideZonelessChangeDetection(), { provide: UsersApiService, useValue: api }],
    });
    directory = TestBed.inject(UsersDirectoryService);
  });

  it('recorre todas las páginas de la API con el tamaño máximo', () => {
    directory.load();

    expect(api.getUsers.calls.allArgs().map(([query]) => query.page)).toEqual([1, 2, 3]);
    expect(api.getUsers.calls.argsFor(0)[0].pageSize).toBe(MAX_PAGE_SIZE);
    expect(directory.users().map((u) => u.id)).toEqual(['a', 'b', 'c', 'd', 'e']);
    expect(directory.findById('d')?.name).toBe('Usuario d');
  });

  it('no vuelve a cargar salvo que se pida', () => {
    directory.load();
    directory.load();
    expect(api.getUsers.calls.count()).toBe(3);

    directory.load(true);
    expect(api.getUsers.calls.count()).toBe(6);
  });
});
