import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonicModule } from '@ionic/angular';
import { CertificadosService } from '../certificados.service';

@Component({
  selector: 'app-pago-certificado',
  templateUrl: './pago-certificado.component.html',
  styleUrls: ['./pago-certificado.component.css'],
  standalone: true,
  imports: [CommonModule, IonicModule]
})
export class PagoCertificadoComponent implements OnInit {
  solicitudes: any[] = [];
  solicitudSeleccionada: any = null;
  monto: number = 0;
  pagado: boolean = false;

  constructor(private certificadosService: CertificadosService) {}

  ngOnInit(): void {
    this.obtenerSolicitudes();
  }

  obtenerSolicitudes() {
    this.certificadosService.obtenerMisSolicitudes().subscribe(data => {
      this.solicitudes = data.filter((s: any) => s.estado === 'pendiente_pago');
    });
  }

  seleccionarSolicitud(solicitud: any) {
    this.solicitudSeleccionada = solicitud;
    this.monto = solicitud.monto;
    this.pagado = false;
  }

  pagar() {
    this.certificadosService.pagarCertificado(this.solicitudSeleccionada.id).subscribe(() => {
      this.pagado = true;
      this.obtenerSolicitudes();
    });
  }
} 