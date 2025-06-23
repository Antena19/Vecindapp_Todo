import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { 
  Noticia, 
  Notificacion, 
  NotificacionUsuario, 
  ComentarioNoticia, 
  EstadisticasComunicacion 
} from '../modelos/comunicacion.model';

@Injectable({
  providedIn: 'root'
})
export class ComunicacionService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  // ===== NOTICIAS =====
  
  // Obtener todas las noticias (con filtros opcionales)
  getNoticias(filtros?: {
    categoria?: string;
    alcance?: string;
    estado?: string;
    autorRut?: number;
    fechaDesde?: Date;
    fechaHasta?: Date;
  }): Observable<Noticia[]> {
    let params = '';
    if (filtros) {
      const queryParams = new URLSearchParams();
      Object.keys(filtros).forEach(key => {
        const value = (filtros as any)[key];
        if (value !== undefined && value !== null) {
          queryParams.append(key, value.toString());
        }
      });
      params = '?' + queryParams.toString();
    }
    return this.http.get<Noticia[]>(`${this.apiUrl}/comunicacion/noticias${params}`);
  }

  // Obtener noticia por ID
  getNoticia(id: number): Observable<Noticia> {
    return this.http.get<Noticia>(`${this.apiUrl}/comunicacion/noticias/${id}`);
  }

  // Crear nueva noticia
  crearNoticia(noticia: Partial<Noticia>): Observable<Noticia> {
    return this.http.post<Noticia>(`${this.apiUrl}/comunicacion/noticias`, noticia);
  }

  // Actualizar noticia
  actualizarNoticia(id: number, noticia: Partial<Noticia>): Observable<Noticia> {
    return this.http.put<Noticia>(`${this.apiUrl}/comunicacion/noticias/${id}`, noticia);
  }

  // Eliminar noticia
  eliminarNoticia(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/comunicacion/noticias/${id}`);
  }

  // Publicar noticia (cambiar estado a ACTIVO)
  publicarNoticia(id: number): Observable<Noticia> {
    return this.http.patch<Noticia>(`${this.apiUrl}/comunicacion/noticias/${id}/publicar`, {});
  }

  // ===== NOTIFICACIONES =====

  // Obtener notificaciones del usuario
  getNotificacionesUsuario(rut: number): Observable<NotificacionUsuario[]> {
    return this.http.get<NotificacionUsuario[]>(`${this.apiUrl}/comunicacion/notificaciones/usuario/${rut}`);
  }

  // Marcar notificación como leída
  marcarNotificacionLeida(notificacionId: number, usuarioRut: number): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/comunicacion/notificaciones/${notificacionId}/leer`, {
      usuarioRut: usuarioRut
    });
  }

  // Enviar notificación push
  enviarNotificacionPush(notificacion: Partial<Notificacion>): Observable<Notificacion> {
    return this.http.post<Notificacion>(`${this.apiUrl}/comunicacion/notificaciones/push`, notificacion);
  }

  // Obtener estadísticas de notificaciones
  getEstadisticasNotificaciones(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/comunicacion/notificaciones/estadisticas`);
  }

  // ===== COMENTARIOS =====

  // Obtener comentarios de una noticia
  getComentariosNoticia(noticiaId: number): Observable<ComentarioNoticia[]> {
    return this.http.get<ComentarioNoticia[]>(`${this.apiUrl}/comunicacion/noticias/${noticiaId}/comentarios`);
  }

  // Agregar comentario a una noticia
  agregarComentario(noticiaId: number, comentario: Partial<ComentarioNoticia>): Observable<ComentarioNoticia> {
    return this.http.post<ComentarioNoticia>(`${this.apiUrl}/comunicacion/noticias/${noticiaId}/comentarios`, comentario);
  }

  // Eliminar comentario
  eliminarComentario(comentarioId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/comunicacion/comentarios/${comentarioId}`);
  }

  // ===== ESTADÍSTICAS =====

  // Obtener estadísticas de comunicación
  getEstadisticasComunicacion(): Observable<EstadisticasComunicacion> {
    return this.http.get<EstadisticasComunicacion>(`${this.apiUrl}/comunicacion/estadisticas`);
  }

  // ===== FUNCIONALIDADES ESPECIALES =====

  // Notificar aviso importante (automático)
  notificarAvisoImportante(noticiaId: number): Observable<Notificacion> {
    return this.http.post<Notificacion>(`${this.apiUrl}/comunicacion/noticias/${noticiaId}/notificar-importante`, {});
  }

  // Subir imagen para noticia
  subirImagenNoticia(noticiaId: number, file: File): Observable<{ imagenUrl: string }> {
    const formData = new FormData();
    formData.append('imagen', file);
    return this.http.post<{ imagenUrl: string }>(`${this.apiUrl}/comunicacion/noticias/${noticiaId}/imagen`, formData);
  }

  // Obtener noticias destacadas
  getNoticiasDestacadas(): Observable<Noticia[]> {
    return this.http.get<Noticia[]>(`${this.apiUrl}/comunicacion/noticias/destacadas`);
  }

  // Buscar noticias por texto
  buscarNoticias(texto: string): Observable<Noticia[]> {
    return this.http.get<Noticia[]>(`${this.apiUrl}/comunicacion/noticias/buscar?q=${encodeURIComponent(texto)}`);
  }
} 