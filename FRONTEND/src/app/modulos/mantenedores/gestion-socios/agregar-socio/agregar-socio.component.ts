import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonicModule } from '@ionic/angular';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-agregar-socio',
  templateUrl: './agregar-socio.component.html',
  styleUrls: ['./agregar-socio.component.scss'],
  standalone: true,
  imports: [CommonModule, IonicModule, FormsModule, RouterModule]
})
export class AgregarSocioComponent  implements OnInit {

  constructor() { }

  ngOnInit() {}

}
