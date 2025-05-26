import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonicModule } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { EventosService } from '../../../services/eventos.service';
import { Evento } from '../../../modelos/evento.model';

@Component({
  selector: 'app-reporte-eventos',
  templateUrl: './reporte-eventos.component.html',
  styleUrls: ['./reporte-eventos.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    IonicModule,
    RouterModule,
    FormsModule
  ]
})
export class ReporteEventosComponent {
  eventos: Evento[] = [];
  cargando = false;
  fechaInicio: string = '';
  fechaFin: string = '';

  constructor(private eventosService: EventosService) { }

  generarReporte() {
    if (!this.fechaInicio || !this.fechaFin) return;
    this.cargando = true;
    this.eventosService.generarReporteAsistencia(new Date(this.fechaInicio), new Date(this.fechaFin)).subscribe({
      next: (data) => {
        this.eventos = data;
        this.cargando = false;
      },
      error: (error) => {
        console.error('Error al generar reporte:', error);
        this.cargando = false;
      }
    });
  }
} 