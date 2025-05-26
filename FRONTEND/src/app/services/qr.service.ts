import { Injectable } from '@angular/core';
import * as QRCode from 'qrcode';
import { Filesystem, Directory } from '@capacitor/filesystem';
import { Share } from '@capacitor/share';

@Injectable({
  providedIn: 'root'
})
export class QRService {
  constructor() { }

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

  async compartirQR(uri: string, titulo: string): Promise<void> {
    try {
      await Share.share({
        title: titulo,
        url: uri
      });
    } catch (error) {
      console.error('Error al compartir código QR:', error);
      throw error;
    }
  }
} 