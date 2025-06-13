import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'lista',
    pathMatch: 'full'
  },
  {
    path: 'lista',
    loadComponent: () => import('./lista-eventos/lista-eventos.component').then(m => m.ListaEventosComponent)
  },
  {
    path: 'crear',
    loadComponent: () => import('./crear-evento/crear-evento.component').then(m => m.CrearEventoComponent)
  },
  {
    path: 'detalle/:id',
    loadComponent: () => import('./detalle-evento/detalle-evento.component').then(m => m.DetalleEventoComponent)
  },
  {
    path: 'detalle/:id/asistentes',
    loadComponent: () => import('./detalle-asistentes/detalle-asistentes.component').then(m => m.DetalleAsistentesComponent)
  },
  {
    path: 'editar/:id',
    loadComponent: () => import('./editar-evento/editar-evento.component').then(m => m.EditarEventoComponent)
  },
  {
    path: 'registro-asistencia',
    loadComponent: () => import('./registro-asistencia/registro-asistencia.component').then(m => m.RegistroAsistenciaComponent)
  },
  {
    path: 'historial',
    loadComponent: () => import('./historial-asistencia/historial-asistencia.component').then(m => m.HistorialAsistenciaComponent)
  },
  {
    path: 'reporte',
    loadComponent: () => import('./reporte-eventos/reporte-eventos.component').then(m => m.ReporteEventosComponent)
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class EventosRoutingModule { } 