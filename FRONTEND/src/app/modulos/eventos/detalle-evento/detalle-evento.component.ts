import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonicModule } from '@ionic/angular';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { EventosService } from '../../../services/eventos.service';
import { QRService } from '../../../services/qr.service';
import { Evento } from '../../../modelos/evento.model';

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

  constructor(
    private route: ActivatedRoute,
    private eventosService: EventosService,
    private qrService: QRService,
    private router: Router
  ) { }

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
        error: (error) => {
          console.error('Error al cargar el evento:', error);
          this.cargando = false;
        }
      });
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
        const uri = await this.qrService.guardarQR(this.codigoQR, `qr_evento_${this.evento.id}`);
        await this.qrService.compartirQR(uri, `Código QR - ${this.evento.titulo}`);
      } catch (error) {
        console.error('Error al compartir código QR:', error);
      }
    }
  }

  editarEvento() {
    if (this.evento) {
      window.location.href = `/eventos/editar/${this.evento.id}`;
    }
  }

  eliminarEvento() {
    if (!this.evento) return;
    if (!confirm('¿Estás seguro de que deseas eliminar este evento?')) return;
    this.eliminando = true;
    this.eventosService.eliminarEvento(this.evento.id).subscribe({
      next: () => {
        alert('Evento eliminado correctamente');
        window.location.href = '/eventos';
      },
      error: (error) => {
        alert('Error al eliminar el evento');
        this.eliminando = false;
        console.error(error);
      }
    });
  }

  get isChildRouteActive(): boolean {
    return this.route.children.length > 0;
  }
} 