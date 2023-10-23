import { RockColor } from "../../../Framework/Core/Utilities/rockColor";
import { ColorRecipe } from "../../../Framework/Enums/Core/colorRecipe";

// NOTICE!!!
// This file has a C# version in the Rock.Tests.UnitTests project. If changes
// are made to one the same changes must be made to the other.
// NOTICE!!!

function stringToExpectedRgba(str: string, expectedRed: number, expectedGreen: number, expectedBlue: number, expectedAlpha: number): void {
    const color = new RockColor(str);

    expect(color.r).toStrictEqual(expectedRed);
    expect(color.g).toStrictEqual(expectedGreen);
    expect(color.b).toStrictEqual(expectedBlue);
    expect(color.alpha).toStrictEqual(expectedAlpha);
}

function stringToExpectedRgb(str: string, expectedRed: number, expectedGreen: number, expectedBlue: number): void {
    const color = new RockColor(str);

    expect(color.r).toStrictEqual(expectedRed);
    expect(color.g).toStrictEqual(expectedGreen);
    expect(color.b).toStrictEqual(expectedBlue);
    expect(color.alpha).toStrictEqual(1);
}

function stringToExpectedBlack(str: string): void {
    const color = new RockColor(str);

    expect(color.r).toStrictEqual(0);
    expect(color.g).toStrictEqual(0);
    expect(color.b).toStrictEqual(0);
    expect(color.alpha).toStrictEqual(1);
}

function roundNumber(num: number, scale: number): number {
    // Taken from https://stackoverflow.com/a/12830454.
    if (!("" + num).includes("e")) {
        return +(Math.round(Number(num + "e+" + scale)) + "e-" + scale);
    }
    else {
        const arr = ("" + num).split("e");
        let sig = "";
        if (+arr[1] + scale > 0) {
            sig = "+";
        }
        return +(Math.round(+(arr[0] + "e" + sig + (+arr[1] + scale))) + "e-" + scale);
    }
}

