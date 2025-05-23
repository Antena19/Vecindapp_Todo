export class Socio {
    idsocio: number;
    rut: number;
    dv_rut: string;
    nombre: string;
    apellido_paterno: string;
    apellido_materno: string;
    correo_electronico: string;
    telefono: string;
    direccion: string;
    fecha_registro: Date;
    fecha_solicitud: Date;
    fecha_aprobacion: Date;
    estado: boolean;
    estado_solicitud: string;
  
    constructor(
      idsocio?: number,
      rut?: number,
      dv_rut?: string,
      nombre?: string,
      apellido_paterno?: string,
      apellido_materno?: string,
      correo_electronico?: string,
      telefono?: string,
      direccion?: string,
      fecha_registro?: Date,
      fecha_solicitud?: Date,
      fecha_aprobacion?: Date,
      estado?: boolean,
      estado_solicitud?: string
    ) {
      this.idsocio = idsocio || 0;
      this.rut = rut || 0;
      this.dv_rut = dv_rut || '';
      this.nombre = nombre || '';
      this.apellido_paterno = apellido_paterno || '';
      this.apellido_materno = apellido_materno || '';
      this.correo_electronico = correo_electronico || '';
      this.telefono = telefono || '';
      this.direccion = direccion || '';
      this.fecha_registro = fecha_registro || new Date();
      this.fecha_solicitud = fecha_solicitud || new Date();
      this.fecha_aprobacion = fecha_aprobacion || new Date();
      this.estado = estado || false;
      this.estado_solicitud = estado_solicitud || '';
    }
}
