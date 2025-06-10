import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, forkJoin } from 'rxjs';
import { Evento, AsistenciaEvento, EventoReporte } from '../modelos/evento.model';
import { environment } from '../../environments/environment';
import { map } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class EventosService {
  private apiUrl = `${environment.apiUrl}/api/Eventos`;

  constructor(private http: HttpClient) { }

  getEventos(): Observable<Evento[]> {
    return this.http.get<Evento[]>(this.apiUrl);
  }

  getEvento(id: number): Observable<Evento> {
    return this.http.get<{ evento: Evento }>(`${this.apiUrl}/${id}`)
      .pipe(map(res => res.evento));
  }

  crearEvento(evento: Omit<Evento, 'id' | 'fecha_creacion' | 'codigo_qr'>): Observable<Evento> {
    return this.http.post<Evento>(this.apiUrl, evento);
  }

  actualizarEvento(id: number, evento: Partial<Evento>): Observable<Evento> {
    return this.http.put<Evento>(`${this.apiUrl}/${id}`, evento);
  }

  eliminarEvento(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  cancelarEvento(id: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/${id}/cancelar`, {});
  }

  registrarAsistencia(codigoQr: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/asistencia`, { codigoQr });
  }

  obtenerAsistentes(eventoId: number): Observable<AsistenciaEvento[]> {
    return this.http.get<{ mensaje: string, asistentes: AsistenciaEvento[] }>(`${this.apiUrl}/${eventoId}/asistentes`)
      .pipe(map(response => response.asistentes));
  }

  obtenerHistorialAsistencia(): Observable<AsistenciaEvento[]> {
    return this.http.get<AsistenciaEvento[]>(`${this.apiUrl}/historial`);
  }

  generarReporteAsistencia(fechaInicio: Date, fechaFin: Date): Observable<Evento[]> {
    return this.http.get<Evento[]>(`${this.apiUrl}/reporte`, {
      params: {
        fechaInicio: fechaInicio.toISOString(),
        fechaFin: fechaFin.toISOString()
      }
    });
  }

  // Nuevo método para generar reporte de asistencia por un ID de evento específico
  generarReporteAsistenciaPorEventoId(eventoId: number): Observable<EventoReporte> {
    // Combina la llamada para obtener el evento y la llamada para obtener sus asistentes
    return forkJoin({
      evento: this.getEvento(eventoId),
      asistentes: this.obtenerAsistentes(eventoId)
    }).pipe(
      map(results => ({
        evento: results.evento,
        asistentes: results.asistentes,
        totalAsistentes: results.asistentes.length
      }))
    );
  }
} 