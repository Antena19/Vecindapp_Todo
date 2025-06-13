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
      console.log('Elemento reportContent encontrado:', element);
      console.log('Contenido HTML del elemento:', element.innerHTML);
      this.cargando = true;
      
      // Configuración de opciones para html2pdf
      const opt = {
        margin: 1,
        filename: 'reporte_evento_' + this.eventoIdDesdeRuta + '.pdf',
        image: { type: 'jpeg', quality: 0.98 },
        html2canvas: { 
          scale: 2,
          useCORS: true,
          letterRendering: true,
          logging: true
        },
        jsPDF: { 
          unit: 'cm', 
          format: 'a4', 
          orientation: 'portrait' 
        }
      };

      // Crear un contenedor temporal para el contenido del PDF
      const pdfContentContainer = document.createElement('div');
      pdfContentContainer.style.padding = '20px';
      pdfContentContainer.style.backgroundColor = 'white';
      pdfContentContainer.style.color = 'black';
      pdfContentContainer.style.width = '21cm'; // Ancho A4
      pdfContentContainer.style.boxSizing = 'border-box';

      // Extraer y añadir el título y subtítulo
      const cardHeader = element.querySelector('ion-card-header');
      if (cardHeader) {
        const title = cardHeader.querySelector('ion-card-title');
        const subtitle = cardHeader.querySelector('ion-card-subtitle');
        if (title) pdfContentContainer.appendChild(title.cloneNode(true));
        if (subtitle) pdfContentContainer.appendChild(subtitle.cloneNode(true));
      }

      // Extraer y añadir el contenido principal
      const cardContent = element.querySelector('ion-card-content');
      if (cardContent) {
        pdfContentContainer.appendChild(cardContent.cloneNode(true));
      }

      // Añadir el contenedor temporal al cuerpo del documento (fuera de la vista)
      document.body.appendChild(pdfContentContainer);
      console.log('Contenedor temporal HTML antes de generar PDF:', pdfContentContainer.outerHTML);

      // Generar el PDF con un pequeño retraso
      setTimeout(() => {
        html2pdf().set(opt).from(pdfContentContainer).save().then(() => {
          this.cargando = false;
          this.presentToast('PDF generado exitosamente', 'success');
          document.body.removeChild(pdfContentContainer); // Eliminar el contenedor temporal
        }).catch((error: any) => {
          console.error('Error al generar el PDF:', error);
          this.presentToast('Error al generar el PDF', 'danger');
          this.cargando = false;
          document.body.removeChild(pdfContentContainer); // Eliminar el contenedor temporal incluso en error
        });
      }, 1000); // Retraso de 1000ms
    } else {
      this.presentToast('No se encontró el contenido del reporte para exportar', 'warning');
    }
  }

  get today(): string {
    return new Date().toISOString().substring(0, 10);
  }
} 