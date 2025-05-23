import { EstadisticasSocios } from './estadisticas-socios';
import { Actividad } from './actividad';

export class EstadisticasResponse {
    estadisticas: EstadisticasSocios;
    ultimasActividades: Actividad[];

    constructor(
        estadisticas?: EstadisticasSocios,
        ultimasActividades?: Actividad[]
    ) {
        this.estadisticas = estadisticas || new EstadisticasSocios();
        this.ultimasActividades = ultimasActividades || [];
    }
} 