import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { IonicModule } from '@ionic/angular';

// Componente Standalone que este módulo "envuelve" para enrutamiento
import { ComunicacionComponent } from './comunicacion.component';

const routes: Routes = [
  {
    path: '',
    component: ComunicacionComponent
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
];

@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    IonicModule,
    RouterModule.forChild(routes)
  ]
})
export class ComunicacionModule { } 