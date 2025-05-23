import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, Router } from '@angular/router';
import { Observable, map } from 'rxjs';
import { AutenticacionService } from '../services/autenticacion.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {
  constructor(
    private authService: AutenticacionService,
    private router: Router
  ) {}
  
  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): Observable<boolean> | Promise<boolean> | boolean {
    if (!this.authService.isAuthenticated) {
      this.router.navigate(['/login']);
      return false;
    }

    const rolesRequeridos = route.data['roles'] as Array<string>;
    if (rolesRequeridos && rolesRequeridos.length > 0) {
      return this.authService.usuario.pipe(
        map(usuario => {
          if (!usuario) {
            this.router.navigate(['/unauthorized']);
            return false;
          }
          
          // Si el usuario es admin, tiene acceso a todo
          if (usuario.tipo_usuario === 'admin') {
            return true;
          }

          // Verificar si el usuario tiene alguno de los roles requeridos
          const tieneRolRequerido = rolesRequeridos.includes(usuario.tipo_usuario);
          if (!tieneRolRequerido) {
            this.router.navigate(['/unauthorized']);
            return false;
          }

          return true;
        })
      );
    }

    return true;
  }
}