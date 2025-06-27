import { Html5QrcodeSupportedFormats } from 'html5-qrcode';
/**
 * Enum representing the direction of the camera to be used for barcode scanning.
 */
export var CapacitorBarcodeScannerCameraDirection;
(function (CapacitorBarcodeScannerCameraDirection) {
    CapacitorBarcodeScannerCameraDirection[CapacitorBarcodeScannerCameraDirection["BACK"] = 1] = "BACK";
    CapacitorBarcodeScannerCameraDirection[CapacitorBarcodeScannerCameraDirection["FRONT"] = 2] = "FRONT";
})(CapacitorBarcodeScannerCameraDirection || (CapacitorBarcodeScannerCameraDirection = {}));
/**
 * Enum representing the orientation of the scanner during barcode scanning.
 */
export var CapacitorBarcodeScannerScanOrientation;
(function (CapacitorBarcodeScannerScanOrientation) {
    CapacitorBarcodeScannerScanOrientation[CapacitorBarcodeScannerScanOrientation["PORTRAIT"] = 1] = "PORTRAIT";
    CapacitorBarcodeScannerScanOrientation[CapacitorBarcodeScannerScanOrientation["LANDSCAPE"] = 2] = "LANDSCAPE";
    CapacitorBarcodeScannerScanOrientation[CapacitorBarcodeScannerScanOrientation["ADAPTIVE"] = 3] = "ADAPTIVE";
})(CapacitorBarcodeScannerScanOrientation || (CapacitorBarcodeScannerScanOrientation = {}));
/**
 * Enum representing a special option to indicate that all barcode types are supported.
 */
export var CapacitorBarcodeScannerTypeHintALLOption;
(function (CapacitorBarcodeScannerTypeHintALLOption) {
    CapacitorBarcodeScannerTypeHintALLOption[CapacitorBarcodeScannerTypeHintALLOption["ALL"] = 17] = "ALL";
})(CapacitorBarcodeScannerTypeHintALLOption || (CapacitorBarcodeScannerTypeHintALLOption = {}));
/**
 * Extends supported formats from Html5Qrcode with a special 'ALL' option,
 * indicating support for all barcode types.
 */
export const CapacitorBarcodeScannerTypeHint = Object.assign(Object.assign({}, Html5QrcodeSupportedFormats), CapacitorBarcodeScannerTypeHintALLOption);
/**
 * Enum representing the library to be used for barcode scanning on Android devices.
 */
export var CapacitorBarcodeScannerAndroidScanningLibrary;
(function (CapacitorBarcodeScannerAndroidScanningLibrary) {
    CapacitorBarcodeScannerAndroidScanningLibrary["ZXING"] = "zxing";
    CapacitorBarcodeScannerAndroidScanningLibrary["MLKIT"] = "mlkit";
})(CapacitorBarcodeScannerAndroidScanningLibrary || (CapacitorBarcodeScannerAndroidScanningLibrary = {}));
//# sourceMappingURL=definitions.js.map