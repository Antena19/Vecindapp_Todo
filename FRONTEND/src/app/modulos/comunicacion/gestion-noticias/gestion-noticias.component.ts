import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { Router } from '@angular/router';
import { ComunicacionService } from '../../../services/comunicacion.service';
import { Noticia } from '../../../modelos/comunicacion.model';
import { AlertController, LoadingController, ToastController } from '@ionic/angular';

@Component({
  selector: 'app-gestion-noticias',
  templateUrl: './gestion-noticias.component.html',
  styleUrls: ['./gestion-noticias.component.scss'],
  standalone: true,
  imports: [CommonModule, FormsModule, IonicModule]
})
export class GestionNoticiasComponent implements OnInit {
  noticias: Noticia[] = [];
  noticiasFiltradas: Noticia[] = [];
  loading = false;
  textoBusqueda = '';
  filtroEstado = '';
  filtroCategoria = '';
  filtroAlcance = '';

  estados = [
    { valor: '', texto: 'Todos los estados' },
    { valor: 'ACTIVO', texto: 'Publicadas' },
    { valor: 'INACTIVO', texto: 'Borradores' }
  ];

  categorias = [
    { valor: '', texto: 'Todas las categorías' },
    { valor: 'NOTICIA', texto: 'Noticias' },
    { valor: 'EVENTO', texto: 'Eventos' },
    { valor: 'AVISO', texto: 'Avisos' }
  ];

  alcances = [
    { valor: '', texto: 'Todos los alcances' },
    { valor: 'PUBLICO', texto: 'Público' },
    { valor: 'SOCIOS', texto: 'Solo Socios' }
  ];

  constructor(
    private comunicacionService: ComunicacionService,
    private router: Router,
    private alertController: AlertController,
    private loadingController: LoadingController,
    private toastController: ToastController
  ) { }

  ngOnInit() {
    this.cargarNoticias();
  }

  async cargarNoticias() {
    this.loading = true;
    const loading = await this.loadingController.create({
      message: 'Cargando noticias...'
    });
    await loading.present();

    this.comunicacionService.getNoticias().subscribe({
      next: (noticias) => {
        this.noticias = noticias;
        this.aplicarFiltros();
        this.loading = false;
        loading.dismiss();
      },
      error: (error) => {
        console.error('Error al cargar noticias:', error);
        this.loading = false;
        loading.dismiss();
        this.mostrarError('Error al cargar las noticias');
      }
    });
  }

  aplicarFiltros() {
    this.noticiasFiltradas = this.noticias.filter(noticia => {
      const cumpleBusqueda = !this.textoBusqueda || 
        noticia.titulo.toLowerCase().includes(this.textoBusqueda.toLowerCase()) ||
        noticia.contenido.toLowerCase().includes(this.textoBusqueda.toLowerCase());

      const cumpleEstado = !this.filtroEstado || noticia.estado === this.filtroEstado;
      const cumpleCategoria = !this.filtroCategoria || noticia.categoria === this.filtroCategoria;
      const cumpleAlcance = !this.filtroAlcance || noticia.alcance === this.filtroAlcance;

      return cumpleBusqueda && cumpleEstado && cumpleCategoria && cumpleAlcance;
    });
  }

  buscarNoticias() {
    this.aplicarFiltros();
  }

  limpiarFiltros() {
    this.textoBusqueda = '';
    this.filtroEstado = '';
    this.filtroCategoria = '';
    this.filtroAlcance = '';
    this.aplicarFiltros();
  }

  editarNoticia(noticiaId: number) {
    this.router.navigate(['/comunicacion/editar', noticiaId]);
  }

  async publicarNoticia(noticiaId: number) {
    const alert = await this.alertController.create({
      header: 'Confirmar publicación',
      message: '¿Estás seguro de que quieres publicar esta noticia?',
      buttons: [
        {
          text: 'Cancelar',
          role: 'cancel'
        },
        {
          text: 'Publicar',
          handler: () => {
            this.comunicacionService.publicarNoticia(noticiaId).subscribe({
              next: () => {
                this.mostrarMensaje('Noticia publicada exitosamente');
                this.cargarNoticias();
              },
              error: (error) => {
                console.error('Error al publicar noticia:', error);
                this.mostrarError('Error al publicar la noticia');
              }
            });
          }
        }
      ]
    });
    await alert.present();
  }

  async eliminarNoticia(noticiaId: number) {
    const alert = await this.alertController.create({
      header: 'Confirmar eliminación',
      message: '¿Estás seguro de que quieres eliminar esta noticia? Esta acción no se puede deshacer.',
      buttons: [
        {
          text: 'Cancelar',
          role: 'cancel'
        },
        {
          text: 'Eliminar',
          handler: () => {
            this.comunicacionService.eliminarNoticia(noticiaId).subscribe({
              next: () => {
                this.mostrarMensaje('Noticia eliminada exitosamente');
                this.cargarNoticias();
              },
              error: (error) => {
                console.error('Error al eliminar noticia:', error);
                this.mostrarError('Error al eliminar la noticia');
              }
            });
          }
        }
      ]
    });
    await alert.present();
  }

  async notificarAvisoImportante(noticiaId: number) {
    const alert = await this.alertController.create({
      header: 'Enviar notificación push',
      message: '¿Quieres enviar una notificación push a todos los usuarios sobre esta noticia importante?',
      buttons: [
        {
          text: 'Cancelar',
          role: 'cancel'
        },
        {
          text: 'Enviar',
          handler: () => {
            this.comunicacionService.notificarAvisoImportante(noticiaId).subscribe({
              next: () => {
                this.mostrarMensaje('Notificación enviada exitosamente');
              },
              error: (error) => {
                console.error('Error al enviar notificación:', error);
                this.mostrarError('Error al enviar la notificación');
              }
            });
          }
        }
      ]
    });
    await alert.present();
  }

  getEstadoColor(estado: string): string {
    switch (estado) {
      case 'ACTIVO': return 'success';
      case 'INACTIVO': return 'warning';
      default: return 'medium';
    }
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

  doRefresh(event: any) {
    this.cargarNoticias().then(() => {
      event.target.complete();
    });
  }

  crearNuevaNoticia() {
    this.router.navigate(['/comunicacion/crear']);
  }
} 