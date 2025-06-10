import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonicModule, ToastController, ModalController } from '@ionic/angular';
import { RouterModule, Router } from '@angular/router';
import { EventosService } from './eventos.service';
import { Evento } from '../../modelos/evento.model';
import { CrearEventoComponent } from './crear-evento/crear-evento.component';
import { EditarEventoComponent } from './editar-evento/editar-evento.component';

@Component({
  selector: 'app-eventos',
  templateUrl: './eventos.component.html',
  styleUrls: ['./eventos.component.scss'],
  standalone: true,
  imports: [CommonModule, IonicModule, RouterModule]
})
export class EventosComponent implements OnInit {
  eventos: Evento[] = [];
  cargando = true;
  error = false;

  constructor(
    private eventosService: EventosService,
    private toastController: ToastController,
    private modalController: ModalController,
    public router: Router
  ) {}

  ngOnInit() {
    this.cargarEventos();
  }

  cargarEventos() {
    this.cargando = true;
    this.error = false;
    
    this.eventosService.obtenerEventos().subscribe({
      next: (eventos: Evento[]) => {
        this.eventos = eventos;
        this.cargando = false;
      },
      error: async (error) => {
        console.error('Error al cargar eventos:', error);
        this.error = true;
        this.cargando = false;
        const toast = await this.toastController.create({
          message: 'Error al cargar los eventos. Por favor, intente nuevamente.',
          duration: 3000,
          color: 'danger'
        });
        toast.present();
      }
    });
  }

  async abrirCrearEventoModal() {
    const modal = await this.modalController.create({
      component: CrearEventoComponent
    });
    modal.onDidDismiss().then(() => {
      this.cargarEventos();
    });
    await modal.present();
  }

  async abrirEditarEventoModal(evento: Evento) {
    const modal = await this.modalController.create({
      component: EditarEventoComponent,
      componentProps: { evento }
    });
    modal.onDidDismiss().then(() => {
      this.cargarEventos();
    });
    await modal.present();
  }

  eliminarEvento(id: number) {
    if (confirm('¿Estás seguro de que deseas eliminar este evento?')) {
      this.eventosService.eliminarEvento(id).subscribe({
        next: async () => {
          this.eventos = this.eventos.filter(evento => evento.id !== id);
          const toast = await this.toastController.create({
            message: 'Evento eliminado exitosamente',
            duration: 2000,
            color: 'success'
          });
          toast.present();
        },
        error: async (error) => {
          console.error('Error al eliminar evento:', error);
          const toast = await this.toastController.create({
            message: 'Error al eliminar el evento',
            duration: 2000,
            color: 'danger'
          });
          toast.present();
        }
      });
    }
  }
} 