import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonicModule } from '@ionic/angular';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { SociosService } from 'src/app/services/socios.service';
import { SocioActivoDTO } from 'src/app/modelos/DTOs/socio-activo.dto';

@Component({
  selector: 'app-lista-socios',
  templateUrl: './lista-socios.component.html',
  styleUrls: ['./lista-socios.component.scss'],
  standalone: true,
  imports: [CommonModule, IonicModule, FormsModule, RouterModule]
})
export class ListaSociosComponent implements OnInit {
  socios: SocioActivoDTO[] = [];
  sociosFiltrados: SocioActivoDTO[] = [];
  loading: boolean = true;
  error: string = '';
  terminoBusqueda: string = '';
  tipoBusqueda: 'id' | 'rut' | 'nombre' = 'nombre';
  filtroEstado: 'todos' | 'activos' = 'activos';

  constructor(private sociosService: SociosService) { }

  ngOnInit() {
    this.cargarSocios();
  }

  cambiarFiltroEstado(event: any) {
    console.log('Toggle changed:', event.detail.checked);
    this.filtroEstado = event.detail.checked ? 'todos' : 'activos';
    this.cargarSocios();
  }

  cargarSocios() {
    this.loading = true;
    this.sociosService.listarTodosSocios().subscribe({
      next: (data) => {
        this.socios = data;
        this.sociosFiltrados = data;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error al cargar socios:', error);
        this.error = 'Error al cargar la lista de socios';
        this.loading = false;
      }
    });
  }

  buscar(event: any) {
    const termino = event?.target?.value?.toLowerCase() || '';
    this.terminoBusqueda = termino;
    this.aplicarFiltro();
  }

  cambiarTipoBusqueda(event: any) {
    const tipo = event?.detail?.value;
    if (tipo && ['id', 'rut', 'nombre'].includes(tipo)) {
      this.tipoBusqueda = tipo as 'id' | 'rut' | 'nombre';
      this.aplicarFiltro();
    }
  }

  private aplicarFiltro() {
    if (!this.terminoBusqueda) {
      this.sociosFiltrados = this.socios;
      return;
    }

    this.sociosFiltrados = this.socios.filter(socio => {
      switch (this.tipoBusqueda) {
        case 'id':
          return socio.idSocio.toString().includes(this.terminoBusqueda);
        case 'rut':
          const rutCompleto = `${socio.rut}${socio.dvRut}`;
          return rutCompleto.includes(this.terminoBusqueda);
        case 'nombre':
          return socio.nombreCompleto.toLowerCase().includes(this.terminoBusqueda);
        default:
          return true;
      }
    });
  }

  formatearRut(rut: number, dv: string): string {
    if (!rut) return '';
    return `${rut}-${dv}`;
  }

  verDetalle(id: number) {
    // La navegación se maneja en el template con routerLink
  }
}
