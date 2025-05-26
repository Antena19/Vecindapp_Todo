import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonicModule } from '@ionic/angular';
import { Router } from '@angular/router';
import { AutenticacionService } from '../../services/autenticacion.service';
import { Usuario } from '../../modelos/Usuario';
import { SociosService } from 'src/app/services/socios.service';

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
  imports: [IonicModule, CommonModule]
})
export class HomePage implements OnInit {
  usuario: Usuario | null = null;
  tipoUsuario: string = 'vecino';
  solicitudSocioEstado: string | null = null;

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
    },
    { 
      title: 'Estado de Cuotas', 
      icon: 'cash-outline', 
      route: '/estado-cuotas'
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
      title: 'Gestión de Vecinos', 
      icon: 'people-outline', 
      route: '/mantenedores/gestion-usuarios'
    },
    { 
      title: 'Solicitudes', 
      icon: 'clipboard-outline', 
      route: '/mantenedores/gestion-socios/solicitudes'
    },
    { 
      title: 'Gestión Financiera', 
      icon: 'stats-chart-outline', 
      route: '/gestion-financiera'
    }
  ]

  menuOptions: MenuOption[] = [];

  constructor(
    private router: Router,
    private autenticacionService: AutenticacionService,
    private sociosService: SociosService
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
            // Si es la opción "Hacerme Socio", solo mostrarla si no hay solicitud pendiente o aprobada
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
  }

  consultarEstadoSolicitudSocio() {
    this.sociosService.consultarEstadoSolicitudSocio().subscribe({
      next: (resp: any) => {
        this.solicitudSocioEstado = resp?.estadoSolicitud || resp?.estado_solicitud || 'ninguna';
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
      error: () => {
        this.solicitudSocioEstado = null;
      }
    });
  }

  getBienvenida(): string {
    return `Bienvenido, ${this.usuario?.nombre} ${this.usuario?.apellido_paterno}`;
  }

  navigateTo(route: string) {
    this.router.navigate([route]);
  }
}