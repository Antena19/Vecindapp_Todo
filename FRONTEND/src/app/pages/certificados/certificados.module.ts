import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { CertificadosPageRoutingModule } from './certificados-routing.module';
import { PagoCertificadoComponent } from './pago-certificado/pago-certificado.component';
import { RetornoPagoComponent } from './retorno-pago/retorno-pago.component';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    IonicModule,
    CertificadosPageRoutingModule
  ],
  declarations: [
    PagoCertificadoComponent,
    RetornoPagoComponent
  ]
})
export class CertificadosPageModule {} 