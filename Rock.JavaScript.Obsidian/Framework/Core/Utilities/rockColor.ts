// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//

// NOTICE!!!
// This file has a C# version in the Rock.Common project. If changes are made
// to one the same changes must be made to the other.
// NOTICE!!!

import { ColorPair } from "./colorPair";
import { ColorScheme } from "@Obsidian/Enums/Core/colorScheme";
import { ColorRecipe } from "@Obsidian/Enums/Core/colorRecipe";

const html4Colors: Map<string, number> = new Map([
    ["aliceblue", 0xf0f8ff],
    ["antiquewhite", 0xfaebd7],
    ["aqua", 0x00ffff],
    ["aquamarine", 0x7fffd4],
    ["azure", 0xf0ffff],
    ["beige", 0xf5f5dc],
    ["bisque", 0xffe4c4],
    ["black", 0x000000],
    ["blanchedalmond", 0xffebcd],
    ["blue", 0x0000ff],
    ["blueviolet", 0x8a2be2],
    ["brown", 0xa52a2a],
    ["burlywood", 0xdeb887],
    ["cadetblue", 0x5f9ea0],
    ["chartreuse", 0x7fff00],
    ["chocolate", 0xd2691e],
    ["coral", 0xff7f50],
    ["cornflowerblue", 0x6495ed],
    ["cornsilk", 0xfff8dc],
    ["crimson", 0xdc143c],
    ["cyan", 0x00ffff],
    ["darkblue", 0x00008b],
    ["darkcyan", 0x008b8b],
    ["darkgoldenrod", 0xb8860b],
    ["darkgray", 0xa9a9a9],
    ["darkgrey", 0xa9a9a9],
    ["darkgreen", 0x006400],
    ["darkkhaki", 0xbdb76b],
    ["darkmagenta", 0x8b008b],
    ["darkolivegreen", 0x556b2f],
    ["darkorange", 0xff8c00],
    ["darkorchid", 0x9932cc],
    ["darkred", 0x8b0000],
    ["darksalmon", 0xe9967a],
    ["darkseagreen", 0x8fbc8f],
    ["darkslateblue", 0x483d8b],
    ["darkslategray", 0x2f4f4f],
    ["darkslategrey", 0x2f4f4f],
    ["darkturquoise", 0x00ced1],
    ["darkviolet", 0x9400d3],
    ["deeppink", 0xff1493],
    ["deepskyblue", 0x00bfff],
    ["dimgray", 0x696969],
    ["dimgrey", 0x696969],
    ["dodgerblue", 0x1e90ff],
    ["firebrick", 0xb22222],
    ["floralwhite", 0xfffaf0],
    ["forestgreen", 0x228b22],
    ["fuchsia", 0xff00ff],
    ["gainsboro", 0xdcdcdc],
    ["ghostwhite", 0xf8f8ff],
    ["gold", 0xffd700],
    ["goldenrod", 0xdaa520],
    ["gray", 0x808080],
    ["grey", 0x808080],
    ["green", 0x008000],
    ["greenyellow", 0xadff2f],
    ["honeydew", 0xf0fff0],
    ["hotpink", 0xff69b4],
    ["indianred", 0xcd5c5c],
    ["indigo", 0x4b0082],
    ["ivory", 0xfffff0],
    ["khaki", 0xf0e68c],
    ["lavender", 0xe6e6fa],
    ["lavenderblush", 0xfff0f5],
    ["lawngreen", 0x7cfc00],
    ["lemonchiffon", 0xfffacd],
    ["lightblue", 0xadd8e6],
    ["lightcoral", 0xf08080],
    ["lightcyan", 0xe0ffff],
    ["lightgoldenrodyellow", 0xfafad2],
    ["lightgray", 0xd3d3d3],
    ["lightgrey", 0xd3d3d3],
    ["lightgreen", 0x90ee90],
    ["lightpink", 0xffb6c1],
    ["lightsalmon", 0xffa07a],
    ["lightseagreen", 0x20b2aa],
    ["lightskyblue", 0x87cefa],
    ["lightslategray", 0x778899],
    ["lightslategrey", 0x778899],
    ["lightsteelblue", 0xb0c4de],
    ["lightyellow", 0xffffe0],
    ["lime", 0x00ff00],
    ["limegreen", 0x32cd32],
    ["linen", 0xfaf0e6],
    ["magenta", 0xff00ff],
    ["maroon", 0x800000],
    ["mediumaquamarine", 0x66cdaa],
    ["mediumblue", 0x0000cd],
    ["mediumorchid", 0xba55d3],
    ["mediumpurple", 0x9370d8],
    ["mediumseagreen", 0x3cb371],
    ["mediumslateblue", 0x7b68ee],
    ["mediumspringgreen", 0x00fa9a],
    ["mediumturquoise", 0x48d1cc],
    ["mediumvioletred", 0xc71585],
    ["midnightblue", 0x191970],
    ["mintcream", 0xf5fffa],
    ["mistyrose", 0xffe4e1],
    ["moccasin", 0xffe4b5],
    ["navajowhite", 0xffdead],
    ["navy", 0x000080],
    ["oldlace", 0xfdf5e6],
    ["olive", 0x808000],
    ["olivedrab", 0x6b8e23],
    ["orange", 0xffa500],
    ["orangered", 0xff4500],
    ["orchid", 0xda70d6],
    ["palegoldenrod", 0xeee8aa],
    ["palegreen", 0x98fb98],
    ["paleturquoise", 0xafeeee],
    ["palevioletred", 0xd87093],
    ["papayawhip", 0xffefd5],
    ["peachpuff", 0xffdab9],
    ["peru", 0xcd853f],
    ["pink", 0xffc0cb],
    ["plum", 0xdda0dd],
    ["powderblue", 0xb0e0e6],
    ["purple", 0x800080],
    ["red", 0xff0000],
    ["rosybrown", 0xbc8f8f],
    ["royalblue", 0x4169e1],
    ["saddlebrown", 0x8b4513],
    ["salmon", 0xfa8072],
    ["sandybrown", 0xf4a460],
    ["seagreen", 0x2e8b57],
    ["seashell", 0xfff5ee],
    ["sienna", 0xa0522d],
    ["silver", 0xc0c0c0],
    ["skyblue", 0x87ceeb],
    ["slateblue", 0x6a5acd],
    ["slategray", 0x708090],
    ["slategrey", 0x708090],
    ["snow", 0xfffafa],
    ["springgreen", 0x00ff7f],
    ["steelblue", 0x4682b4],
    ["tan", 0xd2b48c],
    ["teal", 0x008080],
    ["thistle", 0xd8bfd8],
    ["tomato", 0xff6347],
    ["turquoise", 0x40e0d0],
    ["violet", 0xee82ee],
    ["wheat", 0xf5deb3],
    ["white", 0xffffff],
    ["whitesmoke", 0xf5f5f5],
    ["yellow", 0xffff00],
    ["yellowgreen", 0x9acd32]
]);

