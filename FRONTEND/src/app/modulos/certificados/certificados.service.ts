import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { tap, catchError } from 'rxjs/operators';
import { throwError } from 'rxjs';
import { environment } from 'src/environments/environment';

@Injectable({ providedIn: 'root' })
export class CertificadosService {
  private apiUrl = `${environment.apiUrl}/Certificados`;

  constructor(private http: HttpClient) {}

  solicitarCertificado(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/solicitar`, data);
  }

  obtenerSolicitudesPendientes(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/pendientes`);
  }

  aprobarSolicitud(id: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/aprobar`, { SolicitudId: id });
  }

  rechazarSolicitud(id: number, motivo: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/rechazar`, { SolicitudId: id, MotivoRechazo: motivo });
  }

  obtenerMisSolicitudes(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/mis-solicitudes`);
  }

  pagarCertificado(id: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/pagar/${id}`, {});
  }

  obtenerHistorial(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/mis-solicitudes`);
  }

  descargarCertificado(certificadoId: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/descargar/${certificadoId}`).pipe(
      tap(response => console.log('Respuesta de descarga:', response)),
      catchError(error => {
        console.error('Error al descargar certificado:', error);
        return throwError(() => error);
      })
    );
  }

  iniciarPagoTransbank(data: any): Observable<any> {
    console.log('Iniciando pago con datos:', data);
    return this.http.post(`${this.apiUrl}/pago/iniciar`, data).pipe(
      tap(response => console.log('Respuesta del servidor:', response)),
      catchError(error => {
        console.error('Error en el servicio de pago:', error);
        return throwError(() => error);
      })
    );
  }

  confirmarPago(token: string): Observable<any> {
    console.log('Confirmando pago con token:', token);
    return this.http.post(`${this.apiUrl}/pago/confirmar`, { Token: token }).pipe(
      tap(response => console.log('Respuesta de confirmación:', response)),
      catchError(error => {
        console.error('Error al confirmar pago:', error);
        return throwError(() => error);
      })
    );
  }

  obtenerEstadoConectividad(): Observable<any> {
    return this.http.get(`${this.apiUrl}/estado-conectividad`).pipe(
      tap(response => console.log('Estado de conectividad:', response)),
      catchError(error => {
        console.error('Error al obtener estado de conectividad:', error);
        return throwError(() => error);
      })
    );
  }

  aprobarCertificadoSinPago(solicitudId: number, motivo: string, observaciones: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/aprobar-sin-pago`, {
      SolicitudId: solicitudId,
      Motivo: motivo,
      Observaciones: observaciones
    }).pipe(
      tap(response => console.log('Respuesta de aprobación sin pago:', response)),
      catchError(error => {
        console.error('Error al aprobar certificado sin pago:', error);
        return throwError(() => error);
      })
    );
  }

  confirmarPagoSimulado(solicitudId: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/pago/confirmar-simulado`, {
      SolicitudId: solicitudId
    }).pipe(
      tap(response => console.log('Respuesta de confirmación de pago simulado:', response)),
      catchError(error => {
        console.error('Error al confirmar pago simulado:', error);
        return throwError(() => error);
      })
    );
  }
} 