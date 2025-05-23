import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { GestionUsuariosComponent } from './gestion-usuarios/gestion-usuarios.component';
import { ListarUsuariosComponent } from './listar-usuarios/listar-usuarios.component';

const routes: Routes = [
  {
    path: '',
    component: GestionUsuariosComponent,
    children: [
      {
        path: 'listar',
        component: ListarUsuariosComponent
      },
      {
        path: '',
        redirectTo: 'listar',
        pathMatch: 'full'
      }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class GestionUsuariosRoutingModule { } 