import { Component, OnInit, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonicModule } from '@ionic/angular';
import { Router, RouterModule } from '@angular/router';
import { AutenticacionService } from '../../services/autenticacion.service';
import { Usuario } from '../../modelos/Usuario';
import { SociosService } from 'src/app/services/socios.service';
import { SolicitudSocioDTO } from '../../modelos/DTOs/solicitud-socio.dto';
import { ComunicacionService } from 'src/app/services/comunicacion.service';
import { Noticia } from 'src/app/modelos/comunicacion.model';

interface MenuOption {
  title: string;
  icon: string;
  route: string;
}

@Component({
  selector: 'app-home',
  templateUrl: './home.page.html',
  styleUrls: ['./home.page.scss'],
  standalone: true,
  imports: [IonicModule, CommonModule, RouterModule],
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class HomePage implements OnInit {
  usuario: Usuario | null = null;
  tipoUsuario: string = 'vecino';
  solicitudSocioEstado: string | null = null;
  detallesSolicitud: SolicitudSocioDTO | null = null;
  noticiasDestacadas: Noticia[] = [];

  // Menús específicos por rol
  private menuVecino: MenuOption[] = [
    { 
      title: 'Mi Perfil', 
      icon: 'person-outline', 
      route: '/mi-perfil'
    },
    { 
      title: 'Noticias', 
      icon: 'newspaper-outline', 
      route: '/noticias'
    },
    { 
      title: 'Certificados', 
      icon: 'document-text-outline', 
      route: '/certificados'
    },
    { 
      title: 'Registro de Asistencia', 
      icon: 'calendar-outline', 
      route: '/eventos/registro-asistencia'
    },
    { 
      title: 'Hacerme Socio', 
      icon: 'people-outline', 
      route: '/mantenedores/gestion-socios/solicitar-membresia'
    }
  ];

  private menuSocio: MenuOption[] = [
    { 
      title: 'Mi Perfil', 
      icon: 'person-outline', 
      route: '/mi-perfil'
    },
    { 
      title: 'Noticias', 
      icon: 'newspaper-outline', 
      route: '/noticias'
    },
    { 
      title: 'Certificados', 
      icon: 'document-text-outline', 
      route: '/certificados'
    },
    { 
      title: 'Registro de Asistencia', 
      icon: 'calendar-outline', 
      route: '/eventos/registro-asistencia'
    }
  ];

  private menuDirectiva: MenuOption[] = [
    { 
      title: 'Mi Perfil', 
      icon: 'person-outline', 
      route: '/mi-perfil'
    },
    { 
      title: 'Noticias', 
      icon: 'newspaper-outline', 
      route: '/noticias'
    },
    { 
      title: 'Certificados', 
      icon: 'document-text-outline', 
      route: '/certificados'
    },
    { 
      title: 'Eventos', 
      icon: 'calendar-outline', 
      route: '/eventos'
    },
    { 
      title: 'Gestión de Socios', 
      icon: 'people-circle-outline', 
      route: '/mantenedores/gestion-socios'
    },
    { 
      title: 'Solicitudes', 
      icon: 'clipboard-outline', 
      route: '/mantenedores/gestion-socios/solicitudes'
    }
  ]

  menuOptions: MenuOption[] = [];

  constructor(
    private router: Router,
    private autenticacionService: AutenticacionService,
    private sociosService: SociosService,
    private comunicacionService: ComunicacionService
  ) {}

  ngOnInit() {
    this.autenticacionService.usuario.subscribe(user => {
      this.usuario = user;
      this.tipoUsuario = user?.tipo_usuario?.toLowerCase() || 'vecino';
      
      // Asignar menú según el rol
      switch(this.tipoUsuario) {
        case 'socio':
          this.menuOptions = this.menuSocio;
          break;
        case 'directiva':
          this.menuOptions = this.menuDirectiva;
          break;
        default: // 'vecino'
          this.menuOptions = this.menuVecino.filter(option => {
            if (option.title === 'Hacerme Socio') {
              return this.solicitudSocioEstado !== 'pendiente' && this.solicitudSocioEstado !== 'aprobada';
            }
            return true;
          });
      }

      // Si es vecino, consultar estado de solicitud de membresía
      if (this.tipoUsuario === 'vecino' && this.usuario) {
        this.consultarEstadoSolicitudSocio();
      }
    });

    this.cargarNoticiasDestacadas();

    // Suscribirse a los eventos de navegación para actualizar el estado
    this.router.events.subscribe(() => {
      if (this.tipoUsuario === 'vecino' && this.usuario) {
        this.consultarEstadoSolicitudSocio();
      }
    });
  }

  cargarNoticiasDestacadas() {
    this.comunicacionService.getNoticiasDestacadas().subscribe({
      next: (noticias) => {
        this.noticiasDestacadas = noticias;
      },
      error: (error) => {
        console.error('Error al cargar noticias destacadas:', error);
      }
    });
  }

  consultarEstadoSolicitudSocio() {
    this.sociosService.consultarEstadoSolicitudSocio().subscribe({
      next: (resp: SolicitudSocioDTO) => {
        if (resp) {
          this.detallesSolicitud = resp;
          this.solicitudSocioEstado = resp.estadoSolicitud;
        } else {
          this.solicitudSocioEstado = 'ninguna';
          this.detallesSolicitud = null;
        }
        
        // Actualizar el menú cuando cambie el estado de la solicitud
        if (this.tipoUsuario === 'vecino') {
          this.menuOptions = this.menuVecino.filter(option => {
            if (option.title === 'Hacerme Socio') {
              return this.solicitudSocioEstado !== 'pendiente' && this.solicitudSocioEstado !== 'aprobada';
            }
            return true;
          });
        }
      },
      error: (error) => {
        console.error('Error al consultar estado de solicitud:', error);
        this.solicitudSocioEstado = 'ninguna';
        this.detallesSolicitud = null;
        
        if (this.tipoUsuario === 'vecino') {
          this.menuOptions = this.menuVecino;
        }
      }
    });
  }

  getBienvenida(): string {
    return `Bienvenido, ${this.usuario?.nombre} ${this.usuario?.apellido_paterno}`;
  }

  navigateTo(route: string) {
    this.router.navigate([route]);
  }

  verDetalleNoticia(noticiaId: number) {
    this.router.navigate(['/comunicacion/detalle-noticia', noticiaId]);
  }
}