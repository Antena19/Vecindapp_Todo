import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';
import { Constantes } from '../constantes';
import { Usuario } from '../modelos/Usuario';

@Injectable({
  providedIn: 'root'
})
export class AutenticacionService {
  private _usuario = new BehaviorSubject<Usuario | null>(null);
  private _token = new BehaviorSubject<string | null>(null);

  constructor(private http: HttpClient) {
    console.log('Inicializando AutenticacionService');
    this.cargarDatosGuardados();
  }

  get usuario(): Observable<Usuario | null> {
    return this._usuario.asObservable();
  }

  get token(): Observable<string | null> {
    return this._token.asObservable();
  }

  get tokenValue(): string | null {
    return this._token.value;
  }

  get isAuthenticated(): boolean {
    return !!this._token.value;
  }
  

  private cargarDatosGuardados() {
    try {
      const token = localStorage.getItem('token');
      
      if (token) {
        this._token.next(token);
        
        // Intentar cargar usuario
        const userData = localStorage.getItem('usuario');
        if (userData && userData !== "undefined") {
          try {
            const usuario = JSON.parse(userData);
            this._usuario.next(usuario);
          } catch (e) {
            console.error('Error al procesar datos de usuario:', e);
            // Limpiar solo datos de usuario inválidos
            localStorage.removeItem('usuario');
          }
        }
      }
    } catch(e) {
      console.error('Error al cargar datos guardados:', e);
      // Limpiar todo en caso de error crítico
      localStorage.removeItem('token');
      localStorage.removeItem('usuario');
    }
  }

  iniciarSesion(rut: number, dv_rut: string, password: string): Observable<any> {
    const url = `${Constantes.API_URL}/api/Autenticacion/login`;
    const body = {
      username: rut.toString(),
      password: password
    };
    
    return this.http.post<any>(url, body).pipe(
      tap(response => {
        if (response && response.token) {
          // Guardar token
          this._token.next(response.token);
          localStorage.setItem('token', response.token);
          
          // Guardar usuario si existe
          if (response.usuario) {
            this._usuario.next(response.usuario);
            localStorage.setItem('usuario', JSON.stringify(response.usuario));
          }
        }
      })
    );
  }
  cerrarSesion() {
    // Eliminar datos de localStorage
    localStorage.removeItem('token');
    localStorage.removeItem('usuario');

    // Resetear BehaviorSubjects
    this._token.next(null);
    this._usuario.next(null);
  }

  // Método para obtener los headers con el token de autorización
  getAuthHeaders(): HttpHeaders {
    return new HttpHeaders({
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${this.tokenValue}`
    });
  }

  // Obtener el token directamente
  getToken(): string | null {
    return localStorage.getItem('token');
  }

  // Obtener el rol del usuario (tipo_usuario)
  getUserRole(): string {
    const usuario = JSON.parse(localStorage.getItem('usuario') || 'null');
    return usuario && usuario.tipo_usuario ? usuario.tipo_usuario : '';
  }

  // Verificar si el token ha expirado
  isTokenExpired(): boolean {
    const token = this.getToken();
    if (!token) return true;
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return payload.exp < Date.now() / 1000;
    } catch {
      return true;
    }
  }

  // Obtener el usuario actual como objeto (no observable)
  obtenerUsuarioActual(): Usuario | null {
    const userData = localStorage.getItem('usuario');
    return userData ? JSON.parse(userData) : null;
  }
}