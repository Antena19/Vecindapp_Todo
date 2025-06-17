import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { IonicModule, AlertController, LoadingController, ToastController } from '@ionic/angular';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-mi-perfil',
  templateUrl: './mi-perfil.component.html',
  styleUrls: ['./mi-perfil.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    IonicModule,
    RouterModule
  ]
})
export class MiPerfilComponent implements OnInit {
  perfilForm!: FormGroup;
  passwordForm!: FormGroup;
  usuario: any = null;
  cargando = false;
  editando = false;
  cambiandoPassword = false;
  isSubmitted = false;
  loading: HTMLIonLoadingElement | null = null;

  constructor(
    private formBuilder: FormBuilder,
    private http: HttpClient,
    private router: Router,
    private alertController: AlertController,
    private loadingController: LoadingController,
    private toastController: ToastController
  ) { }

  ngOnInit() {
    this.inicializarFormularios();
    this.cargarDatosUsuario();
  }

  inicializarFormularios() {
    // Formulario para datos personales
    this.perfilForm = this.formBuilder.group({
      nombre: ['', [Validators.required, Validators.maxLength(100)]],
      apellido_paterno: ['', [Validators.required, Validators.maxLength(100)]],
      apellido_materno: ['', Validators.maxLength(100)],
      correo_electronico: ['', [Validators.required, Validators.email, Validators.maxLength(100)]],
      telefono: ['', [Validators.pattern(/^\+?56?[9]\d{8}$/), Validators.maxLength(15)]],
      direccion: ['', [Validators.required, Validators.maxLength(200)]]
    });

    // Formulario para cambio de contraseña
    this.passwordForm = this.formBuilder.group({
      passwordActual: ['', Validators.required],
      passwordNueva: ['', [
        Validators.required, 
        Validators.minLength(8),
        Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$/)
      ]],
      confirmPasswordNueva: ['', Validators.required]
    }, {
      validators: this.passwordMatchValidator
    });