const html4ColorsReverse: Map<number, string> = new Map();

html4Colors.forEach((value, key) => {
    // Reverse the order.
    if (!html4ColorsReverse.has(value)) {
        html4ColorsReverse.set(value, key);
    }
});

/**
 * Normalizes the value to ensure it falls between the minimum and maximum
 * values, inclusively.
 *
 * @param value The value to be normalized.
 * @param minimum The minimum value allowed.
 * @param maximum The maximum value allowed.
 *
 * @returns The number after it has been constrained to the allowed values.
 */
function normalize(value: number, minimum: number, maximum: number): number {
    return value < minimum ? minimum : value > maximum ? maximum : value;
}

/**
 * Helper method to perform some conversion of HSL values to RGB values.
 *
 * @param q1 The first value.
 * @param q2 The second value.
 * @param hue The hue value.
 */
function qqhToRgb(q1: number, q2: number, hue: number): number {
    if (hue > 360) {
        hue -= 360;
    }
    else if (hue < 0) {
        hue += 360;
    }

    if (hue < 60) {
        return q1 + (q2 - q1) * hue / 60;
    }
    else if (hue < 180) {
        return q2;
    }
    else if (hue < 240) {
        return q1 + (q2 - q1) * (240 - hue) / 60;
    }
    else {
        return q1;
    }
}

/**
 * Parses a hex string into a single numerical value.
 *
 * @param hex The string containing the hex characters.
 *
 * @returns The numerical value of the hex representation.
 */
function parseHexValue(hex: string): number {
    const value = parseInt(hex, 16);

    return isNaN(value) ? 0 : value;
}

