import { environment } from '../environments/environment';

export class Constantes {
  public static get API_URL(): string {
    return environment.apiUrl;
  }
}