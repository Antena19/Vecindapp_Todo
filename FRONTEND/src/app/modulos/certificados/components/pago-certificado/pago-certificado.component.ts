import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { AlertController, LoadingController, IonicModule } from '@ionic/angular';
import { CommonModule } from '@angular/common';
import { CertificadoService } from '../../../../services/certificado.service';
import { PagoCertificado } from '../../../../modelos/certificado.interface';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';

@Component({
  selector: 'app-pago-certificado',
  templateUrl: './pago-certificado.component.html',
  styleUrls: ['./pago-certificado.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    IonicModule,
    ReactiveFormsModule
  ],
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class PagoCertificadoComponent implements OnInit {
  pagoForm: FormGroup;
  certificadoId: number = 0;
  monto: number = 0;
  comprobante: File | null = null;

  constructor(
    private fb: FormBuilder,
    private certificadoService: CertificadoService,
    private router: Router,
    private route: ActivatedRoute,
    private alertController: AlertController,
    private loadingController: LoadingController
  ) {
    this.pagoForm = this.fb.group({
      metodoPago: ['', Validators.required],
      comprobante: [''],
      observaciones: ['']
    });
  }

  ngOnInit() {
    this.certificadoId = Number(this.route.snapshot.paramMap.get('id'));
    this.cargarDetallesCertificado();
  }

  cargarDetallesCertificado() {
    const loading = this.loadingController.create({
      message: 'Cargando detalles...'
    });
    loading.then(loader => loader.present());

    this.certificadoService.obtenerCertificado(this.certificadoId).subscribe({
      next: (response) => {
        this.monto = response.monto;
        loading.then(loader => loader.dismiss());
      },
      error: async (error) => {
        loading.then(loader => loader.dismiss());
        const alert = await this.alertController.create({
          header: 'Error',
          message: 'Error al cargar los detalles del certificado',
          buttons: ['OK']
        });
        await alert.present();
      }
    });
  }

  onFileSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      this.comprobante = file;
      this.pagoForm.patchValue({ comprobante: file });
    }
  }

  async onSubmit() {
    if (this.pagoForm.invalid) {
      return;
    }

    const loading = await this.loadingController.create({
      message: 'Procesando pago...'
    });
    await loading.present();

    const formData = new FormData();
    formData.append('metodoPago', this.pagoForm.get('metodoPago')?.value);
    formData.append('observaciones', this.pagoForm.get('observaciones')?.value || '');
    
    if (this.comprobante) {
      formData.append('comprobante', this.comprobante);
    }

    this.certificadoService.registrarPago(this.certificadoId, formData).subscribe({
      next: async (response) => {
        await loading.dismiss();
        const alert = await this.alertController.create({
          header: 'Éxito',
          message: 'Pago registrado correctamente',
          buttons: ['OK']
        });
        await alert.present();
        this.router.navigate(['/certificados/lista']);
      },
      error: async (error) => {
        await loading.dismiss();
        const alert = await this.alertController.create({
          header: 'Error',
          message: 'Error al registrar el pago',
          buttons: ['OK']
        });
        await alert.present();
      }
    });
  }
} 