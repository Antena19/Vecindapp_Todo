import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonicModule } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { EventosService } from '../../../services/eventos.service';
import { BarcodeScanner } from '@capacitor-community/barcode-scanner';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-registro-asistencia',
  templateUrl: './registro-asistencia.component.html',
  styleUrls: ['./registro-asistencia.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    IonicModule,
    RouterModule,
    FormsModule
  ]
})
export class RegistroAsistenciaComponent implements OnInit, OnDestroy {
  escaneando = false;
  mensaje = '';
  codigoManual = '';
  mostrarInputManual = false;

  constructor(private eventosService: EventosService) { }

  ngOnInit() {
    this.solicitarPermisos();
  }

  ngOnDestroy() {
    this.detenerEscaneo();
  }

  async solicitarPermisos() {
    try {
      const status = await BarcodeScanner.checkPermissions();
      if (!status.camera) {
        const requestStatus = await BarcodeScanner.requestPermissions();
        if (!requestStatus.camera) {
          this.mensaje = 'Se requieren permisos de cámara para escanear códigos QR';
        }
      }
    } catch (error) {
      console.error('Error al solicitar permisos:', error);
      this.mensaje = 'Error al solicitar permisos de cámara';
    }
  }

  async iniciarEscaneo() {
    try {
      this.escaneando = true;
      // @ts-ignore: El método existe en tiempo de ejecución aunque el tipado no lo reconozca
      BarcodeScanner.hideBackground();
      document.body.style.background = 'transparent';
      document.body.style.opacity = '0';

      // @ts-ignore: El método existe en tiempo de ejecución aunque el tipado no lo reconozca
      const resultado = await BarcodeScanner.startScan();

      if (resultado && resultado.hasContent) {
        await this.registrarAsistencia(resultado.content, true);
      }
    } catch (error) {
      console.error('Error al escanear:', error);
      this.mensaje = 'Error al escanear el código QR';
    } finally {
      this.detenerEscaneo();
    }
  }

  async detenerEscaneo() {
    try {
      // @ts-ignore: El método existe en tiempo de ejecución aunque el tipado no lo reconozca
      BarcodeScanner.showBackground();
      // @ts-ignore: El método existe en tiempo de ejecución aunque el tipado no lo reconozca
      BarcodeScanner.stopScan();
      document.body.style.background = '';
      document.body.style.opacity = '1';
      this.escaneando = false;
    } catch (error) {
      console.error('Error al detener el escáner:', error);
    }
  }

  toggleInputManual() {
    this.mostrarInputManual = !this.mostrarInputManual;
    if (this.mostrarInputManual) {
      this.detenerEscaneo();
    }
  }

  registrarCodigoManual() {
    const codigo = String(this.codigoManual).trim();
    if (codigo && /^\d+$/.test(codigo)) {
      console.log('Código a enviar:', codigo);
      this.registrarAsistencia(codigo, false);
    } else {
      this.mensaje = 'Por favor, ingrese un código numérico válido';
    }
  }

  private async registrarAsistencia(codigo: string, esCodigoQr: boolean) {
    if (!codigo || codigo.trim() === '') {
      this.mensaje = 'Por favor, ingrese un código válido';
      return;
    }

    try {
      // Asegurarnos de que el código sea string
      const codigoFormateado = String(codigo).trim();
      console.log('Código formateado para envío:', codigoFormateado);

      this.eventosService.registrarAsistencia(codigoFormateado, esCodigoQr).subscribe({
        next: (response) => {
          console.log('Respuesta del servidor:', response);
          this.mensaje = 'Asistencia registrada correctamente';
          this.codigoManual = ''; // Limpiar el input después de registrar
          this.mostrarInputManual = false; // Ocultar el input manual después de registrar
        },
        error: (error) => {
          console.error('Error al registrar asistencia:', error);
          if (error.status === 400) {
            const mensajeError = error.error?.mensaje || 'Error al registrar la asistencia';
            console.log('Detalles del error:', {
              mensaje: mensajeError,
              codigo: codigoFormateado,
              error: error.error,
              payload: error.error
            });
            
            if (mensajeError.includes('inactivo')) {
              this.mensaje = 'El evento está inactivo o ha sido cancelado';
            } else if (mensajeError.includes('inválido')) {
              this.mensaje = 'El código ingresado no corresponde a ningún evento válido';
            } else if (mensajeError.includes('Ya existe')) {
              this.mensaje = 'Ya se ha registrado la asistencia para este evento';
            } else {
              this.mensaje = mensajeError;
            }
          } else if (error.status === 404) {
            this.mensaje = 'El código ingresado no corresponde a ningún evento válido';
          } else if (error.status === 409) {
            this.mensaje = 'Ya se ha registrado la asistencia para este evento';
          } else {
            this.mensaje = 'Error al registrar la asistencia. Por favor, intente nuevamente';
          }
        }
      });
    } catch (error) {
      console.error('Error al procesar el registro:', error);
      this.mensaje = 'Error al procesar el registro. Por favor, intente nuevamente';
    }
  }
} 