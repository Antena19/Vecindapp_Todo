export class SocioActivoDTO {
    idSocio: number;          // Número de socio
    rut: number;
    dvRut: string;
    nombreCompleto: string;
    correo: string;
    telefono: string;
    direccion: string;
    fechaRegistro: Date;
    fechaAprobacion?: Date;
    activo: boolean;
    motivoDesactivacion?: string;

    constructor(
        idSocio?: number,
        rut?: number,
        dvRut?: string,
        nombreCompleto?: string,
        correo?: string,
        telefono?: string,
        direccion?: string,
        fechaRegistro?: Date,
        fechaAprobacion?: Date,
        activo?: boolean,
        motivoDesactivacion?: string
    ) {
        this.idSocio = idSocio || 0;
        this.rut = rut || 0;
        this.dvRut = dvRut || '';
        this.nombreCompleto = nombreCompleto || '';
        this.correo = correo || '';
        this.telefono = telefono || '';
        this.direccion = direccion || '';
        this.fechaRegistro = fechaRegistro || new Date();
        this.fechaAprobacion = fechaAprobacion;
        this.activo = activo ?? true;
        this.motivoDesactivacion = motivoDesactivacion;
    }
} 