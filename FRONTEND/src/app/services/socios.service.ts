import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { Socio } from '../modelos/Socio';
import { SolicitudMembresia } from '../modelos/DTOs/solicitud-membresia';
import { SolicitudSocioDTO } from '../modelos/DTOs/solicitud-socio.dto';
import { RechazoDTO } from '../modelos/DTOs/rechazo.dto';
import { EstadisticasResponse } from '../modelos/DTOs/estadisticas-response';
import { SocioActivoDTO } from '../modelos/DTOs/socio-activo.dto';

@Injectable({
    providedIn: 'root'
})
export class SociosService {
    private apiUrlSocios = environment.apiUrl + '/api/socios';
    private apiUrlDirectiva = environment.apiUrl + '/api/directiva';

    constructor(private http: HttpClient) { }

    // Métodos para la directiva
    obtenerEstadisticas(): Observable<EstadisticasResponse> {
        return this.http.get<EstadisticasResponse>(`${this.apiUrlDirectiva}/estadisticas`);
    }

    listarSociosActivos(): Observable<SocioActivoDTO[]> {
        return this.http.get<SocioActivoDTO[]>(`${this.apiUrlDirectiva}/socios/activos`);
    }

    listarTodosSocios(): Observable<SocioActivoDTO[]> {
        return this.http.get<SocioActivoDTO[]>(`${this.apiUrlDirectiva}/socios/todos`);
    }

    // Métodos para socios normales
    listarSocios(idSocio: number = -1): Observable<Socio[]> {
        let params = new HttpParams();
        if (idSocio !== -1) {
            params = params.set('idsocio', idSocio.toString());
        }
        return this.http.get<Socio[]>(this.apiUrlSocios, { params });
    }

    solicitarMembresia(solicitud: SolicitudMembresia): Observable<{ mensaje: string }> {
        const formData = new FormData();
        formData.append('rut', solicitud.rut.toString());
        if (solicitud.documentoIdentidad) {
            formData.append('documentoIdentidad', solicitud.documentoIdentidad);
        }
        if (solicitud.documentoDomicilio) {
            formData.append('documentoDomicilio', solicitud.documentoDomicilio);
        }
        return this.http.post<{ mensaje: string }>(`${this.apiUrlSocios}/solicitar`, formData);
    }

    consultarSolicitudes(estado?: string): Observable<SolicitudSocioDTO[]> {
        let params = new HttpParams();
        if (estado) {
            params = params.set('estadoSolicitud', estado);
        }
        return this.http.get<SolicitudSocioDTO[]>(`${this.apiUrlDirectiva}/solicitudes`, { params });
    }

    aprobarSolicitud(rut: number): Observable<{ mensaje: string }> {
        // Endpoint para la directiva
        return this.http.post<{ mensaje: string }>(`${this.apiUrlDirectiva}/solicitudes/${rut}/aprobar`, {});
    }

    rechazarSolicitud(rut: number, rechazo: RechazoDTO): Observable<{ mensaje: string }> {
        // Endpoint para la directiva
        return this.http.post<{ mensaje: string }>(`${this.apiUrlDirectiva}/solicitudes/${rut}/rechazar`, rechazo);
    }

    actualizarEstadoSocio(idSocio: number, estado: boolean, motivo?: string): Observable<{ mensaje: string }> {
        const body = {
            Estado: estado ? 1 : 0,
            Motivo: motivo || ''
        };
        // Endpoint para la directiva
        return this.http.put<{ mensaje: string }>(`${this.apiUrlDirectiva}/socios/${idSocio}/estado`, body);
    }

    consultarEstadoSolicitudSocio(): Observable<SolicitudSocioDTO> {
        return this.http.get<SolicitudSocioDTO>(`${this.apiUrlSocios}/solicitud/estado`);
    }
}
