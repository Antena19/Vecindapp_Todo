import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonicModule } from '@ionic/angular';
import { CertificadosService } from '../certificados.service';

@Component({
  selector: 'app-historial-certificados',
  templateUrl: './historial-certificados.component.html',
  styleUrls: ['./historial-certificados.component.css'],
  standalone: true,
  imports: [CommonModule, IonicModule]
})
export class HistorialCertificadosComponent implements OnInit {
  historial: any[] = [];

  constructor(private certificadosService: CertificadosService) {}

  ngOnInit(): void {
    this.certificadosService.obtenerHistorial().subscribe(data => {
      this.historial = data;
    });
  }

  descargar(certificadoId: number) {
    this.certificadosService.descargarCertificado(certificadoId);
  }
} 