    // Deshabilitar formularios hasta que se carguen los datos
    this.perfilForm.disable();
  }

  // Validador para confirmar que las contraseñas coincidan
  passwordMatchValidator(form: FormGroup) {
    const password = form.get('passwordNueva')?.value;
    const confirmPassword = form.get('confirmPasswordNueva')?.value;

    return password === confirmPassword ? null : { passwordMismatch: true };
  }

  async cargarDatosUsuario() {
    try {
      this.loading = await this.loadingController.create({
        message: 'Cargando datos...',
        spinner: 'crescent',
        backdropDismiss: false,
        keyboardClose: false
      });
      await this.loading.present();

      this.http.get(`${environment.apiUrl}/api/Usuarios/autenticado`)
        .subscribe({
          next: (response: any) => {
            this.usuario = response.usuario;
            
            // Cargar datos en el formulario
            this.perfilForm.patchValue({
              nombre: this.usuario.nombre,
              apellido_paterno: this.usuario.apellido_paterno,
              apellido_materno: this.usuario.apellido_materno,
              correo_electronico: this.usuario.correo_electronico,
              telefono: this.usuario.telefono,
              direccion: this.usuario.direccion
            });
          },
          error: async (error) => {
            console.error('Error al cargar datos del usuario:', error);
            
            const alert = await this.alertController.create({
              header: 'Error',
              message: 'No se pudieron cargar tus datos. Por favor, intenta nuevamente más tarde.',
              buttons: [
                {
                  text: 'Aceptar',
                  handler: () => {
                    this.router.navigate(['/home']);
                  }
                }
              ]
            });
            
            await alert.present();
          },
          complete: () => {
            if (this.loading) {
              this.loading.dismiss();
              this.loading = null;
            }
          }
        });
    } catch (error) {
      console.error('Error al mostrar loading:', error);
    }
  }

  toggleEdicion() {
    this.editando = !this.editando;
    
    if (this.editando) {
      this.perfilForm.enable();
    } else {
      this.perfilForm.disable();
      // Recargar datos originales
      this.perfilForm.patchValue({
        nombre: this.usuario.nombre,
        apellido_paterno: this.usuario.apellido_paterno,
        apellido_materno: this.usuario.apellido_materno,
        correo_electronico: this.usuario.correo_electronico,
        telefono: this.usuario.telefono,
        direccion: this.usuario.direccion
      });
    }
  }

  toggleCambioPassword() {
    this.cambiandoPassword = !this.cambiandoPassword;
    
    if (!this.cambiandoPassword) {
      // Limpiar formulario de cambio de contraseña
      this.passwordForm.reset();
    }
  }

  async guardarPerfil() {
    this.isSubmitted = true;
    
    if (this.perfilForm.invalid) {
      return;
    }
    
    this.cargando = true;
    
    try {
      this.loading = await this.loadingController.create({
        message: 'Guardando cambios...',
        spinner: 'crescent',
        backdropDismiss: false,
        keyboardClose: false
      });
      await this.loading.present();
      
      const datosActualizados = {
        Nombres: this.perfilForm.value.nombre,
        Apellidos: this.perfilForm.value.apellido_paterno,
        ApellidoMaterno: this.perfilForm.value.apellido_materno || null,
        CorreoElectronico: this.perfilForm.value.correo_electronico,
        Telefono: this.perfilForm.value.telefono || null,
        Direccion: this.perfilForm.value.direccion
      };

      this.http.put(`${environment.apiUrl}/api/Usuarios/${this.usuario.rut}`, datosActualizados)
        .subscribe({
          next: async () => {
            // Actualizar datos locales
            this.usuario = {
              ...this.usuario,
              nombre: this.perfilForm.value.nombre,
              apellido_paterno: this.perfilForm.value.apellido_paterno,
              apellido_materno: this.perfilForm.value.apellido_materno,
              correo_electronico: this.perfilForm.value.correo_electronico,
              telefono: this.perfilForm.value.telefono,
              direccion: this.perfilForm.value.direccion
            };
            
            // Salir del modo edición
            this.editando = false;
            this.perfilForm.disable();
            
            const toast = await this.toastController.create({
              message: 'Tus datos han sido actualizados exitosamente',
              duration: 3000,
              color: 'success',
              position: 'top'
            });
            
            await toast.present();
          },
          error: async (error) => {
            console.log('Error completo:', error);
            console.log('Respuesta del servidor:', error.error);
            
            // Si es un error 400 con statusText "OK", tratar como éxito
            if (error.status === 400 && error.statusText === "OK") {
              // Actualizar datos locales
              this.usuario = {
                ...this.usuario,
                nombre: this.perfilForm.value.nombre,
                apellido_paterno: this.perfilForm.value.apellido_paterno,
                apellido_materno: this.perfilForm.value.apellido_materno,
                correo_electronico: this.perfilForm.value.correo_electronico,
                telefono: this.perfilForm.value.telefono,
                direccion: this.perfilForm.value.direccion
              };
              
              // Salir del modo edición
              this.editando = false;
              this.perfilForm.disable();
              
              const toast = await this.toastController.create({
                message: 'Tus datos han sido actualizados exitosamente',
                duration: 3000,
                color: 'success',
                position: 'top'
              });
              
              await toast.present();
              return;
            }
            
            // Manejar otros errores normalmente
            let mensajeError = 'Ocurrió un error al actualizar tus datos. Por favor, intenta nuevamente.';
            
            if (error.status === 409) {
              mensajeError = 'El correo electrónico ya está registrado con otro usuario.';
            } else if (error.status === 400) {
              mensajeError = error.error?.mensaje || 'Datos inválidos. Verifica la información proporcionada.';
            }
            
            const alert = await this.alertController.create({
              header: 'Error',
              message: mensajeError,
              buttons: ['Aceptar']
            });
            
            await alert.present();
          },
          complete: () => {
            this.cargando = false;
            if (this.loading) {
              this.loading.dismiss();
              this.loading = null;
            }
          }
        });
    } catch (error) {
      console.error('Error al mostrar loading:', error);
      this.cargando = false;
    }
  }

  async cambiarPassword() {
    this.isSubmitted = true;
    
    if (this.passwordForm.invalid) {
      return;
    }
    
    this.cargando = true;
    
    const loading = await this.loadingController.create({
      message: 'Actualizando contraseña...',
      spinner: 'crescent'
    });
    await loading.present();
    
    const datosPassword = {
      contrasenaActual: this.passwordForm.value.passwordActual,
      nuevaContrasena: this.passwordForm.value.passwordNueva
    };
    
    this.http.post(`${environment.apiUrl}/api/Autenticacion/cambiar-contrasena`, datosPassword)
      .subscribe({
        next: async () => {
          this.cargando = false;
          await loading.dismiss();
          
          // Limpiar formulario y salir del modo cambio de contraseña
          this.passwordForm.reset();
          this.cambiandoPassword = false;
          this.isSubmitted = false;
          
          const toast = await this.toastController.create({
            message: 'Tu contraseña ha sido actualizada exitosamente',
            duration: 3000,
            color: 'success',
            position: 'top'
          });
          
          await toast.present();
        },
        error: async (error) => {
          this.cargando = false;
          await loading.dismiss();
          
          let mensajeError = 'Ocurrió un error al actualizar tu contraseña. Por favor, intenta nuevamente.';
          
          if (error.status === 400) {
            if (error.error?.mensaje?.includes('actual')) {
              mensajeError = 'La contraseña actual es incorrecta.';
            } else if (error.error?.mensaje?.includes('requisitos')) {
              mensajeError = 'La nueva contraseña no cumple con los requisitos de seguridad.';
            } else {
              mensajeError = error.error?.mensaje || 'Datos inválidos. Verifica la información proporcionada.';
            }
          }
          
          const alert = await this.alertController.create({
            header: 'Error',
            message: mensajeError,
            buttons: ['Aceptar']
          });
          
          await alert.present();
        }
      });
  }

  volverAlInicio() {
    this.router.navigate(['/home']);
  }
}