import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonicModule, ToastController } from '@ionic/angular';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { SociosService } from 'src/app/services/socios.service';
import { SocioActivoDTO } from 'src/app/modelos/DTOs/socio-activo.dto';

interface EstadoSocio {
  estado: boolean;
  motivo?: string;
}

@Component({
  selector: 'app-editar-socio',
  templateUrl: './editar-socio.component.html',
  styleUrls: ['./editar-socio.component.scss'],
  standalone: true,
  imports: [CommonModule, IonicModule, FormsModule, ReactiveFormsModule, RouterModule]
})
export class EditarSocioComponent implements OnInit {
  socio: SocioActivoDTO | null = null;
  loading: boolean = true;
  error: string = '';
  guardando: boolean = false;
  estadoActual: EstadoSocio = {
    estado: true,
    motivo: ''
  };

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private sociosService: SociosService,
    private toastController: ToastController
  ) { }

  ngOnInit() {
    const idSocio = this.route.snapshot.paramMap.get('id');
    if (idSocio) {
      this.cargarSocio(+idSocio);
    } else {
      this.error = 'ID de socio no proporcionado';
      this.loading = false;
    }
  }

  cargarSocio(id: number) {
    this.loading = true;
    // Usamos listarTodosSocios para obtener todos los socios, incluyendo inactivos
    this.sociosService.listarTodosSocios().subscribe({
      next: (socios) => {
        const socioEncontrado = socios.find(s => s.idSocio === id);
        if (socioEncontrado) {
          this.socio = socioEncontrado;
          console.log('Valor de socioEncontrado.estado en editar-socio:', socioEncontrado.estado, typeof socioEncontrado.estado);
          // Establecemos el estado actual basado en el estado del socio
          this.estadoActual.estado = socioEncontrado.estado;
          this.estadoActual.motivo = socioEncontrado.motivoDesactivacion;
        } else {
          this.error = 'Socio no encontrado';
        }
        this.loading = false;
      },
      error: (error) => {
        console.error('Error al cargar el socio:', error);
        this.error = 'Error al cargar la información del socio';
        this.loading = false;
      }
    });
  }

  async guardarCambios() {
    if (!this.socio) return;

    this.guardando = true;
    try {
      // Requerimos motivo tanto para activar como para desactivar
      if (!this.estadoActual.motivo) {
        const accion = this.estadoActual.estado ? 'activar' : 'desactivar';
        const errorMessage = `Debe proporcionar un motivo para ${accion} al socio`;
        await this.presentToast(errorMessage, 'danger');
        this.guardando = false;
        return;
      }

      this.sociosService.actualizarEstadoSocio(
        this.socio.idSocio,
        this.estadoActual.estado,
        this.estadoActual.motivo
      ).subscribe({
        next: async () => {
          await this.presentToast('Estado del socio actualizado correctamente', 'success');
          this.router.navigate(['/mantenedores/gestion-socios/detalle', this.socio?.idSocio]);
        },
        error: async (error) => {
          const errorMessage = error.error?.mensaje || 'Error al actualizar el estado del socio';
          await this.presentToast(errorMessage, 'danger');
          console.error('Error al actualizar el estado del socio:', error);
          this.guardando = false;
        }
      });
    } catch (error: any) {
      const errorMessage = error.message || 'Error al guardar los cambios';
      await this.presentToast(errorMessage, 'danger');
      this.guardando = false;
    }
  }

  async presentToast(message: string, color: string) {
    const toast = await this.toastController.create({
      message: message,
      duration: 3000,
      position: 'bottom',
      color: color,
    });
    toast.present();
  }

  formatearRut(rut: number, dv: string): string {
    if (!rut) return '';
    return `${rut}-${dv}`;
  }
}
