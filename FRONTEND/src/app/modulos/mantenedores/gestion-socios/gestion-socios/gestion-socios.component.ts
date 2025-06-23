import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonicModule } from '@ionic/angular';
import { Router } from '@angular/router';
import { SociosService } from 'src/app/services/socios.service';

interface Actividad {
  id: number;
  titulo: string;
  descripcion: string;
  fecha: Date;
  icono: string;
  color: string;
}

@Component({
  selector: 'app-gestion-socios',
  templateUrl: './gestion-socios.component.html',
  styleUrls: ['./gestion-socios.component.scss'],
  standalone: true,
  imports: [CommonModule, IonicModule]
})
export class GestionSociosComponent implements OnInit {
  // Variables para estadísticas inicializadas en 0
  totalSocios: number = 0;
  solicitudesPendientes: number = 0;
  sociosActivos: number = 0;
  sociosInactivos: number = 0;
  totalVecinos: number = 0;

  // Array para las últimas actividades inicializado vacío
  ultimasActividades: Actividad[] = [];

  constructor(
    private router: Router,
    private sociosService: SociosService
  ) {}

  ngOnInit() {
    this.cargarEstadisticas();
  }

  cargarEstadisticas() {
    this.sociosService.obtenerEstadisticas().subscribe({
      next: (response) => {
        // Actualizar estadísticas
        this.totalSocios = response.estadisticas.totalSocios;
        this.solicitudesPendientes = response.estadisticas.solicitudesPendientes;
        this.sociosActivos = response.estadisticas.sociosActivos;
        this.sociosInactivos = response.estadisticas.sociosInactivos;
        this.totalVecinos = response.estadisticas.totalVecinos;

        // Actualizar actividades
        this.ultimasActividades = response.ultimasActividades;
      },
      error: (error) => {
        console.error('Error al cargar estadísticas:', error);
        // Aquí podrías mostrar un mensaje de error al usuario
      }
    });
  }

  // Método para navegar a diferentes secciones
  navigateTo(route: string) {
    this.router.navigate([`/mantenedores/gestion-socios/${route}`]);
  }
} 