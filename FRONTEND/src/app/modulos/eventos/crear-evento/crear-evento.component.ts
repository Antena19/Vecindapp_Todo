import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { IonicModule, ModalController, ToastController } from '@ionic/angular';
import { CommonModule } from '@angular/common';
import { EventosService } from '../eventos.service';
import { Evento } from 'src/app/modelos/evento.model';
import { Router } from '@angular/router';
import { AutenticacionService } from 'src/app/services/autenticacion.service';

@Component({
  selector: 'app-crear-evento',
  templateUrl: './crear-evento.component.html',
  styleUrls: ['./crear-evento.component.scss'],
  standalone: true,
  imports: [IonicModule, CommonModule, ReactiveFormsModule]
})
export class CrearEventoComponent implements OnInit {
  eventoForm: FormGroup;
  tiposEvento = ['Reunión', 'Fiesta', 'Taller', 'Otro'];

  constructor(
    private fb: FormBuilder,
    private eventosService: EventosService,
    private modalController: ModalController,
    private toastController: ToastController,
    private router: Router,
    private autenticacionService: AutenticacionService
  ) {
    this.eventoForm = this.fb.group({
      titulo: ['', Validators.required],
      descripcion: ['', Validators.required],
      fechaEvento: ['', Validators.required],
      horaEvento: ['', Validators.required],
      lugar: ['', Validators.required],
      notas: ['']
    });
  }

  ngOnInit() {}

  async onSubmit() {
    if (this.eventoForm.valid) {
      const usuarioActual = this.autenticacionService.obtenerUsuarioActual();
      const directivaRut = usuarioActual?.rut;

      if (!directivaRut) {
        const toast = await this.toastController.create({
          message: 'Error: No se pudo obtener el RUT del usuario logueado. Inicie sesión nuevamente.',
          duration: 3000,
          color: 'danger'
        });
        toast.present();
        return; // Detener la ejecución si no hay RUT
      }

      const evento: Evento = {
        ...this.eventoForm.value,
        directivaRut: directivaRut,
        estado: 'activo', // Estado por defecto para nuevos eventos
        codigoQr: '', // Se generará al ver el detalle o en el backend
        fechaCreacion: new Date().toISOString()
      };

      this.eventosService.crearEvento(evento).subscribe({
        next: async () => {
          const toast = await this.toastController.create({
            message: 'Evento creado exitosamente',
            duration: 2000,
            color: 'success'
          });
          toast.present();
          this.router.navigate(['/eventos/lista']);
        },
        error: async (error) => {
          console.error('Error al crear el evento:', error);
          const toast = await this.toastController.create({
            message: 'Error al crear el evento',
            duration: 2000,
            color: 'danger'
          });
          toast.present();
        }
      });
    }
  }
} 