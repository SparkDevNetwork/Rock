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
// uses it as a fill only where the text on it can be read: the color itself with dark ink when ink
// reads and beats white, else the color darkened in small steps until white reads. The tints and
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

const ink = "#0D1117";
const white = "#FFFFFF";

describe("the brand fill", () => {
    // The five swatches the design was drawn against. The expected fills are what the design's own
    // rule gives for each, run from the prototype: only Forest is too light for white as it stands,
    // and one 5% darkening step brings it to 4.76:1.
    const swatches: Array<[string, string, string, string]> = [
        ["Rock orange", "#FF791D", "#FF791D", ink],
        ["Ocean", "#1F6FEB", "#1F6FEB", white],
        ["Forest", "#1E8A54", "#1D8350", white],
        ["Plum", "#7A4FD1", "#7A4FD1", white],
        ["Crimson", "#C8283F", "#C8283F", white]
    ];

    test.each(swatches)("%s %s fills with %s and %s text", (_name, church, fill, onBrand) => {
        const result = brandFill(church);

        expectColor(result.fill, fill, "fill");
        expectColor(result.onBrand, onBrand, "on-brand");
    });

    test.each(swatches)("%s's fill and its text reach 4.5 to 1", (_name, church) => {
        const result = brandFill(church);

        expect(contrast(result.fill, result.onBrand)).toBeGreaterThanOrEqual(4.5);
    });

    test("Rock orange keeps itself because ink reads on it and white does not", () => {
        // 7.21 to 1 for ink against 2.62 for white: the case the rule's first branch exists for.
        expect(contrast("#FF791D", ink)).toBeGreaterThan(contrast("#FF791D", white));
        expectColor(brandFill("#FF791D").fill, "#FF791D", "fill");
    });

    test("a church color written in lower case gives the same fill", () => {
        expectColor(brandFill("#1e8a54").fill, "#1D8350", "fill");
        expectColor(brandFill("#1e8a54").onBrand, white, "on-brand");
    });
});

describe("the tokens drawn from the church color", () => {
    // The design's fallback column: what each token resolves to for Rock orange.
    const rockOrange = {
        light: {
            "brand-fill": "#FF791D",
            "on-brand": ink,
            "brand-ink": "#7A3608",
            "brand-tint": "#FDECE0",
            "brand-wash": "#F9EBE6",
            "brand-ring": "rgba(255, 121, 29, 0.32)",
            "flash": "rgba(255, 121, 29, 0.20)",
            "own-hairline": "transparent",
            "mn-bg": "rgba(255, 121, 29, 0.11)",
            "mn-bg-hover": "rgba(255, 121, 29, 0.18)",
            "mn-bg-own": "rgba(255, 121, 29, 0.16)",
            "announcement-pill": "rgba(255, 121, 29, 0.13)",
            "announcement-ring": "#EFB999",
            "row-announcement": "#FDF5F0",
            "badge-quiet": "#F9E6DD",
            "current-line": "#ECC4AE",
            "focus-line": "#F3A97A"
        },
        dark: {
            "brand-fill": "#FF791D",
            "on-brand": ink,
            "brand-ink": "#FFAC73",
            "brand-tint": "#503A35",
            "brand-wash": "#2F1C12",
            "brand-ring": "rgba(255, 121, 29, 0.32)",
            "flash": "rgba(255, 121, 29, 0.26)",
            "own-hairline": "rgba(255, 121, 29, 0.24)",
            "mn-bg": "rgba(255, 121, 29, 0.11)",
            "mn-bg-hover": "rgba(255, 121, 29, 0.18)",
            "mn-bg-own": "rgba(255, 121, 29, 0.16)",
            "announcement-pill": "rgba(255, 121, 29, 0.13)",
            "announcement-ring": "#7A4828",
            "row-announcement": "#1C1717",
            "badge-quiet": "#2C1B12",
            "current-line": "#64402A",
            "focus-line": "#9C5425"
        }
    };

    // Forest separates a computation from Rock orange's values written in: every derived token
    // moves, and its fill is the darkened one while the tints still mix the church's own color.
    // Worked out with the design's mixes (ink 58% with black in oklab light, 62% with white in
    // srgb dark; tint 13% over the light surface in srgb, 24% over the dark bubble base in oklab;
    // wash 10% and 15% over the sidebar in srgb). The message row's mixes, all in srgb: a mention
    // chip at 11%, 18% on hover and 16% inside the person's own bubble, and the announcement pill
    // at 13%, each as alpha over whatever is under it; the announcement bubble's ring 40% over the
    // line (#E4E4EC, #22272F); Community's announcement row 6% over the surface (#FDFDFD,
    // #0D1117); a muted room's quiet badge 14% over the sidebar (#F8F8FC, #0A0C10); the open room's
    // icon box 30% and the focused search box 55% over the line.
    const forest = {
        light: {
            "brand-fill": "#1D8350",
            "on-brand": white,
            "brand-ink": "#083F24",
            "brand-tint": "#E0EEE7",
            "brand-wash": "#E2EDEB",
            "brand-ring": "rgba(30, 138, 84, 0.32)",
            "flash": "rgba(30, 138, 84, 0.20)",
            "own-hairline": "transparent",
            "mn-bg": "rgba(30, 138, 84, 0.11)",
            "mn-bg-hover": "rgba(30, 138, 84, 0.18)",
            "mn-bg-own": "rgba(30, 138, 84, 0.16)",
            "announcement-pill": "rgba(30, 138, 84, 0.13)",
            "announcement-ring": "#95C0AF",
            "row-announcement": "#F0F6F3",
            "badge-quiet": "#D9E9E4",
            "current-line": "#A9C9BE",
            "focus-line": "#77B398"
        },
        dark: {
            "brand-fill": "#1D8350",
            "on-brand": white,
            "brand-ink": "#74B695",
            "brand-tint": "#233B38",
            "brand-wash": "#0D1F1A",
            "brand-ring": "rgba(30, 138, 84, 0.32)",
            "flash": "rgba(30, 138, 84, 0.26)",
            "own-hairline": "rgba(30, 138, 84, 0.24)",
            "mn-bg": "rgba(30, 138, 84, 0.11)",
            "mn-bg-hover": "rgba(30, 138, 84, 0.18)",
            "mn-bg-own": "rgba(30, 138, 84, 0.16)",
            "announcement-pill": "rgba(30, 138, 84, 0.13)",
            "announcement-ring": "#204F3E",
            "row-announcement": "#0E181B",
            "badge-quiet": "#0D1E1A",
            "current-line": "#21453A",
            "focus-line": "#205D43"
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
