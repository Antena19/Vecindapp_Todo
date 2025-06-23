import { Component, OnInit, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { CertificadosService } from '../certificados.service';

@Component({
  selector: 'app-aprobar-certificado',
  templateUrl: './aprobar-certificado.component.html',
  styleUrls: ['./aprobar-certificado.component.css'],
  standalone: true,
  imports: [CommonModule, FormsModule, IonicModule]
})
export class AprobarCertificadoComponent implements OnInit {
  solicitudes: any[] = [];
  solicitudSeleccionada: any = null;
  motivoRechazo: string = '';
  conectividadTransbank: boolean = false;
  cargandoConectividad: boolean = true;
  motivoAprobacionSinPago: string = '';
  observacionesAprobacion: string = '';

  constructor(@Inject(CertificadosService) private certificadosService: CertificadosService) {}

  ngOnInit(): void {
    this.obtenerSolicitudes();
    this.verificarConectividad();
  }

  obtenerSolicitudes() {
    this.certificadosService.obtenerSolicitudesPendientes().subscribe(data => {
      this.solicitudes = data;
    });
  }

  verificarConectividad() {
    this.cargandoConectividad = true;
    this.certificadosService.obtenerEstadoConectividad().subscribe({
      next: (response: any) => {
        this.conectividadTransbank = response.conectividad;
        this.cargandoConectividad = false;
        console.log('Estado de conectividad:', response);
      },
      error: (error) => {
        this.conectividadTransbank = false;
        this.cargandoConectividad = false;
        console.error('Error al verificar conectividad:', error);
      }
    });
  }

  seleccionarSolicitud(solicitud: any) {
    this.solicitudSeleccionada = solicitud;
  }

  aprobar() {
    if (this.conectividadTransbank) {
      // Flujo normal con pago
      this.certificadosService.aprobarSolicitud(this.solicitudSeleccionada.id).subscribe(() => {
        this.obtenerSolicitudes();
        this.solicitudSeleccionada = null;
      });
    } else {
      // Flujo sin pago cuando no hay conectividad
      if (!this.motivoAprobacionSinPago.trim()) {
        alert('Debe especificar un motivo para la aprobación sin pago');
        return;
      }
      
      this.certificadosService.aprobarCertificadoSinPago(
        this.solicitudSeleccionada.id,
        this.motivoAprobacionSinPago,
        this.observacionesAprobacion
      ).subscribe({
        next: (response) => {
          alert('Certificado aprobado exitosamente sin pago');
          this.obtenerSolicitudes();
          this.solicitudSeleccionada = null;
          this.motivoAprobacionSinPago = '';
          this.observacionesAprobacion = '';
        },
        error: (error) => {
          alert('Error al aprobar certificado: ' + (error?.error?.mensaje || 'Error desconocido'));
        }
      });
    }
  }

  rechazar() {
    this.certificadosService.rechazarSolicitud(this.solicitudSeleccionada.id, this.motivoRechazo).subscribe(() => {
      this.obtenerSolicitudes();
      this.solicitudSeleccionada = null;
      this.motivoRechazo = '';
    });
  }
} 