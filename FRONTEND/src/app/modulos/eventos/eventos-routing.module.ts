import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { EventosComponent } from './eventos.component';

export const routes: Routes = [
  {
    path: '',
    component: EventosComponent,
    children: [
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
        path: 'detalle/:id',
        loadComponent: () => import('./detalle-evento/detalle-evento.component').then(m => m.DetalleEventoComponent)
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
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class EventosRoutingModule { } 