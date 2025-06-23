import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ComunicacionService } from '../../../services/comunicacion.service';
import { AutenticacionService } from '../../../services/autenticacion.service';
import { AlertController, LoadingController, ActionSheetController } from '@ionic/angular';
import { Camera, CameraResultType, CameraSource } from '@capacitor/camera';

@Component({
  selector: 'app-crear-noticia',
  templateUrl: './crear-noticia.component.html',
  styleUrls: ['./crear-noticia.component.scss'],
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, IonicModule]
})
export class CrearNoticiaComponent implements OnInit {
  noticiaForm: FormGroup;
  imagenSeleccionada: string | null = null;
  archivoImagen: File | null = null;
  usuarioActual: any;

  categorias = [
    { valor: 'NOTICIA', texto: 'Noticia' },
    { valor: 'EVENTO', texto: 'Evento' },
    { valor: 'AVISO', texto: 'Aviso' }
  ];

  alcances = [
    { valor: 'PUBLICO', texto: 'Público - Todos los vecinos' },
    { valor: 'SOCIOS', texto: 'Solo Socios' }
  ];

  prioridades = [
    { valor: 'BAJA', texto: 'Baja' },
    { valor: 'MEDIA', texto: 'Media' },
    { valor: 'ALTA', texto: 'Alta' }
  ];

  constructor(
    private formBuilder: FormBuilder,
    private comunicacionService: ComunicacionService,
    private autenticacionService: AutenticacionService,
    private router: Router,
    private alertController: AlertController,
    private loadingController: LoadingController,
    private actionSheetController: ActionSheetController
  ) {
    this.noticiaForm = this.formBuilder.group({
      titulo: ['', [Validators.required, Validators.minLength(5), Validators.maxLength(200)]],
      contenido: ['', [Validators.required, Validators.minLength(20)]],
      categoria: ['NOTICIA', Validators.required],
      alcance: ['PUBLICO', Validators.required],
      prioridad: ['MEDIA', Validators.required],
      tags: [''],
      publicarInmediatamente: [false]
    });
  }

  ngOnInit() {
    this.cargarUsuarioActual();
  }

  async cargarUsuarioActual() {
    this.usuarioActual = this.autenticacionService.obtenerUsuarioActual();
    if (!this.usuarioActual) {
      await this.mostrarError('No se pudo obtener la información del usuario');
      this.router.navigate(['/login']);
    }
  }

  async seleccionarImagen() {
    const actionSheet = await this.actionSheetController.create({
      header: 'Seleccionar imagen',
      buttons: [
        {
          text: 'Cámara',
          icon: 'camera',
          handler: () => {
            this.tomarFoto(CameraSource.Camera);
          }
        },
        {
          text: 'Galería',
          icon: 'image',
          handler: () => {
            this.tomarFoto(CameraSource.Photos);
          }
        },
        {
          text: 'Cancelar',
          icon: 'close',
          role: 'cancel'
        }
      ]
    });
    await actionSheet.present();
  }

  async tomarFoto(source: CameraSource) {
    try {
      const image = await Camera.getPhoto({
        quality: 80,
        allowEditing: false,
        resultType: CameraResultType.DataUrl,
        source: source
      });

      if (image.dataUrl) {
        this.imagenSeleccionada = image.dataUrl;
        // Convertir DataURL a File
        this.archivoImagen = this.dataURLtoFile(image.dataUrl, 'imagen.jpg');
      }
    } catch (error) {
      console.error('Error al tomar foto:', error);
    }
  }

  dataURLtoFile(dataurl: string, filename: string): File {
    const arr = dataurl.split(',');
    const mime = arr[0].match(/:(.*?);/)?.[1];
    const bstr = atob(arr[1]);
    let n = bstr.length;
    const u8arr = new Uint8Array(n);
    while (n--) {
      u8arr[n] = bstr.charCodeAt(n);
    }
    return new File([u8arr], filename, { type: mime });
  }

  eliminarImagen() {
    this.imagenSeleccionada = null;
    this.archivoImagen = null;
  }

  async guardarNoticia() {
    if (this.noticiaForm.invalid) {
      await this.mostrarError('Por favor completa todos los campos requeridos');
      return;
    }

    const loading = await this.loadingController.create({
      message: 'Guardando noticia...'
    });
    await loading.present();

    try {
      const noticiaData = {
        ...this.noticiaForm.value,
        autorRut: this.usuarioActual.rut,
        autorNombre: `${this.usuarioActual.nombre} ${this.usuarioActual.apellido}`,
        estado: this.noticiaForm.value.publicarInmediatamente ? 'ACTIVO' : 'INACTIVO',
        fechaCreacion: new Date(),
        fechaPublicacion: this.noticiaForm.value.publicarInmediatamente ? new Date() : null,
        tags: this.noticiaForm.value.tags ? this.noticiaForm.value.tags.split(',').map((tag: string) => tag.trim()) : []
      };

      // Crear la noticia
      const noticia = await this.comunicacionService.crearNoticia(noticiaData).toPromise();

      // Subir imagen si existe
      if (this.archivoImagen && noticia) {
        await this.comunicacionService.subirImagenNoticia(noticia.id, this.archivoImagen).toPromise();
      }

      // Si es de alta prioridad y se publica inmediatamente, enviar notificación
      if (noticia && this.noticiaForm.value.prioridad === 'ALTA' && this.noticiaForm.value.publicarInmediatamente) {
        await this.comunicacionService.notificarAvisoImportante(noticia.id).toPromise();
      }

      loading.dismiss();
      
      const mensaje = this.noticiaForm.value.publicarInmediatamente 
        ? 'Noticia publicada exitosamente' 
        : 'Noticia guardada como borrador';
      
      await this.mostrarExito(mensaje);
      this.router.navigate(['/comunicacion/gestion']);

    } catch (error) {
      console.error('Error al guardar noticia:', error);
      loading.dismiss();
      await this.mostrarError('Error al guardar la noticia');
    }
  }

  async guardarBorrador() {
    this.noticiaForm.patchValue({ publicarInmediatamente: false });
    await this.guardarNoticia();
  }

  async mostrarError(mensaje: string) {
    const alert = await this.alertController.create({
      header: 'Error',
      message: mensaje,
      buttons: ['OK']
    });
    await alert.present();
  }

  async mostrarExito(mensaje: string) {
    const alert = await this.alertController.create({
      header: 'Éxito',
      message: mensaje,
      buttons: ['OK']
    });
    await alert.present();
  }

  cancelar() {
    this.router.navigate(['/comunicacion']);
  }
} 