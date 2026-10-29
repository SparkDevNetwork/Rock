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
// The church color, worked out once per page load. A church picks any color it likes, so the chat
// uses it as a fill only where the text on it can be read: always white text, on the color darkened
// in small oklab lightness steps only until white reads at 4.5 to 1. The tints and
// washes drawn from it are computed here too, because the browsers Rock supports have no
// color-mix, and each is set on the chat's root in a light and a dark form so the sheet can pick
// one under whichever theme is in force without running script again.
import { applyBrand, brandFill, brandTokens } from "../../../../../src/Communication/Chat/ChatShell/ui/brand.partial";

type Rgba = [number, number, number, number];

/** Reads a hex, rgb() or rgba() color or the keyword transparent, so the test does not care which form the code writes. */
function parse(color: string): Rgba {
    const value = color.trim().toLowerCase();

    if (value === "transparent") {
        return [0, 0, 0, 0];
    }

    const hex = /^#([0-9a-f]{6})([0-9a-f]{2})?$/.exec(value);
    if (hex) {
        const channels = hex[1].match(/../g)!.map(pair => parseInt(pair, 16));
        return [channels[0], channels[1], channels[2], hex[2] ? parseInt(hex[2], 16) / 255 : 1];
    }

    const functional = /^rgba?\(([^)]+)\)$/.exec(value);
    if (functional) {
        const parts = functional[1].split(/[\s,/]+/).filter(Boolean).map(Number);
        return [parts[0], parts[1], parts[2], parts.length > 3 ? parts[3] : 1];
    }

    throw new Error(`not a color this test can read: ${color}`);
}

/** One step of rounding either way, since an oklab mix lands between two whole channel values. */
function expectColor(actual: string | undefined, expected: string, name: string): void {
    expect({ name, defined: actual !== undefined }).toEqual({ name, defined: true });
    const [r, g, b, a] = parse(actual!);
    const [er, eg, eb, ea] = parse(expected);

    expect({ name, alpha: Math.abs(a - ea) <= 0.005 }).toEqual({ name, alpha: true });

    // A fully transparent color has no channels worth comparing.
    if (ea === 0) {
        return;
    }

    expect({ name, channels: [r, g, b].every((c, i) => Math.abs(c - [er, eg, eb][i]) <= 1) })
        .toEqual({ name, channels: true });
}

/** WCAG relative luminance and contrast, written out here so the fill is checked by an independent sum. */
function luminance(color: string): number {
    const [r, g, b] = parse(color).map(c => c / 255).map(c => c <= 0.04045 ? c / 12.92 : Math.pow((c + 0.055) / 1.055, 2.4));
    return 0.2126 * r + 0.7152 * g + 0.0722 * b;
}

function contrast(a: string, b: string): number {
    const [high, low] = [luminance(a), luminance(b)].sort((x, y) => y - x);
    return (high + 0.05) / (low + 0.05);
}

const white = "#FFFFFF";

/** Oklab lightness and a copy of the color with a different lightness, written out so the minimal-darkening check is independent of the code under test. */
function oklab(color: string): [number, number, number] {
    const [r, g, b] = parse(color).slice(0, 3).map(c => c / 255).map(c => c <= 0.04045 ? c / 12.92 : Math.pow((c + 0.055) / 1.055, 2.4));
    const l = Math.cbrt(0.4122214708 * r + 0.5363325363 * g + 0.0514459929 * b);
    const m = Math.cbrt(0.2119034982 * r + 0.6806995451 * g + 0.1073969566 * b);
    const s = Math.cbrt(0.0883024619 * r + 0.2817188376 * g + 0.6299787005 * b);

    return [
        0.2104542553 * l + 0.7936177850 * m - 0.0040720468 * s,
        1.9779984951 * l - 2.4285922050 * m + 0.4505937099 * s,
        0.0259040371 * l + 0.7827717662 * m - 0.8086757660 * s
    ];
}

function hexFromOklab([lightness, a, b]: [number, number, number]): string {
    const l = Math.pow(lightness + 0.3963377774 * a + 0.2158037573 * b, 3);
    const m = Math.pow(lightness - 0.1055613458 * a - 0.0638541728 * b, 3);
    const s = Math.pow(lightness - 0.0894841775 * a - 1.2914855480 * b, 3);
    const encode = (c: number): string => {
        const value = 255 * (c <= 0.0031308 ? 12.92 * c : 1.055 * Math.pow(c, 1 / 2.4) - 0.055);
        return Math.round(Math.min(255, Math.max(0, value))).toString(16).padStart(2, "0");
    };

    return "#" + encode(4.0767416621 * l - 3.3077115913 * m + 0.2309699292 * s)
        + encode(-1.2684380046 * l + 2.6097574011 * m - 0.3413193965 * s)
        + encode(-0.0041960863 * l - 0.7034186147 * m + 1.7076147010 * s);
}

