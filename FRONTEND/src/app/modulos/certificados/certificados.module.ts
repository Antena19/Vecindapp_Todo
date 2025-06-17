import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { HttpClientModule } from '@angular/common/http';
import { SolicitarCertificadoComponent } from './solicitar-certificado/solicitar-certificado.component';
import { AprobarCertificadoComponent } from './aprobar-certificado/aprobar-certificado.component';
import { PagoCertificadoComponent } from './pago-certificado/pago-certificado.component';
import { HistorialCertificadosComponent } from './historial-certificados/historial-certificados.component';

const routes: Routes = [
  { path: '', redirectTo: 'solicitar', pathMatch: 'full' },
  { path: 'solicitar', component: SolicitarCertificadoComponent },
  { path: 'aprobar', component: AprobarCertificadoComponent },
  { path: 'pago', component: PagoCertificadoComponent },
  { path: 'historial', component: HistorialCertificadosComponent },
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