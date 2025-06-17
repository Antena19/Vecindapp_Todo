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

  constructor(@Inject(CertificadosService) private certificadosService: CertificadosService) {}

  ngOnInit(): void {
    this.obtenerSolicitudes();
  }

  obtenerSolicitudes() {
    this.certificadosService.obtenerSolicitudesPendientes().subscribe(data => {
      this.solicitudes = data;
    });
  }

  seleccionarSolicitud(solicitud: any) {
    this.solicitudSeleccionada = solicitud;
  }

  aprobar() {
    this.certificadosService.aprobarSolicitud(this.solicitudSeleccionada.id).subscribe(() => {
      this.obtenerSolicitudes();
      this.solicitudSeleccionada = null;
    });
  }

  rechazar() {
    this.certificadosService.rechazarSolicitud(this.solicitudSeleccionada.id, this.motivoRechazo).subscribe(() => {
      this.obtenerSolicitudes();
      this.solicitudSeleccionada = null;
      this.motivoRechazo = '';
    });
  }
} 