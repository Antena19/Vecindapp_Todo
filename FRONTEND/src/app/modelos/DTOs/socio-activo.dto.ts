export class SocioActivoDTO {
    idSocio: number;          // Número de socio
    num_socio: number;        // Añadida la propiedad num_socio
    rut: number;
    dvRut: string;
    nombreCompleto: string;
    correo: string;
    telefono: string;
    direccion: string;
    fechaRegistro: Date;
    fechaAprobacion?: Date;
    estado: boolean;
    motivoDesactivacion?: string;

    constructor(
        idSocio?: number,
        num_socio?: number, // Añadido al constructor
        rut?: number,
        dvRut?: string,
        nombreCompleto?: string,
        correo?: string,
        telefono?: string,
        direccion?: string,
        fechaRegistro?: Date,
        fechaAprobacion?: Date,
        estado?: boolean | number,
        motivoDesactivacion?: string
    ) {
        this.idSocio = idSocio || 0;
        this.num_socio = num_socio || 0; // Inicialización
        this.rut = rut || 0;
        this.dvRut = dvRut || '';
        this.nombreCompleto = nombreCompleto || '';
        this.correo = correo || '';
        this.telefono = telefono || '';
        this.direccion = direccion || '';
        this.fechaRegistro = fechaRegistro || new Date();
        this.fechaAprobacion = fechaAprobacion;
        this.estado = (typeof estado === 'number') ? (estado === 1) : (estado ?? true);
        this.motivoDesactivacion = motivoDesactivacion;
    }
} 