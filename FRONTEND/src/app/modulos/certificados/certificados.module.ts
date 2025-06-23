import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { HttpClientModule } from '@angular/common/http';

const routes: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  { path: 'solicitar', loadComponent: () => import('./solicitar-certificado/solicitar-certificado.component').then(m => m.SolicitarCertificadoComponent) },
  { path: 'aprobar', loadComponent: () => import('./aprobar-certificado/aprobar-certificado.component').then(m => m.AprobarCertificadoComponent) },
  { path: 'pago', loadComponent: () => import('./pago-certificado/pago-certificado.component').then(m => m.PagoCertificadoComponent) },
  { path: 'historial', loadComponent: () => import('./historial-certificados/historial-certificados.component').then(m => m.HistorialCertificadosComponent) },
  { path: 'dashboard', loadComponent: () => import('./dashboard-certificados/dashboard-certificados.component').then(m => m.DashboardCertificadosComponent) },
];

@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    HttpClientModule,
    RouterModule.forChild(routes)
  ]
})
export class CertificadosModule { } 