export class EstadisticasSocios {
    totalSocios: number;
    solicitudesPendientes: number;
    sociosActivos: number;
    sociosInactivos: number;

    constructor(
        totalSocios?: number,
        solicitudesPendientes?: number,
        sociosActivos?: number,
        sociosInactivos?: number
    ) {
        this.totalSocios = totalSocios || 0;
        this.solicitudesPendientes = solicitudesPendientes || 0;
        this.sociosActivos = sociosActivos || 0;
        this.sociosInactivos = sociosInactivos || 0;
    }
} 