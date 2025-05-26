import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonicModule } from '@ionic/angular';
import { RouterModule } from '@angular/router';
import { EventosService } from '../../../services/eventos.service';
import { BarcodeScanner } from '@capacitor-community/barcode-scanner';

@Component({
  selector: 'app-registro-asistencia',
  templateUrl: './registro-asistencia.component.html',
  styleUrls: ['./registro-asistencia.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    IonicModule,
    RouterModule
  ]
})
export class RegistroAsistenciaComponent implements OnInit, OnDestroy {
  escaneando = false;
  mensaje = '';

  constructor(private eventosService: EventosService) { }

  ngOnInit() {
    // Solicitar permisos de cámara al iniciar
    this.solicitarPermisos();
  }

  ngOnDestroy() {
    // Detener el escáner al salir del componente
    this.detenerEscaneo();
  }

  async solicitarPermisos() {
    try {
      const status = await BarcodeScanner.checkPermission({ force: true });
      if (!status.granted) {
        this.mensaje = 'Se requieren permisos de cámara para escanear códigos QR';
      }
    } catch (error) {
      console.error('Error al solicitar permisos:', error);
      this.mensaje = 'Error al solicitar permisos de cámara';
    }
  }

  async iniciarEscaneo() {
    try {
      this.escaneando = true;
      document.body.style.background = 'transparent';
      document.body.style.opacity = '0';
      
      const resultado = await BarcodeScanner.startScan();
      
      if (resultado.hasContent) {
        this.registrarAsistencia(resultado.content);
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
      await BarcodeScanner.stopScan();
      document.body.style.background = '';
      document.body.style.opacity = '1';
      this.escaneando = false;
    } catch (error) {
      console.error('Error al detener el escáner:', error);
    }
  }

  private registrarAsistencia(codigoQr: string) {
    this.eventosService.registrarAsistencia(codigoQr).subscribe({
      next: () => {
        this.mensaje = 'Asistencia registrada correctamente';
      },
      error: (error) => {
        console.error('Error al registrar asistencia:', error);
        this.mensaje = 'Error al registrar la asistencia';
      }
    });
  }
} 