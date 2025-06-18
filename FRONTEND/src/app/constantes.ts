import { environment } from '../environments/environment';

export class Constantes {
    public static get API_URL(): string {
        //return "http://localhost:8080";
       //return "https://vecindapptodo-production.up.railway.app";
       return environment.apiUrl;
    }
}