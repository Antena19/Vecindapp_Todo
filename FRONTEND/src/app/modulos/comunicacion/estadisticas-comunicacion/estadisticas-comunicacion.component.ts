import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonicModule } from '@ionic/angular';
import { ComunicacionService } from '../../../services/comunicacion.service';
import { EstadisticasComunicacion } from '../../../modelos/comunicacion.model';
import { AlertController } from '@ionic/angular';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-estadisticas-comunicacion',
  templateUrl: './estadisticas-comunicacion.component.html',
  styleUrls: ['./estadisticas-comunicacion.component.scss'],
  standalone: true,
  imports: [CommonModule, IonicModule]
})
export class EstadisticasComunicacionComponent implements OnInit, OnDestroy {
  estadisticas: EstadisticasComunicacion | null = null;
  loading = false;
  private statsSubscription: Subscription | null = null;

  constructor(
    private comunicacionService: ComunicacionService,
    private alertController: AlertController
  ) { }

  ngOnInit() {
    this.cargarEstadisticas();
  }

  ngOnDestroy() {
    if (this.statsSubscription) {
      this.statsSubscription.unsubscribe();
    }
  }

  cargarEstadisticas(): Promise<void> {
    if (this.loading) {
      return Promise.resolve();
    }
    this.loading = true;
    
    return new Promise((resolve) => {
        this.statsSubscription = this.comunicacionService.getEstadisticasComunicacionTest().subscribe({
            next: (estadisticas) => {
              this.estadisticas = {
                NoticiasPublicadas: estadisticas.NoticiasPublicadas ?? (estadisticas as any).noticiasPublicadas ?? 0,
                TotalLecturas: estadisticas.TotalLecturas ?? (estadisticas as any).totalLecturas ?? 0,
                TotalComentarios: estadisticas.TotalComentarios ?? (estadisticas as any).totalComentarios ?? 0,
                TasaLectura: estadisticas.TasaLectura ?? (estadisticas as any).tasaLectura ?? 0
              };
              this.loading = false;
              resolve();
            },
            error: (error) => {
              console.error('Error al cargar estadísticas:', error);
              this.estadisticas = null;
              this.loading = false;
              this.mostrarError('Error al cargar las estadísticas');
              resolve();
            }
          });
    });
  }

  calcularPorcentaje(valor: number, total: number): number {
    if (total === 0) return 0;
    return Math.round((valor / total) * 100);
  }

  getColorPorTasa(tasa: number): string {
    if (tasa >= 80) return 'success';
    if (tasa >= 60) return 'warning';
    return 'danger';
  }

  async mostrarError(mensaje: string) {
    const alert = await this.alertController.create({
      header: 'Error',
      message: mensaje,
      buttons: ['OK']
    });
    await alert.present();
  }

  doRefresh(event: any) {
    this.cargarEstadisticas().then(() => {
      event.target.complete();
    });
  }
} 