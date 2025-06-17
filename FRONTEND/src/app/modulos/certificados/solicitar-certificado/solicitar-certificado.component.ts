import { Component, OnInit, Inject, ElementRef, ViewChild, AfterViewInit } from '@angular/core';
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
export class SolicitarCertificadoComponent implements OnInit, AfterViewInit {
  @ViewChild('signatureCanvas', { static: false }) signatureCanvas!: ElementRef<HTMLCanvasElement>;
  
  solicitudForm: FormGroup;
  usuario: Usuario | null = null;
  paraOtro: boolean = false;
  documentos: File[] = [];
  public precio: number = 0;
  fechaFormateada: string = '';
  certificadoListo: boolean = false;
  
  // Propiedades para firma digital
  private canvas!: HTMLCanvasElement;
  private ctx!: CanvasRenderingContext2D;
  private isDrawing: boolean = false;
  public signatureData: string | null = null;
  public firmaGuardada: boolean = false;
  public hashVerificacion: string = '';
  public timestampFirma: string = '';
  
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

  ngAfterViewInit(): void {
    // Inicializar canvas después de que la vista se haya cargado
    setTimeout(() => {
      this.initSignatureCanvas();
    }, 100);
  }

  formatearFecha() {
    const meses = [
      'enero', 'febrero', 'marzo', 'abril', 'mayo', 'junio',
      'julio', 'agosto', 'septiembre', 'octubre', 'noviembre', 'diciembre'
    ];
    const hoy = new Date();
    this.fechaFormateada = `${hoy.getDate()} DE ${meses[hoy.getMonth()]} ${hoy.getFullYear()}`;
  }

  // ========================
  // FUNCIONES FIRMA DIGITAL
  // ========================

  initSignatureCanvas(): void {
    if (this.signatureCanvas) {
      this.canvas = this.signatureCanvas.nativeElement;
      this.ctx = this.canvas.getContext('2d')!;
      
      // Configurar el canvas
      this.ctx.strokeStyle = '#000';
      this.ctx.lineWidth = 2;
      this.ctx.lineCap = 'round';
      this.ctx.lineJoin = 'round';

      // Event listeners para mouse
      this.canvas.addEventListener('mousedown', this.startDrawing.bind(this));
      this.canvas.addEventListener('mousemove', this.draw.bind(this));
      this.canvas.addEventListener('mouseup', this.stopDrawing.bind(this));
      this.canvas.addEventListener('mouseout', this.stopDrawing.bind(this));

      // Event listeners para touch (móvil)
      this.canvas.addEventListener('touchstart', this.handleTouch.bind(this));
      this.canvas.addEventListener('touchmove', this.handleTouch.bind(this));
      this.canvas.addEventListener('touchend', this.stopDrawing.bind(this));
      
      // Prevenir scroll en móvil cuando se dibuja
      this.canvas.addEventListener('touchstart', (e) => e.preventDefault());
      this.canvas.addEventListener('touchmove', (e) => e.preventDefault());
    }
  }

  private startDrawing(e: MouseEvent): void {
    this.isDrawing = true;
    const rect = this.canvas.getBoundingClientRect();
    this.ctx.beginPath();
    this.ctx.moveTo(e.clientX - rect.left, e.clientY - rect.top);
  }

  private draw(e: MouseEvent): void {
    if (!this.isDrawing) return;
    const rect = this.canvas.getBoundingClientRect();
    this.ctx.lineTo(e.clientX - rect.left, e.clientY - rect.top);
    this.ctx.stroke();
  }

  private stopDrawing(): void {
    if (this.isDrawing) {
      this.isDrawing = false;
      this.ctx.beginPath();
    }
  }

  private handleTouch(e: TouchEvent): void {
    e.preventDefault();
    const touch = e.touches[0];
    const mouseEvent = new MouseEvent(
      e.type === 'touchstart' ? 'mousedown' : 
      e.type === 'touchmove' ? 'mousemove' : 'mouseup', 
      {
        clientX: touch.clientX,
        clientY: touch.clientY
      }
    );
    this.canvas.dispatchEvent(mouseEvent);
  }

  limpiarFirma(): void {
    if (this.ctx && this.canvas) {
      this.ctx.clearRect(0, 0, this.canvas.width, this.canvas.height);
      this.signatureData = null;
      this.firmaGuardada = false;
      this.hashVerificacion = '';
      this.timestampFirma = '';
    }
  }

  guardarFirma(): void {
    if (!this.canvas || this.isCanvasBlank()) {
      alert('Por favor, dibuje su firma antes de guardar.');
      return;
    }

    this.signatureData = this.canvas.toDataURL();
    this.firmaGuardada = true;
    this.generarHashVerificacion();
    
    // Mostrar mensaje de éxito
    console.log('✅ Firma guardada exitosamente');
  }

  private isCanvasBlank(): boolean {
    const blank = document.createElement('canvas');
    blank.width = this.canvas.width;
    blank.height = this.canvas.height;
    return this.canvas.toDataURL() === blank.toDataURL();
  }

  private generarHashVerificacion(): void {
    const formValue = this.solicitudForm.value;
    const timestamp = new Date().toISOString();
    
    const data = {
      nombre: formValue.nombre,
      rut: formValue.rut,
      direccion: formValue.direccion,
      motivo: formValue.motivo,
      signature: this.signatureData,
      timestamp: timestamp,
      usuario_firmante: this.usuario?.rut
    };

    // Generar hash simple (en producción usar crypto-js)
    this.hashVerificacion = btoa(JSON.stringify(data)).substring(0, 32);
    this.timestampFirma = new Date().toLocaleString('es-CL');
  }

