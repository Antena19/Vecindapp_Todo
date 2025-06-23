import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ComunicacionService } from '../../../services/comunicacion.service';
import { Noticia } from '../../../modelos/comunicacion.model';
import { AlertController, LoadingController, ToastController } from '@ionic/angular';

@Component({
  selector: 'app-editar-noticia',
  templateUrl: './editar-noticia.component.html',
  styleUrls: ['./editar-noticia.component.scss'],
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, IonicModule]
})
export class EditarNoticiaComponent implements OnInit {
  noticiaForm: FormGroup;
  noticia: Noticia | null = null;
  loading = false;
  imagenSeleccionada: string | null = null;
  imagenArchivo: File | null = null;

  categorias = [
    { valor: 'NOTICIA', texto: 'Noticia' },
    { valor: 'EVENTO', texto: 'Evento' },
    { valor: 'AVISO', texto: 'Aviso' }
  ];

  alcances = [
    { valor: 'PUBLICO', texto: 'Público' },
    { valor: 'SOCIOS', texto: 'Solo Socios' }
  ];

  prioridades = [
    { valor: 'BAJA', texto: 'Baja' },
    { valor: 'MEDIA', texto: 'Media' },
    { valor: 'ALTA', texto: 'Alta' }
  ];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private formBuilder: FormBuilder,
    private comunicacionService: ComunicacionService,
    private alertController: AlertController,
    private loadingController: LoadingController,
    private toastController: ToastController
  ) {
    this.noticiaForm = this.formBuilder.group({
      titulo: ['', [Validators.required, Validators.minLength(5), Validators.maxLength(200)]],
      contenido: ['', [Validators.required, Validators.minLength(20)]],
      categoria: ['', Validators.required],
      alcance: ['', Validators.required],
      prioridad: ['', Validators.required],
      tags: [''],
      publicarInmediatamente: [false]
    });
  }

  ngOnInit() {
    const noticiaId = this.route.snapshot.paramMap.get('id');
    if (noticiaId) {
      this.cargarNoticia(parseInt(noticiaId));
    }
  }

  async cargarNoticia(id: number) {
    this.loading = true;
    const loading = await this.loadingController.create({
      message: 'Cargando noticia...'
    });
    await loading.present();

    this.comunicacionService.getNoticia(id).subscribe({
      next: (noticia) => {
        this.noticia = noticia;
        this.cargarDatosEnFormulario(noticia);
        this.loading = false;
        loading.dismiss();
      },
      error: (error) => {
        console.error('Error al cargar noticia:', error);
        this.loading = false;
        loading.dismiss();
        this.mostrarError('Error al cargar la noticia');
      }
    });
  }

  cargarDatosEnFormulario(noticia: Noticia) {
    this.noticiaForm.patchValue({
      titulo: noticia.titulo,
      contenido: noticia.contenido,
      categoria: noticia.categoria,
      alcance: noticia.alcance,
      prioridad: noticia.prioridad,
      tags: noticia.tags ? noticia.tags.join(', ') : '',
      publicarInmediatamente: noticia.estado === 'ACTIVO'
    });

    if (noticia.imagenUrl) {
      this.imagenSeleccionada = noticia.imagenUrl;
    }
  }

  async seleccionarImagen() {
    const input = document.createElement('input');
    input.type = 'file';
    input.accept = 'image/*';
    input.onchange = (event: any) => {
      const file = event.target.files[0];
      if (file) {
        this.imagenArchivo = file;
        const reader = new FileReader();
        reader.onload = (e: any) => {
          this.imagenSeleccionada = e.target.result;
        };
        reader.readAsDataURL(file);
      }
    };
    input.click();
  }

  eliminarImagen() {
    this.imagenSeleccionada = null;
    this.imagenArchivo = null;
  }

  async guardarNoticia() {
    if (this.noticiaForm.invalid || !this.noticia) {
      return;
    }

    const loading = await this.loadingController.create({
      message: 'Guardando noticia...'
    });
    await loading.present();

    const formData = this.noticiaForm.value;
    const noticiaActualizada = {
      titulo: formData.titulo,
      contenido: formData.contenido,
      categoria: formData.categoria,
      alcance: formData.alcance,
      prioridad: formData.prioridad,
      tags: formData.tags ? formData.tags.split(',').map((tag: string) => tag.trim()) : [],
      estado: formData.publicarInmediatamente ? 'ACTIVO' as const : 'INACTIVO' as const
    };

    this.comunicacionService.actualizarNoticia(this.noticia.id, noticiaActualizada).subscribe({
      next: async (noticia) => {
        // Subir imagen si se seleccionó una nueva
        if (this.imagenArchivo) {
          try {
            await this.subirImagen(noticia.id);
          } catch (error) {
            console.error('Error al subir imagen:', error);
          }
        }

        loading.dismiss();
        this.mostrarMensaje('Noticia actualizada exitosamente');
        this.router.navigate(['/comunicacion/gestion']);
      },
      error: (error) => {
        console.error('Error al actualizar noticia:', error);
        loading.dismiss();
        this.mostrarError('Error al actualizar la noticia');
      }
    });
  }

  async subirImagen(noticiaId: number): Promise<void> {
    if (!this.imagenArchivo) return;

    return new Promise((resolve, reject) => {
      this.comunicacionService.subirImagenNoticia(noticiaId, this.imagenArchivo!).subscribe({
        next: () => resolve(),
        error: (error) => reject(error)
      });
    });
  }

  async guardarBorrador() {
    if (this.noticiaForm.invalid || !this.noticia) {
      return;
    }

    const formData = this.noticiaForm.value;
    const noticiaActualizada = {
      titulo: formData.titulo,
      contenido: formData.contenido,
      categoria: formData.categoria,
      alcance: formData.alcance,
      prioridad: formData.prioridad,
      tags: formData.tags ? formData.tags.split(',').map((tag: string) => tag.trim()) : [],
      estado: 'INACTIVO' as const
    };

    const loading = await this.loadingController.create({
      message: 'Guardando borrador...'
    });
    await loading.present();

    this.comunicacionService.actualizarNoticia(this.noticia.id, noticiaActualizada).subscribe({
      next: () => {
        loading.dismiss();
        this.mostrarMensaje('Borrador guardado exitosamente');
      },
      error: (error) => {
        console.error('Error al guardar borrador:', error);
        loading.dismiss();
        this.mostrarError('Error al guardar el borrador');
      }
    });
  }

  async cancelar() {
    const alert = await this.alertController.create({
      header: 'Confirmar cancelación',
      message: '¿Estás seguro de que quieres cancelar? Los cambios no guardados se perderán.',
      buttons: [
        {
          text: 'Continuar editando',
          role: 'cancel'
        },
        {
          text: 'Cancelar',
          handler: () => {
            this.router.navigate(['/comunicacion/gestion']);
          }
        }
      ]
    });
    await alert.present();
  }

  async mostrarError(mensaje: string) {
    const alert = await this.alertController.create({
      header: 'Error',
      message: mensaje,
      buttons: ['OK']
    });
    await alert.present();
  }

  async mostrarMensaje(mensaje: string) {
    const toast = await this.toastController.create({
      message: mensaje,
      duration: 2000,
      position: 'bottom'
    });
    await toast.present();
  }
} 