describe("the brand fill", () => {
    // Text on the church color is always white; the fill is darkened only as far as white needs.
    // Teal, orange, gold and a very light color start below 4.5 to 1; the dark blue and plum already pass.
    const swatches: Array<[string, string]> = [
        ["Sandbox teal", "#0092B8"],
        ["Rock orange", "#FF791D"],
        ["Gold", "#F4C430"],
        ["A very light color", "#E8F5C8"],
        ["Ocean", "#1F6FEB"],
        ["Plum", "#7A4FD1"],
        ["An already dark color", "#1A3A5C"]
    ];

    test.each(swatches)("%s %s gets white text on a fill that reads at 4.5 to 1", (_name, church) => {
        const result = brandFill(church);

        expectColor(result.onBrand, white, "on-brand");
        expect(contrast(result.fill, white)).toBeGreaterThanOrEqual(4.5);
    });

    test.each(swatches)("%s %s is darkened no further than white needs", (_name, church) => {
        const result = brandFill(church);

        if (contrast(church, white) >= 4.5) {
            expectColor(result.fill, church, "fill");
            return;
        }

        // One 0.01 step of lightness back toward the church color must fail, or the fill went too far.
        const [lightness, a, b] = oklab(result.fill);
        expect(contrast(hexFromOklab([lightness + 0.01, a, b]), white)).toBeLessThan(4.5);
    });

    test("the sandbox teal deepens to about the design's value", () => {
        expectColor(brandFill("#0092B8").fill, "#0080A5", "fill");
    });

    test("a color that already reads with white is unchanged", () => {
        expectColor(brandFill("#1A3A5C").fill, "#1A3A5C", "fill");
    });

    test("a church color written in lower case gives the same fill", () => {
        expect(brandFill("#0092b8")).toEqual(brandFill("#0092B8"));
    });
});

