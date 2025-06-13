import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonicModule } from '@ionic/angular';
import { EventosRoutingModule } from './eventos-routing.module';
import { EventosComponent } from './eventos.component';

@NgModule({
  imports: [
    CommonModule,
    IonicModule,
    EventosRoutingModule,
    EventosComponent
  ]
})
export class EventosModule { } 