/**
 * Parses a hexadecimal representation of a color into the RGBA values.
 *
 * @param hex The hexadecimal representation of the color.
 *
 * @returns A 4 segment array of numbers that represent the RGBA values.
 */
function parseHexString(hex: string): number[] {
    hex = hex.startsWith("#") ? hex.substring(1) : hex;

    if (hex.length === 8) {
        return [
            parseHexValue(hex.substring(0, 2)),
            parseHexValue(hex.substring(2, 4)),
            parseHexValue(hex.substring(4, 6)),
            parseHexValue(hex.substring(6, 8)) / 255
        ];
    }
    else if (hex.length === 6) {
        return [
            parseHexValue(hex.substring(0, 2)),
            parseHexValue(hex.substring(2, 4)),
            parseHexValue(hex.substring(4, 6)),
            1
        ];
    }
    else if (hex.length === 4) {
        return [
            parseHexValue(hex.substring(0, 1).repeat(2)),
            parseHexValue(hex.substring(1, 2).repeat(2)),
            parseHexValue(hex.substring(2, 3).repeat(2)),
            parseHexValue(hex.substring(3, 4).repeat(2)) / 255
        ];
    }
    else if (hex.length === 3) {
        return [
            parseHexValue(hex.substring(0, 1).repeat(2)),
            parseHexValue(hex.substring(1, 2).repeat(2)),
            parseHexValue(hex.substring(2, 3).repeat(2)),
            1
        ];
    }
    else {
        return [0, 0, 0, 1];
    }
}

/**
 * Helper function to parse a floating point number from a string with error
 * handling for non-numbers.
 *
 * @param str The string to be parsed.
 *
 * @returns The numerical value or 0 if it could not be parsed.
 */
function asFloat(str: string): number {
    const num = parseFloat(str);

    return isNaN(num) ? 0 : num;
}

/**
 * Utility class for color manipulation.
 */
export class RockColor {
    // #region Fields

    private readonly rgbInternal: number[] = [0, 0, 0];
    private alphaInternal: number = 1;
    private hueInternal!: number;
    private saturationInternal!: number;
    private luminosityInternal!: number;
    private textInternal?: string;

    // #endregion

    // #region Properties

    /**
     * Gets or sets the Alpha level. This will be between 0 and 1.
     */
    public get alpha(): number {
        return this.alphaInternal;
    }

    public set alpha(value: number) {
        this.alphaInternal = normalize(value, 0, 1);
    }

    /**
     * Gets or sets the Red value of the color. This will be between 0 and 255.
     */
    public get r(): number {
        return this.rgbInternal[0];
    }

    public set r(value: number) {
        this.rgbInternal[0] = normalize(value, 0, 255);
        this.updateHslFromRgb();
    }

    /**
     * Gets or sets the Green value of the color. This will be between 0 and 255.
     */
    public get g(): number {
        return this.rgbInternal[1];
    }

    public set g(value: number) {
        this.rgbInternal[1] = normalize(value, 0, 255);
        this.updateHslFromRgb();
    }

    /**
     * Gets or sets the Blue value of the color. This will be between 0 and 255.
     */
    public get b(): number {
        return this.rgbInternal[2];
    }

    public set b(value: number) {
        this.rgbInternal[2] = normalize(value, 0, 255);
        this.updateHslFromRgb();
    }

    /**
     * Calculates the luma value based on the W3 Standard.
     */
    public get luma(): number {
        const { red, green, blue } = this.toStandardRgb();

        return (0.2126 * red) + (0.7152 * green) + (0.0722 * blue);
    }

    /**
     * Gets or sets the hue of the color.
     */
    public get hue(): number {
        return this.hueInternal;
    }

    public set hue(value: number) {
        // Take care of translating things like 540 to 360.
        while (value > 360) {
            value -= 360;
        }

        while (value < 0) {
            value += 360;
        }

        this.hueInternal = value;
        this.updateRgbFromHsl();
    }

    /**
     * Gets or sets the saturation of the color.
     */
    public get saturation(): number {
        return this.saturationInternal;
    }

    public set saturation(value: number) {
        this.saturationInternal = normalize(value, 0, 1);
        this.updateRgbFromHsl();
    }

    /**
     * Gets or sets the luminosity of the color.
     */
    public get luminosity(): number {
        return this.luminosityInternal;
    }

    public set luminosity(value: number) {
        this.luminosityInternal = normalize(value, 0, 1);
        this.updateRgbFromHsl();
    }

