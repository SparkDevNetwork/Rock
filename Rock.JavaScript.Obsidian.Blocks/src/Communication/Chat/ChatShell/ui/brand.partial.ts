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

// The chat's theme as worked out in script, once per page load: the church color's fill and the
// tints drawn from it, and whether the page's Rock theme has a dark mode. The browsers Rock supports
// have no color-mix, so the mixes the sheet would otherwise write are computed here and set on the
// chat's root, each in a light and a dark form the sheet picks between.

import { onScopeDispose, Ref, ref } from "vue";

/** The tokens drawn from the church color, by their name in the sheet without the prefix. */
export type BrandTokenSet = Record<"brand-fill" | "on-brand" | "brand-ink" | "brand-tint" | "brand-wash" | "brand-ring" | "brand-line" | "reaction-mine-line" | "flash" | "own-hairline"
    | "mn-bg" | "mn-bg-hover" | "mn-bg-own" | "announcement-pill" | "announcement-ring" | "thread-unread-line" | "row-announcement"
    | "current-line" | "focus-line" | "composer-line" | "composer-glow", string>;

/** The variable only Rock's newer themes define; the older themes have no dark mode. */
const rockThemeV2Probe = "--popup-box-shadow";

type Rgb = [number, number, number];

// #region Color arithmetic

// The colors the arithmetic starts from. The style gate allows a literal inside this region only.

/** Rock's own orange, used when the page states no primary color. */
const rockOrange = "#FF791D";
const white = "#FFFFFF";

// The backgrounds the tints are mixed over, as the sheet gives them: the light surface and
// sidebar, the dark sidebar, and the dark base own messages are tinted from.
const lightSurface = "#FDFDFD";
const lightSidebar = "#F8F8FC";
const darkSidebar = "#0A0C10";
const darkSurface = "#0D1117";
const lightLine = "#E4E4EC";
const darkLine = "#22272F";
const darkTintBase = "#1E242E";

