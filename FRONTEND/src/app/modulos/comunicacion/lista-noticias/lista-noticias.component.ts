import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { Router } from '@angular/router';
import { ComunicacionService } from '../../../services/comunicacion.service';
import { Noticia } from '../../../modelos/comunicacion.model';
import { AlertController, LoadingController } from '@ionic/angular';

@Component({
  selector: 'app-lista-noticias',
  templateUrl: './lista-noticias.component.html',
  styleUrls: ['./lista-noticias.component.scss'],
  standalone: true,
  imports: [CommonModule, FormsModule, IonicModule]
})
export class ListaNoticiasComponent implements OnInit {
  noticias: Noticia[] = [];
  noticiasFiltradas: Noticia[] = [];
  loading = false;
  textoBusqueda = '';
  filtroCategoria = '';
  filtroAlcance = '';
  filtroPrioridad = '';

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

  prioridades = [
    { valor: '', texto: 'Todas las prioridades' },
    { valor: 'ALTA', texto: 'Alta Prioridad' },
    { valor: 'MEDIA', texto: 'Media Prioridad' },
    { valor: 'BAJA', texto: 'Baja Prioridad' }
  ];

  constructor(
    private comunicacionService: ComunicacionService,
    private router: Router,
    private alertController: AlertController,
    private loadingController: LoadingController
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

      const cumpleCategoria = !this.filtroCategoria || noticia.categoria === this.filtroCategoria;
      const cumpleAlcance = !this.filtroAlcance || noticia.alcance === this.filtroAlcance;
      const cumplePrioridad = !this.filtroPrioridad || noticia.prioridad === this.filtroPrioridad;

      return cumpleBusqueda && cumpleCategoria && cumpleAlcance && cumplePrioridad;
    });
  }

  buscarNoticias() {
    this.aplicarFiltros();
  }

  limpiarFiltros() {
    this.textoBusqueda = '';
    this.filtroCategoria = '';
    this.filtroAlcance = '';
    this.filtroPrioridad = '';
    this.aplicarFiltros();
  }

  verDetalleNoticia(noticiaId: number) {
    this.router.navigate(['/comunicacion/detalle', noticiaId]);
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

  doRefresh(event: any) {
    this.cargarNoticias().then(() => {
      event.target.complete();
    });
  }
} 