    /**
     * Gets the color as a hexadecimal string, including the leading `#`.
     */
    public get hex(): string {
        return this.toHex();
    }

    /**
     * Determines if the color is a light color.
     */
    public get isLight(): boolean {
        return this.luminosityInternal > 0.5;
    }

    /**
     * Determines if the color is a dark color.
     */
    public get isDark(): boolean {
        return !this.isLight;
    }

    // #endregion

    // #region Constructors

    constructor(color: string);
    constructor(rgb: number[]);
    constructor(rgb: number[], alpha: number);
    constructor(rgb: number[], alpha: number, text: string);
    constructor(color: number);
    constructor(red: number, green: number, blue: number);
    constructor(red: number, green: number, blue: number, alpha: number);
    constructor(red: number, green: number, blue: number, alpha?: number, text?: string);

    constructor(p1: number | string | number[], p2?: number, p3?: number | string, p4?: number, p5?: string) {
        if (typeof p1 === "number") {
            if (typeof p2 === "undefined") {
                this.constructFromSingleNumber(p1);
            }
            else if (typeof p3 === "number") {
                if (typeof p4 === "number") {
                    this.constructFromRgb([p1, p2, p3], p4);
                }
                else {
                    this.constructFromRgb([p1, p2, p3], 1);
                }

                if (typeof p5 === "string") {
                    this.textInternal = p5;
                }
            }
        }
        else if (typeof p1 === "string") {
            this.constructFromString(p1);
        }
        else if (p1.length === 3) {
            if (typeof p2 === "number") {
                this.constructFromRgb([p1[0], p1[1], p1[2]], p2);
            }
            else {
                this.constructFromRgb([p1[0], p1[1], p1[2]], 1);
            }

            if (typeof p3 === "string") {
                this.textInternal = p3;
            }
        }

        this.updateHslFromRgb();
    }

    /**
     * Creates this instance from the specified number value. This is a numeric
     * representation such as 0x112233.
     *
     * @param color The color has a number value.
     */
    private constructFromSingleNumber(color: number): void {
        this.rgbInternal[0] = color & 0xff;
        color >>= 8;
        this.rgbInternal[1] = color & 0xff;
        color >>= 8;
        this.rgbInternal[2] = color & 0xff;

        this.alphaInternal = 1;
    }

    /**
     * Creates this instance from the specified text value. This can be either
     * a `#` hex value, an `rgba` value, an `rgb` value or a named HTML color.
     *
     * @param color The string that represents the color.
     */
    private constructFromString(color: string): void {
        // Check for colors in #ee7625 format.
        if (color.startsWith("#")) {
            const rgba = parseHexString(color);
            this.rgbInternal[0] = normalize(rgba[0], 0, 255);
            this.rgbInternal[1] = normalize(rgba[1], 0, 255);
            this.rgbInternal[2] = normalize(rgba[2], 0, 255);
            this.alphaInternal = normalize(rgba[3], 0, 1);
        }

        // Check for colors in rgba(255, 99, 71, 0.5) format.
        else if (color.startsWith("rgba")) {
            const parts = color.replace(/ /g, "")
                .replace("rgba(", "")
                .replace(")", "")
                .split(",");

            if (parts.length === 4) {
                this.rgbInternal[0] = normalize(asFloat(parts[0].trim()), 0, 255);
                this.rgbInternal[1] = normalize(asFloat(parts[1].trim()), 0, 255);
                this.rgbInternal[2] = normalize(asFloat(parts[2].trim()), 0, 255);
                this.alphaInternal = normalize(asFloat(parts[3].trim()), 0, 1);
            }
        }

        // Check for colors in rgb(255, 99, 71) format.
        else if (color.startsWith("rgb")) {
            const parts = color.replace(/ /g, "")
                .replace("rgb(", "")
                .replace(")", "")
                .split(",");

            if (parts.length === 3) {
                this.rgbInternal[0] = normalize(asFloat(parts[0].trim()), 0, 255);
                this.rgbInternal[1] = normalize(asFloat(parts[1].trim()), 0, 255);
                this.rgbInternal[2] = normalize(asFloat(parts[2].trim()), 0, 255);
                this.alphaInternal = 1;
            }
        }

        // Check if it is a named color.
        else {
            const namedColor = RockColor.getColorFromKeyword(color);

            if (namedColor != null) {
                this.rgbInternal[0] = namedColor.r;
                this.rgbInternal[1] = namedColor.g;
                this.rgbInternal[2] = namedColor.b;
                this.alpha = namedColor.alpha;
            }
        }
    }

