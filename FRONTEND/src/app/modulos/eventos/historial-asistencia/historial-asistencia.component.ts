import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonicModule } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { EventosService } from '../../../services/eventos.service';
import { AsistenciaEvento } from '../../../modelos/evento.model';

@Component({
  selector: 'app-historial-asistencia',
  templateUrl: './historial-asistencia.component.html',
  styleUrls: ['./historial-asistencia.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    IonicModule,
    RouterModule
  ]
})
export class HistorialAsistenciaComponent implements OnInit {
  asistencias: AsistenciaEvento[] = [];
  cargando = true;

  constructor(private eventosService: EventosService) { }

  ngOnInit() {
    this.eventosService.obtenerHistorialAsistencia().subscribe({
      next: (data) => {
        this.asistencias = data;
        this.cargando = false;
      },
      error: (error) => {
        console.error('Error al cargar historial:', error);
        this.cargando = false;
      }
    });
  }
} 