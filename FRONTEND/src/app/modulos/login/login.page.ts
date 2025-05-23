import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { IonicModule, AlertController, LoadingController } from '@ionic/angular';
import { Router } from '@angular/router';
import { AutenticacionService } from '../../services/autenticacion.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.page.html',
  styleUrls: ['./login.page.scss'],
  standalone: true,
  imports: [IonicModule, CommonModule, ReactiveFormsModule]
})
export class LoginPage implements OnInit {
  loginForm!: FormGroup;
  isLoading = false;
  showPassword = false;

  constructor(
    private formBuilder: FormBuilder,
    private router: Router,
    private alertController: AlertController,
    private loadingController: LoadingController,
    private autenticacionService: AutenticacionService
  ) { }

  ngOnInit() {
    this.initForm();
  }

  initForm() {
    this.loginForm = this.formBuilder.group({
      rut: ['', [Validators.required, Validators.pattern(/^[0-9]{7,8}$/)]],
      dv_rut: ['', [Validators.required, Validators.pattern(/^[0-9kK]$/)]],
      password: ['', [Validators.required, Validators.minLength(8)]]
    });
  }

  togglePassword() {
    this.showPassword = !this.showPassword;
  }

  async login() {
    if (this.loginForm.invalid) {
      this.mostrarMensaje('Error de validación', 'Por favor, completa correctamente todos los campos.');
      return;
    }

    this.isLoading = true;
    const loading = await this.loadingController.create({
      message: 'Iniciando sesión...',
      spinner: 'circles'
    });
    await loading.present();

    const { rut, dv_rut, password } = this.loginForm.value;
    console.log("FLAG1");
    
    this.autenticacionService.iniciarSesion(rut, dv_rut, password).subscribe({
      next: (response) => {
        console.log("FLAG2");
        loading.dismiss();
        this.isLoading = false;
        this.router.navigate(['/home']);
      },
      error: (error) => {
        loading.dismiss();
        this.isLoading = false;
        console.error('Error de inicio de sesión:', error);
        
        const mensaje = error.error?.mensaje || 'No se pudo iniciar sesión. Verifica tus credenciales e intenta nuevamente.';
        this.mostrarMensaje('Error de inicio de sesión', mensaje);
      }
    });
  }

  async mostrarMensaje(titulo: string, mensaje: string) {
    const alert = await this.alertController.create({
      header: titulo,
      message: mensaje,
      buttons: ['OK']
    });
    await alert.present();
  }

  irARegistro() {
    this.router.navigate(['/registro']);
  }

  irARecuperarPassword() {
    this.router.navigate(['/recuperar-password']);
  }
}