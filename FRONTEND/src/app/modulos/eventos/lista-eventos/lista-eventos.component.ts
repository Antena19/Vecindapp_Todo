import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonicModule } from '@ionic/angular';
import { RouterModule, Router } from '@angular/router';
import { EventosService } from '../../../services/eventos.service';
import { Evento } from '../../../modelos/evento.model';

@Component({
  selector: 'app-lista-eventos',
  templateUrl: './lista-eventos.component.html',
  styleUrls: ['./lista-eventos.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    IonicModule,
    RouterModule
  ]
})
export class ListaEventosComponent implements OnInit {
  eventos: Evento[] = [];

  constructor(
    private router: Router,
    private eventosService: EventosService
  ) { }

  ngOnInit() {
    this.cargarEventos();
  }

  cargarEventos() {
    this.eventosService.getEventos().subscribe({
      next: (eventos) => {
        this.eventos = eventos;
      },
      error: (error) => {
        console.error('Error al cargar eventos:', error);
        // TODO: Mostrar mensaje de error al usuario
      }
    });
  }

  crearEvento() {
    // TODO: Implementar la navegación al formulario de creación
    console.log('Crear nuevo evento');
  }

  verDetalle(id: number) {
    this.router.navigate(['/eventos/detalle', id]);
  }
} 