    /**
     * Creates this instance from the specified RGB and alpha values.
     *
     * @param rgb The RGB components, this must always have 3 components.
     * @param alpha The alpha value to assign.
     */
    private constructFromRgb(rgb: number[], alpha: number): void {
        this.rgbInternal[0] = normalize(rgb[0], 0, 255);
        this.rgbInternal[1] = normalize(rgb[1], 0, 255);
        this.rgbInternal[2] = normalize(rgb[2], 0, 255);
        this.alphaInternal = normalize(alpha, 0, 1);
    }

    // #endregion

    // #region Private Functions

    /**
     * Updates the HSL values of the color from the RGB values.
     */
    private updateHslFromRgb(): void {
        // Convert the RGB values to representations of between 0.0 and 1.0.
        const red = this.rgbInternal[0] / 255;
        const green = this.rgbInternal[1] / 255;
        const blue = this.rgbInternal[2] / 255;

        // Get the maximum value between all three components.
        const max = Math.max(red, green, blue);

        // Get the minmimum value between all three components.
        const min = Math.min(red, green, blue);

        // Update the luminosity.
        this.luminosityInternal = (max + min) / 2;

        const diff = max - min;

        // Update the hue and saturation values.
        if (Math.abs(diff) < 0.00001) {
            this.saturationInternal = 0;
            this.hueInternal = 0;
        }
        else {
            if (this.luminosityInternal <= 0.5) {
                this.saturationInternal = diff / (max + min);
            }
            else {
                this.saturationInternal = diff / (2 - max - min);
            }

            const redDist = (max - red) / diff;
            const greenDist = (max - green) / diff;
            const blueDist = (max - blue) / diff;

            let hue: number;

            if (red == max) {
                hue = blueDist - greenDist;
            }
            else if (green == max) {
                hue = 2 + redDist - blueDist;
            }
            else {
                hue = 4 + greenDist - redDist;
            }

            hue = hue * 60;
            if (hue < 0) {
                hue += 360;
            }

            this.hueInternal = normalize(hue, 0, 360);
        }
    }

    /**
     * Updates the RGB values of the color from the HSL values.
     */
    private updateRgbFromHsl(): void {
        const p2 = this.luminosityInternal <= 0.5
            ? this.luminosityInternal * (1 + this.saturationInternal)
            : this.luminosityInternal + this.saturationInternal - this.luminosityInternal * this.saturationInternal;
        const p1 = 2 * this.luminosityInternal - p2;

        let red: number;
        let green: number;
        let blue: number;

        if (this.saturationInternal === 0) {
            red = this.luminosityInternal;
            green = this.luminosityInternal;
            blue = this.luminosityInternal;
        }
        else {
            red = qqhToRgb(p1, p2, this.hueInternal + 120);
            green = qqhToRgb(p1, p2, this.hueInternal);
            blue = qqhToRgb(p1, p2, this.hueInternal - 120);
        }

        // Convert the decimal RGB values to 0 to 255 range.
        this.rgbInternal[0] = normalize(Math.round(red * 255), 0, 255);
        this.rgbInternal[1] = normalize(Math.round(green * 255), 0, 255);
        this.rgbInternal[2] = normalize(Math.round(blue * 255), 0, 255);
    }

    // #endregion

    // #region Public Functions

    /**
     * Gets the {@link RockColor} that corresponds to the HTML color keyword.
     *
     * @param keyword The HTML color keyword.
     *
     * @returns An instance of {@link RockColor} that represents the color or `null` if not found.
     */
    public static getColorFromKeyword(keyword: string): RockColor | null {
        if (keyword === "transparent") {
            return new RockColor(0, 0, 0, 0, keyword);
        }

        const rgb = html4Colors.get(keyword);
        if (rgb !== undefined) {
            const red = (rgb >> 16) & 0xff;
            const green = (rgb >> 8) & 0xff;
            const blue = rgb & 0xff;

            return new RockColor(red, green, blue, 1, keyword);
        }

        return null;
    }

