import { type CapacitorBarcodeScannerOptions, type CapacitorBarcodeScannerScanResult } from './definitions';
declare class CapacitorBarcodeScanner {
    static scanBarcode(options: CapacitorBarcodeScannerOptions): Promise<CapacitorBarcodeScannerScanResult>;
}
export { CapacitorBarcodeScanner };
export * from './definitions';
