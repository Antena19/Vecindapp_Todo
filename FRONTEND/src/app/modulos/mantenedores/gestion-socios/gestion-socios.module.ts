import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { GestionSociosRoutingModule } from './gestion-socios-routing.module';
import { SociosService } from '../../../services/socios.service';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    IonicModule,
    GestionSociosRoutingModule
  ],
  providers: [SociosService]
})
export class GestionSociosModule { } 