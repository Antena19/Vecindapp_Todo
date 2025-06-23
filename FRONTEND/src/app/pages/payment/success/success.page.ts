import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AlertController, IonicModule } from '@ionic/angular';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-success',
  templateUrl: './success.page.html',
  styleUrls: ['./success.page.scss'],
  standalone: true,
  imports: [IonicModule, CommonModule]
})
export class SuccessPage implements OnInit {
  token: string = '';
  isSimulated: boolean = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private alertCtrl: AlertController
  ) { }

  ngOnInit() {
    this.token = this.route.snapshot.queryParamMap.get('token') || '';
    this.isSimulated = this.route.snapshot.queryParamMap.get('simulated') === 'true';
  }

  async volverACertificados() {
    await this.router.navigate(['/certificados/solicitar']);
  }

  async mostrarInformacion() {
    const alert = await this.alertCtrl.create({
      header: 'Información del Pago',
      message: this.isSimulated 
        ? 'Esta fue una transacción simulada para desarrollo. En producción, esto sería una transacción real procesada por Transbank.'
        : 'Transacción procesada exitosamente por Transbank.',
      buttons: ['OK']
    });
    await alert.present();
  }
} 