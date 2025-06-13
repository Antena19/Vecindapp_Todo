import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonicModule } from '@ionic/angular';
import { FormsModule } from '@angular/forms';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { SociosService } from 'src/app/services/socios.service';
import { SocioActivoDTO } from 'src/app/modelos/DTOs/socio-activo.dto';

@Component({
  selector: 'app-detalle-socio',
  templateUrl: './detalle-socio.component.html',
  styleUrls: ['./detalle-socio.component.scss'],
  standalone: true,
  imports: [CommonModule, IonicModule, FormsModule, RouterModule]
})
export class DetalleSocioComponent implements OnInit {
  socio: SocioActivoDTO | null = null;
  loading: boolean = true;
  error: string = '';
  private socioId: number | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private sociosService: SociosService
  ) { }

  ngOnInit() {
    const idSocio = this.route.snapshot.paramMap.get('id');
    if (idSocio) {
      this.socioId = +idSocio;
      this.cargarDetalleSocio(this.socioId);
    } else {
      this.error = 'ID de socio no proporcionado';
      this.loading = false;
    }
  }

  // Este método se ejecutará cada vez que la vista se active
  ionViewWillEnter() {
    if (this.socioId) {
      this.cargarDetalleSocio(this.socioId);
    }
  }

  cargarDetalleSocio(id: number) {
    this.loading = true;
    this.error = '';
    // Usamos listarTodosSocios para obtener todos los socios
    this.sociosService.listarTodosSocios().subscribe({
      next: (socios) => {
        const socioEncontrado = socios.find(s => s.idSocio === id);
        if (socioEncontrado) {
          this.socio = socioEncontrado;
          console.log('Valor de socio.estado recibido:', this.socio.estado);
        } else {
          this.error = 'Socio no encontrado';
        }
        this.loading = false;
      },
      error: (error) => {
        console.error('Error al cargar el detalle del socio:', error);
        this.error = 'Error al cargar el detalle del socio';
        this.loading = false;
      }
    });
  }

  irAEditar() {
    if (this.socio) {
      this.router.navigate(['/mantenedores/gestion-socios/editar', this.socio.idSocio]);
    }
  }

  formatearRut(rut: number, dv: string): string {
    if (!rut) return '';
    return `${rut}-${dv}`;
  }
}
