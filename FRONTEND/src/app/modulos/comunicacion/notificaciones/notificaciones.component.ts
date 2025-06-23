import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { ComunicacionService } from '../../../services/comunicacion.service';
import { NotificacionUsuario } from '../../../modelos/comunicacion.model';
import { AlertController, LoadingController, ToastController } from '@ionic/angular';

@Component({
  selector: 'app-notificaciones',
  templateUrl: './notificaciones.component.html',
  styleUrls: ['./notificaciones.component.scss'],
  standalone: true,
  imports: [CommonModule, FormsModule, IonicModule]
})
export class NotificacionesComponent implements OnInit {
  notificaciones: NotificacionUsuario[] = [];
  notificacionesFiltradas: NotificacionUsuario[] = [];
  loading = false;
  filtroLeidas = '';
  usuarioActual: any; // Se obtendrá del servicio de autenticación

  filtros = [
    { valor: '', texto: 'Todas las notificaciones' },
    { valor: 'false', texto: 'No leídas' },
    { valor: 'true', texto: 'Leídas' }
  ];

  constructor(
    private comunicacionService: ComunicacionService,
    private alertController: AlertController,
    private loadingController: LoadingController,
    private toastController: ToastController
  ) { }

  ngOnInit() {
    this.cargarNotificaciones();
  }

  async cargarNotificaciones() {
    if (!this.usuarioActual?.rut) {
      this.mostrarError('Usuario no autenticado');
      return;
    }

    this.loading = true;
    const loading = await this.loadingController.create({
      message: 'Cargando notificaciones...'
    });
    await loading.present();

    this.comunicacionService.getNotificacionesUsuario(this.usuarioActual.rut).subscribe({
      next: (notificaciones) => {
        this.notificaciones = notificaciones;
        this.aplicarFiltros();
        this.loading = false;
        loading.dismiss();
      },
      error: (error) => {
        console.error('Error al cargar notificaciones:', error);
        this.loading = false;
        loading.dismiss();
        this.mostrarError('Error al cargar las notificaciones');
      }
    });
  }

  aplicarFiltros() {
    this.notificacionesFiltradas = this.notificaciones.filter(notificacion => {
      if (!this.filtroLeidas) return true;
      return notificacion.leida.toString() === this.filtroLeidas;
    });
  }

  async marcarComoLeida(notificacionId: number) {
    if (!this.usuarioActual?.rut) return;

    this.comunicacionService.marcarNotificacionLeida(notificacionId, this.usuarioActual.rut).subscribe({
      next: () => {
        // Actualizar el estado local
        const notificacion = this.notificaciones.find(n => n.id === notificacionId);
        if (notificacion) {
          notificacion.leida = true;
          notificacion.fechaLectura = new Date();
        }
        this.aplicarFiltros();
        this.mostrarMensaje('Notificación marcada como leída');
      },
      error: (error) => {
        console.error('Error al marcar notificación como leída:', error);
        this.mostrarError('Error al marcar la notificación como leída');
      }
    });
  }

  async marcarTodasComoLeidas() {
    const alert = await this.alertController.create({
      header: 'Confirmar acción',
      message: '¿Quieres marcar todas las notificaciones como leídas?',
      buttons: [
        {
          text: 'Cancelar',
          role: 'cancel'
        },
        {
          text: 'Marcar todas',
          handler: () => {
            const notificacionesNoLeidas = this.notificaciones.filter(n => !n.leida);
            let completadas = 0;
            
            if (notificacionesNoLeidas.length === 0) {
              this.mostrarMensaje('No hay notificaciones sin leer');
              return;
            }

            notificacionesNoLeidas.forEach(notificacion => {
              this.comunicacionService.marcarNotificacionLeida(notificacion.id, this.usuarioActual.rut).subscribe({
                next: () => {
                  notificacion.leida = true;
                  notificacion.fechaLectura = new Date();
                  completadas++;
                  
                  if (completadas === notificacionesNoLeidas.length) {
                    this.aplicarFiltros();
                    this.mostrarMensaje('Todas las notificaciones marcadas como leídas');
                  }
                },
                error: (error) => {
                  console.error('Error al marcar notificación como leída:', error);
                }
              });
            });
          }
        }
      ]
    });
    await alert.present();
  }

  async eliminarNotificacion(notificacionId: number) {
    const alert = await this.alertController.create({
      header: 'Confirmar eliminación',
      message: '¿Estás seguro de que quieres eliminar esta notificación?',
      buttons: [
        {
          text: 'Cancelar',
          role: 'cancel'
        },
        {
          text: 'Eliminar',
          handler: () => {
            // Aquí se implementaría la eliminación en el backend
            this.notificaciones = this.notificaciones.filter(n => n.id !== notificacionId);
            this.aplicarFiltros();
            this.mostrarMensaje('Notificación eliminada');
          }
        }
      ]
    });
    await alert.present();
  }

  getNotificacionesNoLeidas(): number {
    return this.notificaciones.filter(n => !n.leida).length;
  }

  getTipoIcono(tipo: string): string {
    switch (tipo) {
      case 'PUSH': return 'notifications';
      case 'EMAIL': return 'mail';
      case 'SMS': return 'chatbubble';
      default: return 'information-circle';
    }
  }

  getTipoColor(tipo: string): string {
    switch (tipo) {
      case 'PUSH': return 'primary';
      case 'EMAIL': return 'secondary';
      case 'SMS': return 'tertiary';
      default: return 'medium';
    }
  }

  getPrioridadColor(prioridad: string): string {
    switch (prioridad) {
      case 'ALTA': return 'danger';
      case 'MEDIA': return 'warning';
      case 'BAJA': return 'success';
      default: return 'medium';
    }
  }

  async mostrarError(mensaje: string) {
    const alert = await this.alertController.create({
      header: 'Error',
      message: mensaje,
      buttons: ['OK']
    });
    await alert.present();
  }

  async mostrarMensaje(mensaje: string) {
    const toast = await this.toastController.create({
      message: mensaje,
      duration: 2000,
      position: 'bottom'
    });
    await toast.present();
  }

  doRefresh(event: any) {
    this.cargarNotificaciones().then(() => {
      event.target.complete();
    });
  }
} 