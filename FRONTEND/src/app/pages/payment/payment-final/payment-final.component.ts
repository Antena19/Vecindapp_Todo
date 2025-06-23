import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CertificadosService } from '../../../modulos/certificados/certificados.service';
import { AlertController, LoadingController, IonicModule } from '@ionic/angular';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-payment-final',
  templateUrl: './payment-final.component.html',
  styleUrls: ['./payment-final.component.scss'],
  standalone: true,
  imports: [CommonModule, IonicModule]
})
export class PaymentFinalComponent implements OnInit {
  token: string = '';
  loading: boolean = true;
  error: string = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private certificadosService: CertificadosService,
    private loadingCtrl: LoadingController,
    private alertCtrl: AlertController
  ) { }

  ngOnInit() {
    this.token = this.route.snapshot.queryParamMap.get('token_ws') || '';
    
    if (this.token) {
      this.confirmarPago();
    } else {
      this.mostrarError('No se recibió token de pago');
    }
  }

  async confirmarPago() {
    const loading = await this.loadingCtrl.create({
      message: 'Confirmando pago...',
      spinner: 'crescent'
    });
    await loading.present();

    try {
      this.certificadosService.confirmarPago(this.token).subscribe({
        next: async (response: any) => {
          await loading.dismiss();
          this.loading = false;
          
          console.log('Respuesta de confirmación:', response);
          
          if (response.estado === 'AUTHORIZED' || response.status === 'success') {
            // Pago exitoso - redirigir a página de certificados con descarga
            if (response.redirectUrl) {
              const [path, queryString] = response.redirectUrl.split('?');
              const queryParams = new URLSearchParams(queryString);
              const params: { [key: string]: any } = {};
              queryParams.forEach((value, key) => {
                params[key] = value;
              });
              this.router.navigate([path], { queryParams: params });
            } else {
              // Fallback a la página de éxito
              this.router.navigate(['/payment/success'], {
                queryParams: { 
                  token: this.token,
                  confirmed: 'true'
                }
              });
            }
          } else {
            // Pago fallido
            this.error = 'El pago no fue autorizado. Estado: ' + (response.estado || response.status);
            this.mostrarError(this.error);
          }
        },
        error: async (error: any) => {
          await loading.dismiss();
          this.loading = false;
          console.error('Error al confirmar pago:', error);
          
          this.error = 'Ocurrió un error al confirmar el pago';
          this.mostrarError(this.error);
        }
      });
    } catch (error) {
      await loading.dismiss();
      this.loading = false;
      console.error('Error en confirmarPago:', error);
      
      this.error = 'Error inesperado al procesar el pago';
      this.mostrarError(this.error);
    }
  }

  async mostrarError(mensaje: string) {
    const alert = await this.alertCtrl.create({
      header: 'Error en el Pago',
      message: mensaje,
      buttons: [
        {
          text: 'Volver a Certificados',
          handler: () => {
            this.router.navigate(['/certificados/solicitar']);
          }
        }
      ]
    });
    await alert.present();
  }

  volverACertificados() {
    this.router.navigate(['/certificados/solicitar']);
  }
} 