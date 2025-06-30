/**
 * Predefined CSS rules for styling barcode scanner components.
 * Each object in the array defines a CSS rule, with a selector and the CSS properties to apply.
 */
export declare const barcodeScannerCss: {
    selector: string;
    css: string;
}[];
/**
 * Dynamically applies a set of CSS rules to the document.
 * If a custom style element with a specific ID does not exist, it is created and appended to the document's head.
 * Existing rules in the style element are cleared before new ones are applied.
 * This function supports both modern and older browsers by using `CSSStyleSheet.insertRule` and `textContent` respectively.
 *
 * @param {Array<{selector: string, css: string}>} cssRules - An array of objects containing CSS selectors and styles to apply.
 */
export declare function applyCss(cssRules: {
    selector: string;
    css: string;
}[]): void;
