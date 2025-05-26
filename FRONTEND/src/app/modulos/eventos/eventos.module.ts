import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EventosRoutingModule } from './eventos-routing.module';
import { EventosComponent } from './eventos.component';

@NgModule({
  imports: [
    CommonModule,
    EventosRoutingModule,
    EventosComponent
  ]
})
export class EventosModule { } 