import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CertificadosService } from '../../../modulos/certificados/certificados.service';
import { AlertController, LoadingController, IonicModule } from '@ionic/angular';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-confirmar-pago-simulado',
  standalone: true,
  imports: [CommonModule, IonicModule],
  template: `
    <ion-content>
      <div class="container">
        <div class="header">
          <h1>Confirmar Pago Simulado</h1>
          <p>Debido a problemas de conectividad con Transbank</p>
        </div>

        <div class="content" *ngIf="!loading">
          <div class="info-card">
            <h2>Información de la Solicitud</h2>
            <p><strong>Solicitud ID:</strong> {{ solicitudId }}</p>
            <p><strong>Estado:</strong> Pendiente de confirmación</p>
            <p><strong>Motivo:</strong> Problemas de conectividad con Transbank</p>
          </div>

          <div class="warning-card">
            <h3>⚠️ Importante</h3>
            <p>Esta confirmación es válida solo cuando no hay conectividad con Transbank.</p>
            <p>El certificado será generado automáticamente después de la confirmación.</p>
          </div>

          <div class="actions">
            <ion-button expand="block" color="success" (click)="confirmarPago()">
              ✅ Confirmar Pago Simulado
            </ion-button>
            <ion-button expand="block" color="medium" (click)="cancelar()">
              ❌ Cancelar
            </ion-button>
          </div>
        </div>

        <div class="loading" *ngIf="loading">
          <ion-spinner name="crescent"></ion-spinner>
          <p>Confirmando pago simulado...</p>
        </div>
      </div>
    </ion-content>
  `,
  styles: [`
    .container {
      padding: 20px;
      max-width: 600px;
      margin: 0 auto;
    }
    
    .header {
      text-align: center;
      margin-bottom: 30px;
    }
    
    .header h1 {
      color: #2c3e50;
      margin-bottom: 10px;
    }
    
    .header p {
      color: #7f8c8d;
      font-size: 16px;
    }
    
    .info-card, .warning-card {
      background: white;
      border-radius: 8px;
      padding: 20px;
      margin-bottom: 20px;
      box-shadow: 0 2px 10px rgba(0,0,0,0.1);
    }
    
    .info-card h2, .warning-card h3 {
      color: #2c3e50;
      margin-bottom: 15px;
    }
    
    .warning-card {
      background: #fff3cd;
      border: 1px solid #ffeaa7;
    }
    
    .warning-card h3 {
      color: #856404;
    }
    
    .actions {
      display: flex;
      flex-direction: column;
      gap: 15px;
    }
    
    .loading {
      text-align: center;
      padding: 40px;
    }
    
    .loading p {
      margin-top: 15px;
      color: #7f8c8d;
    }
  `]
})
export class ConfirmarPagoSimuladoComponent implements OnInit {
  solicitudId: number = 0;
  loading: boolean = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private certificadosService: CertificadosService,
    private alertController: AlertController,
    private loadingController: LoadingController
  ) {}

  ngOnInit() {
    this.solicitudId = Number(this.route.snapshot.paramMap.get('id'));
    if (!this.solicitudId) {
      this.mostrarError('ID de solicitud no válido');
      this.router.navigate(['/certificados']);
    }
  }

  async confirmarPago() {
    const alert = await this.alertController.create({
      header: 'Confirmar Pago Simulado',
      message: `¿Está seguro de que desea confirmar el pago simulado para la solicitud ${this.solicitudId}?`,
      buttons: [
        {
          text: 'Cancelar',
          role: 'cancel'
        },
        {
          text: 'Confirmar',
          handler: () => {
            this.procesarConfirmacion();
          }
        }
      ]
    });

    await alert.present();
  }

  async procesarConfirmacion() {
    this.loading = true;
    
    try {
      const response = await this.certificadosService.confirmarPagoSimulado(this.solicitudId).toPromise();
      
      const successAlert = await this.alertController.create({
        header: '✅ Pago Confirmado',
        message: `El pago simulado ha sido confirmado exitosamente.\n\nCertificado generado para la solicitud ${this.solicitudId}`,
        buttons: [
          {
            text: 'OK',
            handler: () => {
              this.router.navigate(['/certificados']);
            }
          }
        ]
      });
      
      await successAlert.present();
    } catch (error: any) {
      console.error('Error al confirmar pago simulado:', error);
      
      const errorMessage = error?.error?.mensaje || 'Error desconocido al confirmar el pago';
      
      const errorAlert = await this.alertController.create({
        header: '❌ Error',
        message: errorMessage,
        buttons: ['OK']
      });
      
      await errorAlert.present();
    } finally {
      this.loading = false;
    }
  }

  cancelar() {
    this.router.navigate(['/certificados']);
  }

  async mostrarError(mensaje: string) {
    const alert = await this.alertController.create({
      header: 'Error',
      message: mensaje,
      buttons: ['OK']
    });
    await alert.present();
  }
} 