/** Reads #rgb, #rrggbb or rgb()/rgba() into channels of 0 to 255, or null for anything else. */
function parseColor(color: string): Rgb | null {
    const value = color.trim().toLowerCase();

    const short = /^#([0-9a-f])([0-9a-f])([0-9a-f])$/.exec(value);
    if (short) {
        return [parseInt(short[1] + short[1], 16), parseInt(short[2] + short[2], 16), parseInt(short[3] + short[3], 16)];
    }

    const long = /^#([0-9a-f]{2})([0-9a-f]{2})([0-9a-f]{2})$/.exec(value);
    if (long) {
        return [parseInt(long[1], 16), parseInt(long[2], 16), parseInt(long[3], 16)];
    }

    const functional = /^rgba?\(\s*(\d+)[\s,]+(\d+)[\s,]+(\d+)/.exec(value);
    if (functional) {
        return [Number(functional[1]), Number(functional[2]), Number(functional[3])];
    }

    return null;
}

function toHex(rgb: Rgb): string {
    return "#" + rgb.map(c => Math.round(Math.min(255, Math.max(0, c))).toString(16).padStart(2, "0")).join("").toUpperCase();
}

function toLinear(channel: number): number {
    const c = channel / 255;
    return c <= 0.04045 ? c / 12.92 : Math.pow((c + 0.055) / 1.055, 2.4);
}

function fromLinear(c: number): number {
    return 255 * (c <= 0.0031308 ? 12.92 * c : 1.055 * Math.pow(c, 1 / 2.4) - 0.055);
}

/** WCAG contrast between two colors. */
function contrast(a: Rgb, b: Rgb): number {
    const luminance = (rgb: Rgb): number => {
        const [r, g, b2] = rgb.map(toLinear);
        return 0.2126 * r + 0.7152 * g + 0.0722 * b2;
    };
    const [high, low] = [luminance(a), luminance(b)].sort((x, y) => y - x);

    return (high + 0.05) / (low + 0.05);
}

/** The share of the first color mixed over the second, channel by channel in sRGB. */
function mixSrgb(a: Rgb, b: Rgb, share: number): Rgb {
    return a.map((c, i) => c * share + b[i] * (1 - share)) as Rgb;
}

function toOklab(rgb: Rgb): Rgb {
    const [r, g, b] = rgb.map(toLinear);
    const l = Math.cbrt(0.4122214708 * r + 0.5363325363 * g + 0.0514459929 * b);
    const m = Math.cbrt(0.2119034982 * r + 0.6806995451 * g + 0.1073969566 * b);
    const s = Math.cbrt(0.0883024619 * r + 0.2817188376 * g + 0.6299787005 * b);

    return [
        0.2104542553 * l + 0.7936177850 * m - 0.0040720468 * s,
        1.9779984951 * l - 2.4285922050 * m + 0.4505937099 * s,
        0.0259040371 * l + 0.7827717662 * m - 0.8086757660 * s
    ];
}

function fromOklab([lightness, a, b]: Rgb): Rgb {
    const l = Math.pow(lightness + 0.3963377774 * a + 0.2158037573 * b, 3);
    const m = Math.pow(lightness - 0.1055613458 * a - 0.0638541728 * b, 3);
    const s = Math.pow(lightness - 0.0894841775 * a - 1.2914855480 * b, 3);

    return [
        fromLinear(4.0767416621 * l - 3.3077115913 * m + 0.2309699292 * s),
        fromLinear(-1.2684380046 * l + 2.6097574011 * m - 0.3413193965 * s),
        fromLinear(-0.0041960863 * l - 0.7034186147 * m + 1.7076147010 * s)
    ];
}

/** The share of the first color mixed over the second in oklab, which keeps a dark mix from going muddy. */
function mixOklab(a: Rgb, b: Rgb, share: number): Rgb {
    const first = toOklab(a);
    const second = toOklab(b);

    return fromOklab(first.map((c, i) => c * share + second[i] * (1 - share)) as Rgb);
}

function withAlpha(rgb: Rgb, alpha: number): string {
    return `rgba(${rgb[0]}, ${rgb[1]}, ${rgb[2]}, ${alpha})`;
}

// #endregion Color arithmetic

// #region Brand

/**
 * The church color as a fill, and the text drawn on it. Text on the church color is always white,
 * so the fill is darkened in oklab lightness, 0.01 at a time, only until white reads on it at 4.5
 * to 1; a color that already reads is kept as it is. The hue and chroma stay the church's own.
 *
 * On a dark surface the fill is also a shape (the unread dot, a switch track, a solid button), and a
 * dark fill such as navy would vanish into it. With `against` given, a fill that reads under 3 to 1
 * on that surface is lightened, again in oklab and only as far as needed, while white text keeps
 * 4.5 to 1. Where both cannot hold for a color, white text wins and the shape stays as it is.
 *
 * @param color The church color, as hex or rgb().
 * @param against The surface the fill is drawn on, as hex, when the shape must stand out from it.
 *
 * @returns The fill and the text color, both as uppercase hex.
 */
export function brandFill(color: string, against?: string): { fill: string, onBrand: string } {
    const church = parseColor(color) ?? parseColor(rockOrange)!;
    const whiteRgb = parseColor(white)!;
    const [lightness, a, b] = toOklab(church);

    // Contrast is read on the rounded channels the sheet will draw, not on the exact oklab value.
    // The loop ends well before lightness 0, where the fill is black and white reads at 21 to 1.
    let base: Rgb = [0, 0, 0];
    let baseLightness = 0;

    for (let step = 0; step * 0.01 < lightness; step++) {
        const candidate = step === 0 ? church : fromOklab([lightness - step * 0.01, a, b]);
        const rounded = parseColor(toHex(candidate))!;

        if (contrast(rounded, whiteRgb) >= 4.5) {
            base = rounded;
            baseLightness = lightness - step * 0.01;
            break;
        }
    }

    if (against !== undefined) {
        const surface = parseColor(against)!;

        // Black has no chroma to carry, so the lift is a gray; from the darkened base it keeps the hue.
        const [, baseA, baseB] = toOklab(base);

        for (let step = 0; contrast(base, surface) < 3 && step < 100; step++) {
            const lifted = parseColor(toHex(fromOklab([baseLightness + (step + 1) * 0.01, baseA, baseB])))!;

            if (contrast(lifted, whiteRgb) < 4.5) {
                break;
            }

            base = lifted;
        }
    }

    return { fill: toHex(base), onBrand: white };
}

/**
 * Every token the sheet draws from the church color, light and dark. The tints mix the church's own
 * color, not the fill, so a darkened fill does not darken the washes around it.
 *
 * @param color The church color, as hex or rgb().
 *
 * @returns The light and the dark value of each token.
 */
export function brandTokens(color: string): { light: BrandTokenSet, dark: BrandTokenSet } {
    const church = parseColor(color) ?? parseColor(rockOrange)!;
    const { fill, onBrand } = brandFill(color);
    const darkFill = brandFill(color, darkSurface);
    const ring = withAlpha(church, 0.32);

    // A pressed pill's border, the church color at 45% over whatever is under it.
    const line = withAlpha(church, 0.45);

    // Your reaction chip's border, a little stronger, so a pressed chip reads on a tinted bubble.
    const reactionLine = withAlpha(church, 0.55);

    // The message row's tints. The chips and the pill are the church color over whatever lies
    // under them, a bubble or a tinted row, so they stay alpha; the rest sit on a known background.
    // An unread thread's border is drawn a little stronger than the announcement ring, 45% over the line.
    const row = (surface: string, line: string): Pick<BrandTokenSet, "mn-bg" | "mn-bg-hover" | "mn-bg-own" | "announcement-pill" | "announcement-ring" | "thread-unread-line" | "row-announcement"> => ({
        "mn-bg": withAlpha(church, 0.11),
        "mn-bg-hover": withAlpha(church, 0.18),
        "mn-bg-own": withAlpha(church, 0.16),
        "announcement-pill": withAlpha(church, 0.13),
        "announcement-ring": toHex(mixSrgb(church, parseColor(line)!, 0.40)),
        "thread-unread-line": toHex(mixSrgb(church, parseColor(line)!, 0.45)),
        "row-announcement": toHex(mixSrgb(church, parseColor(surface)!, 0.06))
    });

    // The lines in the church color: the open room's icon box at 30% over the line, the search
    // box's border while it has focus at 55%, and the composer's at 60% with a glow under it.
    const lines = (line: string): Pick<BrandTokenSet, "current-line" | "focus-line" | "composer-line" | "composer-glow"> => ({
        "current-line": toHex(mixSrgb(church, parseColor(line)!, 0.30)),
        "focus-line": toHex(mixSrgb(church, parseColor(line)!, 0.55)),
        "composer-line": toHex(mixSrgb(church, parseColor(line)!, 0.60)),
        "composer-glow": withAlpha(church, 0.6)
    });

    return {
        light: {
            "brand-fill": fill,
            "on-brand": onBrand,
            "brand-ink": toHex(mixOklab(church, [0, 0, 0], 0.58)),
            "brand-tint": toHex(mixSrgb(church, parseColor(lightSurface)!, 0.13)),
            "brand-wash": toHex(mixSrgb(church, parseColor(lightSidebar)!, 0.10)),
            "brand-ring": ring,
            "brand-line": line,
            "reaction-mine-line": reactionLine,
            "flash": withAlpha(church, 0.20),
            "own-hairline": "transparent",
            ...row(lightSurface, lightLine),
            ...lines(lightLine)
        },
        dark: {
            "brand-fill": darkFill.fill,
            "on-brand": darkFill.onBrand,
            "brand-ink": toHex(mixSrgb(church, [255, 255, 255], 0.62)),
            "brand-tint": toHex(mixOklab(church, parseColor(darkTintBase)!, 0.24)),
            "brand-wash": toHex(mixSrgb(church, parseColor(darkSidebar)!, 0.15)),
            "brand-ring": ring,
            "brand-line": line,
            "reaction-mine-line": reactionLine,
            "flash": withAlpha(church, 0.26),
            "own-hairline": withAlpha(church, 0.24),
            ...row(darkSurface, darkLine),
            ...lines(darkLine)
        }
    };
}

/**
 * Sets each brand token on the chat's root as `--chat-<name>-light` and `--chat-<name>-dark`.
 * The sheet picks one under whichever theme is in force, so a theme change needs no script.
 *
 * @param root The chat's root element.
 * @param color The church color.
 */
export function applyBrand(root: HTMLElement, color: string): void {
    const tokens = brandTokens(color);

    for (const theme of ["light", "dark"] as const) {
        for (const [name, value] of Object.entries(tokens[theme])) {
            root.style.setProperty(`--chat-${name}-${theme}`, value);
        }
    }
}

/**
 * A person's avatar hue, from the name the chat shows. The sidebar knows people only by name, so a
 * hue keyed on anything else would color one person two ways on one screen. A rename changes the
 * hue, which is accepted. Shared by the avatar and the small face inside a mention chip.
 *
 * @param name The name as the chat shows it.
 *
 * @returns A hue from 0 to 359.
 */
export function avatarHue(name: string): number {
    // FNV-1a: cheap, stable across loads, and spreads similar names apart.
    let hash = 2166136261;
    for (const character of name.trim()) {
        hash = Math.imul(hash ^ character.codePointAt(0)!, 16777619);
    }

    return (hash >>> 0) % 360;
}

// #endregion Brand

// #region The page's theme

/**
 * Applies the church color and the page's theme to the chat's root, once at mount. The church
 * color is Rock's primary color on the document root. The system's dark mode is followed only on a
 * Rock theme that has one: the older themes set no theme attribute and stay light, so the sheet
 * reads the system's dark only under the mark set here.
 *
 * @param root The chat's root element.
 */
export function applyChatTheme(root: HTMLElement): void {
    const page = getComputedStyle(document.documentElement);
    const primary = page.getPropertyValue("--color-primary").trim();

    applyBrand(root, parseColor(primary) ? primary : rockOrange);

    if (page.getPropertyValue(rockThemeV2Probe).trim() !== "") {
        root.setAttribute("data-rock-theme", "v2");
    }
    else {
        root.removeAttribute("data-rock-theme");
    }
}

/**
 * A duration the sheet names, in milliseconds, for a timer in script. The sheet holds every
 * timing so a component holds none; this falls back when the sheet is not on the page.
 *
 * @param element An element inside the chat.
 * @param token The token's full name, such as `--chat-d-tip-delay`.
 * @param fallbackMs The value to use when the token is not set.
 *
 * @returns The duration in milliseconds.
 */
export function tokenDuration(element: Element, token: string, fallbackMs: number): number {
    const value = getComputedStyle(element).getPropertyValue(token).trim();
    const match = /^(\d*\.?\d+)(ms|s)$/.exec(value);

    if (!match) {
        return fallbackMs;
    }

    return match[2] === "s" ? Number(match[1]) * 1000 : Number(match[1]);
}

// #endregion The page's theme

// #region The page's motion preference

/**
 * Whether the person asked the page for less motion, followed as it changes: the setting can be
 * turned on with the chat open, and what was drawn moving must go still at once. Called from a
 * component's setup, which is when the listener is ended.
 *
 * @returns A ref that is true while less motion is asked for.
 */
export function useIsMotionReduced(): Ref<boolean> {
    const query = window.matchMedia?.("(prefers-reduced-motion: reduce)");
    const isReduced = ref(query?.matches ?? false);
    const onChange = (event: { matches: boolean }): void => {
        isReduced.value = event.matches;
    };

    query?.addEventListener?.("change", onChange);
    onScopeDispose(() => query?.removeEventListener?.("change", onChange));
    return isReduced;
}

// #endregion The page's motion preference
