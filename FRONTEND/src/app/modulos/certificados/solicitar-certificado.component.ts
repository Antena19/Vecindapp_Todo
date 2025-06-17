import { FormGroup } from '@angular/forms';
import html2pdf from 'html2pdf.js';
import { OnInit } from '@angular/core';

export class SolicitarCertificadoComponent implements OnInit {
  solicitudForm: FormGroup;

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

  ngOnInit(): void {}
} 