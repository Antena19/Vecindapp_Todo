import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { PaymentService } from '../../../services/payment.service';
import { AlertController, LoadingController, IonicModule } from '@ionic/angular';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-pago-certificado',
  templateUrl: './pago-certificado.component.html',
  styleUrls: ['./pago-certificado.component.scss'],
  standalone: true,
  imports: [CommonModule, FormsModule, IonicModule]
})
export class PagoCertificadoComponent implements OnInit {
  certificadoId: string = '';
  monto: number = 1000; // Monto por defecto, ajustar según necesidad

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private paymentService: PaymentService,
    private loadingCtrl: LoadingController,
    private alertCtrl: AlertController
  ) { }

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.certificadoId = id;
    }
  }

  async iniciarPago() {
    const loading = await this.loadingCtrl.create({
      message: 'Procesando pago...'
    });
    await loading.present();

    try {
      const buyOrder = `cert-${this.certificadoId}-${Date.now()}`;
      const sessionId = `sesion-${Date.now()}`;

      const response = await this.paymentService.createTransaction(
        this.monto,
        buyOrder,
        sessionId
      ).toPromise();

      // Redirigir a la página de pago de Transbank
      window.location.href = response.url;
    } catch (error) {
      await loading.dismiss();
      const alert = await this.alertCtrl.create({
        header: 'Error',
        message: 'No se pudo iniciar el pago. Por favor, intente nuevamente.',
        buttons: ['OK']
      });
      await alert.present();
    }
  }
} 