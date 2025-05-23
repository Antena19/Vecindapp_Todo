import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonicModule } from '@ionic/angular';
import { FormsModule } from '@angular/forms';
import { UsuariosService, Usuario } from 'src/app/services/usuarios.service';

@Component({
  selector: 'app-listar-usuarios',
  templateUrl: './listar-usuarios.component.html',
  styleUrls: ['./listar-usuarios.component.scss'],
  standalone: true,
  imports: [CommonModule, IonicModule, FormsModule]
})
export class ListarUsuariosComponent implements OnInit {
  usuarios: Usuario[] = [];
  usuariosFiltrados: Usuario[] = [];
  filtroEstado: 'todos' | 'activo' | 'inactivo' = 'todos';
  terminoBusqueda: string = '';

  constructor(private usuariosService: UsuariosService) {}

  ngOnInit() {
    this.cargarUsuarios();
  }

  cargarUsuarios() {
    this.usuariosService.getUsuariosPorRol('vecino').subscribe(
      usuarios => {
        this.usuarios = usuarios;
        this.usuariosFiltrados = usuarios;
      }
    );
  }

  verDetalleUsuario(id: number) {
    // Implementar navegación al detalle
    console.log('Ver detalle usuario:', id);
  }

  agregarUsuario() {
    // Implementar navegación a formulario de creación
    console.log('Agregar nuevo usuario');
  }

  filtrarUsuarios() {
    if (this.filtroEstado === 'todos') {
      this.usuariosFiltrados = this.usuarios;
    } else {
      this.usuariosFiltrados = this.usuarios.filter(
        usuario => usuario.estado === this.filtroEstado
      );
    }
    this.buscarUsuarios();
  }

  buscarUsuarios() {
    if (!this.terminoBusqueda) {
      return;
    }
    const busqueda = this.terminoBusqueda.toLowerCase();
    this.usuariosFiltrados = this.usuariosFiltrados.filter(usuario =>
      usuario.nombre.toLowerCase().includes(busqueda) ||
      usuario.apellido.toLowerCase().includes(busqueda) ||
      usuario.email.toLowerCase().includes(busqueda) ||
      usuario.direccion.toLowerCase().includes(busqueda)
    );
  }

  actualizarEstado(id: number, estado: 'activo' | 'inactivo') {
    this.usuariosService.actualizarEstado(id, estado).subscribe(
      usuarioActualizado => {
        const index = this.usuarios.findIndex(u => u.id === id);
        if (index !== -1) {
          this.usuarios[index] = usuarioActualizado;
          this.filtrarUsuarios();
        }
      }
    );
  }
} 