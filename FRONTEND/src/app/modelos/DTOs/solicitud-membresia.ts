export class SolicitudMembresia {
    rut: number;
    documentoIdentidad: File | null;
    documentoDomicilio: File | null;

    constructor(
        rut?: number,
        documentoIdentidad?: File | null,
        documentoDomicilio?: File | null
    ) {
        this.rut = rut || 0;
        this.documentoIdentidad = documentoIdentidad || null;
        this.documentoDomicilio = documentoDomicilio || null;
    }
} 