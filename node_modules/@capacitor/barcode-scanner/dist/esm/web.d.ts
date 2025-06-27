import { WebPlugin } from '@capacitor/core';
import type { CapacitorBarcodeScannerPlugin, CapacitorBarcodeScannerOptions, CapacitorBarcodeScannerScanResult } from './definitions';
/**
 * Implements OSBarcodePlugin to provide web functionality for barcode scanning.
 */
export declare class CapacitorBarcodeScannerWeb extends WebPlugin implements CapacitorBarcodeScannerPlugin {
    /**
     * Stops the barcode scanner and hides its UI.
     * @private
     * @returns {Promise<void>} A promise that resolves when the scanner has stopped and its UI is hidden.
     */
    private stopAndHideScanner;
    /**
     * Builds the HTML elements necessary for the barcode scanner UI.
     * This method checks if the scanner container exists before creating it to avoid duplicates.
     * It also sets up the close button to stop and hide the scanner on click.
     * @private
     */
    private buildScannerElement;
    /**
     * Initiates a barcode scan using the user's camera and HTML5 QR code scanner.
     * Displays the scanner UI and waits for a scan to complete or fail.
     * @param {OSBarcodeScanOptions} options Configuration options for the scan, including camera direction and UI preferences.
     * @returns {Promise<OSBarcodeScanResult>} A promise that resolves with the scan result or rejects with an error.
     */
    scanBarcode(options: CapacitorBarcodeScannerOptions): Promise<CapacitorBarcodeScannerScanResult>;
}
