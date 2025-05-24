import { Component, OnInit, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { IonicModule, AlertController, LoadingController } from '@ionic/angular';
import { CertificadoService } from '../../../../services/certificado.service';
import { Certificado } from '../../../../modelos/certificado.interface';
import { AuthService } from '../../../../services/auth.service';

@Component({
  selector: 'app-solicitud-certificado',
  templateUrl: './solicitud-certificado.component.html',
  styleUrls: ['./solicitud-certificado.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule,
    IonicModule
  ],
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class SolicitudCertificadoComponent implements OnInit {
  solicitudForm: FormGroup;
  documentos: File[] = [];
  cargando = false;
  isSocio = false;

  constructor(
    private fb: FormBuilder,
    private certificadoService: CertificadoService,
    private alertController: AlertController,
    private loadingController: LoadingController,
    private authService: AuthService
  ) {
    this.solicitudForm = this.fb.group({
      tipo: ['', Validators.required],
      observaciones: ['']
    });
  }

  ngOnInit() {
    this.verificarRolSocio();
  }

  private verificarRolSocio() {
    const usuario = this.authService.getUsuarioActual();
    this.isSocio = usuario?.rol === 'Socio';
  }

  onFileSelected(event: any) {
    const files = event.target.files;
    if (files) {
      this.documentos = Array.from(files);
    }
  }

  removeDocument(index: number) {
    this.documentos.splice(index, 1);
  }

  async onSubmit() {
    if (this.solicitudForm.invalid) {
      const alert = await this.alertController.create({
        header: 'Error',
        message: 'Por favor complete todos los campos requeridos',
        buttons: ['OK']
      });
      await alert.present();
      return;
    }

    if (this.documentos.length === 0) {
      const alert = await this.alertController.create({
        header: 'Error',
        message: 'Por favor seleccione al menos un documento',
        buttons: ['OK']
      });
      await alert.present();
      return;
    }

    const loading = await this.loadingController.create({
      message: 'Enviando solicitud...'
    });
    await loading.present();

    const formData = new FormData();
    formData.append('tipo', this.solicitudForm.get('tipo')?.value);
    this.documentos.forEach((doc, index) => {
      formData.append('documentos', doc);
    });
    if (this.solicitudForm.get('observaciones')?.value) {
      formData.append('observaciones', this.solicitudForm.get('observaciones')?.value);
    }

    this.certificadoService.solicitarCertificado(formData).subscribe({
      next: async (response: Certificado) => {
        await loading.dismiss();
        const alert = await this.alertController.create({
          header: 'Éxito',
          message: 'Solicitud enviada correctamente',
          buttons: ['OK']
        });
        await alert.present();
        this.solicitudForm.reset();
        this.documentos = [];
      },
      error: async (error: any) => {
        await loading.dismiss();
        const alert = await this.alertController.create({
          header: 'Error',
          message: 'Error al enviar la solicitud',
          buttons: ['OK']
        });
        await alert.present();
      }
    });
  }
} 