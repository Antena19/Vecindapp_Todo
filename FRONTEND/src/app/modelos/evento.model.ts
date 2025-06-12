export interface Evento {
  id: number;
  titulo: string;
  descripcion: string;
  fechaEvento: Date;
  horaEvento: string;
  lugar: string;
  directivaRut: number;
  estado: string;
  codigoQr: string;
  codigoNumerico?: string;
  fechaCreacion: Date;
  notas?: string;
  total_asistentes?: number;
}

export interface AsistenciaEvento {
  id: number;
  eventoId: number;
  usuarioRut: number;
  nombre: string;
  apellido: string;
  fechaAsistencia: Date;
}

export interface EventoReporte {
  evento: Evento;
  asistentes: AsistenciaEvento[];
  totalAsistentes: number;
} 