import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonicModule } from '@ionic/angular';
import { CertificadosService } from '../certificados.service';

@Component({
  selector: 'app-historial-certificados',
  templateUrl: './historial-certificados.component.html',
  styleUrls: ['./historial-certificados.component.css'],
  standalone: true,
  imports: [CommonModule, IonicModule]
})
export class HistorialCertificadosComponent implements OnInit {
  historial: any[] = [];

  constructor(private certificadosService: CertificadosService) {}

  ngOnInit(): void {
    this.certificadosService.obtenerHistorial().subscribe(data => {
      this.historial = data;
      console.log('Historial cargado:', data);
    });
  }

  descargar(certificadoId: number) {
    if (!certificadoId) {
      alert('No hay certificado disponible para descargar. El certificado debe estar aprobado.');
      return;
    }

    this.certificadosService.descargarCertificado(certificadoId).subscribe(
      (response) => {
        // Decodificar el base64
        const byteCharacters = atob(response.file);
        const byteNumbers = new Array(byteCharacters.length);
        for (let i = 0; i < byteCharacters.length; i++) {
          byteNumbers[i] = byteCharacters.charCodeAt(i);
        }
        const byteArray = new Uint8Array(byteNumbers);
        const blob = new Blob([byteArray], { type: 'application/pdf' });

        // Crear un enlace para la descarga
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = response.fileName;
        document.body.appendChild(a);
        a.click();
        
        // Limpiar
        window.URL.revokeObjectURL(url);
        document.body.removeChild(a);

        // Mostrar información del certificado descargado
        const mensaje = `Certificado descargado exitosamente\n\n` +
                       `Código de verificación: ${response.codigoVerificacion || 'No disponible'}\n` +
                       `Fecha de aprobación: ${response.fechaAprobacion || 'No disponible'}`;
        alert(mensaje);

      },
      (error) => {
        console.error('Error al descargar:', error);
        alert('Hubo un error al descargar el certificado.');
      }
    );
  }
} 