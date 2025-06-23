import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonicModule } from '@ionic/angular';
import { ComunicacionService } from '../../../services/comunicacion.service';
import { EstadisticasComunicacion } from '../../../modelos/comunicacion.model';
import { AlertController, LoadingController } from '@ionic/angular';

@Component({
  selector: 'app-estadisticas-comunicacion',
  templateUrl: './estadisticas-comunicacion.component.html',
  styleUrls: ['./estadisticas-comunicacion.component.scss'],
  standalone: true,
  imports: [CommonModule, IonicModule]
})
export class EstadisticasComunicacionComponent implements OnInit {
  estadisticas: EstadisticasComunicacion | null = null;
  loading = false;

  constructor(
    private comunicacionService: ComunicacionService,
    private alertController: AlertController,
    private loadingController: LoadingController
  ) { }

  ngOnInit() {
    this.cargarEstadisticas();
  }

  async cargarEstadisticas() {
    this.loading = true;
    const loading = await this.loadingController.create({
      message: 'Cargando estadísticas...'
    });
    await loading.present();

    this.comunicacionService.getEstadisticasComunicacion().subscribe({
      next: (estadisticas) => {
        this.estadisticas = estadisticas;
        this.loading = false;
        loading.dismiss();
      },
      error: (error) => {
        console.error('Error al cargar estadísticas:', error);
        this.loading = false;
        loading.dismiss();
        this.mostrarError('Error al cargar las estadísticas');
      }
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