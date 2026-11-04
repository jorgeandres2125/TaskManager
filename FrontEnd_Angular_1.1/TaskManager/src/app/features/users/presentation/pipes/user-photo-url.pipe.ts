import { Pipe, PipeTransform, inject } from '@angular/core';
import { UserPhotoReference } from '../../domain/models/user';
import { UsersApiService } from '../../infrastructure/users-api.service';

@Pipe({ name: 'userPhotoUrl' })
export class UserPhotoUrlPipe implements PipeTransform {
  private readonly api = inject(UsersApiService);

  transform(user: UserPhotoReference | null | undefined): string | null {
    return this.api.photoUrl(user);
  }
}
