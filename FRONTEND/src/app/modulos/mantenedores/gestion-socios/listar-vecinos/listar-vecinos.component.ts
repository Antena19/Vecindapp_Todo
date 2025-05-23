import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonicModule } from '@ionic/angular';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { UsuariosService, Usuario } from 'src/app/services/usuarios.service';

@Component({
  selector: 'app-listar-vecinos',
  templateUrl: './listar-vecinos.component.html',
  styleUrls: ['./listar-vecinos.component.scss'],
  standalone: true,
  imports: [CommonModule, IonicModule, FormsModule]
})
export class ListarVecinosComponent implements OnInit {
  usuarios: Usuario[] = [];
  usuariosFiltrados: Usuario[] = [];
  filtroEstado: 'todos' | 'activo' | 'inactivo' = 'todos';
  terminoBusqueda: string = '';

  constructor(
    private router: Router,
    private usuariosService: UsuariosService
  ) {}

  ngOnInit() {
    this.cargarUsuarios();
  }

  private cargarUsuarios() {
    this.usuariosService.getUsuariosPorRol('vecino').subscribe(
      usuarios => {
        this.usuarios = usuarios;
        this.usuariosFiltrados = usuarios;
      }
    );
  }

  verDetalleUsuario(id: number) {
    this.router.navigate([`/mantenedores/gestion-socios/detalle-usuario/${id}`]);
  }

  agregarUsuario() {
    this.router.navigate(['/mantenedores/gestion-socios/agregar-usuario']);
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