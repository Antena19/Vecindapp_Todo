import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { RouterModule, Routes } from '@angular/router';

// Servicios
import { ComunicacionService } from '../../services/comunicacion.service';

const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./comunicacion.component').then(m => m.ComunicacionComponent),
    children: [
      {
        path: '',
        redirectTo: 'lista',
        pathMatch: 'full'
      },
      {
        path: 'lista',
        loadComponent: () => import('./lista-noticias/lista-noticias.component').then(m => m.ListaNoticiasComponent)
      },
      {
        path: 'detalle/:id',
        loadComponent: () => import('./detalle-noticia/detalle-noticia.component').then(m => m.DetalleNoticiaComponent)
      },
      {
        path: 'crear',
        loadComponent: () => import('./crear-noticia/crear-noticia.component').then(m => m.CrearNoticiaComponent)
      },
      {
        path: 'editar/:id',
        loadComponent: () => import('./editar-noticia/editar-noticia.component').then(m => m.EditarNoticiaComponent)
      },
      {
        path: 'notificaciones',
        loadComponent: () => import('./notificaciones/notificaciones.component').then(m => m.NotificacionesComponent)
      },
      {
        path: 'gestion',
        loadComponent: () => import('./gestion-noticias/gestion-noticias.component').then(m => m.GestionNoticiasComponent)
      },
      {
        path: 'estadisticas',
        loadComponent: () => import('./estadisticas-comunicacion/estadisticas-comunicacion.component').then(m => m.EstadisticasComunicacionComponent)
      }
    ]
  }
];

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    IonicModule,
    RouterModule.forChild(routes)
  ],
  providers: [
    ComunicacionService
  ]
})
export class ComunicacionModule { } 