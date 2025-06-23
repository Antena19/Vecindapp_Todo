import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonicModule, AlertController } from '@ionic/angular';
import { Router, RouterModule } from '@angular/router';
import { ComunicacionService } from '../../../services/comunicacion.service';
import { Noticia } from '../../../modelos/comunicacion.model';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-lista-noticias',
  templateUrl: './lista-noticias.component.html',
  styleUrls: ['./lista-noticias.component.scss'],
  standalone: true,
  imports: [CommonModule, FormsModule, IonicModule, RouterModule]
})
export class ListaNoticiasComponent implements OnInit, OnDestroy {
  noticias: Noticia[] = [];
  noticiasFiltradas: Noticia[] = [];
  isLoading = false;
  textoBusqueda = '';
  filtroCategoria = '';
  filtroAlcance = '';
  filtroPrioridad = '';
  private noticiasSubscription: Subscription | null = null;

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
    private alertController: AlertController
  ) { }

  ngOnInit() {
    this.cargarNoticias();
  }

  ngOnDestroy() {
    if (this.noticiasSubscription) {
      this.noticiasSubscription.unsubscribe();
    }
  }

  cargarNoticias(): Promise<void> {
    this.isLoading = true;
    return new Promise((resolve) => {
      this.noticiasSubscription = this.comunicacionService.getNoticias().subscribe({
        next: (noticias) => {
          this.noticias = noticias;
          this.aplicarFiltros();
          this.isLoading = false;
          resolve();
        },
        error: (error) => {
          console.error('Error al cargar noticias:', error);
          this.isLoading = false;
          this.mostrarError('Error al cargar las noticias');
          resolve();
        }
      });
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