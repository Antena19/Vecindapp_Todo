import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { PaymentService } from '../../../services/payment.service';
import { AlertController, LoadingController, IonicModule } from '@ionic/angular';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-retorno-pago',
  templateUrl: './retorno-pago.component.html',
  styleUrls: ['./retorno-pago.component.scss'],
  standalone: true,
  imports: [CommonModule, FormsModule, IonicModule]
})
export class RetornoPagoComponent implements OnInit {
  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private paymentService: PaymentService,
    private loadingCtrl: LoadingController,
    private alertCtrl: AlertController
  ) { }

  ngOnInit() {
    this.procesarRetorno();
  }

  async procesarRetorno() {
    const loading = await this.loadingCtrl.create({
      message: 'Procesando pago...'
    });
    await loading.present();

    try {
      const token = this.route.snapshot.queryParamMap.get('token_ws');
      
      if (!token) {
        throw new Error('No se recibió token de pago');
      }

      const response = await this.paymentService.commitTransaction(token).toPromise();
      
      await loading.dismiss();

      if (response.status === 'AUTHORIZED') {
        const alert = await this.alertCtrl.create({
          header: '¡Pago Exitoso!',
          message: 'Tu certificado ha sido pagado correctamente.',
          buttons: [{
            text: 'OK',
            handler: () => {
              this.router.navigate(['/certificados']);
            }
          }]
        });
        await alert.present();
      } else {
        const alert = await this.alertCtrl.create({
          header: 'Pago Rechazado',
          message: 'El pago no pudo ser procesado. Por favor, intente nuevamente.',
          buttons: [{
            text: 'OK',
            handler: () => {
              this.router.navigate(['/certificados']);
            }
          }]
        });
        await alert.present();
      }
    } catch (error) {
      await loading.dismiss();
      const alert = await this.alertCtrl.create({
        header: 'Error',
        message: 'Ocurrió un error al procesar el pago. Por favor, intente nuevamente.',
        buttons: [{
          text: 'OK',
          handler: () => {
            this.router.navigate(['/certificados']);
          }
        }]
      });
      await alert.present();
    }
  }
} 