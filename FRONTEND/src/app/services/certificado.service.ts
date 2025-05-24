import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Certificado } from '../modelos/certificado.interface';

@Injectable({
  providedIn: 'root'
})
export class CertificadoService {
  private apiUrl = `${environment.apiUrl}/certificados`;

  constructor(private http: HttpClient) {}

  obtenerCertificados(pagina: number = 1): Observable<Certificado[]> {
    return this.http.get<Certificado[]>(`${this.apiUrl}?pagina=${pagina}`);
  }

  obtenerCertificado(id: number): Observable<Certificado> {
    return this.http.get<Certificado>(`${this.apiUrl}/${id}`);
  }

  solicitarCertificado(formData: FormData): Observable<Certificado> {
    return this.http.post<Certificado>(this.apiUrl, formData);
  }

  aprobarCertificado(id: number): Observable<Certificado> {
    return this.http.post<Certificado>(`${this.apiUrl}/${id}/aprobar`, {});
  }

  rechazarCertificado(id: number, motivo: string): Observable<Certificado> {
    return this.http.post<Certificado>(`${this.apiUrl}/${id}/rechazar`, { motivo });
  }

  registrarPago(id: number, formData: FormData): Observable<Certificado> {
    return this.http.post<Certificado>(`${this.apiUrl}/${id}/pago`, formData);
  }

  descargarCertificado(id: number): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/${id}/descargar`, { responseType: 'blob' });
  }
} 