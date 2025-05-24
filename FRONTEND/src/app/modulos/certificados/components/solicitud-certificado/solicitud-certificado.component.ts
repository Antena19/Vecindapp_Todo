import { Component, OnInit, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
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
  precio = 2000;
  cedulaFile: File | null = null;
  comprobanteFile: File | null = null;
  readonly tipoCertificadoIdResidencia = 3;

  constructor(
    private fb: FormBuilder,
    private certificadoService: CertificadoService,
    private alertController: AlertController,
    private loadingController: LoadingController,
    private authService: AuthService,
    private router: Router
  ) {
    this.solicitudForm = this.fb.group({
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

  onCedulaSelected(event: any) {
    const file = event.target.files[0];
    this.cedulaFile = file ? file : null;
  }

  onComprobanteSelected(event: any) {
    const file = event.target.files[0];
    this.comprobanteFile = file ? file : null;
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

    if (!this.cedulaFile || !this.comprobanteFile) {
      const alert = await this.alertController.create({
        header: 'Error',
        message: 'Debe adjuntar la copia de cédula y el comprobante de domicilio',
        buttons: ['OK']
      });
      await alert.present();
      return;
    }

    const loading = await this.loadingController.create({
      message: 'Enviando solicitud...'
    });
    await loading.present();

    const usuario = this.authService.getUsuarioActual();
    const data = {
      usuario_rut: usuario?.id,
      tipo_certificado_id: this.tipoCertificadoIdResidencia,
      motivo: '',
      DocumentosAdjuntos: '',
      observaciones: this.solicitudForm.get('observaciones')?.value || '',
      estado: 'pendiente',
      precio: this.precio
    };

    this.certificadoService.solicitarCertificado(data).subscribe({
      next: async (response: Certificado) => {
        await loading.dismiss();
        const alert = await this.alertController.create({
          header: 'Éxito',
          message: 'Solicitud enviada correctamente',
          buttons: ['OK']
        });
        await alert.present();
        this.solicitudForm.reset();
        this.cedulaFile = null;
        this.comprobanteFile = null;
        this.router.navigate(['/certificados/pago', response.id]);
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