import { environment } from '../environments/environment';

<<<<<<< HEAD
    public static get API_URL(): string {
        return "http://localhost:8080";
       //return "https://vecindapptodo-production.up.railway.app";
    }
=======
export class Constantes {
  public static get API_URL(): string {
    return environment.apiUrl;
  }
>>>>>>> 923be5b832d9c0f0dce001bda4d98a01a94723c7
}