    /**
     * Calculates the contrast ratio between two colors.
     *
     * @param color1 The first color.
     * @param color2 The second color.
     *
     * @returns A number that represents the contrast ratio between the two colors.
     */
    public static calculateContrastRatio(color1: RockColor, color2: RockColor): number {
        // Formula: (L1 + 0.05) / (L2 + 0.05)
        // https://medium.muz.li/the-science-of-color-contrast-an-expert-designers-guide-33e84c41d156
        // https://www.w3.org/TR/2012/NOTE-WCAG20-TECHS-20120103/G17.html
        // L1 = Lighter color
        // L2 = Darker color

        let l1: RockColor;
        let l2: RockColor;

        // Determine the lighter and darker color.
        if (color1.luminosity > color2.luminosity) {
            l1 = color1;
            l2 = color2;
        }
        else {
            l1 = color2;
            l2 = color1;
        }

        return (l1.luma + 0.05) / (l2.luma + 0.05);
    }

    /**
     * Creates a color pair from a single color with logic for light and
     * dark modes.
     *
     * @param color The base color to use when creating the color pair.
     * @param colorScheme The color scheme the color will be used in.
     *
     * @returns A new instance of {@link ColorPair}.
     */
    public static calculateColorPair(color: RockColor, colorScheme: ColorScheme = ColorScheme.Light): ColorPair {
        const foregroundColor = RockColor.calculateColorRecipe(color, ColorRecipe.Darkest);
        const backgroundColor = RockColor.calculateColorRecipe(color, ColorRecipe.Lightest);

        const colorPair = new ColorPair(foregroundColor, backgroundColor);

        if (colorScheme === ColorScheme.Dark) {
            colorPair.flip();
        }

        return colorPair;
    }

    /**
     * Creates a recipe color from the provided color.
     *
     * @param color The base color to calculate the new color from.
     * @param recipe The recipe to use when calculating the new color.
     *
     * @returns A new instance of {@link RockColor}.
     */
    public static calculateColorRecipe(color: RockColor, recipe: ColorRecipe): RockColor {
        const recipeColor = color.clone();
        let recipeSaturation = 0;
        let recipeLuminosity = 0;

        switch (recipe) {
            case ColorRecipe.Lightest:
                recipeSaturation = 0.88;
                recipeLuminosity = 0.87;
                break;

            case ColorRecipe.Light:
                recipeSaturation = 0.10;
                recipeLuminosity = 0.95;
                break;

            case ColorRecipe.Medium:
                recipeSaturation = 0.20;
                recipeLuminosity = 0.66;
                break;

            case ColorRecipe.Dark:
                recipeSaturation = 0.30;
                recipeLuminosity = 0.45;
                break;

            case ColorRecipe.Darkest:
                recipeSaturation = 0.60;
                recipeLuminosity = 0.20;
                break;

            case ColorRecipe.Primary:
                recipeSaturation = 0.70;
                recipeLuminosity = 0.80;
                break;
        }

        // If the saturation of the original color is very low then we'll use
        // a different recipe so the color looks more like the original (which
        // would be gray).
        if (color.saturation <= 0.15) {
            recipeSaturation = color.saturation;
        }

        recipeColor.saturation = recipeSaturation;
        recipeColor.luminosity = recipeLuminosity;

        return recipeColor;
    }

    /**
     * Lightens the color by the provided percentage.
     *
     * @param percentage The percentage amount to lighten the color as a number between 0 and 100.
     */
    public lighten(percentage: number): void {
        this.luminosity = this.luminosity + (percentage / 100);
    }

    /**
     * Darkens the color by the provided percentage.
     *
     * @param percentage The percentage amount to darken the color as a number between 0 and 100.
     */
    public darken(percentage: number): void {
        this.luminosity = this.luminosity - (percentage / 100);
    }

    /**
     * Saturates the color by the provided percentage.
     *
     * @param percentage The percentage amount to saturate the color as a number between 0 and 100.
     */
    public saturate(percentage: number): void {
        this.saturation = this.saturation + (percentage / 100);
    }

    /**
     * Desaturates the color by the provided percentage.
     *
     * @param percentage The percentage amount to desaturate the color as a number between 0 and 100.
     */
    public desaturate(percentage: number): void {
        this.saturation = this.saturation - (percentage / 100);
    }

