export class SolicitudSocioDTO {
    rut: number;
    nombre: string;
    apellidoPaterno: string;
    apellidoMaterno: string;
    fechaSolicitud: Date;
    estadoSolicitud: string;
    motivoRechazo: string;
    DocumentoIdentidad?: string;
    DocumentoDomicilio?: string;

    constructor(
        rut?: number,
        nombre?: string,
        apellidoPaterno?: string,
        apellidoMaterno?: string,
        fechaSolicitud?: Date,
        estadoSolicitud?: string,
        motivoRechazo?: string,
        DocumentoIdentidad?: string,
        DocumentoDomicilio?: string
    ) {
        this.rut = rut || 0;
        this.nombre = nombre || '';
        this.apellidoPaterno = apellidoPaterno || '';
        this.apellidoMaterno = apellidoMaterno || '';
        this.fechaSolicitud = fechaSolicitud || new Date();
        this.estadoSolicitud = estadoSolicitud || '';
        this.motivoRechazo = motivoRechazo || '';
        this.DocumentoIdentidad = DocumentoIdentidad || '';
        this.DocumentoDomicilio = DocumentoDomicilio || '';
    }
} 