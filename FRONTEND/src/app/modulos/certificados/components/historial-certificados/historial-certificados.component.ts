import { Component, OnInit, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { IonicModule } from '@ionic/angular';
import { CertificadoService } from '../../../../services/certificado.service';
import { Certificado } from '../../../../modelos/certificado.interface';

@Component({
  selector: 'app-historial-certificados',
  templateUrl: './historial-certificados.component.html',
  styleUrls: ['./historial-certificados.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    IonicModule
  ],
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class HistorialCertificadosComponent implements OnInit {
  certificados: Certificado[] = [];
  certificadosFiltrados: Certificado[] = [];
  filtroEstado: string = 'TODOS';
  cargando = false;
  pagina = 1;
  hayMas = true;

  constructor(private certificadoService: CertificadoService) {}

  ngOnInit() {
    this.cargarCertificados();
  }

  cargarCertificados() {
    if (this.cargando || !this.hayMas) return;
    
    this.cargando = true;
    this.certificadoService.obtenerCertificados(this.pagina).subscribe({
      next: (response: Certificado[]) => {
        this.certificados = [...this.certificados, ...response];
        this.filtrarPorEstado(this.filtroEstado);
        this.cargando = false;
        this.hayMas = response.length > 0;
        this.pagina++;
      },
      error: (error: any) => {
        console.error('Error al cargar certificados:', error);
        this.cargando = false;
      }
    });
  }

  filtrarPorEstado(estado: any) {
    this.filtroEstado = String(estado);
    if (this.filtroEstado === 'TODOS') {
      this.certificadosFiltrados = this.certificados;
    } else {
      this.certificadosFiltrados = this.certificados.filter(
        cert => cert.estado === this.filtroEstado
      );
    }
  }

  getEstadoColor(estado: string): string {
    switch (estado) {
      case 'PENDIENTE':
        return 'warning';
      case 'APROBADO':
        return 'success';
      case 'RECHAZADO':
        return 'danger';
      case 'PAGADO':
        return 'primary';
      case 'EMITIDO':
        return 'secondary';
      default:
        return 'medium';
    }
  }

  cargarMas(event: any) {
    this.cargarCertificados();
    event.target.complete();
  }
} 