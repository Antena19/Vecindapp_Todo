export interface Certificado {
  id: number;
  usuarioId: number;
  tipo: 'RESIDENCIA';
  estado: 'PENDIENTE' | 'APROBADO' | 'RECHAZADO' | 'PAGADO' | 'EMITIDO';
  fechaSolicitud: Date;
  fechaAprobacion?: Date;
  fechaPago?: Date;
  fechaEmision?: Date;
  monto: number;
  montoDescuento?: number;
  montoFinal: number;
  documentos: Documento[];
  motivoRechazo?: string;
  certificadoUrl?: string;
}

export interface Documento {
  id: number;
  nombre: string;
  tipo: string;
  url: string;
  fechaSubida: Date;
}

export interface SolicitudCertificado {
  tipo: 'RESIDENCIA';
  documentos: File[];
  observaciones?: string;
}

export interface PagoCertificado {
  certificadoId: number;
  metodoPago: 'TRANSFERENCIA' | 'EFECTIVO';
  comprobante?: File;
  observaciones?: string;
} 