describe("RockColor", () => {
    describe("Constructor for named color", () => {
        test.each([
            ["red", 255, 0, 0, 1],
            ["green", 0, 128, 0, 1],
            ["blue", 0, 0, 255, 1],
            ["deeppink", 255, 20, 147, 1],
            ["orange", 255, 165, 0, 1],
            ["transparent", 0, 0, 0, 0]
        ])("Color '%s' parses correctly", stringToExpectedRgba);

        it("Invalid color is black", () => {
            const color = new RockColor("bogon");

            expect(color.r).toStrictEqual(0);
            expect(color.g).toStrictEqual(0);
            expect(color.b).toStrictEqual(0);
            expect(color.alpha).toStrictEqual(1);
        });
    });

    describe("Constructor for hex color", () => {
        test.each([
            ["#f00", 255, 0, 0, 1],
            ["#0f04", 0, 255, 0, 0x44 / 255],
            ["#008000", 0, 128, 0, 1],
            ["#0000ff80", 0, 0, 255, 0x80 / 255]
        ])("Color '%s' parses correctly", stringToExpectedRgba);

        test.each([
            ["#"],
            ["#f"],
            ["#ff"],
            ["#fffff"],
            ["#fffffff"],
            ["#fffffffff"]
        ])("Invalid color '%s' is black", stringToExpectedBlack);
    });

    describe("Constructor for rgba color", () => {
        test.each([
            ["rgba(255, 0, 0, 1)", 255, 0, 0, 1],
            ["rgba(0, 128, 0, 0)", 0, 128, 0, 0],
            ["rgba(0, 0, 64, 0.5", 0, 0, 64, 0.5],
            ["rgba(5, 10, 15, 1)", 5, 10, 15, 1]
        ])("Color '%s' parses correctly", stringToExpectedRgba);

        test.each([
            ["rgba(255)"],
            ["rgba(255, 255)"],
            ["rgba(255, 255, 255)"],
            ["rgba(255, 255, 255, 255, 255)"]
        ])("Invalid color '%s' is black", stringToExpectedBlack);

        test.each([
            ["rgba(500, -23, 0, -0.4)", 255, 0, 0, 0],
            ["rgba(0, 0, 0, 23)", 0, 0, 0, 1]
        ])("Color '%s' is normalized", stringToExpectedRgba);

        test.each([
            ["rgba(1,2,3,0.5)", 1, 2, 3, 0.5],
            ["rgba(  1,2,3,0.5", 1, 2, 3, 0.5],
            ["rgba(1,2,3,0.5  )", 1, 2, 3, 0.5],
            ["rgba(1,2,  3  ,0.5  )", 1, 2, 3, 0.5]
        ])("Color '%s' handles whitespace", stringToExpectedRgba);
    });

    describe("Constructor for rgb color", () => {
        test.each([
            ["rgb(255, 0, 0)", 255, 0, 0],
            ["rgb(0, 128, 0)", 0, 128, 0],
            ["rgb(0, 0, 64)", 0, 0, 64],
            ["rgb(5, 10, 15)", 5, 10, 15]
        ])("Color '%s' parses correctly", stringToExpectedRgb);

        test.each([
            ["rgb(255)"],
            ["rgb(255, 255)"],
            ["rgb(255, 255, 255, 255)"]
        ])("Invalid color '%s' is black", stringToExpectedBlack);

        test.each([
            ["rgb(500, -23, 0)", 255, 0, 0]
        ])("Color '%s' is normalized", stringToExpectedRgb);

        test.each([
            ["rgb(1,2,3)", 1, 2, 3],
            ["rgb(  1,2,3)", 1, 2, 3],
            ["rgb(1,2,3  )", 1, 2, 3],
            ["rgb(1,  2  ,3)", 1, 2, 3]
        ])("Color '%s' handles whitespace", stringToExpectedRgb);
    });

    describe("updateHslFromRgb", () => {
        test.each([
            ["#ee7725", 24.48, 0.86, 0.54],
            ["#346137", 124, 0.30, 0.29]
        ])("Color '%s' calculates correctly", (hexColor: string, expectedHue: number, expectedSaturation: number, expectedLuminosity: number) => {
            const color = new RockColor(hexColor);

            expect(roundNumber(color.hue, 2)).toStrictEqual(expectedHue);
            expect(roundNumber(color.saturation, 2)).toStrictEqual(expectedSaturation);
            expect(roundNumber(color.luminosity, 2)).toStrictEqual(expectedLuminosity);
        });
    });

    describe("updateRgbFromHsl", () => {
        test.each([
            [24, 0.86, 0.54, 239, 118, 37],
            [124, 0.30, 0.29, 52, 96, 55]
        ])("HSL (%d, %d, %d) calculates correctly", (hue: number, saturation: number, luminosity: number, expectedRed: number, expectedGreen: number, expectedBlue: number) => {
            const color = new RockColor("#000000");

            color.hue = hue;
            color.saturation = saturation;
            color.luminosity = luminosity;

            expect(color.r).toStrictEqual(expectedRed);
            expect(color.g).toStrictEqual(expectedGreen);
            expect(color.b).toStrictEqual(expectedBlue);
        });
    });

    describe("luma", () => {
        test.each([
            ["#ee7725", 0.32],
            ["#245678", 0.08],
            ["#cccccc", 0.60]
        ])("Color '%s' calculates correctly", (hexColor: string, expectedLuma: number) => {
            const color = new RockColor(hexColor);

            expect(roundNumber(color.luma, 2)).toStrictEqual(expectedLuma);
        });
    });

    describe("hue", () => {
        test("Huge value wraps correctly", () => {
            const color = new RockColor(255, 255, 255);

            color.hue = (360 * 4) + 30;

            expect(color.hue).toStrictEqual(30);
        });

        test("Negative value wraps correctly", () => {
            const color = new RockColor(255, 255, 255);

            color.hue = -30;

            expect(color.hue).toStrictEqual(360 - 30);
        });
    });

    describe("toHex", () => {
        test.each([
            [255, 0, 0, 1, "#ff0000"],
            [0, 128, 0, 1, "#008000"],
            [0, 0, 64, 0.5, "#00004080"]
        ])("RGBA (%d, %d, %d, %d) calculates correctly", (red: number, green: number, blue: number, alpha: number, expectedHex: string) => {
            const color = new RockColor(red, green, blue, alpha);

            expect(color.toHex()).toStrictEqual(expectedHex);
        });
    });

    describe("toRgba", () => {
        test("Correctly formats color", () => {
            const color = new RockColor(100.2, 255, 0, 0.5);

            expect(color.toRgba()).toStrictEqual("rgba(100, 255, 0, 0.5)");
        });
    });

    describe("isLight", () => {
        test("White returns true", () => {
            const color = new RockColor(255, 255, 255);

            expect(color.isLight).toStrictEqual(true);
        });

        test("Black returns false", () => {
            const color = new RockColor(0, 0, 0);

            expect(color.isLight).toStrictEqual(false);
        });
    });

    describe("isDark", () => {
        test("Black returns true", () => {
            const color = new RockColor(0, 0, 0);

            expect(color.isDark).toStrictEqual(true);
        });

        test("White returns false", () => {
            const color = new RockColor(255, 255, 255);

            expect(color.isDark).toStrictEqual(false);
        });
    });

    describe("compareTo", () => {
        test("Returns zero when equal", () => {
            const color1 = new RockColor(255, 0, 0);
            const color2 = new RockColor(255, 0, 0);

            expect(color1.compareTo(color2)).toStrictEqual(0);
        });

        test("Returns -1 when darker", () => {
            const color1 = new RockColor(0, 32, 0);
            const color2 = new RockColor(255, 0, 0);

            expect(color1.compareTo(color2)).toStrictEqual(-1);
        });

        test("Returns 1 when lighter", () => {
            const color1 = new RockColor(0, 0, 255);
            const color2 = new RockColor(32, 0, 0);

            expect(color1.compareTo(color2)).toStrictEqual(1);
        });
    });

    describe("lighten", () => {
        test("10 increases luminosity by 0.1", () => {
            const color = new RockColor(128, 128, 128);
            const oldLuminosity = color.luminosity;

            color.lighten(10);

            expect(color.luminosity).toStrictEqual(oldLuminosity + 0.1);
        });

        test("-10 decreases luminosity by 0.1", () => {
            const color = new RockColor(128, 128, 128);
            const oldLuminosity = color.luminosity;

            color.lighten(-10);

            expect(color.luminosity).toStrictEqual(oldLuminosity - 0.1);
        });
    });

    describe("darken", () => {
        test("10 decreases luminosity by 0.1", () => {
            const color = new RockColor(128, 128, 128);
            const oldLuminosity = color.luminosity;

            color.darken(10);

            expect(color.luminosity).toStrictEqual(oldLuminosity - 0.1);
        });

        test("-10 increases luminosity by 0.1", () => {
            const color = new RockColor(128, 128, 128);
            const oldLuminosity = color.luminosity;

            color.darken(-10);

            expect(color.luminosity).toStrictEqual(oldLuminosity + 0.1);
        });
    });

    describe("saturate", () => {
        test("10 increases saturation by 0.1", () => {
            const color = new RockColor(64, 64, 96);
            const oldSaturation = color.saturation;

            color.saturate(10);

            expect(color.saturation).toStrictEqual(oldSaturation + 0.1);
        });

        test("-10 decreases saturation by 0.1", () => {
            const color = new RockColor(64, 64, 96);
            const oldSaturation = color.saturation;

            color.saturate(-10);

            expect(color.saturation).toStrictEqual(oldSaturation - 0.1);
        });
    });

    describe("desaturate", () => {
        test("10 decreases saturation by 0.1", () => {
            const color = new RockColor(64, 64, 96);
            const oldSaturation = color.saturation;

            color.desaturate(10);

            expect(color.saturation).toStrictEqual(oldSaturation - 0.1);
        });

        test("-10 increases saturation by 0.1", () => {
            const color = new RockColor(64, 64, 96);
            const oldSaturation = color.saturation;

            color.desaturate(-10);

            expect(color.saturation).toStrictEqual(oldSaturation + 0.1);
        });
    });

    describe("fadeIn", () => {
        test("10 increases alpha by 0.1", () => {
            const color = new RockColor(128, 128, 128, 0.5);
            const oldAlpha = color.alpha;

            color.fadeIn(10);

            expect(color.alpha).toStrictEqual(oldAlpha + 0.1);
        });

        test("-10 decreases alpha by 0.1", () => {
            const color = new RockColor(128, 128, 128, 0.5);
            const oldAlpha = color.alpha;

            color.fadeIn(-10);

            expect(color.alpha).toStrictEqual(oldAlpha - 0.1);
        });
    });

    describe("fadeOut", () => {
        test("10 decreases alpha by 0.1", () => {
            const color = new RockColor(128, 128, 128, 0.5);
            const oldAlpha = color.alpha;

            color.fadeOut(10);

            expect(color.alpha).toStrictEqual(oldAlpha - 0.1);
        });

        test("-10 increases alpha by 0.1", () => {
            const color = new RockColor(128, 128, 128, 0.5);
            const oldAlpha = color.alpha;

            color.fadeOut(-10);

            expect(color.alpha).toStrictEqual(oldAlpha + 0.1);
        });
    });

    describe("adjustHueByPercent", () => {
        test("10 increases alpha by 36 degrees", () => {
            const color = new RockColor(128, 128, 0);
            const oldHue = color.hue;

            color.adjustHueByPercent(10);

            expect(color.hue).toStrictEqual(oldHue + 36);
        });

        test("-10 decreases hue by 36 degrees", () => {
            const color = new RockColor(128, 128, 0);
            const oldHue = color.hue;

            color.adjustHueByPercent(-10);

            expect(color.hue).toStrictEqual(oldHue - 36);
        });
    });

    describe("adjustHueByDegrees", () => {
        test("10 increases alpha by 10 degrees", () => {
            const color = new RockColor(128, 128, 0);
            const oldHue = color.hue;

            color.adjustHueByDegrees(10);

            expect(color.hue).toStrictEqual(oldHue + 10);
        });

        test("-10 decreases hue by 10 degrees", () => {
            const color = new RockColor(128, 128, 0);
            const oldHue = color.hue;

            color.adjustHueByDegrees(-10);

            expect(color.hue).toStrictEqual(oldHue - 10);
        });
    });

    describe("tint", () => {
        test("Red fifty percent produces expected color", () => {
            const color = new RockColor(255, 0, 0);

            color.tint(50);

            expect(color.r).toStrictEqual(255);
            expect(color.g).toStrictEqual(127.5);
            expect(color.b).toStrictEqual(127.5);
        });
    });

    describe("shade", () => {
        test("Red fifty percent produces expected color", () => {
            const color = new RockColor(255, 0, 0);

            color.shade(50);

            expect(color.r).toStrictEqual(127.5);
            expect(color.g).toStrictEqual(0);
            expect(color.b).toStrictEqual(0);
        });
    });

    describe("grayscale", () => {
        test("Only changes saturation", () => {
            const expectedColor = new RockColor("#ee7725");
            const color = new RockColor("#ee7725");

            color.grayscale();

            expect(color.hue).toStrictEqual(expectedColor.hue);
            expect(color.saturation).toStrictEqual(0);
            expect(color.luminosity).toStrictEqual(expectedColor.luminosity);
        });
    });

    describe("clone", () => {
        test("Produces identical color", () => {
            const expectedColor = new RockColor("#ee7725");

            expectedColor.alpha = 0.25;

            const color = expectedColor.clone();

            expect(color.r).toStrictEqual(expectedColor.r);
            expect(color.g).toStrictEqual(expectedColor.g);
            expect(color.b).toStrictEqual(expectedColor.b);
            expect(color.alpha).toStrictEqual(expectedColor.alpha);
            expect(color === expectedColor).toStrictEqual(false);
        });
    });

    describe("calculateContrastRatio", () => {
        test.each([
            ["#404040", "#a8a8a8", 4.36],
            ["#0000ff", "#ffffff", 8.59],
            ["#8a8aff", "#ffffff", 2.93]
        ])("Ratio between '%s' and '%s' is %d", (hexForeground: string, hexBackground: string, expectedRatio: number) => {
            const foregroundColor = new RockColor(hexForeground);
            const backgroundColor = new RockColor(hexBackground);

            const ratio = RockColor.calculateContrastRatio(backgroundColor, foregroundColor);

            expect(roundNumber(ratio, 2)).toStrictEqual(expectedRatio);
        });

        test("Order does not matter", () => {
            const color1 = new RockColor("#000000");
            const color2 = new RockColor("#ffffff");

            const ratio1 = RockColor.calculateContrastRatio(color1, color2);
            const ratio2 = RockColor.calculateContrastRatio(color2, color1);

            expect(ratio1).toStrictEqual(ratio2);
        });
    });

    describe("calculateColorPair", () => {
        test.each([
            ["#219ff3", "#143952", "#c1e4fb"],
            ["#4caf50", "#145217", "#c1fbc3"],
            ["#cd2bba", "#52144a", "#fbc1f4"],
            ["#cdb6b6", "#521414", "#fbc1c1"],
            ["#8f5252", "#521414", "#fbc1c1"],
            ["#ffffff", "#333333", "#dedede"]
        ])("Color '%s' produces '%s' and '%s'", (hexColor: string, expectedForegroundColor: string, expectedBackgroundColor: string) => {
            const color = new RockColor(hexColor);
            const pair = RockColor.calculateColorPair(color);

            expect(pair.foregroundColor.toHex()).toStrictEqual(expectedForegroundColor);
            expect(pair.backgroundColor.toHex()).toStrictEqual(expectedBackgroundColor);
        });
    });

    describe("calculateColorRecipe", () => {
        test.each([
            ["#219ff3", ColorRecipe.Lightest, "#c1e4fb"],
            ["#219ff3", ColorRecipe.Light, "#f1f3f4"],
            ["#219ff3", ColorRecipe.Medium, "#97acba"],
            ["#219ff3", ColorRecipe.Dark, "#507a95"],
            ["#219ff3", ColorRecipe.Darkest, "#143952"],
            ["#219ff3", ColorRecipe.Primary, "#a8d3f0"]
        ])("Color '%s' for recipe '%d' produces '%s'", (hexColor: string, recipe: ColorRecipe, expectedHexColor: string) => {
            const color = new RockColor(hexColor);
            const recipeColor = RockColor.calculateColorRecipe(color, recipe);

            expect(recipeColor.toHex()).toStrictEqual(expectedHexColor);
        });
    });
});
