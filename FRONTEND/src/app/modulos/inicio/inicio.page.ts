import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonicModule, NavController } from '@ionic/angular';
import { Router, RouterModule } from '@angular/router';

@Component({
  selector: 'app-inicio',
  templateUrl: './inicio.page.html',
  styleUrls: ['./inicio.page.scss'],
  standalone: true,
  imports: [IonicModule, CommonModule, RouterModule]
})
export class InicioPage implements OnInit {
  constructor(
    private navCtrl: NavController,
    private router: Router
  ) {}

  ngOnInit() {
    console.log('Inicio page initialized');
    console.log('Available routes:', this.router.config);
  }

  irALogin() {
    console.log('Botón ENTRAR presionado');
    
    try {
      console.log('Intentando navegar con Router...');
      this.router.navigate(['/login']).then(success => {
        console.log('Resultado de Router.navigate:', success);
      }).catch(error => {
        console.error('Error con Router.navigate:', error);
      });
    } catch (e) {
      console.error('Excepción en navegación con Router:', e);
    }
    
    try {
      console.log('Intentando navegar con NavController...');
      this.navCtrl.navigateForward('/login');
      console.log('NavController.navigateForward ejecutado');
    } catch (e) {
      console.error('Excepción en navegación con NavController:', e);
    }
    
    console.log('Intentando navegación directa...');
    setTimeout(() => {
      console.log('Ejecutando navegación window.location después de 1 segundo');
      window.location.href = 'login';
    }, 1000);
  }
}