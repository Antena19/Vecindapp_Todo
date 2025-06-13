import { Injectable } from '@angular/core';
import * as QRCode from 'qrcode';
import { Filesystem, Directory } from '@capacitor/filesystem';
import { Share } from '@capacitor/share';
import { Platform } from '@ionic/angular';

@Injectable({
  providedIn: 'root'
})
export class QRService {
  constructor(private platform: Platform) { }

  async generarQR(data: string): Promise<string> {
    try {
      return await QRCode.toDataURL(data, {
        errorCorrectionLevel: 'H',
        margin: 1,
        width: 300,
        color: {
          dark: '#000000',
          light: '#ffffff'
        }
      });
    } catch (error) {
      console.error('Error al generar código QR:', error);
      throw error;
    }
  }

  async guardarQR(dataUrl: string, nombreArchivo: string): Promise<string> {
    try {
      // Convertir data URL a blob
      const response = await fetch(dataUrl);
      const blob = await response.blob();
      
      // Guardar archivo
      const resultado = await Filesystem.writeFile({
        path: `${nombreArchivo}.png`,
        data: dataUrl.split(',')[1],
        directory: Directory.Cache,
        recursive: true
      });

      return resultado.uri;
    } catch (error) {
      console.error('Error al guardar código QR:', error);
      throw error;
    }
  }

  async compartirQR(dataUrl: string, titulo: string): Promise<void> {
    try {
      if (this.platform.is('hybrid')) {
        // Para dispositivos móviles (Android/iOS)
        const resultado = await Filesystem.writeFile({
          path: 'codigo-qr.png',
          data: dataUrl.split(',')[1],
          directory: Directory.Cache,
          recursive: true
        });

        await Share.share({
          title: titulo,
          text: 'Código QR del evento',
          files: [resultado.uri],
          dialogTitle: 'Compartir código QR'
        });
      } else {
        // Para web
        // Crear un elemento <a> temporal
        const link = document.createElement('a');
        link.href = dataUrl;
        link.download = 'codigo-qr.png';
        link.target = '_blank';
        
        // Simular clic para descargar
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);

        // Mostrar mensaje al usuario
        alert('El código QR se ha descargado. Puedes compartirlo manualmente desde tu dispositivo.');
      }
    } catch (error) {
      console.error('Error al compartir código QR:', error);
      throw error;
    }
  }
} 