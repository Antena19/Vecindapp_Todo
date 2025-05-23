export class Usuario {
  rut: number;
  dv_rut: string;
  nombre: string;
  apellido_paterno: string;
  apellido_materno: string;
  correo_electronico: string;
  telefono: string;
  direccion: string;
  password: string;
  fecha_registro: Date;
  estado: number;
  tipo_usuario: string;
  token_recuperacion?: string;
  fecha_token_recuperacion?: Date;

  constructor(
    rut?: number,
    dv_rut?: string,
    nombre?: string,
    apellido_paterno?: string,
    apellido_materno?: string,
    correo_electronico?: string,
    telefono?: string,
    direccion?: string,
    password?: string,
    fecha_registro?: Date,
    estado?: number,
    tipo_usuario?: string,
    token_recuperacion?: string,
    fecha_token_recuperacion?: Date
  ) {
    this.rut = rut || 0;
    this.dv_rut = dv_rut || '';
    this.nombre = nombre || '';
    this.apellido_paterno = apellido_paterno || '';
    this.apellido_materno = apellido_materno || '';
    this.correo_electronico = correo_electronico || '';
    this.telefono = telefono || '';
    this.direccion = direccion || '';
    this.password = password || '';
    this.fecha_registro = fecha_registro || new Date();
    this.estado = estado || 1;
    this.tipo_usuario = tipo_usuario || '';
    this.token_recuperacion = token_recuperacion;
    this.fecha_token_recuperacion = fecha_token_recuperacion;
  }
}