export class EstadisticasSocios {
    totalSocios: number;
    solicitudesPendientes: number;
    sociosActivos: number;
    sociosInactivos: number;
    totalVecinos: number;

    constructor(
        totalSocios?: number,
        solicitudesPendientes?: number,
        sociosActivos?: number,
        sociosInactivos?: number,
        totalVecinos?: number
    ) {
        this.totalSocios = totalSocios || 0;
        this.solicitudesPendientes = solicitudesPendientes || 0;
        this.sociosActivos = sociosActivos || 0;
        this.sociosInactivos = sociosInactivos || 0;
        this.totalVecinos = totalVecinos || 0;
    }
} 