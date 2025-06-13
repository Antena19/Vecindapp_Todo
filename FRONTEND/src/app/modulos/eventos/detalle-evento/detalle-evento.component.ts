import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonicModule } from '@ionic/angular';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { EventosService } from '../../../services/eventos.service';
import { QRService } from '../../../services/qr.service';
import { Evento } from '../../../modelos/evento.model';
import { ToastController, AlertController } from '@ionic/angular';

@Component({
  selector: 'app-detalle-evento',
  templateUrl: './detalle-evento.component.html',
  styleUrls: ['./detalle-evento.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    IonicModule,
    RouterModule
  ]
})
export class DetalleEventoComponent implements OnInit {
  evento: Evento | null = null;
  cargando = true;
  codigoQR: string | null = null;
  generandoQR = false;
  eliminando = false;
  usuarioRut: number;

  constructor(
    private route: ActivatedRoute,
    private eventosService: EventosService,
    private qrService: QRService,
    private router: Router,
    private toastController: ToastController,
    private alertController: AlertController
  ) {
    this.usuarioRut = Number(localStorage.getItem('rut') || '0');
  }

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.eventosService.getEvento(Number(id)).subscribe({
        next: (evento: Evento) => {
          this.evento = evento;
          this.cargando = false;
          if (evento.codigoQr) {
            this.generarQR(evento.codigoQr);
          }
        },
        error: (error: Error) => {
          console.error('Error al cargar el evento:', error);
          this.cargando = false;
          this.router.navigate(['/eventos']);
        }
      });
    } else {
      this.router.navigate(['/eventos']);
    }
  }

  async generarQR(codigo: string) {
    try {
      this.generandoQR = true;
      this.codigoQR = await this.qrService.generarQR(codigo);
    } catch (error) {
      console.error('Error al generar código QR:', error);
    } finally {
      this.generandoQR = false;
    }
  }

  async compartirQR() {
    if (this.codigoQR && this.evento) {
      try {
        await this.qrService.compartirQR(this.codigoQR, `Código QR - ${this.evento.titulo}`);
      } catch (error) {
        console.error('Error al compartir código QR:', error);
      }
    }
  }

  async copiarCodigoNumerico() {
    if (this.evento?.codigoNumerico) {
      try {
        await navigator.clipboard.writeText(this.evento.codigoNumerico);
        const toast = await this.toastController.create({
          message: 'Código copiado al portapapeles',
          duration: 2000,
          position: 'bottom'
        });
        toast.present();
      } catch (error) {
        console.error('Error al copiar código:', error);
        const toast = await this.toastController.create({
          message: 'Error al copiar el código',
          duration: 2000,
          position: 'bottom',
          color: 'danger'
        });
        toast.present();
      }
    }
  }

  editarEvento() {
    if (this.evento) {
      this.router.navigate(['/eventos/editar', this.evento.id]);
    }
  }

  async eliminarEvento() {
    if (!this.evento) return;

    const alert = await this.alertController.create({
      header: 'Confirmar eliminación',
      message: '¿Estás seguro de que deseas eliminar este evento? Esta acción no se puede deshacer.',
      buttons: [
        {
          text: 'Cancelar',
          role: 'cancel',
          cssClass: 'secondary'
        },
        {
          text: 'Eliminar',
          cssClass: 'danger',
          handler: () => {
            this.eliminando = true;
            this.eventosService.eliminarEvento(this.evento!.id).subscribe({
              next: async () => {
                const toast = await this.toastController.create({
                  message: 'Evento eliminado correctamente',
                  duration: 2000,
                  color: 'success',
                  position: 'top'
                });
                await toast.present();
                this.router.navigate(['/eventos']);
              },
              error: async (error: Error) => {
                console.error('Error al eliminar el evento:', error);
                const toast = await this.toastController.create({
                  message: 'Error al eliminar el evento',
                  duration: 2000,
                  color: 'danger',
                  position: 'top'
                });
                await toast.present();
                this.eliminando = false;
              }
            });
          }
        }
      ]
    });

    await alert.present();
  }
} 