    /**
     * Increases the opacity level by the given percentage. This makes the
     * color less transparent and more opaque.
     *
     * @param percentage The percentage amount to adjust the alpha as a number between 0 and 100.
     */
    public fadeIn(percentage: number): void {
        this.alpha = this.alpha + (percentage / 100);
    }

    /**
     * Decreases the opacity level by the given percentage. This makes the
     * color more transparent and less opaque.
     *
     * @param percentage The percentage amount to adjust the alpha as a number between 0 and 100.
     */
    public fadeOut(percentage: number): void {
        this.alpha = this.alpha - (percentage / 100);
    }

    /**
     * Adjusts the hue by the specified percentage.
     *
     * @param percentage The percentage to adjust the hue by as a value between -100 and 100.
     */
    public adjustHueByPercent(percentage: number): void {
        this.hue = this.hue + (360 * (percentage / 100));
    }

    /**
     * Adjusts the hue by the specified number of degrees.
     *
     * @param degrees The number of degrees to adjust the hue by as a value between -360 and 360.
     */
    public adjustHueByDegrees(degrees: number): void {
        this.hue = this.hue + degrees;
    }

    /**
     * Tints the specified percentage amount. This mixes this color with white
     * by the percentage.
     *
     * @param percentage The percentage amount as a value between 0 and 100.
     */
    public tint(percentage: number): void {
        this.mix(new RockColor("#ffffff"), percentage);
    }

    /**
     * Shades the specified percentage amount. This mixes this color with black
     * by the percentage.
     *
     * @param percentage The percentage amount as a value between 0 and 100.
     */
    public shade(percentage: number): void {
        this.mix(new RockColor("#000000"), percentage);
    }

    /**
     * Mixes the specified color into the current color with an optional
     * percentage amount.
     *
     * @param mixColor The color to be mixed into this color.
     * @param percentage The percentage amount to be mixed as a value between 0 and 100. Defaults to 50.
     */
    public mix(mixColor: RockColor, percentage?: number): void {
        const amount = (percentage ?? 50) / 100;

        this.r = (mixColor.r * amount) + this.r * (1 - amount);
        this.g = (mixColor.g * amount) + this.g * (1 - amount);
        this.b = (mixColor.b * amount) + this.b * (1 - amount);
    }

    /**
     * Turns the color to it's grayscale value.
     */
    public grayscale(): void {
        this.saturate(-100);
    }

    /**
     * Creates a clone of this color.
     *
     * @returns A new instance that has the same color values as this instance.
     */
    public clone(): RockColor {
        return new RockColor(this.rgbInternal, this.alpha);
    }

    /**
     * Converts the current color to a CSS `rgba` string format.
     *
     * @returns A string representation of the color.
     */
    public toRgba(): string {
        return `rgba(${Math.floor(this.r)}, ${Math.floor(this.g)}, ${Math.floor(this.b)}, ${this.alpha})`;
    }

    /**
     * Converts the current color to an HTML hexadecimal `#rrggbbaa` string
     * format. If the alpha value is 1 then the "aa" component will not be
     * included.
     *
     * @returns Hexadecimal version of the color.
     */
    public toHex(): string {
        const r = Math.round(this.r).toString(16).padStart(2, "0");
        const g = Math.round(this.g).toString(16).padStart(2, "0");
        const b = Math.round(this.b).toString(16).padStart(2, "0");
        const a = Math.round(this.alpha * 255).toString(16).padStart(2, "0");

        if (a === "ff") {
            return `#${r}${g}${b}`;
        }
        else {
            return `#${r}${g}${b}${a}`;
        }
    }

    /**
     * Gets a numeric value representing this color. If two colors have the
     * same RGB and Alpha values then they will return the same value here.
     *
     * @returns A numeric value that represents this unique color.
     */
    public valueOf(): number {
        return (this.r + this.g + this.b) * this.alpha;
    }

    /**
     * Compares this color against another color to see which one has a higher
     * value.
     *
     * @param otherColor The other color to compare this color to.
     *
     * @returns `0` if both colors are the same, `-1` if this color is less than the other color and `1` if this color is greater than the other color.
     */
    public compareTo(otherColor: RockColor | undefined | null): number {
        if (otherColor === undefined || otherColor === null) {
            return -1;
        }

        // Check if they are identical.
        if (this.r === otherColor.r && this.g === otherColor.g && this.b === otherColor.b && this.alpha === otherColor.alpha) {
            return 0;
        }

        return this.valueOf() > otherColor.valueOf() ? 1 : -1;
    }

