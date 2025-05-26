import { CapacitorConfig } from '@capacitor/cli';

const config: CapacitorConfig = {
  appId: 'com.vecindapp.app',
  appName: 'Vecindapp',
  webDir: 'www',
  server: {
    androidScheme: 'https'
  },
  plugins: {
    BarcodeScanner: {
      // Configuración específica del plugin de escaneo
      formats: ['QR_CODE'],
      showTorchButton: true,
      showFlipCameraButton: true
    }
  }
};

export default config;
