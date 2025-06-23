import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AlertController, LoadingController, IonicModule } from '@ionic/angular';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-payment',
  templateUrl: './payment.page.html',
  styleUrls: ['./payment.page.scss'],
  standalone: true,
  imports: [IonicModule, CommonModule]
})
export class PaymentPage implements OnInit {
  token: string = '';
  isSimulated: boolean = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private loadingCtrl: LoadingController,
    private alertCtrl: AlertController
  ) { }

  ngOnInit() {
    this.token = this.route.snapshot.queryParamMap.get('token_ws') || '';
    this.isSimulated = this.token.startsWith('simulated_');
    
    if (this.isSimulated) {
      this.handleSimulatedPayment();
    }
  }

  async handleSimulatedPayment() {
    const loading = await this.loadingCtrl.create({
      message: 'Procesando pago simulado...'
    });
    await loading.present();

    try {
      // Simular un delay para que parezca real
      await new Promise(resolve => setTimeout(resolve, 2000));
      
      await loading.dismiss();
      
      const alert = await this.alertCtrl.create({
        header: 'Pago Simulado Exitoso',
        message: 'Esta es una transacción simulada para desarrollo. En producción, esto sería una transacción real de Transbank.',
        buttons: [
          {
            text: 'Continuar',
            handler: () => {
              // Redirigir a la página de éxito
              this.router.navigate(['/payment/success'], {
                queryParams: { 
                  token: this.token,
                  simulated: 'true'
                }
              });
            }
          }
        ]
      });
      await alert.present();
    } catch (error) {
      await loading.dismiss();
      console.error('Error en pago simulado:', error);
    }
  }
} 