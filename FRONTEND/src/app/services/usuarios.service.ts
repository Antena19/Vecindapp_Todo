import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of, map } from 'rxjs';
import { environment } from 'src/environments/environment';

export interface Usuario {
  id: number;
  nombre: string;
  apellido: string;
  email: string;
  telefono: string;
  direccion: string;
  fechaRegistro: Date;
  estado: 'activo' | 'inactivo';
  rol: 'vecino' | 'admin';
  ultimoAcceso?: Date;
  notificacionesActivas: boolean;
  preferenciasComunicacion: 'email' | 'sms' | 'ambos';
}

@Injectable({
  providedIn: 'root'
})
export class UsuariosService {
  private apiUrl = `${environment.apiUrl}/usuarios`;

  constructor(private http: HttpClient) {}

  getUsuarios(): Observable<Usuario[]> {
    // Temporalmente retornamos datos mockeados
    return of([
      {
        id: 1,
        nombre: 'Juan',
        apellido: 'Pérez',
        email: 'juan.perez@email.com',
        telefono: '+56912345678',
        direccion: 'Calle Principal 123',
        fechaRegistro: new Date('2024-01-15'),
        estado: 'activo',
        rol: 'vecino',
        ultimoAcceso: new Date('2024-03-15'),
        notificacionesActivas: true,
        preferenciasComunicacion: 'email'
      },
      {
        id: 2,
        nombre: 'María',
        apellido: 'González',
        email: 'maria.gonzalez@email.com',
        telefono: '+56987654321',
        direccion: 'Avenida Central 456',
        fechaRegistro: new Date('2024-02-01'),
        estado: 'activo',
        rol: 'vecino',
        ultimoAcceso: new Date('2024-03-14'),
        notificacionesActivas: true,
        preferenciasComunicacion: 'ambos'
      },
      {
        id: 3,
        nombre: 'Carlos',
        apellido: 'Rodríguez',
        email: 'carlos.rodriguez@email.com',
        telefono: '+56923456789',
        direccion: 'Plaza Mayor 789',
        fechaRegistro: new Date('2024-01-20'),
        estado: 'inactivo',
        rol: 'vecino',
        ultimoAcceso: new Date('2024-03-10'),
        notificacionesActivas: false,
        preferenciasComunicacion: 'sms'
      }
    ]);

    // Cuando el backend esté listo, usar:
    // return this.http.get<Usuario[]>(this.apiUrl);
  }

  getUsuario(id: number): Observable<Usuario> {
    // Temporalmente retornamos un usuario mockeado
    return of({
      id: id,
      nombre: 'Juan',
      apellido: 'Pérez',
      email: 'juan.perez@email.com',
      telefono: '+56912345678',
      direccion: 'Calle Principal 123',
      fechaRegistro: new Date('2024-01-15'),
      estado: 'activo',
      rol: 'vecino',
      ultimoAcceso: new Date('2024-03-15'),
      notificacionesActivas: true,
      preferenciasComunicacion: 'email'
    });

    // Cuando el backend esté listo, usar:
    // return this.http.get<Usuario>(`${this.apiUrl}/${id}`);
  }

  crearUsuario(usuario: Omit<Usuario, 'id'>): Observable<Usuario> {
    // Temporalmente retornamos el usuario creado
    return of({
      id: Math.floor(Math.random() * 1000),
      ...usuario
    });

    // Cuando el backend esté listo, usar:
    // return this.http.post<Usuario>(this.apiUrl, usuario);
  }

  actualizarUsuario(id: number, usuario: Partial<Usuario>): Observable<Usuario> {
    // Temporalmente retornamos el usuario actualizado
    return of({
      id: id,
      ...usuario
    } as Usuario);

    // Cuando el backend esté listo, usar:
    // return this.http.put<Usuario>(`${this.apiUrl}/${id}`, usuario);
  }

  eliminarUsuario(id: number): Observable<void> {
    // Temporalmente retornamos void
    return of(void 0);

    // Cuando el backend esté listo, usar:
    // return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  // Métodos específicos para usuarios
  actualizarPreferencias(id: number, preferencias: { notificacionesActivas: boolean, preferenciasComunicacion: 'email' | 'sms' | 'ambos' }): Observable<Usuario> {
    return this.actualizarUsuario(id, preferencias);
  }

  actualizarEstado(id: number, estado: 'activo' | 'inactivo'): Observable<Usuario> {
    return this.actualizarUsuario(id, { estado });
  }

  getUsuariosActivos(): Observable<Usuario[]> {
    return this.getUsuarios().pipe(
      map(usuarios => usuarios.filter(usuario => usuario.estado === 'activo'))
    );
  }

  getUsuariosPorRol(rol: 'vecino' | 'admin'): Observable<Usuario[]> {
    return this.getUsuarios().pipe(
      map(usuarios => usuarios.filter(usuario => usuario.rol === rol))
    );
  }
} 