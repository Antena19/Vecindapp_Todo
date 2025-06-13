import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonicModule, ToastController } from '@ionic/angular';
import { ActivatedRoute, Router } from '@angular/router';
import { EventosService } from '../../../services/eventos.service';
import { AsistenciaEvento } from '../../../modelos/evento.model';

@Component({
  selector: 'app-detalle-asistentes',
  templateUrl: './detalle-asistentes.component.html',
  styleUrls: ['./detalle-asistentes.component.scss'],
  standalone: true,
  imports: [CommonModule, IonicModule]
})
export class DetalleAsistentesComponent implements OnInit {
  eventoId!: number;
  asistentes: AsistenciaEvento[] = [];
  cargando: boolean = true;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private eventosService: EventosService,
    private toastController: ToastController
  ) { }

  ngOnInit() {
    console.log('Iniciando componente de asistentes');
    this.route.paramMap.subscribe(params => {
      const id = params.get('id');
      console.log('ID del evento recibido:', id);
      if (id) {
        this.eventoId = +id;
        console.log('Cargando asistentes para el evento:', this.eventoId);
        this.cargarAsistentes(this.eventoId);
      } else {
        console.error('No se proporcionó ID de evento');
        this.presentToast('ID de evento no proporcionado.', 'danger');
        this.router.navigate(['/eventos/lista']);
      }
    });
  }

  cargarAsistentes(eventoId: number) {
    console.log('Iniciando carga de asistentes para evento:', eventoId);
    this.cargando = true;
    this.eventosService.obtenerAsistentes(eventoId).subscribe({
      next: (asistentes: AsistenciaEvento[]) => {
        console.log('Asistentes recibidos:', asistentes);
        this.asistentes = asistentes;
        this.cargando = false;
      },
      error: (error) => {
        console.error('Error al cargar asistentes:', error);
        this.presentToast('Error al cargar la lista de asistentes.', 'danger');
        this.cargando = false;
        this.asistentes = [];
      }
    });
  }

  async presentToast(message: string, color: string) {
    const toast = await this.toastController.create({
      message: message,
      duration: 2000,
      color: color
    });
    toast.present();
  }

  volverADetalleEvento() {
    this.router.navigate(['/eventos/detalle', this.eventoId]);
  }
} 