  // ========================
  // FUNCIONES ORIGINALES MEJORADAS
  // ========================

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
    // Validar que la firma esté presente
    if (!this.firmaGuardada || !this.signatureData) {
      alert('⚠️ Debe firmar digitalmente el certificado antes de enviarlo.');
      return;
    }

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
      Direccion: formValue.direccion,
      // Datos de la firma digital
      FirmaDigital: this.signatureData,
      HashVerificacion: this.hashVerificacion,
      TimestampFirma: this.timestampFirma,
      UsuarioFirmante: usuario.rut
    };

    console.log('usuario_rut que se enviará:', solicitud.UsuarioRut);
    console.log('Enviando solicitud con firma digital:', solicitud);

    this.certificadosService.solicitarCertificado(solicitud).subscribe({
      next: (res: any) => {
        if (precio > 0) {
          const pago = {
            SolicitudId: res.id || res.solicitudId,
            Monto: precio,
            RutUsuario: usuario.rut + '-' + (usuario.dv_rut || '')
          };
          this.certificadosService.iniciarPagoTransbank(pago).subscribe({
            next: (pagoRes: any) => {
              if (pagoRes.url && pagoRes.token) {
                window.location.href = pagoRes.url + '?token_ws=' + pagoRes.token;
              } else {
                alert('Error: No se recibió la URL de pago correctamente');
              }
            },
            error: (err: any) => {
              console.error('Error en pago:', err);
              alert('Error al iniciar el pago: ' + (err?.error?.mensaje || 'Error desconocido'));
            }
          });
        } else {
          alert('¡Solicitud enviada correctamente con firma digital!');
        }
        this.resetForm();
      },
      error: (err: any) => {
        console.error('Error en solicitud:', err);
        alert('Error al enviar la solicitud: ' + (err?.error?.mensaje || 'Error desconocido'));
      }
    });
  }

  exportarPDF() {
    if (!this.firmaGuardada) {
      alert('⚠️ Debe firmar el certificado antes de exportar el PDF.');
      return;
    }

    const element = document.getElementById('certificadoFormal');
    if (element) {
      const opt = {
        margin: 0.2,
        filename: `certificado_residencia_${this.hashVerificacion}.pdf`,
        image: { type: 'jpeg', quality: 0.98 },
        html2canvas: { scale: 2 },
        jsPDF: { unit: 'in', format: 'letter', orientation: 'portrait' }
      };
      html2pdf().set(opt).from(element).save();
    }
  }

  previsualizarPDF() {
    if (!this.firmaGuardada) {
      alert('⚠️ Debe firmar el certificado antes de previsualizar.');
      return;
    }

    const element = document.getElementById('certificadoFormal');
    if (element) {
      const originalDisplay = element.style.display;
      element.style.display = 'block';

      const opt = {
        margin: 0.2,
        filename: `certificado_residencia_${this.hashVerificacion}.pdf`,
        image: { type: 'jpeg', quality: 0.98 },
        html2canvas: { scale: 2 },
        jsPDF: { unit: 'in', format: 'letter', orientation: 'portrait' }
      };
      
      html2pdf().set(opt).from(element).toPdf().get('pdf').then((pdf: any) => {
        const blob = pdf.output('blob');
        const url = URL.createObjectURL(blob);
        window.open(url, '_blank');
        element.style.display = originalDisplay;
      });
      
      this.certificadoListo = true;
    }
  }

  private resetForm(): void {
    this.solicitudForm.reset();
    this.limpiarFirma();
    this.documentos = [];
    this.certificadoListo = false;
  }

  // Función auxiliar para validar el estado del certificado
  puedeGenerarPDF(): boolean {
    return this.firmaGuardada && this.solicitudForm.valid;
  }

  // Función para obtener el estado de la firma
  getEstadoFirma(): string {
    if (this.firmaGuardada) {
      return 'Firmado digitalmente';
    } else {
      return 'Pendiente de firma';
    }
  }

  pagar() {
    // Validar que la firma esté presente
    if (!this.firmaGuardada || !this.signatureData) {
      alert('⚠️ Debe firmar digitalmente el certificado antes de pagar.');
      return;
    }

    const usuario = this.usuario;
    if (!usuario) {
      alert('No se pudo obtener el usuario autenticado.');
      return;
    }
    if (!this.solicitudForm.valid) {
      alert('Por favor, complete todos los campos requeridos correctamente.');
      return;
    }
    const formValue = this.solicitudForm.value;
    const documentoAdjunto = this.documentos && this.documentos.length > 0 ? this.documentos[0].name : 'Sin documentos adjuntos';
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
      Direccion: formValue.direccion,
      FirmaDigital: this.signatureData,
      HashVerificacion: this.hashVerificacion,
      TimestampFirma: this.timestampFirma,
      UsuarioFirmante: usuario.rut
    };
    this.certificadosService.solicitarCertificado(solicitud).subscribe({
      next: (res: any) => {
        const pago = {
          SolicitudId: res.id || res.solicitudId,
          Monto: precio,
          RutUsuario: usuario.rut + '-' + (usuario.dv_rut || '')
        };
        this.certificadosService.iniciarPagoTransbank(pago).subscribe({
          next: (pagoRes: any) => {
            if (pagoRes.url && pagoRes.token) {
              window.location.href = pagoRes.url + '?token_ws=' + pagoRes.token;
            } else {
              alert('Error: No se recibió la URL de pago correctamente');
            }
          },
          error: (err: any) => {
            console.error('Error en pago:', err);
            alert('Error al iniciar el pago: ' + (err?.error?.mensaje || 'Error desconocido'));
          }
        });
      },
      error: (err: any) => {
        console.error('Error en solicitud:', err);
        alert('Error al enviar la solicitud: ' + (err?.error?.mensaje || 'Error desconocido'));
      }
    });
  }
}