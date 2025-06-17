import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class CertificadosService {
  private apiUrl = '/api/Certificados';

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
    return this.http.get<any[]>(`${this.apiUrl}/historial`);
  }

  descargarCertificado(certificadoId: number) {
    window.open(`${this.apiUrl}/descargar/${certificadoId}`, '_blank');
  }

  iniciarPagoTransbank(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/pago/iniciar`, data);
  }
} 