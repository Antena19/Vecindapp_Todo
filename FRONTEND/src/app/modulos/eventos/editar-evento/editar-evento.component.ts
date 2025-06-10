import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { IonicModule, ToastController } from '@ionic/angular';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute } from '@angular/router';
import { EventosService } from '../../../services/eventos.service';
import { Evento } from 'src/app/modelos/evento.model';

@Component({
  selector: 'app-editar-evento',
  templateUrl: './editar-evento.component.html',
  styleUrls: ['./editar-evento.component.scss'],
  standalone: true,
  imports: [IonicModule, CommonModule, ReactiveFormsModule]
})
export class EditarEventoComponent implements OnInit {
  evento!: Evento;
  eventoForm: FormGroup;

  constructor(
    private fb: FormBuilder,
    private eventosService: EventosService,
    private toastController: ToastController,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.eventoForm = this.fb.group({
      titulo: ['', Validators.required],
      descripcion: ['', Validators.required],
      fechaEvento: ['', Validators.required],
      horaEvento: ['', Validators.required],
      lugar: ['', Validators.required],
      notas: [''],
      estado: ['', Validators.required]
    });
  }

  ngOnInit() {
    console.log('ngOnInit en EditarEventoComponent');
    this.route.paramMap.subscribe(params => {
      const eventoId = Number(params.get('id'));
      console.log('ID del evento desde la URL:', eventoId);
      if (eventoId) {
        this.eventosService.getEvento(eventoId).subscribe({
          next: (evento: Evento) => {
            this.evento = evento;
            console.log('Evento cargado exitosamente:', this.evento);
            this.eventoForm.patchValue(this.evento);
            console.log('Formulario rellenado con datos del evento.');
          },
          error: (error: any) => {
            console.error('Error al obtener el evento:', error);
            this.presentToast('Error al cargar el evento', 'danger');
            this.router.navigate(['/eventos/lista']);
          }
        });
      } else {
        console.warn('No se proporcionó un ID de evento en la URL.');
        this.presentToast('ID de evento no proporcionado', 'danger');
        this.router.navigate(['/eventos/lista']);
      }
    });
  }

  async onSubmit() {
    if (this.eventoForm.valid) {
      console.log('Formulario válido, enviando cambios...');

      const formValues = this.eventoForm.value;

      // Formatear horaEvento a HH:MM:00 si es necesario
      let formattedHoraEvento = formValues.horaEvento;
      if (formattedHoraEvento && formattedHoraEvento.match(/^\d{2}:\d{2}$/)) {
        formattedHoraEvento += ':00';
      }

      const eventoParaActualizar: Partial<Evento> = {
        titulo: formValues.titulo,
        descripcion: formValues.descripcion,
        fechaEvento: formValues.fechaEvento,
        horaEvento: formattedHoraEvento, // Usar la hora formateada
        lugar: formValues.lugar,
        notas: formValues.notas,
        estado: formValues.estado,
        codigoQr: this.evento.codigoQr,
        directivaRut: this.evento.directivaRut,
        fechaCreacion: this.evento.fechaCreacion
      };

      console.log('Evento a actualizar:', eventoParaActualizar);

      this.eventosService.actualizarEvento(this.evento.id, eventoParaActualizar).subscribe({
        next: async () => {
          this.presentToast('Evento editado exitosamente', 'success');
          console.log('Evento editado exitosamente, navegando a detalle.');
          this.router.navigate(['/eventos/detalle', this.evento.id]);
        },
        error: async (error) => {
          console.error('Error al editar el evento:', error);
          this.presentToast('Error al editar el evento', 'danger');
        }
      });
    }
  }

  async presentToast(message: string, color: string) {
    const toast = await this.toastController.create({
      message: message,
      duration: 2000,
      color: color
    });
    toast.present();
  }

  volverADetalle() {
    console.log('Botón Cancelar presionado.');
    if (this.evento && this.evento.id) {
      console.log('Navegando al detalle del evento con ID:', this.evento.id);
      this.router.navigate(['/eventos/detalle', this.evento.id]);
    } else {
      console.warn('No se pudo navegar al detalle del evento: ID de evento no disponible.', this.evento);
      this.router.navigate(['/eventos/lista']); // Navegar a la lista como fallback
    }
  }
} 