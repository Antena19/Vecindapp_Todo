import { Component, OnInit, Inject, ElementRef, ViewChild, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { AutenticacionService } from 'src/app/services/autenticacion.service';
import { Usuario } from 'src/app/modelos/Usuario';
import html2pdf from 'html2pdf.js';
import { CertificadosService } from '../certificados.service';
import { ActivatedRoute } from '@angular/router';
import { Filesystem, Directory, Encoding } from '@capacitor/filesystem';
import { FileOpener } from '@capacitor-community/file-opener';
import { Capacitor } from '@capacitor/core';

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
  public puedeDescargarPDF: boolean = false;
  
  constructor(
    private fb: FormBuilder,
    private authService: AutenticacionService,
    @Inject(CertificadosService) private certificadosService: CertificadosService,
    private route: ActivatedRoute
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
    this.route.queryParams.subscribe(params => {
      if (params['descargar']) {
        this.puedeDescargarPDF = true;
        // Descargar automáticamente el certificado más reciente
        this.descargarCertificadoReciente();
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
      FirmaDigital: this.signatureData || '',
      HashVerificacion: this.hashVerificacion || '',
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
                // Usar window.open en lugar de window.location.href para mejor compatibilidad
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
          alert('¡Solicitud enviada correctamente con firma digital!');
        }
        this.resetForm();
        this.puedeDescargarPDF = true;
      },
      error: (err: any) => {
        console.error('Error en solicitud:', err);
        alert('Error al enviar la solicitud: ' + (err?.error?.mensaje || 'Error desconocido'));
      }
    });
  }

  exportarPDF() {
    this.certificadosService.obtenerHistorial().subscribe({
      next: (historial: any[]) => {
        let codigo = '';
        let fecha = '';

        if (historial && historial.length > 0) {
          const ultimoCertificado = historial[0]; // El historial viene ordenado por fecha desc.
          codigo = ultimoCertificado.codigoVerificacion || 'No disponible';
          if (ultimoCertificado.fechaEmision) {
            fecha = new Date(ultimoCertificado.fechaEmision).toLocaleString('es-CL');
          } else {
            fecha = 'No disponible';
          }
        }
        
        this.generarPDF(codigo, fecha, true);
      },
      error: (err) => {
        console.error('Error al obtener historial para PDF, usando datos locales.', err);
        this.generarPDF(this.hashVerificacion, this.timestampFirma, true);
      }
    });
  }

  previsualizarPDF() {
    if (!this.firmaGuardada) {
      alert('⚠️ Debe firmar el certificado antes de previsualizar.');
      return;
    }
    // Para la previsualización, no mostramos código ni fecha.
    this.generarPDF('', '', false);
  }

  private generarPDF(codigo: string, fecha: string, guardarArchivo: boolean): void {
    const element = document.getElementById('certificadoFormal');
    if (!element) {
      console.error('Elemento #certificadoFormal no encontrado');
      return;
    }

    const codigoElement = element.querySelector('#pdf-codigo-verificacion') as HTMLElement;
    const fechaElement = element.querySelector('#pdf-fecha-emision') as HTMLElement;
    const validadoElement = codigoElement?.parentElement;

    // Guardar estado original
    const codigoOriginal = codigoElement?.innerText;
    const fechaOriginal = fechaElement?.innerText;
    const displayOriginal = validadoElement?.style.display ?? '';
    const originalElementDisplay = element.style.display;

    // Modificar para la generación del PDF
    if (validadoElement) {
      if (codigo || fecha) {
        if(codigoElement) codigoElement.innerText = codigo;
        if(fechaElement) fechaElement.innerText = fecha;
        validadoElement.style.display = 'block';
      } else {
        validadoElement.style.display = 'none';
      }
    }
    
    element.style.display = 'block';

    // *** Usamos un setTimeout para dar tiempo al DOM a renderizar los cambios ***
    setTimeout(() => {
      const opt = {
        margin: 0.2,
        filename: `certificado_residencia_${codigo || 'preview'}.pdf`,
        image: { type: 'jpeg', quality: 0.98 },
        html2canvas: { scale: 2 },
        jsPDF: { unit: 'in', format: 'letter', orientation: 'portrait' }
      };

      const promise = guardarArchivo 
        ? html2pdf().set(opt).from(element).save() 
        : html2pdf().set(opt).from(element).toPdf().get('pdf').then((pdf: any) => {
            const blob = pdf.output('blob');
            const url = URL.createObjectURL(blob);
            window.open(url, '_blank');
          });

      promise.catch((err: any) => console.error("Error al generar el PDF:", err))
        .finally(() => {
          // Restaurar estado original
          if (validadoElement) {
            if(codigoElement && codigoOriginal) codigoElement.innerText = codigoOriginal;
            if(fechaElement && fechaOriginal) fechaElement.innerText = fechaOriginal;
            validadoElement.style.display = displayOriginal;
          }
          element.style.display = originalElementDisplay;
          if (!guardarArchivo) this.certificadoListo = true;
        });
    }, 100);
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
      FirmaDigital: this.signatureData || '',
      HashVerificacion: this.hashVerificacion || '',
      TimestampFirma: new Date().toISOString(),
      UsuarioFirmante: String(usuario.rut || '')
    };

    // Validación final antes de enviar
    const camposRequeridos = [
      { campo: 'Estado', valor: solicitud.Estado },
      { campo: 'Motivo', valor: solicitud.Motivo },
      { campo: 'Telefono', valor: solicitud.Telefono },
      { campo: 'Direccion', valor: solicitud.Direccion },
      { campo: 'FirmaDigital', valor: solicitud.FirmaDigital },
      { campo: 'Observaciones', valor: solicitud.Observaciones },
      { campo: 'RutSolicitante', valor: solicitud.RutSolicitante },
      { campo: 'UsuarioFirmante', valor: solicitud.UsuarioFirmante },
      { campo: 'HashVerificacion', valor: solicitud.HashVerificacion },
      { campo: 'NombreSolicitante', valor: solicitud.NombreSolicitante }
    ];

    const camposVacios = camposRequeridos.filter(item => !item.valor || item.valor.toString().trim() === '');
    
    if (camposVacios.length > 0) {
      const listaCampos = camposVacios.map(item => item.campo).join(', ');
      alert(`Los siguientes campos están vacíos: ${listaCampos}`);
      console.error('Campos vacíos:', camposVacios);
      return;
    }

    console.log('Enviando solicitud completa:', JSON.stringify(solicitud, null, 2));

    this.certificadosService.solicitarCertificado(solicitud).subscribe({
      next: (res: any) => {
        console.log('Respuesta del servidor:', res);
        if (!res.id && !res.solicitudId) {
          alert('Error: No se recibió un ID válido de la solicitud');
          return;
        }

        const pago = {
          SolicitudId: res.id || res.solicitudId,
          Monto: precio,
          RutUsuario: usuario.rut + '-' + (usuario.dv_rut || ''),
          Token: ''
        };

        console.log('Iniciando pago con datos:', pago);
        
        this.certificadosService.iniciarPagoTransbank(pago).subscribe({
          next: (pagoRes: any) => {
            console.log('Respuesta de Transbank:', pagoRes);
            if (pagoRes.url && pagoRes.token) {
              console.log('Redirigiendo a:', pagoRes.url + '?token_ws=' + pagoRes.token);
              // Usar window.open en lugar de window.location.href para mejor compatibilidad
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
      },
      error: (err: any) => {
        console.error('Error en solicitud:', err);
        
        if (err.error && err.error.errors) {
          console.error('Errores de validación del backend:', err.error.errors);
          const errorMessages = Object.entries(err.error.errors)
            .map(([field, messages]: [string, any]) => `${field}: ${Array.isArray(messages) ? messages.join(', ') : messages}`)
            .join('\n');
          alert(`Errores de validación del backend:\n${errorMessages}`);
        } else {
          alert('Error al enviar la solicitud: ' + (err?.error?.mensaje || err?.message || 'Error desconocido'));
        }
      }
    });
  }

  async descargarCertificadoReciente(): Promise<void> {
    try {
      console.log('Descargando certificado más reciente...');
      
      // Obtener las solicitudes del usuario
      this.certificadosService.obtenerHistorial().subscribe({
        next: (solicitudes: any[]) => {
          if (solicitudes && solicitudes.length > 0) {
            // Filtrar solo las solicitudes aprobadas
            const solicitudesAprobadas = solicitudes.filter(s => s.estado === 'aprobado' || s.estado === 'Aprobado');
            
            if (solicitudesAprobadas.length > 0) {
              // Obtener la solicitud aprobada más reciente
              const solicitudReciente = solicitudesAprobadas[0];
              console.log('Solicitud aprobada más reciente encontrada:', solicitudReciente);
              
              // Mostrar información del certificado
              const fechaAprobacion = solicitudReciente.fechaAprobacion ? 
                new Date(solicitudReciente.fechaAprobacion).toLocaleDateString('es-CL') : 'No disponible';
              
              console.log(`Certificado aprobado el: ${fechaAprobacion}`);
              console.log(`Código de validación: ${solicitudReciente.codigoVerificacion || 'No disponible'}`);
              
              // Descargar el certificado
              this.certificadosService.descargarCertificado(solicitudReciente.id).subscribe({
                next: async (response: any) => {
                  console.log('Certificado descargado exitosamente');
                  
                  // Crear un blob con el PDF
                  const blob = new Blob([response], { type: 'application/pdf' });
                  const fileName = `certificado_${solicitudReciente.id}.pdf`;
                  
                  try {
                    // Detectar plataforma y usar el método correcto
                    if (this.isAndroid()) {
                      await this.descargarEnAndroid(blob, fileName);
                    } else {
                      this.descargarEnNavegador(blob, fileName);
                    }
                    
                    // Mostrar mensaje de éxito con información
                    this.mostrarMensajeExito(`Certificado descargado exitosamente\nFecha de aprobación: ${fechaAprobacion}\nCódigo de validación: ${solicitudReciente.codigoVerificacion || 'No disponible'}`);
                  } catch (error) {
                    console.error('Error al procesar la descarga:', error);
                    this.mostrarMensajeError('Error al procesar la descarga: ' + (error instanceof Error ? error.message : 'Error desconocido'));
                  }
                },
                error: (error) => {
                  console.error('Error al descargar certificado:', error);
                  this.mostrarMensajeError('Error al descargar el certificado: ' + (error?.error?.mensaje || 'Error desconocido'));
                }
              });
            } else {
              console.log('No se encontraron certificados aprobados para descargar');
              this.mostrarMensajeError('No tienes certificados aprobados para descargar. Tus solicitudes están pendientes de aprobación.');
            }
          } else {
            console.log('No se encontraron solicitudes de certificados');
            this.mostrarMensajeError('No tienes solicitudes de certificados. Debes solicitar un certificado primero.');
          }
        },
        error: (error) => {
          console.error('Error al obtener solicitudes de certificados:', error);
          this.mostrarMensajeError('Error al obtener tus certificados: ' + (error?.error?.mensaje || 'Error desconocido'));
        }
      });
    } catch (error) {
      console.error('Error al descargar certificado:', error);
      this.mostrarMensajeError('Error al descargar el certificado');
    }
  }

  mostrarMensajeExito(mensaje: string): void {
    // No hacer nada para evitar la alerta.
    console.log('Éxito:', mensaje);
  }

  mostrarMensajeError(mensaje: string): void {
    // Aquí puedes implementar la lógica para mostrar un mensaje de error
    // Por ejemplo, usando un toast o alert
    alert('Error: ' + mensaje);
  }

  // Función para detectar si estamos en Android
  private isAndroid(): boolean {
    return Capacitor.getPlatform() === 'android';
  }

  // Función para convertir Blob a base64
  private blobToBase64(blob: Blob): Promise<string> {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.onloadend = () => {
        const result = reader.result as string;
        // Extraer solo la parte base64 (sin el prefijo data:application/pdf;base64,)
        const base64 = result.split(',')[1];
        resolve(base64);
      };
      reader.onerror = reject;
      reader.readAsDataURL(blob);
    });
  }

  // Función para descargar en Android
  private async descargarEnAndroid(blob: Blob, fileName: string): Promise<void> {
    try {
      // Convertir blob a base64
      const base64Data = await this.blobToBase64(blob);
      
      // Guardar el archivo en el directorio de documentos
      const savedFile = await Filesystem.writeFile({
        path: fileName,
        data: base64Data,
        directory: Directory.Documents,
        recursive: true
      });

      // Abrir el archivo con el visor nativo
      await FileOpener.open({
        filePath: savedFile.uri,
        contentType: 'application/pdf'
      });

      console.log('PDF guardado y abierto en Android:', savedFile.uri);
    } catch (error) {
      console.error('Error al descargar en Android:', error);
      throw new Error('No se pudo descargar el archivo en Android');
    }
  }

  // Función para descargar en navegador
  private descargarEnNavegador(blob: Blob, fileName: string): void {
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    window.URL.revokeObjectURL(url);
  }
}