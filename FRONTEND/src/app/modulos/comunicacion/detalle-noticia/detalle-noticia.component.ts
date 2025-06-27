import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { ActivatedRoute, Router } from '@angular/router';
import { ComunicacionService } from '../../../services/comunicacion.service';
import { Noticia, ComentarioNoticia } from '../../../modelos/comunicacion.model';
import { AlertController, LoadingController, ToastController } from '@ionic/angular';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

@Component({
  selector: 'app-detalle-noticia',
  templateUrl: './detalle-noticia.component.html',
  styleUrls: ['./detalle-noticia.component.scss'],
  standalone: true,
  imports: [CommonModule, IonicModule, ReactiveFormsModule]
})
export class DetalleNoticiaComponent implements OnInit {
  noticia: Noticia | null = null;
  comentarios: ComentarioNoticia[] = [];
  loading = false;
  comentariosLoading = false;
  comentarioForm: FormGroup;
  usuarioActual: any; // Se obtendrá del servicio de autenticación

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private comunicacionService: ComunicacionService,
    private alertController: AlertController,
    private loadingController: LoadingController,
    private toastController: ToastController,
    private formBuilder: FormBuilder
  ) {
    this.comentarioForm = this.formBuilder.group({
      contenido: ['', [Validators.required, Validators.minLength(5), Validators.maxLength(500)]]
    });
  }

  ngOnInit() {
    // Obtener usuario autenticado del localStorage
    const usuario = localStorage.getItem('usuario');
    if (usuario) {
      this.usuarioActual = JSON.parse(usuario);
    }

    const noticiaId = this.route.snapshot.paramMap.get('id');
    if (noticiaId) {
      this.cargarNoticia(parseInt(noticiaId));
      this.cargarComentarios(parseInt(noticiaId));
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

  async cargarComentarios(noticiaId: number) {
    this.comentariosLoading = true;
    
    this.comunicacionService.getComentariosNoticia(noticiaId).subscribe({
      next: (comentarios) => {
        this.comentarios = comentarios;
        this.comentariosLoading = false;
      },
      error: (error) => {
        console.error('Error al cargar comentarios:', error);
        this.comentariosLoading = false;
      }
    });
  }

  async agregarComentario() {
    if (this.comentarioForm.invalid || !this.noticia) {
      return;
    }

    const comentario = {
      contenido: this.comentarioForm.get('contenido')?.value,
      usuarioRut: this.usuarioActual?.rut || 0
    };

    this.comunicacionService.agregarComentario(this.noticia.id, comentario).subscribe({
      next: (nuevoComentario) => {
        this.comentarios.unshift(nuevoComentario);
        this.comentarioForm.reset();
        this.mostrarMensaje('Comentario agregado exitosamente');
      },
      error: (error) => {
        console.error('Error al agregar comentario:', error);
        this.mostrarError('Error al agregar el comentario');
      }
    });
  }

  async eliminarComentario(comentarioId: number) {
    const alert = await this.alertController.create({
      header: 'Confirmar eliminación',
      message: '¿Estás seguro de que quieres eliminar este comentario?',
      buttons: [
        {
          text: 'Cancelar',
          role: 'cancel'
        },
        {
          text: 'Eliminar',
          handler: () => {
            this.comunicacionService.eliminarComentario(comentarioId).subscribe({
              next: () => {
                this.comentarios = this.comentarios.filter(c => c.id !== comentarioId);
                this.mostrarMensaje('Comentario eliminado exitosamente');
              },
              error: (error) => {
                console.error('Error al eliminar comentario:', error);
                this.mostrarError('Error al eliminar el comentario');
              }
            });
          }
        }
      ]
    });
    await alert.present();
  }

  getPrioridadColor(prioridad: string): string {
    switch (prioridad) {
      case 'ALTA': return 'danger';
      case 'MEDIA': return 'warning';
      case 'BAJA': return 'success';
      default: return 'medium';
    }
  }

  getAlcanceColor(alcance: string): string {
    switch (alcance) {
      case 'PUBLICO': return 'primary';
      case 'SOCIOS': return 'secondary';
      default: return 'medium';
    }
  }

  getCategoriaIcon(categoria: string): string {
    switch (categoria) {
      case 'NOTICIA': return 'newspaper';
      case 'EVENTO': return 'calendar';
      case 'AVISO': return 'warning';
      default: return 'document';
    }
  }

  compartirNoticia() {
    if (navigator.share && this.noticia) {
      navigator.share({
        title: this.noticia.titulo,
        text: this.noticia.contenido,
        url: window.location.href
      });
    } else {
      // Fallback para navegadores que no soportan Web Share API
      this.copiarEnlace();
    }
  }

  async copiarEnlace() {
    try {
      await navigator.clipboard.writeText(window.location.href);
      this.mostrarMensaje('Enlace copiado al portapapeles');
    } catch (error) {
      console.error('Error al copiar enlace:', error);
      this.mostrarError('Error al copiar el enlace');
    }
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

  volver() {
    this.router.navigate(['/comunicacion']);
  }
} 