import { EnvironmentNameEnum } from './environment-name.enum';

export interface Environment {
  readonly name: EnvironmentNameEnum;
  readonly production: boolean;
  readonly apiBaseUrl: string;
}
