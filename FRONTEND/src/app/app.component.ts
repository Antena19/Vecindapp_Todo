import { Component, OnInit } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';
import { AutenticacionService } from './services/autenticacion.service';
import { Usuario } from './modelos/Usuario';
import { SociosService } from './services/socios.service';

// Definir una interfaz para las opciones del menú
interface MenuOption {
  title: string;
  url: string;
  icon: string;
}

@Component({
  selector: 'app-root',
  templateUrl: 'app.component.html',
  styleUrls: ['app.component.scss'],
  standalone: false,
})
export class AppComponent implements OnInit {
  public showMenu = false;
  public appPages: MenuOption[] = []; // Usar la interfaz para el tipado
  public currentUser: Usuario | null = null;
  public solicitudSocioEstado: string | null = null;
  
  // Definición de menús por rol
  private menuVecino: MenuOption[] = [
    { title: 'Home', url: '/home', icon: 'home' },
    { title: 'Mi Perfil', url: '/mi-perfil', icon: 'person' },
    { title: 'Noticias', url: '/noticias', icon: 'newspaper' },
    { title: 'Certificados', url: '/certificados', icon: 'document-text' },
    { title: 'Registro de Asistencia', url: '/eventos/registro-asistencia', icon: 'calendar' },
    { title: 'Solicitar Membresía', url: '/mantenedores/gestion-socios/solicitar-membresia', icon: 'people' },
  ];
  
  private menuSocio: MenuOption[] = [
    { title: 'Home', url: '/home', icon: 'home' },
    { title: 'Mi Perfil', url: '/mi-perfil', icon: 'person' },
    { title: 'Noticias', url: '/noticias', icon: 'newspaper' },
    { title: 'Certificados', url: '/certificados', icon: 'document-text' },
    { title: 'Registro de Asistencia', url: '/eventos/registro-asistencia', icon: 'calendar' },
  ];
  
  private menuDirectiva: MenuOption[] = [
    { title: 'Home', url: '/home', icon: 'home' },
    { title: 'Mi Perfil', url: '/mi-perfil', icon: 'person' },
    { title: 'Noticias', url: '/noticias', icon: 'newspaper' },
    { title: 'Certificados', url: '/certificados', icon: 'document-text' },
    { title: 'Eventos', url: '/eventos', icon: 'calendar' },
    { title: 'Gestión de Socios', url: '/mantenedores/gestion-socios', icon: 'people-circle' },
  ];

  constructor(
    private router: Router,
    private autenticacionService: AutenticacionService,
    private sociosService: SociosService
  ) {}

  ngOnInit() {
    // Determinar cuándo mostrar el menú
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe((event: NavigationEnd) => {
      // No mostrar menú en páginas de login, registro e inicio
      const hideMenuRoutes = ['/login', '/registro', '/inicio', '/recuperar-password'];
      this.showMenu = !hideMenuRoutes.some(route => event.url.startsWith(route));
      
      // Actualizar menú si debe mostrarse
      if (this.showMenu) {
        this.updateMenu();
      }
    });

    // Suscribirse a cambios en el usuario
    this.autenticacionService.usuario.subscribe(usuario => {
      this.currentUser = usuario;
      if (this.getUserRole() === 'vecino' && usuario) {
        this.consultarEstadoSolicitudSocio();
      } else {
        this.updateMenu();
      }
    });
  }

  // Método para obtener el rol del usuario actual
  private getUserRole(): string {
    if (!this.currentUser) {
      return 'vecino'; // Por defecto si no hay usuario logueado
    }
    
    // Usar el campo tipo_usuario para determinar el rol
    if (this.currentUser.tipo_usuario) {
      // Convertir a minúsculas para hacer la comparación insensible a mayúsculas/minúsculas
      const tipoUsuario = this.currentUser.tipo_usuario.toLowerCase();
      
      if (tipoUsuario === 'directiva') {
        return 'directiva';
      } else if (tipoUsuario === 'socio') {
        return 'socio';
      }
    }
    
    // Si no tiene tipo_usuario o es 'vecino'
    return 'vecino';
  }

  // Actualizar menú según el rol del usuario
  private updateMenu() {
    const userRole = this.getUserRole();
    
    switch(userRole) {
      case 'socio':
        this.appPages = this.menuSocio;
        break;
      case 'directiva':
        this.appPages = this.menuDirectiva;
        break;
      default: // 'vecino' u otro rol
        this.appPages = this.menuVecino.filter(option => {
          if (option.title === 'Solicitar Membresía') {
            return this.solicitudSocioEstado !== 'pendiente' && this.solicitudSocioEstado !== 'aprobada';
          }
          return true;
        });
    }
  }

  // Método para cerrar sesión
  logout() {
    this.autenticacionService.cerrarSesion();
    this.router.navigate(['/login']);
  }

  private consultarEstadoSolicitudSocio() {
    this.sociosService.consultarEstadoSolicitudSocio().subscribe({
      next: (resp: any) => {
        this.solicitudSocioEstado = resp?.estadoSolicitud || resp?.estado_solicitud || 'ninguna';
        this.updateMenu(); // Actualiza el menú cuando cambia el estado
      },
      error: () => {
        this.solicitudSocioEstado = null;
        this.updateMenu();
      }
    });
  }
}