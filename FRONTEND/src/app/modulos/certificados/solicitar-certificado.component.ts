import { FormGroup } from '@angular/forms';
import html2pdf from 'html2pdf.js';
import { OnInit, Component } from '@angular/core';

@Component({
  selector: 'app-solicitar-certificado',
  templateUrl: './solicitar-certificado.component.html',
  styleUrls: ['./solicitar-certificado.component.css']
})
export class SolicitarCertificadoComponent implements OnInit {
  solicitudForm!: FormGroup;
  firmaGuardada: boolean = false;
  signatureData: string | null = null;
  usuario: any = null;
  documentos: File[] = [];
  paraOtro: boolean = false;
  hashVerificacion: string = '';
  timestampFirma: string = '';
  certificadosService: any;

  exportarPDF() {
    const formValue = this.solicitudForm.value;
    const htmlFormal = `
      <div style='font-family: Arial, sans-serif; padding: 24px; max-width: 700px;'>
        <div style='text-align:center; margin-bottom: 16px;'>
          <img src='assets/images/logo-junta.png' alt='Logo Junta de Vecinos' style='width:120px; margin-bottom:10px;'>
          <h1 style='font-size:2.2rem; font-weight:bold; margin:0;'>CERTIFICADO DE RESIDENCIA.</h1>
          <div style='font-size:1.1rem; margin: 8px 0 18px 0;'>PUERTO MONTT, ____ DE ______________ 2024.</div>
        </div>
        <div style='font-size:1.05rem; text-align:justify; margin-bottom:18px;'>
          LA JUNTA DE VECINOS PORTALDE PUERTO MONTT, RUT: 65.066.453 – 1, PERSONALIDAD JURICA N°3849 CON FOJA 3850, CONSTITUIDA EL 13 DE MARZO 2013 EN PUERTO MONTT, perteneciente a la unidad N° 20 con facultad que otorga la Ley N°19.418 en el artículo 43 certifica que:<br><br>
          Sr.(a): <b>${formValue.nombre || ''}</b><br>
          RUT: <b>${formValue.rut || ''}</b> &nbsp;&nbsp;&nbsp; Teléfono: <b>${formValue.telefono || ''}</b><br>
          Reside en: <b>${formValue.direccion || ''}</b><br><br>
          Se extiende el presente documento para ser presentado en la institución que lo requiera.<br><br>
          <b>Ley 20.718 de 02 de enero 2014</b> faculta a las juntas de vecinos a emitir certificados de residencia, siéndole aplicable al requirente que faltare a la verdad cuanto a los datos proporcionados al efecto, las sanciones contempladas en el artículo 212 del código penal.
        </div>
        <div style='margin-top: 48px; text-align:center;'>
          <div style='margin-bottom:8px;'>
            <img src='assets/images/firma-digital.png' alt='Firma Digital' style='height:60px;'>
          </div>
          <div style='font-weight:bold;'>FIRMA DIGITAL JUNTA DE VECINOS</div>
          <div style='font-size:0.95rem;'>Presidente</div>
        </div>
      </div>
    `;
    const container = document.getElementById('certificadoFormalContainer') as HTMLElement;
    if (container) {
      container.innerHTML = htmlFormal;
      container.style.display = 'block';
    }
    setTimeout(() => {
      if (container) {
        const opt = {
          margin: 0.2,
          filename: 'certificado_residencia.pdf',
          image: { type: 'jpeg', quality: 0.98 },
          html2canvas: { scale: 2 },
          jsPDF: { unit: 'in', format: 'letter', orientation: 'portrait' }
        };
        html2pdf.js().set(opt).from(container).save().then(() => {
          container.style.display = 'none';
          container.innerHTML = '';
        });
      }
    }, 100);
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
      Id: 0,
      TipoCertificadoId: 3,
      FechaSolicitud: new Date().toISOString(),
      Estado: 'Pendiente',
      Motivo: 'Certificado de Residencia',
      DocumentosAdjuntos: documentoAdjunto,
      FechaAprobacion: null,
      DirectivaRut: null,
      Precio: precio,
      Observaciones: this.paraOtro ? `Solicitud para: ${formValue.nombre} - Parentesco: ${formValue.parentesco}` : 'Solicitud personal',
      NombreSolicitante: formValue.nombre,
      RutSolicitante: formValue.rut
    };

    console.log('Enviando solicitud:', solicitud);

    this.certificadosService.solicitarCertificado(usuario.rut, solicitud).subscribe({
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
            
            // Verificar si es un error de conectividad con Transbank
            if (err?.status === 503 && err?.error?.conectividad === false) {
              const mensaje = `⚠️ ${err.error.mensaje}\n\n` +
                            `📋 Su solicitud ha sido registrada con ID: ${err.error.solicitudId}\n\n` +
                            `📞 Contacte a la directiva para aprobación manual.\n\n` +
                            `💡 Recomendación: ${err.error.recomendacion}`;
              
              alert(mensaje);
              
              // Opcional: Redirigir a una página de estado de solicitud
              // window.location.href = `/certificados/estado/${err.error.solicitudId}`;
            } else {
              alert('Error al iniciar el pago: ' + (err?.error?.mensaje || 'Error desconocido'));
            }
          }
        });
      },
      error: (err: any) => {
        console.error('Error en solicitud:', err);
        alert('Error al enviar la solicitud: ' + (err?.error?.mensaje || 'Error desconocido'));
      }
    });
  }

  ngOnInit(): void {}
} 