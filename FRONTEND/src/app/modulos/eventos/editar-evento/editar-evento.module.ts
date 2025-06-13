import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EditarEventoComponent } from './editar-evento.component';
import { EditarEventoRoutingModule } from './editar-evento-routing.module';

@NgModule({
  imports: [CommonModule, EditarEventoRoutingModule, EditarEventoComponent]
})
export class EditarEventoModule { } 