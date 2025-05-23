import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonicModule } from '@ionic/angular';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-gestion-solicitudes',
  templateUrl: './gestion-solicitudes.component.html',
  styleUrls: ['./gestion-solicitudes.component.scss'],
  standalone: true,
  imports: [CommonModule, IonicModule, FormsModule, RouterModule]
})
export class GestionSolicitudesComponent  implements OnInit {

  constructor() { }

  ngOnInit() {}

}
