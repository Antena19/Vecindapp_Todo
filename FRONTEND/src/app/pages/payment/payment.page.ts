import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { WebpayService } from '../../services/webpay.service';
import { AlertController, IonicModule } from '@ionic/angular';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-payment',
  templateUrl: './payment.page.html',
  styleUrls: ['./payment.page.scss'],
  standalone: true,
  imports: [IonicModule, CommonModule, FormsModule]
})
export class PaymentPage implements OnInit {
  monto: number = 0;
  private confirmando = false;

  constructor(
    private webpayService: WebpayService,
    private route: ActivatedRoute,
    private router: Router,
    private alertController: AlertController
  ) { }

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      if (params['token_ws'] && !this.confirmando) {
        this.confirmando = true;
        this.confirmarTransaccion(params['token_ws']);
      }
    });
  }

  async iniciarPago(monto: number) {
    try {
      const response = await this.webpayService.crearTransaccion(monto).toPromise();
      
      if (response && response.url && response.token) {
        window.location.href = `${response.url}?token_ws=${response.token}`;
      }
    } catch (error) {
      this.mostrarError('Error al iniciar el pago');
    }
  }

  async confirmarTransaccion(token: string) {
    try {
      const response = await this.webpayService.confirmarTransaccion(token).toPromise();
      console.log('Respuesta de backend al confirmar transacción:', response);
      if ((response.status && response.status === 'AUTHORIZED') || (response.mensaje && response.mensaje.toLowerCase().includes('confirmado'))) {
        await this.mostrarExito('Pago realizado con éxito. Status: ' + (response.status || ''));
        this.router.navigate(['/certificados/solicitar'], { queryParams: { descargar: 1 } });
      } else {
        let msg = 'El pago no pudo ser procesado.';
        if (response.status) {
          msg += ' Status: ' + response.status;
        }
        if (response.mensaje) {
          msg += ' Mensaje: ' + response.mensaje;
        }
        await this.mostrarError(msg);
      }
    } catch (error: any) {
      let msg = 'Error al confirmar el pago';
      if (error?.error?.mensaje && error.error.mensaje.toLowerCase().includes('ya fue procesado')) {
        msg = 'Este pago ya fue procesado o el enlace expiró. Si ya pagaste, revisa tu historial.';
      } else if (error?.error?.mensaje) {
        msg = error.error.mensaje;
      } else if (error?.error?.status) {
        msg += ' Status: ' + error.error.status;
      }
      await this.mostrarError(msg);
    }
  }

  async mostrarExito(mensaje: string) {
    const alert = await this.alertController.create({
      header: 'Éxito',
      message: mensaje,
      buttons: ['OK']
    });
    await alert.present();
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