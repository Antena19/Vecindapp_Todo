import { Component, OnInit, OnDestroy, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonicModule, AlertController } from '@ionic/angular';
import { Router, RouterModule, NavigationEnd } from '@angular/router';
import { AutenticacionService } from '../../services/autenticacion.service';
import { Usuario } from '../../modelos/Usuario';
import { SociosService } from 'src/app/services/socios.service';
import { SolicitudSocioDTO } from '../../modelos/DTOs/solicitud-socio.dto';
import { ComunicacionService } from 'src/app/services/comunicacion.service';
import { Noticia } from 'src/app/modelos/comunicacion.model';
import { Subscription, filter } from 'rxjs';

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
export class HomePage implements OnInit, OnDestroy {
  usuario: Usuario | null = null;
  tipoUsuario: string = 'vecino';
  solicitudSocioEstado: string | null = null;
  detallesSolicitud: SolicitudSocioDTO | null = null;
  noticiasDestacadas: Noticia[] = [];
  isLoading = true;
  private subscriptions = new Subscription();

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
      route: '/comunicacion'
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
      route: '/comunicacion'
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
      route: '/comunicacion'
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
    private comunicacionService: ComunicacionService,
    private alertController: AlertController
  ) {}

  ngOnInit() {
    this.isLoading = true;
    const userSub = this.autenticacionService.usuario.subscribe(user => {
      this.usuario = user;
      this.tipoUsuario = user?.tipo_usuario?.toLowerCase() || 'vecino';
      this.actualizarMenu();
      if (this.tipoUsuario === 'vecino' && this.usuario) {
        this.consultarEstadoSolicitudSocio();
      }
    });
    this.subscriptions.add(userSub);

    this.cargarNoticiasDestacadas();

    const routerSub = this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe(() => {
      if (this.tipoUsuario === 'vecino' && this.usuario) {
        this.consultarEstadoSolicitudSocio();
      }
    });
    this.subscriptions.add(routerSub);
  }

  ngOnDestroy() {
    this.subscriptions.unsubscribe();
  }

  actualizarMenu() {
    switch(this.tipoUsuario) {
      case 'socio':
        this.menuOptions = this.menuSocio;
        break;
      case 'directiva':
        this.menuOptions = this.menuDirectiva;
        break;
      default:
        this.menuOptions = this.menuVecino.filter(option => {
          if (option.title === 'Hacerme Socio') {
            return this.solicitudSocioEstado !== 'pendiente' && this.solicitudSocioEstado !== 'aprobada';
          }
          return true;
        });
    }
  }

  cargarNoticiasDestacadas() {
    this.isLoading = true;
    const noticiasSub = this.comunicacionService.getNoticiasDestacadas().subscribe({
      next: (noticias) => {
        this.noticiasDestacadas = noticias;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error al cargar noticias destacadas:', error);
        this.isLoading = false;
        this.mostrarError('No se pudieron cargar las noticias destacadas.');
      }
    });
    this.subscriptions.add(noticiasSub);
  }

  consultarEstadoSolicitudSocio() {
    const solicitudSub = this.sociosService.consultarEstadoSolicitudSocio().subscribe({
      next: (resp: SolicitudSocioDTO) => {
        if (resp) {
          this.detallesSolicitud = resp;
          this.solicitudSocioEstado = resp.estadoSolicitud;
        } else {
          this.solicitudSocioEstado = 'ninguna';
          this.detallesSolicitud = null;
        }
        this.actualizarMenu();
      },
      error: (error) => {
        console.error('Error al consultar estado de solicitud:', error);
        this.solicitudSocioEstado = 'ninguna';
        this.detallesSolicitud = null;
        this.actualizarMenu();
      }
    });
    this.subscriptions.add(solicitudSub);
  }

  async mostrarError(mensaje: string) {
    const alert = await this.alertController.create({
      header: 'Error',
      message: mensaje,
      buttons: ['OK']
    });
    await alert.present();
  }

  getBienvenida(): string {
    return `Bienvenido, ${this.usuario?.nombre} ${this.usuario?.apellido_paterno}`;
  }

  navigateTo(route: string) {
    this.router.navigate([route]);
  }

  verDetalleNoticia(noticiaId: number) {
    this.router.navigate(['/comunicacion/detalle', noticiaId]);
  }

  doRefresh(event: any) {
    // Recargar todos los datos de la página
    this.cargarNoticiasDestacadas();
    if (this.tipoUsuario === 'vecino' && this.usuario) {
      this.consultarEstadoSolicitudSocio();
    }
    // Simular que la recarga ha terminado para que el refresher se oculte
    setTimeout(() => {
      event.target.complete();
    }, 1000);
  }
}