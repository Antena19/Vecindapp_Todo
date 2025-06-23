import { Component, OnInit } from '@angular/core';
import { IonicModule } from '@ionic/angular';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { CertificadosService } from '../certificados.service';

@Component({
  selector: 'app-dashboard-certificados',
  templateUrl: './dashboard-certificados.component.html',
  styleUrls: ['./dashboard-certificados.component.scss'],
  standalone: true,
  imports: [IonicModule, CommonModule, RouterModule]
})
export class DashboardCertificadosComponent implements OnInit {
  resumen = [
    { titulo: 'Solicitados', cantidad: 0, icono: 'document-outline', color: 'primary' },
    { titulo: 'Aprobados', cantidad: 0, icono: 'checkmark-done-outline', color: 'success' },
    { titulo: 'Rechazados', cantidad: 0, icono: 'close-circle-outline', color: 'danger' },
    { titulo: 'Emitidos', cantidad: 0, icono: 'ribbon-outline', color: 'tertiary' }
  ];

  constructor(private certificadosService: CertificadosService) {}

  ngOnInit() {
    this.certificadosService.obtenerResumenCertificados().subscribe(data => {
      this.resumen[0].cantidad = data.solicitados || 0;
      this.resumen[1].cantidad = data.aprobados || 0;
      this.resumen[2].cantidad = data.rechazados || 0;
      this.resumen[3].cantidad = data.emitidos || 0;
    });
  }
} 