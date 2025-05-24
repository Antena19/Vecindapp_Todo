import { Component, OnInit, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { Router } from '@angular/router';
import { AlertController, LoadingController, IonicModule } from '@ionic/angular';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { CertificadoService } from '../../../../services/certificado.service';
import { Certificado } from '../../../../modelos/certificado.interface';

@Component({
  selector: 'app-lista-certificados',
  templateUrl: './lista-certificados.component.html',
  styleUrls: ['./lista-certificados.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    IonicModule
  ],
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class ListaCertificadosComponent implements OnInit {
  certificados: Certificado[] = [];
  isDirectiva: boolean = false;
  cargando = true;

  constructor(
    private certificadoService: CertificadoService,
    private router: Router,
    private alertController: AlertController,
    private loadingController: LoadingController
  ) { }

  ngOnInit() {
    this.cargarCertificados();
    // TODO: Verificar si el usuario es directiva
  }

  cargarCertificados() {
    const loading = this.loadingController.create({
      message: 'Cargando certificados...'
    });
    loading.then(loader => loader.present());

    this.certificadoService.obtenerCertificados().subscribe({
      next: (response: Certificado[]) => {
        this.certificados = response;
        this.cargando = false;
        loading.then(loader => loader.dismiss());
      },
      error: async (error: any) => {
        console.error('Error al cargar certificados:', error);
        this.cargando = false;
        loading.then(loader => loader.dismiss());
        const alert = await this.alertController.create({
          header: 'Error',
          message: 'Error al cargar los certificados',
          buttons: ['OK']
        });
        await alert.present();
      }
    });
  }

  verDetalle(id: number) {
    this.router.navigate(['/certificados/detalle', id]);
  }

  realizarPago(id: number) {
    this.router.navigate(['/certificados/pago', id]);
  }

  descargarCertificado(id: number) {
    const loading = this.loadingController.create({
      message: 'Descargando certificado...'
    });
    loading.then(loader => loader.present());

    this.certificadoService.descargarCertificado(id).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `certificado-${id}.pdf`;
        link.click();
        window.URL.revokeObjectURL(url);
        loading.then(loader => loader.dismiss());
      },
      error: async (error) => {
        loading.then(loader => loader.dismiss());
        const alert = await this.alertController.create({
          header: 'Error',
          message: 'Error al descargar el certificado',
          buttons: ['OK']
        });
        await alert.present();
      }
    });
  }

  getEstadoColor(estado: string): string {
    switch (estado) {
      case 'PENDIENTE':
        return 'warning';
      case 'APROBADO':
        return 'success';
      case 'RECHAZADO':
        return 'danger';
      case 'PAGADO':
        return 'primary';
      case 'EMITIDO':
        return 'secondary';
      default:
        return 'medium';
    }
  }
} 