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
    this.route.parent!.paramMap.subscribe(params => {
      const id = params.get('id');
      if (id) {
        this.eventoId = +id;
        this.cargarAsistentes(this.eventoId);
      } else {
        this.presentToast('ID de evento no proporcionado.', 'danger');
        this.router.navigate(['/eventos/lista']); // Redirigir si no hay ID
      }
    });
  }

  cargarAsistentes(eventoId: number) {
    this.cargando = true;
    this.eventosService.obtenerAsistentes(eventoId).subscribe({
      next: (data: any) => {
        console.log('Datos recibidos del backend:', data);
        if (data && data.asistentes) {
          this.asistentes = data.asistentes;
          console.log('Asistentes asignados:', this.asistentes);
        } else {
          console.warn('La respuesta del backend no contiene la propiedad "asistentes".', data);
          this.asistentes = [];
        }
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