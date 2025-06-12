import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonicModule } from '@ionic/angular';
import { RouterModule, Router, ActivatedRoute } from '@angular/router';
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
    private route: ActivatedRoute,
    private eventosService: EventosService
  ) { }

  ngOnInit() {
    this.cargarEventos();
  }

  ionViewWillEnter() {
    // Se ejecuta cada vez que la vista está a punto de entrar
    this.cargarEventos();
  }

  cargarEventos() {
    this.eventosService.getEventos().subscribe({
      next: (eventos) => {
        console.log('Eventos antes de ordenar:', eventos);
        this.eventos = this.ordenarEventos(eventos);
        console.log('Eventos después de ordenar:', this.eventos);
      },
      error: (error) => {
        console.error('Error al cargar eventos:', error);
        // TODO: Mostrar mensaje de error al usuario
      }
    });
  }

  ordenarEventos(eventos: Evento[]): Evento[] {
    return eventos.sort((a, b) => {
      const fechaA = new Date(a.fechaEvento);
      const fechaB = new Date(b.fechaEvento);
      return fechaA.getTime() - fechaB.getTime();
    });
  }

  eventoPasado(evento: Evento): boolean {
    const fechaEvento = new Date(evento.fechaEvento);
    const hoy = new Date();
    return fechaEvento < hoy;
  }

  crearEvento() {
    this.router.navigate(['/eventos/crear']);
  }

  verDetalle(id: number) {
    this.router.navigate(['/eventos/detalle', id]);
  }
} 