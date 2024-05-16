(function ($) {
    'use strict';
    window.Rock = window.Rock || {};
    Rock.controls = Rock.controls || {};

    Rock.controls.colorSelector = (function () {

        // #region Utilities

        var ColorUtilities = (function () {
            /**
             * Detects if two RGB colors are similar.
             * @param r1 {number} Red 1 color (0-255).
             * @param g1 {number} Green 1 color (0-255).
             * @param b1 {number} Blue 1 color (0-255).
             * @param r2 {number} Red 2 color (0-255).
             * @param g2 {number} Green 2 color (0-255).
             * @param b2 {number} Blue 2 color (0-255).
             * @param percentSimilar {number|undefined} The percent (0 - 1) that determines if the colors are similar. 
             * @returns {Boolean} `true` if the numbers are similar; otherwise, `false`.
             */
            function areColorsSimilar(r1, g1, b1, r2, g2, b2, percentSimilar) {
                if (typeof percentSimilar === "undefined") {
                    percentSimilar = 0.99;
                }

                var actualDistance = getColorDistance(r1, g1, b1, r2, g2, b2);

                var actualPercent = (maxLumaDistance - actualDistance) / maxLumaDistance;

                // Use the specified percent to determine if the colors are similar.
                return actualPercent >= percentSimilar;
            }

            /**
             * Gets the distance between two RGB colors.
             * @param r1 {number} Red 1 color (0-255).
             * @param g1 {number} Green 1 color (0-255).
             * @param b1 {number} Blue 1 color (0-255).
             * @param r2 {number} Red 2 color (0-255).
             * @param g2 {number} Green 2 color (0-255).
             * @param b2 {number} Blue 2 color (0-255).
             * @returns {number} The distance between two RGB colors.
             */
            function getColorDistance(r1, g1, b1, r2, g2, b2) {
                var redDiffSquared = Math.pow(r2 - r1, 2);
                var greenDiffSquared = Math.pow(g2 - g1, 2);
                var blueDiffSquared = Math.pow(b2 - b1, 2);
                var redAverage = (r2 + r1) / 2;

                return Math.sqrt(
                    (2 * redDiffSquared) +
                    (4 * greenDiffSquared) +
                    (3 * blueDiffSquared) +
                    (redAverage * (redDiffSquared - blueDiffSquared) / 256)
                );
            }

            /**
             * Converts a hex color (#FFFFFF) to an RGB color. 
             * @param {string} hexValue The hex color to convert.
             * @returns {Number[]} The converted RGB color.
             */
            function hexToRgba(hexValue) {
                var hex3Or4 = /^#[0-9a-fA-F]{3}([0-9a-fA-F])?$/;
                if (hex3Or4.test(hexValue)) {
                    var rHex = hexValue[1];
                    var gHex = hexValue[2];
                    var bHex = hexValue[3];
                    var aHex = hexValue[4] || "f";
                    // A 3-digit hex color #123 is equivalent to the 6-digit hex color #112233,
                    // where each color channel (r, g, and b) is repeated.
                    return [
                        parseInt(rHex + rHex, 16),
                        parseInt(gHex + gHex, 16),
                        parseInt(bHex + bHex, 16),
                        parseInt(aHex + aHex, 16)
                    ];
                }

                var hex6Or8 = /^#[0-9a-fA-F]{6}([0-9a-fA-F]{2})?$/;
                if (hex6Or8.test(hexValue)) {
                    var rHex = hexValue.substring(1, 3);
                    var gHex = hexValue.substring(3, 5);
                    var bHex = hexValue.substring(5, 7);
                    var aHex = hexValue.substring(7) || "ff";
                    return [
                        parseInt(rHex, 16),
                        parseInt(gHex, 16),
                        parseInt(bHex, 16),
                        Math.round(parseInt(aHex, 16) / 255)
                    ];
                }

                // Return transparent if not a valid hex color.
                return [0, 0, 0, 0];
            }

            /**
             * Returns the perceived brightness of an RGB color.
             * @param r {number} Red color (0-255).
             * @param g {number} Green color (0-255).
             * @param b {number} Blue color (0-255).
             * @returns {Number[]}
             */
            function rgbToLuma(r, g, b) {
                /**
                 * To get a color's perceived brightness,
                 * each color component is multiplied by a different constant.
                 * The constants are weighted differently based on the perceived brightness by the human eye
                 * --green is perceived as the brightest color, then red, then blue (they all add up to 1).
                 *
                 * The result will be a color in the color space:
                 * [0, 0, 0] to [0.2126 * 255, 0.7152 * 255, 0.0722 * 255]
                 * or...
                 * [0, 0, 0] to [54.213, 182.376, 18.411]
                 */

                return [0.2126 * r, 0.7152 * g, 0.0722 * b];
            }

            /**
             * Returns the RGBA number array from a CSS "rgba(r, g, b, a)" string.
             * @param rgbFunc {string} The "rgb(r, g, b)" string.
             * @returns {Number[]} An RGBA array [r, b, b, a].
             */
            function rgbFuncToRgba(rgbFunc) {
                var rgb = /^rgb[a]?\s*\(\s*(\d+)[, ]\s*(\d+)[, ]\s*(\d+)\)$/;
                var rgbMatches = rgbFunc.match(rgb);
                if (rgbMatches && rgbMatches.length) {
                    return [
                        parseInt(rgbMatches[1]),
                        parseInt(rgbMatches[2]),
                        parseInt(rgbMatches[3]),
                        1
                    ];
                }

                var rgba = /^rgb[a]?\s*\(\s*(\d+)[, ]\s*(\d+)[, ]\s*(\d+)[, ]\s*(\d*(\.\d+)?)\)$/;
                var rgbaMatches = rgbFunc.match(rgba);
                if (rgbaMatches && rgbMatches.length) {
                    return [
                        parseInt(rgbaMatches[1]),
                        parseInt(rgbaMatches[2]),
                        parseInt(rgbaMatches[3]),
                        Number(rgbaMatches[4])
                    ];
                }

                // Return transparent by default.
                return [0, 0, 0, 0];
            }

            return {
                areColorsSimilar,
                getColorDistance,
                hexToRgba,
                rgbToLuma,
                rgbFuncToRgba
            }
        })();

        // #endregion

        // #region Types

        /**
         * The ColorSelector initialization options.
         * @typedef {Object} ColorSelectorInitOptions
         * @property {string} controlId The control ID for the color selector (without leading #).
         * @property {boolean} allowMultiple When true, the Color Selector will behave like a checkbox list; otherwise, it will behave like a radiobutton list.
         */

        // #endregion 

        // #region Constants

        var colorSimilarityPercentage = Object.freeze(0.95);
        var maxLuma = Object.freeze(ColorUtilities.rgbToLuma(255, 255, 255));
        var maxLumaDistance = Object.freeze(ColorUtilities.getColorDistance(maxLuma[0], maxLuma[1], maxLuma[2], 0, 0, 0));
        var camouflagedLightClass = Object.freeze("camouflaged-light");
        var camouflagedDarkClass = Object.freeze("camouflaged-dark");

        // #endregion

        // #region Classes

        // #region Constructor

        /**
         * Creates a new instance of the ColorSelector class.
         * @param {ColorSelectorInitOptions} options
         */
        function ColorSelector(options) {
            if (!options.controlId) {
                throw "`controlId` is required.";
            }

            this.allowMultiple = !!options.allowMultiple;
            this.controlId = options.controlId;

            // Set JQuery selectors.
            this.$colorSelector = $("#" + this.controlId);
            this.$colorSelectorItemContainers = this.$colorSelector.children(".checkbox");

            // Set defaults for background color and perceived brightness.
            // These will be overwritten during initialization.
            this.backgroundColor = [255, 255, 255];
            this.backgroundLuma = maxLuma;
        }

        // #endregion

        // #region Methods

        /**
         * Initializes the color selector.
         */
        ColorSelector.prototype.initialize = function () {
            var colorSelectorElement = this.$colorSelector[0];

            if (!colorSelectorElement) {
                throw "Unable to find color selector element with control ID " + this.controlId;
            }

            // Set the background color of the color selector.
            this.backgroundColor = getInheritedBackgroundColor(colorSelectorElement);

            // Set the perceived brightness of the color picker.
            // The perceived brightness is better for determining
            // when two colors are similar to the human eye
            // instead of comparing RGB values.
            this.backgroundLuma = ColorUtilities.rgbToLuma(this.backgroundColor[0], this.backgroundColor[1], this.backgroundColor[2]);

            var $colorSelectorItemContainers = this.$colorSelectorItemContainers;

            // Initialize the color items.
            var checkedFound = false;
            for (var i = 0; i < $colorSelectorItemContainers.length; i++) {
                var $colorSelectorItemContainer = $($colorSelectorItemContainers[i]);

                // If only one selection is allowed, then only leave the first checked input as checked,
                // and uncheck the others.
                if (!this.allowMultiple) {
                    var $input = findInput($colorSelectorItemContainer);

                    if ($input.prop("checked")) {
                        if (checkedFound) {
                            // A checked input was already found so mark this input as unchecked.
                            $input.prop("checked", false);
                        }

                        checkedFound = true;
                    }
                }

                initializeItem(this, $colorSelectorItemContainer);
            }
        };

        // #endregion

        // #region Private Functions

        /**
         * Finds the checkbox input for a color selector item container.
         * @param {JQuery<HTMLElement>} $colorSelectorItemContainer
         * @returns {JQuery<HTMLInputElement>}
         */
        function findInput($colorSelectorItemContainer) {
            return $colorSelectorItemContainer.find('input[type="checkbox"]');
        }

        /**
         * Initializes the color items.
         * @param {ColorSelector} colorSelector
         * @param {JQuery<HTMLElement>} $colorSelectorItemContainer
         */
        function initializeItem(colorSelector, $colorSelectorItemContainer) {
            $colorSelectorItemContainer.addClass("color-selector-item-container");

            var $checkboxLabel = $colorSelectorItemContainer.find("label");
            $checkboxLabel.addClass("color-selector-item");

            var $checkboxInput = findInput($colorSelectorItemContainer);
            $checkboxInput.addClass("color-selector-item-checkbox");

            styleInput($checkboxInput);

            // Update the checked state whenever it changes.
            $checkboxInput.on("click", function () {
                onCheckboxCheckChanged(colorSelector, $(this));
            });

            // Remove the .checkbox CSS class as we don't want any checkbox styling.
            $colorSelectorItemContainer.removeClass("checkbox");

            // Set the label background color to the color found in the label text.
            var $labelText = $checkboxLabel.find(".label-text");
            var itemHexColor = $labelText.text();
            $checkboxLabel.css("background-color", itemHexColor);

            // Remove the label text so just the color is displayed.
            $labelText.remove();

            // Determine if the item needs a border to separate it from the background.
            var itemRgb = ColorUtilities.hexToRgba(itemHexColor);
            var itemLuma = ColorUtilities.rgbToLuma(itemRgb[0], itemRgb[1], itemRgb[2]);

            if (ColorUtilities.areColorsSimilar(colorSelector.backgroundLuma[0], colorSelector.backgroundLuma[1], colorSelector.backgroundLuma[2], itemLuma[0], itemLuma[1], itemLuma[2], colorSimilarityPercentage)) {
                $checkboxLabel.addClass(getCamouflagedClass(itemLuma[0], itemLuma[1], itemLuma[2]));
            }
        }

        /**
         * Handles the color item checkbox changed event.
         * @param {ColorSelector} colorSelector The color selector.
         * @param {JQuery<HTMLInputElement>} $checkboxInput The checkbox input.
         */
        function onCheckboxCheckChanged(colorSelector, $checkboxInput) {
            styleInput($checkboxInput);

            if (!colorSelector.allowMultiple) {
                // For single select (radio) bevahior, clicking a checkbox should always check it.
                // If the checkbox is already checked, then return false to prevent the default behavior of unchecking it.

                // Uncheck all other checkboxes.
                var $colorSelectorItemContainers = colorSelector.$colorSelectorItemContainers;

                for (var i = 0; i < $colorSelectorItemContainers.length; i++) {
                    var $otherItemInput = findInput($($colorSelectorItemContainers[i]));

                    if ($otherItemInput[0] !== $checkboxInput[0]) {
                        $otherItemInput.prop("checked", false);
                        styleInput($otherItemInput);
                    }
                }
            }
        }

        /**
         * 
         * @param {JQuery<HTMLInputElement>} $input
         * @param {JQuery<HTMLElement | undefined>} $label
         * @param {JQuery<HTMLElement | undefined} $container
         */
        function styleInput($input, $label, $container) {
            if (!$label) {
                $label = $input.parent("label");
            }

            if (!$container) {
                $container = $input.closest(".color-selector-item-container");
            }

            if ($input.prop("checked")) {
                $label.addClass("checked");
                $container.addClass("checked");
            }
            else {
                $label.removeClass("checked");
                $container.removeClass("checked");
            }
        }

        /**
         * Gets the camouflaged CSS class for a luma color (rL, gL, bL).
         * @param {number} rLuma The R luma color.
         * @param {number} gLuma The G luma color.
         * @param {number} bLuma The B luma color.
         * @returns {string} The camouflaged CSS class for a luma color.
         */
        function getCamouflagedClass(rLuma, gLuma, bLuma) {
            var luma = rLuma + gLuma + bLuma;
            if (luma > 128) { // Max is 255
                return camouflagedLightClass;
            }
            else {
                return camouflagedDarkClass;
            }
        }

        /**
         * Gets the first RGBA background color that isn't the default,
         * starting from the element and working its way up the
         * parent element chain.
         * @param {Element} el
         * @returns {Number[]} The first RGBA background color that isn't the default; otherwise, the default color is returned.
         */
        function getInheritedBackgroundColor(el) {

            function getDefaultBackground() {
                var div = document.createElement("div");
                document.head.appendChild(div);
                var bg = window.getComputedStyle(div).backgroundColor;
                document.head.removeChild(div);
                return bg;
            }

            // Get default style for current browser.
            var defaultStyle = getDefaultBackground(); // typically "rgba(0, 0, 0, 0)"

            // Start with the supplied element.
            var elementsToProcess = [
                el
            ];

            while (elementsToProcess.length) {
                var elementToProcess = elementsToProcess.shift();

                if (!elementToProcess) {
                    // Skip to the next element if the current one is nullish.
                    continue;
                }

                var backgroundColor = window.getComputedStyle(elementToProcess).backgroundColor;

                // If we got a real value, return it.
                if (backgroundColor != defaultStyle) {
                    return ColorUtilities.rgbFuncToRgba(backgroundColor);
                }
                // Otherwise, process the next parent element.
                else if (elementToProcess.parentElement) {
                    elementsToProcess.push(elementToProcess.parentElement);
                }
            }

            // If we have reached the top parent element without getting an explicit color,
            // return the default style.
            return ColorUtilities.rgbFuncToRgba(defaultStyle);
        }

        // #endregion

        // #endregion

        // Keeps track of ColorSelector instances. 
        var colorSelectors = {};

        var exports = {
            /**
             * Initializes a new ColorSelector instance.
             * @param {ColorSelectorInitOptions} options
             * @returns {ColorSelector}
             */
            initialize: function (options) {
                var cs = new ColorSelector(options);
                cs.initialize();

                colorSelectors[options.controlId] = cs;

                return cs;
            },

            /**
             * Gets a ColorSelector instance by control ID.
             * @param {string} controlId The control ID.
             * @returns {ColorSelector}
             */
            get: function(controlId) {
                return colorSelectors[controlId];
            },

            /**
             * Sets the camouflaged class if the element color is similar to the target element (or parent if not set).
             * @param {string} elementSelector The selector of the element to compare.
             * @param {string} targetSelector (Optional) The selector of the target element with which to compare colors. Defaults to the parent element.
             */
            setCamouflagedClass: function (elementSelector, targetSelector) {
                var element = document.querySelector(elementSelector);
                var target = targetSelector ? document.querySelector(targetSelector) : element.parentElement;

                var elementColor = getInheritedBackgroundColor(element);
                var targetColor = getInheritedBackgroundColor(target);

                var elementLuma = ColorUtilities.rgbToLuma(elementColor[0], elementColor[1], elementColor[2]);
                var targetLuma = ColorUtilities.rgbToLuma(targetColor[0], targetColor[1], targetColor[2]);

                var isSamePerceivedBrightness = (
                    elementLuma[0] === targetLuma[0]
                    && elementLuma[1] === targetLuma[1]
                    && elementLuma[2] === targetLuma[2]
                );

                // Short-circuit if elements have the same color.
                if (isSamePerceivedBrightness
                    || ColorUtilities.areColorsSimilar(elementLuma[0], elementLuma[1], elementLuma[2], targetLuma[0], targetLuma[1], targetLuma[2], colorSimilarityPercentage)) {
                    element.classList.add(getCamouflagedClass(elementLuma[0], elementLuma[1], elementLuma[2]));
                }
                else {
                    element.classList.remove(camouflagedLightClass);
                    element.classList.remove(camouflagedDarkClass);
                }
            }
        };

        return exports;
    }());
}(jQuery));
