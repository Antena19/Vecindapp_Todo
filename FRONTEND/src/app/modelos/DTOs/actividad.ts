export class Actividad {
    id: number;
    titulo: string;
    descripcion: string;
    fecha: Date;
    icono: string;
    color: string;

    constructor(
        id?: number,
        titulo?: string,
        descripcion?: string,
        fecha?: Date,
        icono?: string,
        color?: string
    ) {
        this.id = id || 0;
        this.titulo = titulo || '';
        this.descripcion = descripcion || '';
        this.fecha = fecha || new Date();
        this.icono = icono || '';
        this.color = color || '';
    }
} 