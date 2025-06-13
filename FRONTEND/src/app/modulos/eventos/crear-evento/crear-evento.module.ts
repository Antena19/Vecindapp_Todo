import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonicModule } from '@ionic/angular';
import { ReactiveFormsModule } from '@angular/forms';
import { CrearEventoComponent } from './crear-evento.component';
import { CrearEventoRoutingModule } from './crear-evento-routing.module';

@NgModule({
  imports: [CommonModule, IonicModule, ReactiveFormsModule, CrearEventoComponent, CrearEventoRoutingModule],
})
export class CrearEventoModule {} 