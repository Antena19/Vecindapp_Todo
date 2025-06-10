import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonicModule, ToastController } from '@ionic/angular';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { EventosService } from '../../../services/eventos.service';
import { Evento } from '../../../modelos/evento.model';
import { ActivatedRoute } from '@angular/router';
import { EventoReporte } from '../../../modelos/evento.model';
import * as html2pdf from 'html2pdf.js'; // Importar html2pdf

@Component({
  selector: 'app-reporte-eventos',
  templateUrl: './reporte-eventos.component.html',
  styleUrls: ['./reporte-eventos.component.scss'],
  standalone: true,
  imports: [CommonModule, IonicModule, ReactiveFormsModule]
})
export class ReporteEventosComponent implements OnInit {
  reporteForm: FormGroup;
  eventosReporte: Evento[] = [];
  totalAsistentes: number = 0;
  cargando: boolean = false;
  eventoIdDesdeRuta: number | null = null;
  reporteEventoUnico: EventoReporte | null = null;

  constructor(
    private fb: FormBuilder,
    private eventosService: EventosService,
    private toastController: ToastController,
    private route: ActivatedRoute
  ) {
    this.reporteForm = this.fb.group({
      fechaInicio: ['', Validators.required],
      fechaFin: ['', Validators.required]
    });
  }

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      const id = params['eventoId'];
      if (id) {
        this.eventoIdDesdeRuta = +id;
        this.reporteForm.disable();
        this.generarReportePorEvento(this.eventoIdDesdeRuta!);
      } else {
        const hoy = new Date();
        const primerDiaMes = new Date(hoy.getFullYear(), hoy.getMonth(), 1).toISOString().substring(0, 10);
        const ultimoDiaMes = new Date(hoy.getFullYear(), hoy.getMonth() + 1, 0).toISOString().substring(0, 10);

        this.reporteForm.patchValue({
          fechaInicio: primerDiaMes,
          fechaFin: ultimoDiaMes
        });
        this.generarReporte();
      }
    });
  }

  generarReporte() {
    if (this.reporteForm.valid) {
      this.cargando = true;
      const { fechaInicio, fechaFin } = this.reporteForm.value;
      this.eventosService.generarReporteAsistencia(new Date(fechaInicio), new Date(fechaFin)).subscribe({
        next: (data) => {
          this.eventosReporte = data;
          this.totalAsistentes = this.eventosReporte.reduce((sum, evento) => sum + (evento.total_asistentes || 0), 0);
          this.presentToast('Reporte generado exitosamente', 'success');
          this.cargando = false;
        },
        error: (error) => {
          console.error('Error al generar el reporte:', error);
          this.presentToast('Error al generar el reporte', 'danger');
          this.eventosReporte = [];
          this.totalAsistentes = 0;
          this.cargando = false;
        }
      });
    } else {
      this.presentToast('Por favor, selecciona un rango de fechas válido.', 'warning');
    }
  }

  generarReportePorEvento(eventoId: number) {
    this.cargando = true;
    this.eventosService.generarReporteAsistenciaPorEventoId(eventoId).subscribe({
      next: (data: EventoReporte) => {
        this.reporteEventoUnico = data;
        this.eventosReporte = [];
        this.totalAsistentes = data.totalAsistentes;
        console.log('Datos de reporte de evento único cargados:', this.reporteEventoUnico);
        this.presentToast('Reporte generado exitosamente para el evento ID: ' + eventoId, 'success');
        this.cargando = false;
      },
      error: (error) => {
        console.error('Error al generar el reporte por evento:', error);
        this.presentToast('Error al generar el reporte por evento.', 'danger');
        this.eventosReporte = [];
        this.totalAsistentes = 0;
        this.cargando = false;
      }
    });
  }

  async presentToast(message: string, color: string) {
    const toast = await this.toastController.create({
      message: message,
      duration: 2000,
      color: color
    });
    toast.present();
  }

  exportReportToPdf() {
    const element = document.getElementById('reportContent');
    if (element) {
      this.cargando = true;
      setTimeout(() => {
        console.log('Contenido del elemento para exportar (dentro de setTimeout):', element.innerHTML);
        html2pdf().from(element).save('reporte_evento_' + this.eventoIdDesdeRuta + '.pdf').then(() => {
          this.cargando = false;
        }).catch((error: any) => {
          console.error('Error al generar el PDF:', error);
          this.presentToast('Error al generar el PDF.', 'danger');
          this.cargando = false;
        });
      }, 500);
    } else {
      this.presentToast('No se encontró el contenido del reporte para exportar.', 'warning');
    }
  }

  get today(): string {
    return new Date().toISOString().substring(0, 10);
  }
} 