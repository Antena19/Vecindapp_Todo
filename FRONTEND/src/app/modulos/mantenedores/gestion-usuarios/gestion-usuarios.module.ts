import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonicModule } from '@ionic/angular';
import { FormsModule } from '@angular/forms';
import { GestionUsuariosRoutingModule } from './gestion-usuarios-routing.module';
import { GestionUsuariosComponent } from './gestion-usuarios/gestion-usuarios.component';
import { ListarUsuariosComponent } from './listar-usuarios/listar-usuarios.component';

@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    IonicModule,
    FormsModule,
    GestionUsuariosRoutingModule,
    GestionUsuariosComponent,
    ListarUsuariosComponent
  ]
})
export class GestionUsuariosModule { } 