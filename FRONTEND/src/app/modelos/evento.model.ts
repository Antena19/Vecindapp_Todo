export interface Evento {
  id: number;
  titulo: string;
  descripcion: string;
  fechaEvento: string;
  horaEvento: string;
  lugar: string;
  directivaRut: number;
  estado: string;
  codigoQr: string;
  fechaCreacion: string;
  notas?: string;
}

export interface AsistenciaEvento {
  id: number;
  evento_id: number;
  usuario_rut: number;
  fecha_asistencia: Date;
} 