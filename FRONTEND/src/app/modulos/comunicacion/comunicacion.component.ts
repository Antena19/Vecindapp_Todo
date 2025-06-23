import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonicModule } from '@ionic/angular';
import { Router, RouterModule } from '@angular/router';
import { ComunicacionService } from '../../services/comunicacion.service';
import { Noticia } from '../../modelos/comunicacion.model';

@Component({
  selector: 'app-comunicacion',
  templateUrl: './comunicacion.component.html',
  styleUrls: ['./comunicacion.component.scss'],
  standalone: true,
  imports: [CommonModule, IonicModule, RouterModule]
})
export class ComunicacionComponent implements OnInit {
  noticiasDestacadas: Noticia[] = [];
  loading = false;

  constructor(
    private comunicacionService: ComunicacionService,
    private router: Router
  ) { }

  ngOnInit() {
    this.cargarNoticiasDestacadas();
  }

  cargarNoticiasDestacadas() {
    this.loading = true;
    this.comunicacionService.getNoticiasDestacadas().subscribe({
      next: (noticias) => {
        this.noticiasDestacadas = noticias;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error al cargar noticias destacadas:', error);
        this.loading = false;
      }
    });
  }

  getPrioridadColor(prioridad: string): string {
    switch (prioridad) {
      case 'ALTA': return 'danger';
      case 'MEDIA': return 'warning';
      case 'BAJA': return 'success';
      default: return 'medium';
    }
  }

  getAlcanceColor(alcance: string): string {
    switch (alcance) {
      case 'PUBLICO': return 'primary';
      case 'SOCIOS': return 'secondary';
      default: return 'medium';
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