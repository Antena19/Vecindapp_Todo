import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { EditarEventoComponent } from './editar-evento.component';

const routes: Routes = [
  {
    path: '',
    component: EditarEventoComponent
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class EditarEventoRoutingModule { } 