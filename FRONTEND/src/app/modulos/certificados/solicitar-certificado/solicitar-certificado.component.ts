import { Component, OnInit, Inject, ElementRef, ViewChild, AfterViewInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { AutenticacionService } from 'src/app/services/autenticacion.service';
import { Usuario } from 'src/app/modelos/Usuario';
import html2pdf from 'html2pdf.js';
import { CertificadosService } from '../certificados.service';
import { ActivatedRoute, Router } from '@angular/router';
import { Filesystem, Directory, Encoding } from '@capacitor/filesystem';
import { FileOpener } from '@capacitor-community/file-opener';
import { Capacitor } from '@capacitor/core';
import { App } from '@capacitor/app';

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
  public puedeDescargarPDF: boolean = false;
  public mostrarPrevisualizacion: boolean = false;
  
  constructor(
    private fb: FormBuilder,
    private authService: AutenticacionService,
    @Inject(CertificadosService) private certificadosService: CertificadosService,
    private route: ActivatedRoute,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {
    this.solicitudForm = this.fb.group({
      para_otro: [false],
      nombre: ['', [Validators.required, Validators.minLength(2)]],
      rut: ['', [Validators.required]],
      parentesco: [''],
      telefono: ['', [Validators.required, Validators.minLength(8)]],
      direccion: ['', [Validators.required, Validators.minLength(5)]],
      documentos: [null],
      motivo: ['', [Validators.required, Validators.minLength(10)]]
    });
  }

  ngOnInit(): void {
    // Restaurar datos si existen en sessionStorage
    const saved = sessionStorage.getItem('certificadoFormData');
    if (saved) {
      const data = JSON.parse(saved);
      this.solicitudForm.patchValue(data.form);
      this.paraOtro = data.paraOtro;
      this.puedeDescargarPDF = data.puedeDescargarPDF;
      this.mostrarPrevisualizacion = data.mostrarPrevisualizacion;
      sessionStorage.removeItem('certificadoFormData');
    }
    this.route.queryParams.subscribe(params => {
      if (params['descargar']) {
        this.puedeDescargarPDF = true;
      }
    });
    this.usuario = this.authService.obtenerUsuarioActual();
    if (this.usuario) {
      this.solicitudForm.patchValue({
        nombre: this.usuario.nombre + ' ' + this.usuario.apellido_paterno + ' ' + this.usuario.apellido_materno,
        rut: this.usuario.rut + '-' + this.usuario.dv_rut,
        telefono: this.usuario.telefono,
        direccion: this.usuario.direccion
      });
      const tipo = this.usuario.tipo_usuario?.toLowerCase();
      if (tipo === 'enrut' || tipo === 'socio' || tipo === 'directiva') {
        this.precio = 2000;
      } else if (tipo === 'vecino') {
        this.precio = 3000;
      } else {
        this.precio = 0;
      }
    }
    this.formatearFecha();

    // Lógica para refrescar el estado al volver a la app
    App.addListener('appStateChange', ({ isActive }) => {
      if (isActive) {
        this.verificarEstadoCertificado();
      }
    });
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
    // Eliminar todas las referencias a 'canvas' y 'ctx' en el componente, incluyendo la inicialización en initSignatureCanvas y cualquier uso en métodos relacionados con la firma.
  }

  // ========================
  // FUNCIONES ORIGINALES MEJORADAS
  // ========================

  onParaOtroChange(event: any) {
    this.paraOtro = event.target.checked;
    const parentescoControl = this.solicitudForm.get('parentesco');
    if (this.paraOtro) {
      parentescoControl?.setValidators([Validators.required]);
    } else {
      parentescoControl?.clearValidators();
      parentescoControl?.setValue('');
    }
    parentescoControl?.updateValueAndValidity();

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
      const errores: string[] = [];
      Object.keys(this.solicitudForm.controls).forEach(key => {
        const control = this.solicitudForm.get(key);
        if (control?.errors) {
          errores.push(`${key}: ${Object.keys(control.errors).join(', ')}`);
        }
      });
      alert(`Por favor, corrija los siguientes errores:\n${errores.join('\n')}`);
      return;
    }

    const formValue = this.solicitudForm.value;
    const documentoAdjunto = this.documentos.length > 0 ? this.documentos[0].name : 'Sin documentos adjuntos';
    
    let precio = 0;
    const tipo = usuario.tipo_usuario?.toLowerCase();
    if (tipo === 'enrut' || tipo === 'socio' || tipo === 'directiva') {
      precio = 2000;
    } else if (tipo === 'vecino') {
      precio = 3000;
    }

    const solicitud = {
      Id: 0,
      UsuarioRut: usuario.rut || '',
      TipoCertificadoId: 3,
      FechaSolicitud: new Date().toISOString(),
      Estado: 'pendiente',
      Motivo: formValue.motivo?.trim() || 'Certificado de Residencia',
      DocumentosAdjuntos: documentoAdjunto || 'Sin documentos adjuntos',
      FechaAprobacion: null,
      DirectivaRut: null,
      Precio: precio,
      Observaciones: this.paraOtro ? `Solicitud para: ${formValue.nombre} - Parentesco: ${formValue.parentesco}` : 'Solicitud personal',
      NombreSolicitante: formValue.nombre?.trim() || '',
      RutSolicitante: formValue.rut?.trim() || '',
      Telefono: formValue.telefono?.trim() || '',
      Direccion: formValue.direccion?.trim() || '',
      FirmaDigital: '',
      HashVerificacion: '',
      TimestampFirma: new Date().toISOString(),
      UsuarioFirmante: String(usuario.rut || '')
    };

    console.log('Enviando solicitud:', solicitud);

    this.certificadosService.solicitarCertificado(solicitud).subscribe({
      next: (res: any) => {
        console.log('Respuesta backend al crear solicitud:', res);
        if (precio > 0) {
          const pago = {
            SolicitudId: res.id || res.solicitudId,
            Monto: precio,
            RutUsuario: usuario.rut + '-' + (usuario.dv_rut || ''),
            Token: ''
          };
          console.log('Objeto pago que se enviará:', pago);
          this.certificadosService.iniciarPagoTransbank(pago).subscribe({
            next: (pagoRes: any) => {
              console.log('Respuesta de Transbank:', pagoRes);
              if (pagoRes.url && pagoRes.token) {
                console.log('Redirigiendo a:', pagoRes.url + '?token_ws=' + pagoRes.token);
                window.open(pagoRes.url + '?token_ws=' + pagoRes.token, '_self');
              } else {
                console.error('Respuesta de pago incompleta:', pagoRes);
                alert('Error: No se recibió la URL de pago correctamente');
              }
            },
            error: (err: any) => {
              console.error('Error detallado en pago:', err);
              alert('Error al iniciar el pago: ' + (err?.error?.mensaje || 'Error desconocido'));
            }
          });
        } else {
          alert('¡Solicitud enviada correctamente!');
        }
      },
      error: (err: any) => {
        console.error('Error en solicitud:', err);
        alert('Error al enviar la solicitud: ' + (err?.error?.mensaje || 'Error desconocido'));
      }
    });
  }

  exportarPDF() {
    this.certificadosService.obtenerHistorial().subscribe({
      next: async (historial: any[]) => {
        let codigo = '';
        let fecha = '';

        if (historial && historial.length > 0) {
          const ultimoCertificado = historial[0];
          codigo = ultimoCertificado.codigoVerificacion || 'No disponible';
          if (ultimoCertificado.fechaEmision) {
            fecha = new Date(ultimoCertificado.fechaEmision).toLocaleString('es-CL');
          } else {
            fecha = 'No disponible';
          }
        }
        
        const element = document.getElementById('certificadoFormal');
        if (!element) {
          console.error('Elemento #certificadoFormal no encontrado');
          return;
        }
        element.style.display = 'block';
        setTimeout(async () => {
          const opt = {
            margin: 0.2,
            filename: `certificado_residencia_${codigo || 'preview'}.pdf`,
            image: { type: 'jpeg', quality: 0.98 },
            html2canvas: { scale: 2 },
            jsPDF: { unit: 'in', format: 'letter', orientation: 'portrait' }
          };
          try {
            if (this.isAndroid()) {
              const pdf = await html2pdf().set(opt).from(element).toPdf().get('pdf');
              const blob = pdf.output('blob');
              await this.descargarEnAndroid(blob, `certificado_residencia_${codigo || 'preview'}.pdf`);
            } else {
              await html2pdf().set(opt).from(element).save();
            }
          } catch (error) {
            console.error('Error al generar el PDF:', error);
          } finally {
            element.style.display = 'none';
            this.router.navigate(['/modulos/certificados/historial-certificados']);
          }
        }, 100);
      },
      error: (err) => {
        console.error('Error al obtener historial para PDF, usando datos locales.', err);
        this.generarPDF('', '', true);
        this.router.navigate(['/modulos/certificados/historial-certificados']);
      }
    });
  }

  previsualizarPDF() {
    this.mostrarPrevisualizacion = true;
  }

  cerrarPrevisualizacion() {
    this.mostrarPrevisualizacion = false;
  }

  private isAndroid(): boolean {
    return Capacitor.getPlatform() === 'android';
  }

  private blobToBase64(blob: Blob): Promise<string> {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.onloadend = () => {
        const result = reader.result as string;
        const base64 = result.split(',')[1];
        resolve(base64);
      };
      reader.onerror = reject;
      reader.readAsDataURL(blob);
    });
  }

  private async descargarEnAndroid(blob: Blob, fileName: string): Promise<void> {
    try {
      const base64Data = await this.blobToBase64(blob);
      const savedFile = await Filesystem.writeFile({
        path: fileName,
        data: base64Data,
        directory: Directory.Documents,
        recursive: true
      });
      await FileOpener.open({
        filePath: savedFile.uri,
        contentType: 'application/pdf'
      });
    } catch (error) {
      console.error('Error al descargar en Android:', error);
      throw new Error('No se pudo descargar el archivo en Android');
    }
  }

  private generarPDF(codigo: string, fecha: string, guardarArchivo: boolean): void {
    const element = document.getElementById('certificadoFormal');
    if (!element) {
      console.error('Elemento #certificadoFormal no encontrado');
      return;
    }
    
    element.style.display = 'block';

    setTimeout(async () => {
      const opt = {
        margin: 0.2,
        filename: `certificado_residencia_${codigo || 'preview'}.pdf`,
        image: { type: 'jpeg', quality: 0.98 },
        html2canvas: { scale: 2 },
        jsPDF: { unit: 'in', format: 'letter', orientation: 'portrait' }
      };

      try {
        if (this.isAndroid()) {
          const pdf = await html2pdf().set(opt).from(element).toPdf().get('pdf');
            const blob = pdf.output('blob');
          await this.descargarEnAndroid(blob, `certificado_residencia_${codigo || 'preview'}.pdf`);
        } else {
          await html2pdf().set(opt).from(element).save();
        }
      } catch (error) {
        console.error('Error al generar el PDF:', error);
      } finally {
        element.style.display = 'none';
          if (!guardarArchivo) this.certificadoListo = true;
      }
    }, 100);
  }

  private resetForm(): void {
    this.solicitudForm.reset();
    this.documentos = [];
    this.certificadoListo = false;
  }

  // Función auxiliar para validar el estado del certificado
  puedeGenerarPDF(): boolean {
    return this.solicitudForm.valid;
  }

  pagar() {
    const usuario = this.usuario;
    if (!usuario) {
      alert('No se pudo obtener el usuario autenticado.');
      return;
    }

    // Validación más detallada del formulario
    if (!this.solicitudForm.valid) {
      const errores: string[] = [];
      Object.keys(this.solicitudForm.controls).forEach(key => {
        const control = this.solicitudForm.get(key);
        if (control?.errors) {
          errores.push(`${key}: ${Object.keys(control.errors).join(', ')}`);
        }
      });
      alert(`Por favor, corrija los siguientes errores:\n${errores.join('\n')}`);
      return;
    }

    const formValue = this.solicitudForm.value;
    
    // Validar campos críticos
    if (!formValue.nombre || !formValue.rut || !formValue.telefono || !formValue.direccion) {
      alert('Por favor, complete todos los campos obligatorios del formulario.');
      return;
    }

    // Validar que el motivo esté presente
    if (!formValue.motivo || formValue.motivo.trim() === '') {
      alert('Por favor, ingrese el motivo de la solicitud.');
      return;
    }

    const documentoAdjunto = this.documentos && this.documentos.length > 0 ? this.documentos[0].name : 'Sin documentos adjuntos';
    let precio = 0;
    const tipo = usuario.tipo_usuario?.toLowerCase();
    if (tipo === 'enrut' || tipo === 'socio' || tipo === 'directiva') {
      precio = 2000;
    } else if (tipo === 'vecino') {
      precio = 3000;
    } else {
      alert('Tipo de usuario no válido para el cálculo del precio.');
      return;
    }

    // Asegurar que todos los campos requeridos tengan valores válidos
    const solicitud = {
      Id: 0,
      UsuarioRut: usuario.rut || '',
      TipoCertificadoId: 3,
      FechaSolicitud: new Date().toISOString(),
      Estado: 'pendiente',
      Motivo: formValue.motivo?.trim() || 'Certificado de Residencia',
      DocumentosAdjuntos: documentoAdjunto || 'Sin documentos adjuntos',
      FechaAprobacion: null,
      DirectivaRut: null,
      Precio: precio,
      Observaciones: this.paraOtro ? `Solicitud para: ${formValue.nombre} - Parentesco: ${formValue.parentesco}` : 'Solicitud personal',
      NombreSolicitante: formValue.nombre?.trim() || '',
      RutSolicitante: formValue.rut?.trim() || '',
      Telefono: formValue.telefono?.trim() || '',
      Direccion: formValue.direccion?.trim() || '',
      FirmaDigital: '',
      HashVerificacion: '',
      TimestampFirma: new Date().toISOString(),
      UsuarioFirmante: String(usuario.rut || '')
    };

    console.log('Enviando solicitud:', solicitud);

    this.certificadosService.solicitarCertificado(solicitud).subscribe({
      next: (res: any) => {
        console.log('Respuesta backend al crear solicitud:', res);
        if (precio > 0) {
        const pago = {
          SolicitudId: res.id || res.solicitudId,
          Monto: precio,
          RutUsuario: usuario.rut + '-' + (usuario.dv_rut || ''),
          Token: ''
        };
          console.log('Objeto pago que se enviará:', pago);
        this.certificadosService.iniciarPagoTransbank(pago).subscribe({
          next: (pagoRes: any) => {
            console.log('Respuesta de Transbank:', pagoRes);
            if (pagoRes.url && pagoRes.token) {
              console.log('Redirigiendo a:', pagoRes.url + '?token_ws=' + pagoRes.token);
              window.open(pagoRes.url + '?token_ws=' + pagoRes.token, '_self');
            } else {
              console.error('Respuesta de pago incompleta:', pagoRes);
              alert('Error: No se recibió la URL de pago correctamente');
            }
          },
          error: (err: any) => {
            console.error('Error detallado en pago:', err);
            alert('Error al iniciar el pago: ' + (err?.error?.mensaje || 'Error desconocido'));
          }
        });
        } else {
          alert('¡Solicitud enviada correctamente!');
        }
      },
      error: (err: any) => {
        console.error('Error en solicitud:', err);
        alert('Error al enviar la solicitud: ' + (err?.error?.mensaje || 'Error desconocido'));
      }
    });
  }

  irAlHistorial() {
    this.router.navigate(['/modulos/certificados/historial-certificados']);
  }

  // Nueva función para verificar el estado del último certificado
  verificarEstadoCertificado() {
    this.certificadosService.obtenerHistorial().subscribe({
      next: (historial: any[]) => {
        console.log('Historial recibido:', historial);
        if (historial && historial.length > 0) {
          const ultimo = historial[0];
          if (
            (ultimo.estado === 'aprobado' || ultimo.estado === 'emitido' || ultimo.estado === 'vigente') &&
            (ultimo.certificadoId || ultimo.certificado_id)
          ) {
            this.puedeDescargarPDF = true;
          } else {
            this.puedeDescargarPDF = false;
          }
        } else {
          this.puedeDescargarPDF = false;
        }
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.puedeDescargarPDF = false;
        this.cdr.detectChanges();
      }
    });
  }

  refresh() {
    // Guardar datos del formulario y variables necesarias
    sessionStorage.setItem('certificadoFormData', JSON.stringify({
      form: this.solicitudForm.value,
      paraOtro: this.paraOtro,
      puedeDescargarPDF: this.puedeDescargarPDF,
      mostrarPrevisualizacion: this.mostrarPrevisualizacion
    }));
    window.location.reload();
  }

  async descargarYabrirPDFCertificado() {
    // Obtén el ID del certificado generado (el más reciente del historial)
    this.certificadosService.obtenerHistorial().subscribe({
      next: async (historial: any[]) => {
        if (historial && historial.length > 0) {
          const ultimo = historial[0];
          const certificadoId = ultimo.certificadoId || ultimo.certificado_id || ultimo.id;
          if (!certificadoId) {
            alert('No se encontró el certificado para descargar.');
            return;
          }

          this.certificadosService.descargarCertificado(certificadoId).subscribe(
            async (response) => {
              // Decodificar el base64
              const byteCharacters = atob(response.file);
              const byteNumbers = new Array(byteCharacters.length);
              for (let i = 0; i < byteCharacters.length; i++) {
                byteNumbers[i] = byteCharacters.charCodeAt(i);
              }
              const byteArray = new Uint8Array(byteNumbers);
              const blob = new Blob([byteArray], { type: 'application/pdf' });

              // Guardar y abrir en Android
              if (Capacitor.getPlatform() === 'android') {
                const base64Data = await this.blobToBase64(blob);
                const fileName = response.fileName || 'certificado_residencia.pdf';
                const savedFile = await Filesystem.writeFile({
                  path: fileName,
                  data: base64Data,
                  directory: Directory.Documents,
                  recursive: true
                });
                await FileOpener.open({
                  filePath: savedFile.uri,
                  contentType: 'application/pdf'
                });
              } else {
                // Para navegador, descarga normal
                const url = window.URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.href = url;
                a.download = response.fileName || 'certificado_residencia.pdf';
                document.body.appendChild(a);
                a.click();
                window.URL.revokeObjectURL(url);
                document.body.removeChild(a);
              }
            },
            (error) => {
              alert('Hubo un error al descargar el certificado.');
            }
          );
        } else {
          alert('No se encontró el certificado para descargar.');
        }
      },
      error: () => {
        alert('No se pudo obtener el historial de certificados.');
      }
    });
  }
}