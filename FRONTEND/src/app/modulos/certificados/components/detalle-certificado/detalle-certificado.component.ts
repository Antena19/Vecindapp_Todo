import { Component, OnInit, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { IonicModule, AlertController, LoadingController } from '@ionic/angular';
import { CertificadoService } from '../../../../services/certificado.service';
import { Certificado } from '../../../../modelos/certificado.interface';
import { AuthService } from '../../../../services/auth.service';

@Component({
  selector: 'app-detalle-certificado',
  templateUrl: './detalle-certificado.component.html',
  styleUrls: ['./detalle-certificado.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    IonicModule
  ],
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class DetalleCertificadoComponent implements OnInit {
  certificado: Certificado | null = null;
  cargando = true;
  window = window; // Para acceder a window desde el template
  isDirectiva = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private certificadoService: CertificadoService,
    private alertController: AlertController,
    private loadingController: LoadingController,
    private authService: AuthService
  ) {}

  ngOnInit() {
    const id = this.route.snapshot.params['id'];
    this.cargarDetalleCertificado(id);
    this.verificarRolDirectiva();
  }

  private verificarRolDirectiva() {
    const usuario = this.authService.getUsuarioActual();
    this.isDirectiva = usuario?.rol === 'Directiva';
  }

  cargarDetalleCertificado(id: number) {
    const loading = this.loadingController.create({
      message: 'Cargando detalles...'
    });
    loading.then(loader => loader.present());

    this.certificadoService.obtenerCertificado(id).subscribe({
      next: (response: Certificado) => {
        this.certificado = response;
        loading.then(loader => loader.dismiss());
        this.cargando = false;
      },
      error: async (error: any) => {
        loading.then(loader => loader.dismiss());
        const alert = await this.alertController.create({
          header: 'Error',
          message: 'Error al cargar los detalles del certificado',
          buttons: ['OK']
        });
        await alert.present();
        this.cargando = false;
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

  async aprobarCertificado() {
    if (!this.certificado) return;

    const loading = await this.loadingController.create({
      message: 'Procesando...'
    });
    await loading.present();

    this.certificadoService.aprobarCertificado(this.certificado.id).subscribe({
      next: async (response: Certificado) => {
        await loading.dismiss();
        const alert = await this.alertController.create({
          header: 'Éxito',
          message: 'Certificado aprobado correctamente',
          buttons: ['OK']
        });
        await alert.present();
        this.cargarDetalleCertificado(this.certificado!.id);
      },
      error: async (error: any) => {
        await loading.dismiss();
        const alert = await this.alertController.create({
          header: 'Error',
          message: 'Error al aprobar el certificado',
          buttons: ['OK']
        });
        await alert.present();
      }
    });
  }

  async rechazarCertificado() {
    if (!this.certificado) return;

    const alert = await this.alertController.create({
      header: 'Rechazar Certificado',
      inputs: [
        {
          name: 'motivo',
          type: 'text',
          placeholder: 'Motivo del rechazo'
        }
      ],
      buttons: [
        {
          text: 'Cancelar',
          role: 'cancel'
        },
        {
          text: 'Rechazar',
          handler: (data) => {
            if (data.motivo) {
              this.procesarRechazo(data.motivo);
            }
          }
        }
      ]
    });

    await alert.present();
  }

  private async procesarRechazo(motivo: string) {
    if (!this.certificado) return;

    const loading = await this.loadingController.create({
      message: 'Procesando...'
    });
    await loading.present();

    this.certificadoService.rechazarCertificado(this.certificado.id, motivo).subscribe({
      next: async (response: Certificado) => {
        await loading.dismiss();
        const alert = await this.alertController.create({
          header: 'Éxito',
          message: 'Certificado rechazado correctamente',
          buttons: ['OK']
        });
        await alert.present();
        this.cargarDetalleCertificado(this.certificado!.id);
      },
      error: async (error: any) => {
        await loading.dismiss();
        const alert = await this.alertController.create({
          header: 'Error',
          message: 'Error al rechazar el certificado',
          buttons: ['OK']
        });
        await alert.present();
      }
    });
  }

  realizarPago() {
    if (!this.certificado) return;
    this.router.navigate(['/certificados/pago', this.certificado.id]);
  }

  descargarCertificado() {
    if (!this.certificado) return;
    this.certificadoService.descargarCertificado(this.certificado.id).subscribe({
      next: (blob: Blob) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `certificado-${this.certificado!.id}.pdf`;
        link.click();
        window.URL.revokeObjectURL(url);
      },
      error: async (error: any) => {
        const alert = await this.alertController.create({
          header: 'Error',
          message: 'Error al descargar el certificado',
          buttons: ['OK']
        });
        await alert.present();
      }
    });
  }
} 