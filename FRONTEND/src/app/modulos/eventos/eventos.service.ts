import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Evento } from '../../modelos/evento.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class EventosService {
  private apiUrl = `${environment.apiUrl}/api/eventos`;

  constructor(private http: HttpClient) {}

  obtenerEventos(): Observable<Evento[]> {
    return this.http.get<Evento[]>(this.apiUrl);
  }

  obtenerEvento(id: number): Observable<Evento> {
    return this.http.get<Evento>(`${this.apiUrl}/${id}`);
  }

  crearEvento(evento: Evento): Observable<Evento> {
    return this.http.post<Evento>(this.apiUrl, evento);
  }

  actualizarEvento(id: number, evento: Evento): Observable<Evento> {
    return this.http.put<Evento>(`${this.apiUrl}/${id}`, evento);
  }

  eliminarEvento(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
} 