describe("the tokens drawn from the church color", () => {
    // The design's fallback column: what each token resolves to for Rock orange.
    const rockOrange = {
        light: {
            "brand-fill": "#CD4B00",
            "on-brand": white,
            "brand-ink": "#7A3608",
            "brand-tint": "#FDECE0",
            "brand-wash": "#F9EBE6",
            "brand-ring": "rgba(255, 121, 29, 0.32)",
            "brand-line": "rgba(255, 121, 29, 0.45)",
            "reaction-mine-line": "rgba(255, 121, 29, 0.55)",
            "flash": "rgba(255, 121, 29, 0.20)",
            "own-hairline": "transparent",
            "mn-bg": "rgba(255, 121, 29, 0.11)",
            "mn-bg-hover": "rgba(255, 121, 29, 0.18)",
            "mn-bg-own": "rgba(255, 121, 29, 0.16)",
            "announcement-pill": "rgba(255, 121, 29, 0.13)",
            "announcement-ring": "#EFB999",
            "thread-unread-line": "#F0B48F",
            "row-announcement": "#FDF5F0",
            "badge-quiet": "#F9E6DD",
            "current-line": "#ECC4AE",
            "focus-line": "#F3A97A",
            "composer-line": "#F4A470",
            "composer-glow": "rgba(255, 121, 29, 0.6)"
        },
        dark: {
            "brand-fill": "#CD4B00",
            "on-brand": white,
            "brand-ink": "#FFAC73",
            "brand-tint": "#503A35",
            "brand-wash": "#2F1C12",
            "brand-ring": "rgba(255, 121, 29, 0.32)",
            "brand-line": "rgba(255, 121, 29, 0.45)",
            "reaction-mine-line": "rgba(255, 121, 29, 0.55)",
            "flash": "rgba(255, 121, 29, 0.26)",
            "own-hairline": "rgba(255, 121, 29, 0.24)",
            "mn-bg": "rgba(255, 121, 29, 0.11)",
            "mn-bg-hover": "rgba(255, 121, 29, 0.18)",
            "mn-bg-own": "rgba(255, 121, 29, 0.16)",
            "announcement-pill": "rgba(255, 121, 29, 0.13)",
            "announcement-ring": "#7A4828",
            "thread-unread-line": "#854C27",
            "row-announcement": "#1C1717",
            "badge-quiet": "#2C1B12",
            "current-line": "#64402A",
            "focus-line": "#9C5425",
            "composer-line": "#A75824",
            "composer-glow": "rgba(255, 121, 29, 0.6)"
        }
    };

    // Forest separates a computation from Rock orange's values written in: every derived token
    // moves, and its fill is the darkened one while the tints still mix the church's own color.
    // Worked out with the design's mixes (ink 58% with black in oklab light, 62% with white in
    // srgb dark; tint 13% over the light surface in srgb, 24% over the dark bubble base in oklab;
    // wash 10% and 15% over the sidebar in srgb). The message row's mixes, all in srgb: a mention
    // chip at 11%, 18% on hover and 16% inside the person's own bubble, and the announcement pill
    // at 13%, each as alpha over whatever is under it; the announcement bubble's ring 40% over the
    // line (#E4E4EC, #22272F), and an unread thread's border 45% over it; Community's announcement row 6% over the surface (#FDFDFD,
    // #0D1117); a muted room's quiet badge 14% over the sidebar (#F8F8FC, #0A0C10); the open room's
    // icon box 30% and the focused search box 55% over the line; the focused composer's border 60% over
    // the line, and its glow the church color at 60% alpha. A pressed
    // pill's border is the church color at 45% alpha, over whatever is under it.
    const forest = {
        light: {
            "brand-fill": "#198751",
            "on-brand": white,
            "brand-ink": "#083F24",
            "brand-tint": "#E0EEE7",
            "brand-wash": "#E2EDEB",
            "brand-ring": "rgba(30, 138, 84, 0.32)",
            "brand-line": "rgba(30, 138, 84, 0.45)",
            "reaction-mine-line": "rgba(30, 138, 84, 0.55)",
            "flash": "rgba(30, 138, 84, 0.20)",
            "own-hairline": "transparent",
            "mn-bg": "rgba(30, 138, 84, 0.11)",
            "mn-bg-hover": "rgba(30, 138, 84, 0.18)",
            "mn-bg-own": "rgba(30, 138, 84, 0.16)",
            "announcement-pill": "rgba(30, 138, 84, 0.13)",
            "announcement-ring": "#95C0AF",
            "thread-unread-line": "#8BBCA8",
            "row-announcement": "#F0F6F3",
            "badge-quiet": "#D9E9E4",
            "current-line": "#A9C9BE",
            "focus-line": "#77B398",
            "composer-line": "#6DAE91",
            "composer-glow": "rgba(30, 138, 84, 0.6)"
        },
        dark: {
            "brand-fill": "#198751",
            "on-brand": white,
            "brand-ink": "#74B695",
            "brand-tint": "#233B38",
            "brand-wash": "#0D1F1A",
            "brand-ring": "rgba(30, 138, 84, 0.32)",
            "brand-line": "rgba(30, 138, 84, 0.45)",
            "reaction-mine-line": "rgba(30, 138, 84, 0.55)",
            "flash": "rgba(30, 138, 84, 0.26)",
            "own-hairline": "rgba(30, 138, 84, 0.24)",
            "mn-bg": "rgba(30, 138, 84, 0.11)",
            "mn-bg-hover": "rgba(30, 138, 84, 0.18)",
            "mn-bg-own": "rgba(30, 138, 84, 0.16)",
            "announcement-pill": "rgba(30, 138, 84, 0.13)",
            "announcement-ring": "#204F3E",
            "thread-unread-line": "#205440",
            "row-announcement": "#0E181B",
            "badge-quiet": "#0D1E1A",
            "current-line": "#21453A",
            "focus-line": "#205D43",
            "composer-line": "#206245",
            "composer-glow": "rgba(30, 138, 84, 0.6)"
        }
    };

    test.each([["Rock orange", "#FF791D", rockOrange], ["Forest", "#1E8A54", forest]])("%s, light and dark", (_name, church, expected) => {
        const tokens = brandTokens(church);

        for (const theme of ["light", "dark"] as const) {
            const want = expected[theme] as Record<string, string>;
            const got = tokens[theme] as Record<string, string>;
            expect(Object.keys(got).sort()).toEqual(Object.keys(want).sort());
            for (const name of Object.keys(want)) {
                expectColor(got[name], want[name], `${theme} ${name}`);
            }
        }
    });

    test("every token is set on the chat's root in its light and its dark form", () => {
        const root = document.createElement("div");
        const tokens = brandTokens("#1E8A54");

        applyBrand(root, "#1E8A54");

        for (const theme of ["light", "dark"] as const) {
            for (const [name, value] of Object.entries(tokens[theme] as Record<string, string>)) {
                expect({ property: `--chat-${name}-${theme}`, value: root.style.getPropertyValue(`--chat-${name}-${theme}`).trim() })
                    .toEqual({ property: `--chat-${name}-${theme}`, value });
            }
        }
    });
});
