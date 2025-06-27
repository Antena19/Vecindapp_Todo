import { Html5QrcodeSupportedFormats } from 'html5-qrcode';
/**
 * Enum representing the direction of the camera to be used for barcode scanning.
 */
export declare enum CapacitorBarcodeScannerCameraDirection {
    BACK = 1,
    FRONT = 2
}
/**
 * Enum representing the orientation of the scanner during barcode scanning.
 */
export declare enum CapacitorBarcodeScannerScanOrientation {
    PORTRAIT = 1,
    LANDSCAPE = 2,
    ADAPTIVE = 3
}
/**
 * Enum representing a special option to indicate that all barcode types are supported.
 */
export declare enum CapacitorBarcodeScannerTypeHintALLOption {
    ALL = 17
}
/**
 * Extends supported formats from Html5Qrcode with a special 'ALL' option,
 * indicating support for all barcode types.
 */
export declare const CapacitorBarcodeScannerTypeHint: {
    [x: number]: string;
    ALL: CapacitorBarcodeScannerTypeHintALLOption.ALL;
    QR_CODE: Html5QrcodeSupportedFormats.QR_CODE;
    AZTEC: Html5QrcodeSupportedFormats.AZTEC;
    CODABAR: Html5QrcodeSupportedFormats.CODABAR;
    CODE_39: Html5QrcodeSupportedFormats.CODE_39;
    CODE_93: Html5QrcodeSupportedFormats.CODE_93;
    CODE_128: Html5QrcodeSupportedFormats.CODE_128;
    DATA_MATRIX: Html5QrcodeSupportedFormats.DATA_MATRIX;
    MAXICODE: Html5QrcodeSupportedFormats.MAXICODE;
    ITF: Html5QrcodeSupportedFormats.ITF;
    EAN_13: Html5QrcodeSupportedFormats.EAN_13;
    EAN_8: Html5QrcodeSupportedFormats.EAN_8;
    PDF_417: Html5QrcodeSupportedFormats.PDF_417;
    RSS_14: Html5QrcodeSupportedFormats.RSS_14;
    RSS_EXPANDED: Html5QrcodeSupportedFormats.RSS_EXPANDED;
    UPC_A: Html5QrcodeSupportedFormats.UPC_A;
    UPC_E: Html5QrcodeSupportedFormats.UPC_E;
    UPC_EAN_EXTENSION: Html5QrcodeSupportedFormats.UPC_EAN_EXTENSION;
};
/**
 * Type definition combining Html5QrcodeSupportedFormats and OSBarcodeTypeHintALLOption
 * to represent the hint for the type of barcode to be scanned.
 */
export type CapacitorBarcodeScannerTypeHint = Html5QrcodeSupportedFormats | CapacitorBarcodeScannerTypeHintALLOption;
/**
 * Enum representing the library to be used for barcode scanning on Android devices.
 */
export declare enum CapacitorBarcodeScannerAndroidScanningLibrary {
    ZXING = "zxing",
    MLKIT = "mlkit"
}
/**
 * Defines the structure of the result returned from a barcode scan.
 */
export type CapacitorBarcodeScannerScanResult = {
    ScanResult: string;
};
/**
 * Defines the options for configuring a barcode scan.
 */
export type CapacitorBarcodeScannerOptions = {
    hint: CapacitorBarcodeScannerTypeHint;
    scanInstructions?: string;
    scanButton?: boolean;
    scanText?: string;
    cameraDirection?: CapacitorBarcodeScannerCameraDirection;
    scanOrientation?: CapacitorBarcodeScannerScanOrientation;
    android?: {
        scanningLibrary?: CapacitorBarcodeScannerAndroidScanningLibrary;
    };
    web?: {
        showCameraSelection?: boolean;
        scannerFPS?: number;
    };
};
/**
 * Interface defining the contract for a plugin capable of scanning barcodes.
 * Requires implementation of the scanBarcode method, which initiates a barcode scan with given options.
 */
export interface CapacitorBarcodeScannerPlugin {
    scanBarcode(options: CapacitorBarcodeScannerOptions): Promise<CapacitorBarcodeScannerScanResult>;
}
