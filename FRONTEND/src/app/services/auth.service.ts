import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

export interface Usuario {
  id: number;
  nombre: string;
  email: string;
  rol: 'Directiva' | 'Socio' | 'Vecino';
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private usuarioActualSubject = new BehaviorSubject<Usuario | null>(null);
  public usuarioActual$ = this.usuarioActualSubject.asObservable();

  constructor() {
    // Cargar usuario del localStorage al iniciar
    const usuarioGuardado = localStorage.getItem('usuario');
    if (usuarioGuardado) {
      this.usuarioActualSubject.next(JSON.parse(usuarioGuardado));
    }
  }

  getUsuarioActual(): Usuario | null {
    return this.usuarioActualSubject.value;
  }

  setUsuarioActual(usuario: Usuario) {
    localStorage.setItem('usuario', JSON.stringify(usuario));
    this.usuarioActualSubject.next(usuario);
  }

  logout() {
    localStorage.removeItem('usuario');
    this.usuarioActualSubject.next(null);
  }

  isAuthenticated(): boolean {
    return !!this.getUsuarioActual();
  }

  hasRole(role: Usuario['rol']): boolean {
    const usuario = this.getUsuarioActual();
    return usuario?.rol === role;
  }
} 