import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { IonicModule, AlertController, LoadingController } from '@ionic/angular';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-recuperar-password',
  templateUrl: './recuperar-password.component.html',
  styleUrls: ['./recuperar-password.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    IonicModule,
    RouterModule
  ]
})
export class RecuperarPasswordComponent implements OnInit {
  recuperacionForm!: FormGroup;
  isSubmitted = false;
  cargando = false;
  showPassword = false;

  constructor(
    private formBuilder: FormBuilder,
    private http: HttpClient,
    private router: Router,
    private alertController: AlertController,
    private loadingController: LoadingController
  ) { }

  ngOnInit() {
    this.inicializarFormulario();
  }

  inicializarFormulario() {
    this.recuperacionForm = this.formBuilder.group({
      rut: ['', [Validators.required, Validators.pattern(/^\d+$/)]],
      dv_rut: ['', [Validators.required, Validators.pattern(/^[0-9kK]$/)]],
      nombreCompleto: ['', [Validators.required, Validators.minLength(3)]],
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

  // Validador para confirmar que las contraseñas coincidan
  passwordMatchValidator(form: FormGroup) {
    const password = form.get('password')?.value;
    const confirmPassword = form.get('confirmPassword')?.value;

    return password === confirmPassword ? null : { passwordMismatch: true };
  }

  // Método para validar RUT chileno
  validarRut() {
    const rut = this.recuperacionForm.get('rut')?.value;
    const dv = this.recuperacionForm.get('dv_rut')?.value;
    
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
      this.recuperacionForm.get('dv_rut')?.setErrors({ rutInvalido: true });
    } else {
      // Limpiar el error si el RUT es válido
      const errores = this.recuperacionForm.get('dv_rut')?.errors;
      if (errores && errores['rutInvalido']) {
        delete errores['rutInvalido'];
        this.recuperacionForm.get('dv_rut')?.setErrors(Object.keys(errores).length === 0 ? null : errores);
      }
    }
  }

  async recuperarContrasena() {
    this.isSubmitted = true;
    
    if (this.recuperacionForm.invalid) {
      return;
    }
    
    this.cargando = true;
    
    const loading = await this.loadingController.create({
      message: 'Procesando solicitud...',
      spinner: 'crescent'
    });
    await loading.present();
    
    // Preparar los datos para el endpoint
    const rut = this.recuperacionForm.get('rut')?.value;
    const dv = this.recuperacionForm.get('dv_rut')?.value;
    const rutCompleto = `${rut}-${dv}`;

    const datos = {
      rut: rutCompleto,
      nombreCompleto: this.recuperacionForm.get('nombreCompleto')?.value,
      nuevaContrasena: this.recuperacionForm.get('password')?.value,
    };
    
    this.http.post(`${environment.apiUrl}/Autenticacion/recuperar-clave-simple`, datos)
      .subscribe({
        next: async () => {
          this.cargando = false;
          await loading.dismiss();
          
          const alert = await this.alertController.create({
            header: 'Contraseña actualizada',
            message: 'Tu contraseña ha sido actualizada exitosamente. Ya puedes iniciar sesión con tu nueva contraseña.',
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
          
          let mensajeError = 'Ocurrió un error al actualizar la contraseña. Por favor, intenta nuevamente.';
          
          if (error.status === 401) {
            mensajeError = 'Los datos proporcionados no son correctos. Verifica tu RUT y nombre completo.';
          } else if (error.status === 400) {
            if (error.error?.mensaje?.includes('contraseña')) {
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

  onSubmit() {
    this.recuperarContrasena();
  }

  volver() {
    this.router.navigate(['/login']);
  }

  togglePassword() {
    this.showPassword = !this.showPassword;
  }
}