    /**
     * Determines if two colors are similar.
     *
     * @param otherColor The other color to compare this color to.
     * @param similarityPercent The minimum percentage to consider the colors similar. This will be between 0 and 1 (defaults to 0.95).
     * @returns `true` if the colors are N percent similar, where N is the `similarityPercentage`; otherwise, returns `false`.
     * @example
     * // this = new RockColor("#FFF")
     * this.isSimilarTo(new RockColor("#FFE"), 0.95) // `true`, color similarity >= 95%
     * this.isSimilarTo(new RockColor("#FFE"), 0.99) // `false`, color similarity < 99%
     * this.isSimilarTo(new RockColor("#FFE"), 1) // `false`, color similarity != 100%
     * this.isSimilarTo(new RockColor("#FFF"), 1) // `true`, color similarity = 100%
     */
    public isSimilarTo(otherColor: RockColor, similarityPercent: number = 0.95): boolean {
        similarityPercent = normalize(similarityPercent, 0, 1);

        if (similarityPercent === 0) {
            // A color similarity of 0% will always return `true`
            // as the colors do not have to be similar at all.
            return true;
        }

        if (this.compareTo(otherColor) === 0) {
            // The same colors will always return `true`
            // regardless of the percent similar passed in.
            return true;
        }

        if (similarityPercent === 1) {
            // The colors are not the same at this point
            // but the similarity percentage was set to 100%,
            // so return `false`.
            return false;
        }

        // Calculate the actual similarity percent and compare the actual and expected values.

        function calculateWeightedColorDistance(r1: number, g1: number, b1: number, r2: number, g2: number, b2: number, maxColorValue: number): number {
            const redDiffSquared = Math.pow(r2 - r1, 2);
            const greenDiffSquared = Math.pow(g2 - g1, 2);
            const blueDiffSquared = Math.pow(b2 - b1, 2);
            const redAverage = (r2 + r1) / 2;
            const redAveragePercentage = redAverage / maxColorValue;

            return Math.sqrt(
                ((2 + redAveragePercentage) * redDiffSquared) +
                (4 * greenDiffSquared) +
                ((3 - redAveragePercentage) * blueDiffSquared)
            );
        }

        function calculateSimilarityPercent(r1: number, g1: number, b1: number, r2: number, g2: number, b2: number, maxColorValue: number): number {
            const maxDistance = calculateWeightedColorDistance(
                maxColorValue, maxColorValue, maxColorValue,
                0, 0, 0,
                maxColorValue);

            const colorDistance = calculateWeightedColorDistance(
                r1, g1, b1,
                r2, g2, b2,
                maxColorValue);

            return 1 - (colorDistance / maxDistance);
        }

        // Compare the sRGB values first.
        const sRgb = this.toStandardRgb();
        const otherSRgb = otherColor.toStandardRgb();

        let actualSimilarityPercent = calculateSimilarityPercent(
            sRgb.red, sRgb.green, sRgb.blue,
            otherSRgb.red, otherSRgb.green, otherSRgb.blue,
            1);

        if (actualSimilarityPercent >= similarityPercent) {
            return true;
        }
        // Only fallback to comparing RGB values if the previous result was close (within 5%).
        else if (similarityPercent - actualSimilarityPercent <= 0.05) {
            actualSimilarityPercent = calculateSimilarityPercent(
                this.r, this.g, this.b,
                otherColor.r, otherColor.g, otherColor.b,
                255);

            return actualSimilarityPercent >= similarityPercent;
        }
        else {
            return false;
        }
    }

    /**
     * Gets the sRGB value of this color.
     *
     * @returns This color in the standard RGB color space.
     */
    private toStandardRgb(): { red: number, green: number, blue: number } {
        const linearR = this.r / 255;
        const linearG = this.g / 255;
        const linearB = this.b / 255;

        const red = (linearR <= 0.03928) ? linearR / 12.92 : Math.pow((linearR + 0.055) / 1.055, 2.4);
        const green = (linearG <= 0.03928) ? linearG / 12.92 : Math.pow((linearG + 0.055) / 1.055, 2.4);
        const blue = (linearB <= 0.03928) ? linearB / 12.92 : Math.pow((linearB + 0.055) / 1.055, 2.4);

        return { red, green, blue };
    }

    // #endregion
}
