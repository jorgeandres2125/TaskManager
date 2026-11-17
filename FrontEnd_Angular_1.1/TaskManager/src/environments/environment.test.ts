import { EnvironmentNameEnum } from './environment-name.enum';
import { Environment } from './environment.model';

export const environment: Environment = {
  name: EnvironmentNameEnum.Test,
  production: false,
  apiBaseUrl: 'http://localhost:15200/api',
};
