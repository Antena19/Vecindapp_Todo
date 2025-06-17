import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { PagoCertificadoComponent } from './pago-certificado/pago-certificado.component';
import { RetornoPagoComponent } from './retorno-pago/retorno-pago.component';

const routes: Routes = [
  {
    path: 'pago/:id',
    component: PagoCertificadoComponent
  },
  {
    path: 'retorno',
    component: RetornoPagoComponent
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class CertificadosPageRoutingModule {} 