import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { 
  IonicModule, 
  AlertController, 
  LoadingController 
} from '@ionic/angular';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-registro-vecino',
  templateUrl: './registro-vecino.component.html',
  styleUrls: ['./registro-vecino.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    IonicModule,
    RouterModule
  ]
})
export class RegistroVecinoComponent implements OnInit {
  registroForm!: FormGroup;
  isSubmitted = false;
  cargando = false;

  constructor(
    private formBuilder: FormBuilder,
    private http: HttpClient,
    private router: Router,
    private alertController: AlertController,
    private loadingController: LoadingController
  ) { }

  ngOnInit() {
    this.registroForm = this.formBuilder.group({
      rut: ['', [Validators.required, Validators.pattern(/^\d+$/)]],
      dv_rut: ['', [Validators.required, Validators.pattern(/^[0-9kK]$/)]],
      nombre: ['', [Validators.required, Validators.maxLength(100)]],
      apellido_paterno: ['', [Validators.required, Validators.maxLength(100)]],
      apellido_materno: ['', Validators.maxLength(100)],
      correo_electronico: ['', [Validators.required, Validators.email, Validators.maxLength(100)]],
      telefono: ['', [Validators.pattern(/^\+?56?[9]\d{8}$/), Validators.maxLength(15)]],
      direccion: ['', [Validators.required, Validators.maxLength(200)]],
      password: ['', [
        Validators.required, 
        Validators.minLength(8),
        Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$/)
      ]],
      confirmPassword: ['', Validators.required]
    }, {
      validators: this.passwordMatchValidator
    });
  }

  // Validador personalizado para confirmar que las contraseñas coincidan
  passwordMatchValidator(formGroup: FormGroup) {
    const password = formGroup.get('password')?.value;
    const confirmPassword = formGroup.get('confirmPassword')?.value;

    return password === confirmPassword ? null : { passwordMismatch: true };
  }

  // Getter para acceder fácilmente a los controles del formulario en la plantilla
  get formulario() { 
    return this.registroForm.controls; 
  }

  // Método para validar RUT chileno
  validarRut() {
    const rut = this.registroForm.get('rut')?.value;
    const dv = this.registroForm.get('dv_rut')?.value;
    
    if (!rut || !dv) return;
    
    // Implementación simple de validación de RUT
    let suma = 0;
    let multiplicador = 2;
    
    // Convertir rut a string y recorrerlo de derecha a izquierda
    let rutStr = rut.toString();
    for (let i = rutStr.length - 1; i >= 0; i--) {
      suma += parseInt(rutStr.charAt(i)) * multiplicador;
      multiplicador = multiplicador === 7 ? 2 : multiplicador + 1;
    }
    
    // Calcular dígito verificador
    const resto = suma % 11;
    const dvCalculado = resto === 0 ? "0" : resto === 1 ? "K" : (11 - resto).toString();
    
    // Verificar si el dígito verificador es válido
    const dvIngresado = dv.toUpperCase();
    if (dvCalculado !== dvIngresado) {
      this.registroForm.get('dv_rut')?.setErrors({ rutInvalido: true });
    } else {
      // Limpiar el error si el RUT es válido
      const errores = this.registroForm.get('dv_rut')?.errors;
      if (errores && errores['rutInvalido']) {
        delete errores['rutInvalido'];
        this.registroForm.get('dv_rut')?.setErrors(Object.keys(errores).length === 0 ? null : errores);
      }
    }
  }

  async onSubmit() {
    this.isSubmitted = true;
    
    // Detener si el formulario es inválido
    if (this.registroForm.invalid) {
      return;
    }
    
    this.cargando = true;
    
    // Mostrar indicador de carga
    const loading = await this.loadingController.create({
      message: 'Registrando usuario...',
      spinner: 'crescent'
    });
    await loading.present();
    
    // Preparar datos para enviar al backend
    const datosUsuario = {
      rut: parseInt(this.registroForm.value.rut),
      dv_rut: this.registroForm.value.dv_rut,
      nombre: this.registroForm.value.nombre,
      apellido_paterno: this.registroForm.value.apellido_paterno,
      apellido_materno: this.registroForm.value.apellido_materno || null,
      correo_electronico: this.registroForm.value.correo_electronico,
      telefono: this.registroForm.value.telefono || null,
      direccion: this.registroForm.value.direccion,
      password: this.registroForm.value.password
      // No es necesario enviar tipo_usuario porque el backend asigna "Vecino" por defecto
    };
    
    // Realizar petición HTTP al backend
    this.http.post(`${environment.apiUrl}/api/Usuarios/registrar`, datosUsuario)
      .subscribe({
        next: async (response: any) => {
          this.cargando = false;
          await loading.dismiss();
          
          const alert = await this.alertController.create({
            header: 'Registro exitoso',
            message: 'Tu cuenta ha sido creada correctamente. Ya puedes iniciar sesión.',
            buttons: [
              {
                text: 'Iniciar sesión',
                handler: () => {
                  this.router.navigate(['/login']);
                }
              }
            ]
          });
          
          await alert.present();
        },
        error: async (error) => {
          this.cargando = false;
          await loading.dismiss();
          
          let mensajeError = 'Ocurrió un error al registrar el usuario. Por favor, intenta nuevamente.';
          
          if (error.status === 409) {
            // Conflicto - RUT o correo ya existen
            mensajeError = error.error.mensaje || 'El RUT o correo electrónico ya están registrados.';
          } else if (error.status === 400) {
            // Error de validación
            mensajeError = error.error.mensaje || 'Datos inválidos. Verifica la información proporcionada.';
          }
          
          const alert = await this.alertController.create({
            header: 'Error de registro',
            message: mensajeError,
            buttons: ['Aceptar']
          });
          
          await alert.present();
        }
      });
  }
}