import { Component, OnInit, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { AutenticacionService } from 'src/app/services/autenticacion.service';
import { Usuario } from 'src/app/modelos/Usuario';
import html2pdf from 'html2pdf.js';
import { CertificadosService } from '../certificados.service';

@Component({
  selector: 'app-solicitar-certificado',
  templateUrl: './solicitar-certificado.component.html',
  styleUrls: ['./solicitar-certificado.component.css'],
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, IonicModule]
})
export class SolicitarCertificadoComponent implements OnInit {
  solicitudForm: FormGroup;
  usuario: Usuario | null = null;
  paraOtro: boolean = false;
  documentos: File[] = [];
  public precio: number = 0;
  fechaFormateada: string = '';
  certificadoListo: boolean = false;

  constructor(
    private fb: FormBuilder,
    private authService: AutenticacionService,
    @Inject(CertificadosService) private certificadosService: CertificadosService
  ) {
    this.solicitudForm = this.fb.group({
      para_otro: [false],
      nombre: [''],
      rut: [''],
      parentesco: [''],
      telefono: [''],
      direccion: [''],
      documentos: [null],
      motivo: ['']
    });
  }

  ngOnInit(): void {
    this.usuario = this.authService.obtenerUsuarioActual();
    if (this.usuario) {
      this.solicitudForm.patchValue({
        nombre: this.usuario.nombre + ' ' + this.usuario.apellido_paterno + ' ' + this.usuario.apellido_materno,
        rut: this.usuario.rut + '-' + this.usuario.dv_rut,
        telefono: this.usuario.telefono,
        direccion: this.usuario.direccion
      });
      
      if (this.usuario.tipo_usuario?.toLowerCase() === 'enrut') {
        this.precio = 2000;
      } else if (this.usuario.tipo_usuario?.toLowerCase() === 'vecino') {
        this.precio = 4000;
      } else {
        this.precio = 0;
      }
    }
    this.formatearFecha();
  }

  formatearFecha() {
    const meses = [
      'enero', 'febrero', 'marzo', 'abril', 'mayo', 'junio',
      'julio', 'agosto', 'septiembre', 'octubre', 'noviembre', 'diciembre'
    ];
    const hoy = new Date();
    this.fechaFormateada = `${hoy.getDate()} DE ${meses[hoy.getMonth()]} ${hoy.getFullYear()}`;
  }

  onParaOtroChange(event: any) {
    this.paraOtro = event.target.checked;
    if (!this.paraOtro && this.usuario) {
      this.solicitudForm.patchValue({
        nombre: this.usuario.nombre + ' ' + this.usuario.apellido_paterno + ' ' + this.usuario.apellido_materno,
        rut: this.usuario.rut + '-' + this.usuario.dv_rut,
        telefono: this.usuario.telefono,
        direccion: this.usuario.direccion,
        parentesco: ''
      });
    } else {
      this.solicitudForm.patchValue({
        nombre: '', rut: '', telefono: '', direccion: '', parentesco: ''
      });
    }
  }

  onFileChange(event: any) {
    if (event.target.files && event.target.files.length > 0) {
      this.documentos = Array.from(event.target.files);
    }
  }

  submit() {
    const usuario = this.authService.obtenerUsuarioActual();
    if (!usuario) {
      alert('No se pudo obtener el usuario autenticado.');
      return;
    }

    if (!this.solicitudForm.valid) {
      alert('Por favor, complete todos los campos requeridos correctamente.');
      return;
    }

    const formValue = this.solicitudForm.value;
    const documentoAdjunto = this.documentos.length > 0 ? this.documentos[0].name : 'Sin documentos adjuntos';
    
    let precio = 0;
    if (usuario.tipo_usuario?.toLowerCase() === 'enrut') {
      precio = 2000;
    } else if (usuario.tipo_usuario?.toLowerCase() === 'vecino') {
      precio = 4000;
    }

    const solicitud = {
      UsuarioRut: usuario.rut,
      TipoCertificadoId: 3,
      Motivo: 'Certificado de Residencia',
      Estado: 'pendiente',
      Observaciones: this.paraOtro ? `Solicitud para: ${formValue.nombre} - Parentesco: ${formValue.parentesco}` : 'Solicitud personal',
      Precio: precio,
      FechaSolicitud: new Date().toISOString(),
      DocumentosAdjuntos: documentoAdjunto,
      NombreSolicitante: formValue.nombre,
      RutSolicitante: formValue.rut,
      Telefono: formValue.telefono,
      Direccion: formValue.direccion
    };

    console.log('usuario_rut que se enviará:', solicitud.UsuarioRut);
    console.log('Enviando solicitud:', solicitud);

    this.certificadosService.solicitarCertificado(solicitud).subscribe({
      next: (res: any) => {
        if (precio > 0) {
          const pago = {
            SolicitudId: res.id || res.solicitudId,
            Monto: precio,
            RutUsuario: usuario.rut + '-' + usuario.dv_rut
          };
          this.certificadosService.iniciarPagoTransbank(pago).subscribe({
            next: (pagoRes: any) => {
              if (pagoRes.url && pagoRes.token) {
                window.location.href = pagoRes.url + '?token_ws=' + pagoRes.token;
              } else {
                alert('Error: No se recibió la URL de pago correctamente');
              }
            },
            error: (err) => {
              console.error('Error en pago:', err);
              alert('Error al iniciar el pago: ' + (err?.error?.mensaje || 'Error desconocido'));
            }
          });
        } else {
          alert('¡Solicitud enviada correctamente!');
        }
        this.solicitudForm.reset();
      },
      error: (err) => {
        console.error('Error en solicitud:', err);
        alert('Error al enviar la solicitud: ' + (err?.error?.mensaje || 'Error desconocido'));
      }
    });
  }

  exportarPDF() {
    const element = document.getElementById('certificadoResidencia');
    if (element) {
      const opt = {
        margin:       0.2,
        filename:     'certificado_residencia.pdf',
        image:        { type: 'jpeg', quality: 0.98 },
        html2canvas:  { scale: 2 },
        jsPDF:        { unit: 'in', format: 'letter', orientation: 'portrait' }
      };
      html2pdf().set(opt).from(element).save();
    }
  }

  previsualizarPDF() {
    const element = document.getElementById('certificadoFormal');
    if (element) {
      // Mostrar temporalmente el div
      const originalDisplay = element.style.display;
      element.style.display = 'block';

      // Eliminar funcionalidad de retroceso, solo dejar el título

      // Funcionalidad del botón de descarga
      setTimeout(() => {
        const descargarBtn = document.getElementById('descargarPDFBtn');
        if (descargarBtn) {
          descargarBtn.onclick = () => {
            const opt = {
              margin: 0.2,
              filename: 'certificado_residencia.pdf',
              image: { type: 'jpeg', quality: 0.98 },
              html2canvas: { scale: 2 },
              jsPDF: { unit: 'in', format: 'letter', orientation: 'portrait' }
            };
            html2pdf().set(opt).from(element).save();
          };
        }
      }, 200);

      const opt = {
        margin: 0.2,
        filename: 'certificado_residencia.pdf',
        image: { type: 'jpeg', quality: 0.98 },
        html2canvas: { scale: 2 },
        jsPDF: { unit: 'in', format: 'letter', orientation: 'portrait' }
      };
      html2pdf().set(opt).from(element).toPdf().get('pdf').then(function(pdf: any) {
        const blob = pdf.output('blob');
        const url = URL.createObjectURL(blob);
        window.open(url, '_blank');
        // Volver a ocultar el div después de generar el PDF
        element.style.display = originalDisplay;
      });
      this.certificadoListo = true;
    }
  }
} 