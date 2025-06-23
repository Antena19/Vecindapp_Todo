export interface Noticia {
  id: number;
  id_noticia?: number; // Alias para compatibilidad con el template
  titulo: string;
  contenido: string;
  resumen?: string; // Resumen de la noticia para mostrar en listas
  fechaPublicacion: Date;
  fechaCreacion: Date;
  autorRut: number;
  autorNombre: string;
  alcance: 'PUBLICO' | 'SOCIOS';
  prioridad: 'BAJA' | 'MEDIA' | 'ALTA';
  estado: 'ACTIVO' | 'INACTIVO';
  imagenUrl?: string;
  categoria: 'NOTICIA' | 'EVENTO' | 'AVISO';
  tags?: string[];
}

export interface Notificacion {
  id: number;
  titulo: string;
  mensaje: string;
  fechaEnvio: Date;
  fechaCreacion: Date;
  tipo: 'PUSH' | 'EMAIL' | 'SMS';
  estado: 'PENDIENTE' | 'ENVIADA' | 'FALLIDA';
  destinatarios: number[]; // Array de RUTs
  noticiaId?: number;
  prioridad: 'BAJA' | 'MEDIA' | 'ALTA';
  metadata?: any;
}

export interface NotificacionUsuario {
  id: number;
  notificacionId: number;
  usuarioRut: number;
  leida: boolean;
  fechaLectura?: Date;
  fechaRecepcion: Date;
  titulo: string;
  mensaje: string;
  tipo: 'PUSH' | 'EMAIL' | 'SMS';
  prioridad: 'BAJA' | 'MEDIA' | 'ALTA';
}

export interface ComentarioNoticia {
  id: number;
  noticiaId: number;
  usuarioRut: number;
  usuarioNombre: string;
  contenido: string;
  fechaCreacion: Date;
  estado: 'ACTIVO' | 'MODERADO' | 'ELIMINADO';
}

export interface EstadisticasComunicacion {
  totalNoticias: number;
  noticiasPublicas: number;
  noticiasSocios: number;
  notificacionesEnviadas: number;
  notificacionesPendientes: number;
  usuariosNotificados: number;
  tasaLectura: number;
} 