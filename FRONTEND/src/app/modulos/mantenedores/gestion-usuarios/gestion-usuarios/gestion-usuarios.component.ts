import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonicModule } from '@ionic/angular';
import { Router } from '@angular/router';

interface Actividad {
  id: number;
  titulo: string;
  descripcion: string;
  fecha: Date;
  icono: string;
  color: string;
}

@Component({
  selector: 'app-gestion-usuarios',
  templateUrl: './gestion-usuarios.component.html',
  styleUrls: ['./gestion-usuarios.component.scss'],
  standalone: true,
  imports: [CommonModule, IonicModule]
})
export class GestionUsuariosComponent implements OnInit {
  // Variables para estadísticas
  totalVecinos: number = 250;
  vecinosActivos: number = 230;
  vecinosInactivos: number = 20;
  nuevosEsteMes: number = 15;

  // Array para las últimas actividades
  ultimasActividades: Actividad[] = [
    {
      id: 1,
      titulo: 'Nuevo Vecino Registrado',
      descripcion: 'Juan Pérez se ha registrado en la plataforma',
      fecha: new Date('2024-03-15T10:30:00'),
      icono: 'person-add-outline',
      color: 'success'
    },
    {
      id: 2,
      titulo: 'Actualización de Perfil',
      descripcion: 'María González ha actualizado su información',
      fecha: new Date('2024-03-14T15:45:00'),
      icono: 'create-outline',
      color: 'primary'
    },
    {
      id: 3,
      titulo: 'Vecino Inactivo',
      descripcion: 'Carlos Rodríguez ha sido marcado como inactivo',
      fecha: new Date('2024-03-14T09:15:00'),
      icono: 'alert-circle-outline',
      color: 'warning'
    }
  ];

  constructor(private router: Router) {}

  ngOnInit() {}

  // Método para navegar a diferentes secciones
  navigateTo(route: string) {
    this.router.navigate([`/mantenedores/gestion-usuarios/${route}`]);
  }
} 