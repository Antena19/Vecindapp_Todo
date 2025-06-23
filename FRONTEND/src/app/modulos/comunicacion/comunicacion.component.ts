import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { LoadingController, IonicModule } from '@ionic/angular';
import { CommonModule } from '@angular/common';
import { AutenticacionService } from 'src/app/services/autenticacion.service';

@Component({
  selector: 'app-comunicacion',
  templateUrl: './comunicacion.component.html',
  styleUrls: ['./comunicacion.component.scss'],
  standalone: true,
  imports: [IonicModule, CommonModule]
})
export class ComunicacionComponent {
  tipoUsuario: string = '';

  constructor(
    private router: Router,
    private loadingController: LoadingController,
    private authService: AutenticacionService
  ) {}

  async ionViewDidEnter() {
    try {
      this.tipoUsuario = this.authService.getUserRole();
      if (this.tipoUsuario === 'vecino' || this.tipoUsuario === 'socio') {
        this.router.navigate(['/comunicacion/lista'], { replaceUrl: true });
        return;
      }
      const activeLoader = await this.loadingController.getTop();
      if (activeLoader) {
        await this.loadingController.dismiss();
      }
    } catch (error) {
      // No es necesario loguear el error si no hay loader activo
    }
  }

  navegarALista() {
    this.router.navigate(['/comunicacion/lista']);
  }

  navegarACrear() {
    this.router.navigate(['/comunicacion/crear']);
  }

  navegarANotificaciones() {
    this.router.navigate(['/comunicacion/notificaciones']);
  }

  navegarAGestion() {
    this.router.navigate(['/comunicacion/gestion']);
  }

  navegarAEstadisticas() {
    this.router.navigate(['/comunicacion/estadisticas']);
  }
} 