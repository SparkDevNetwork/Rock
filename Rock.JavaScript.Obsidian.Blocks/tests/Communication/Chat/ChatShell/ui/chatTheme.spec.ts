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
// The chat's token sheet, read as text. Every color, radius, shadow and timing a chat component
// draws with comes from this one sheet, so dark mode and any church's color work everywhere at
// once. The tokens are prefixed so a church theme's own variables cannot collide with them; each
// leans on Rock's own theme variable where Rock has one, with the chat's value behind it for a
// theme that lacks it; and the dark values apply under exactly the conditions Rock's own dark
// theme does: the theme attribute set to dark, or the system asking for dark when the page sets
// no theme or asks to follow the system.
import { readFileSync } from "fs";
import { resolve } from "path";

// A plain stylesheet the block links from RockWeb, so it loads with the page rather than with the script.
const sheetPath = resolve(__dirname, "../../../../../../RockWeb/Styles/Blocks/Communication/Chat/chat.css");
const blockPath = resolve(__dirname, "../../../../../../Rock.Blocks/Communication/Chat/ChatShell.cs");

// #region The sheet, parsed

type Rule = { selector: string, media: string, declarations: Map<string, string> };
type Sheet = { styleTags: string[], rules: Rule[], keyframes: string[] };

/** Splits at a character found outside brackets and quotes, so a comma or semicolon inside rgba() or a string does not split. */
function splitTopLevel(text: string, separator: string): string[] {
    const parts: string[] = [];
    let depth = 0;
    let quote = "";
    let start = 0;

    for (let i = 0; i < text.length; i++) {
        const c = text[i];
        if (quote) {
            if (c === quote) {
                quote = "";
            }
            continue;
        }
        if (c === "\"" || c === "'") {
            quote = c;
        }
        else if (c === "(" || c === "[") {
            depth++;
        }
        else if (c === ")" || c === "]") {
            depth--;
        }
        else if (c === separator && depth === 0) {
            parts.push(text.slice(start, i));
            start = i + 1;
        }
    }
    parts.push(text.slice(start));

    return parts.map(p => p.trim()).filter(Boolean);
}

/** Walks the brace blocks of a stylesheet, descending into media blocks and noting keyframe names. */
function walk(css: string, media: string, sheet: Sheet): void {
    let i = 0;
    while (i < css.length) {
        const open = css.indexOf("{", i);
        if (open < 0) {
            return;
        }

        let depth = 1;
        let close = open + 1;
        while (close < css.length && depth > 0) {
            if (css[close] === "{") {
                depth++;
            }
            else if (css[close] === "}") {
                depth--;
            }
            close++;
        }

        const prelude = css.slice(i, open).trim();
        const inner = css.slice(open + 1, close - 1);

        // A feature query is read as a condition of its own, as a media query is.
        if (prelude.startsWith("@media") || prelude.startsWith("@supports")) {
            walk(inner, normalize(prelude), sheet);
        }
        else if (/^@(-webkit-)?keyframes/.test(prelude)) {
            sheet.keyframes.push(prelude.replace(/^@(-webkit-)?keyframes\s+/, "").trim());
        }
        else if (!prelude.startsWith("@")) {
            const declarations = new Map<string, string>();
            for (const declaration of splitTopLevel(inner, ";")) {
                const colon = declaration.indexOf(":");
                if (colon > 0) {
                    declarations.set(declaration.slice(0, colon).trim(), declaration.slice(colon + 1).trim());
                }
            }
            sheet.rules.push({ selector: prelude, media, declarations });
        }

        i = close;
    }
}

function readSheet(path = sheetPath): Sheet {
    const source = readFileSync(path, "utf8");
    const sheet: Sheet = { styleTags: [], rules: [], keyframes: [] };
    const styleBlock = /<style([^>]*)>([\s\S]*?)<\/style>/g;
    let match: RegExpExecArray | null;

    // A stylesheet is one unscoped block of its own, read whole.
    if (path.endsWith(".css")) {
        sheet.styleTags.push("");
        walk(source.replace(/\/\*[\s\S]*?\*\//g, ""), "", sheet);
        return sheet;
    }

    while ((match = styleBlock.exec(source)) !== null) {
        sheet.styleTags.push(match[1]);
        walk(match[2].replace(/\/\*[\s\S]*?\*\//g, ""), "", sheet);
    }

    return sheet;
}

/** Compares values as CSS reads them: case, spacing and a leading zero before a decimal point do not matter. */
function normalize(value: string): string {
    return value.toLowerCase().replace(/\s+/g, "").replace(/(^|[^0-9.])0\./g, "$1.").replace(/"/g, "'");
}

const isDarkExplicit = (rule: Rule): boolean => rule.media === "" && /\[theme=["']?dark["']?\]/.test(rule.selector);
// The system's dark applies to a page that sets no theme and to one that asks to follow the system;
// Rock writes both in one rule, and the sheet may split them, so each is looked for on its own.
const isDarkUnthemed = (rule: Rule): boolean => rule.media.includes("prefers-color-scheme:dark") && rule.selector.includes(":not([theme])");
const isDarkFollowingSystem = (rule: Rule): boolean => rule.media.includes("prefers-color-scheme:dark") && /\[theme=["']?system["']?\]/.test(rule.selector);
const isLight = (rule: Rule): boolean => rule.media === "" && !rule.selector.includes("theme");
const isReducedMotion = (rule: Rule): boolean => rule.media.includes("prefers-reduced-motion:reduce");

/** Every value the sheet gives one custom property in the rules a condition picks out. */
function valuesOf(sheet: Sheet, property: string, where: (rule: Rule) => boolean): string[] {
    return sheet.rules.filter(where).filter(r => r.declarations.has(property)).map(r => normalize(r.declarations.get(property)!));
}

// #endregion The sheet, parsed

// #region The tokens, as data

/**
 * One token: its light value, its dark value where it differs, the Rock variable it leans on, and
 * for the church color's tokens the name the script sets for each theme. A value written here is
 * what the sheet's declaration must contain, as the value itself or as the fallback behind a variable.
 */
type Token = { name: string, light: string, dark?: string, rock?: string, darkRock?: string, computed?: boolean };

const tokens: Token[] = [
    // The church color and what is drawn from it. The script works these out once per load and
    // sets a light and a dark form of each; the sheet picks one and holds Rock orange's behind it.
    { name: "brand", light: "#FF791D", rock: "color-primary" },
    { name: "brand-fill", light: "#FF791D", computed: true },
    { name: "on-brand", light: "#0D1117", computed: true },
    { name: "brand-ink", light: "#7A3608", dark: "#FFAC73", computed: true },
    { name: "brand-tint", light: "#FDECE0", dark: "#503A35", computed: true },
    { name: "brand-wash", light: "#F9EBE6", dark: "#2F1C12", computed: true },
    { name: "brand-ring", light: "rgba(255,121,29,.32)", computed: true },
    { name: "own-hairline", light: "transparent", dark: "rgba(255,121,29,.24)", computed: true },
    { name: "flash", light: "rgba(255,121,29,.20)", dark: "rgba(255,121,29,.26)", computed: true },
    { name: "intro-k", light: "8%", dark: "13%" },

    // Surfaces, text and lines.
    { name: "surface", light: "#fdfdfd", dark: "#0D1117", rock: "color-interface-softest" },
    { name: "sidebar", light: "#F8F8FC", dark: "#0a0c10", rock: "color-interface-softer" },
    { name: "raised", light: "#ffffff", dark: "#161B22" },
    { name: "bubble", light: "#F0F0F5", dark: "#1A1F27" },
    { name: "text", light: "#0D1117", dark: "#F8F8FC", rock: "color-interface-stronger" },
    { name: "text-2", light: "#3D444D", dark: "#D9D9E3", rock: "color-interface-strong" },
    { name: "muted", light: "#66667E", dark: "#9C9CB6" },
    { name: "icon", light: "#7A7A94", dark: "#8B8BA7", darkRock: "color-interface-medium" },
    { name: "line", light: "#E4E4EC", dark: "#22272F" },
    { name: "line-strong", light: "#D9D9E3", dark: "#30363D", rock: "color-interface-soft" },
    { name: "hover", light: "rgba(13,17,23,.045)", dark: "rgba(248,248,252,.05)" },
    { name: "press", light: "rgba(13,17,23,.08)", dark: "rgba(248,248,252,.09)" },
    { name: "tip-bg", light: "#1B2029", dark: "#F8F8FC" },
    { name: "tip-fg", light: "#F8F8FC", dark: "#0D1117" },
    { name: "scrim", light: "rgba(13,17,23,.38)", dark: "rgba(0,0,0,.55)" },

    // Status colors. The soft fills are written out, since the browsers Rock supports cannot mix them.
    { name: "danger", light: "#D70015", rock: "base-danger" },
    { name: "danger-ink", light: "#B8000F", dark: "#FF7A85" },
    { name: "danger-soft", light: "#FAE9EA", dark: "#310E17" },
    { name: "warning", light: "#E58600", rock: "base-warning" },
    { name: "warning-ink", light: "#9A5A00", dark: "#FFB547" },
    { name: "warning-soft", light: "#FAEFDF", dark: "#302413" },
    { name: "info", light: "#005DD7", rock: "base-info" },
    { name: "info-ink", light: "#0049A8", dark: "#7DB3FF" },
    { name: "info-soft", light: "#E6EFFA", dark: "#0A203D" },
    { name: "success", light: "#01A11E", rock: "base-success" },
    { name: "mn-me-bg", light: "#F7E0C0", dark: "#4E3410" },
    { name: "mn-all-bg", light: "#DFEAF8", dark: "#0A2241" },

    // An initials avatar's saturation and lightness; the hue is the person's own.
    { name: "av-sat", light: "70%", dark: "28%" },
    { name: "av-l", light: "90%", dark: "23%" },
    { name: "av-tsat", light: "42%", dark: "75%" },
    { name: "av-tl", light: "31%", dark: "82%" },

    // Radii.
    { name: "r-xs", light: "4px", rock: "rounded-xsmall" },
    { name: "r-sm", light: "6px", rock: "rounded-small" },
    { name: "r-md", light: "8px", rock: "rounded-medium" },
    { name: "r-lg", light: "12px", rock: "rounded-large" },
    { name: "r-xl", light: "16px", rock: "rounded-xlarge" },
    { name: "r-2xl", light: "20px" },
    { name: "r-full", light: "9999px", rock: "rounded-full" },

    // Type.
    { name: "font", light: "\"Segoe UI\", -apple-system, BlinkMacSystemFont, Helvetica, Arial, sans-serif", rock: "font-family-body" },

    // Shadows.
    {
        name: "shadow-card",
        light: "0 0 2px rgba(145,158,171,.2), 0 12px 24px -4px rgba(145,158,171,.12)",
        dark: "0 0 0 1px rgba(255,255,255,.04), 0 12px 24px -4px rgba(0,0,0,.5)",
        rock: "box-shadow"
    },
    {
        name: "shadow-pop",
        light: "0 0 0 1px rgba(13,17,23,.06), 0 4px 8px rgba(13,17,23,.04), 0 16px 36px -8px rgba(30,36,52,.24)",
        dark: "0 0 0 1px rgba(255,255,255,.08), 0 16px 40px -8px rgba(0,0,0,.7)"
    },
    {
        name: "shadow-bar",
        light: "0 0 0 1px var(--chat-line), 0 1px 2px rgba(13,17,23,.05), 0 6px 14px -4px rgba(30,36,52,.16)",
        dark: "0 0 0 1px rgba(255,255,255,.08), 0 8px 18px -6px rgba(0,0,0,.65)"
    },
    {
        name: "shadow-frame",
        light: "0 1px 2px rgba(13,17,23,.04), 0 24px 48px -16px rgba(60,66,90,.22)",
        dark: "0 0 0 1px rgba(255,255,255,.05), 0 24px 60px -16px rgba(0,0,0,.8)"
    },

    // Motion.
    { name: "ease-in", light: "cubic-bezier(.1,.9,.2,1)" },
    { name: "ease-out", light: "cubic-bezier(.4,0,1,1)" },
    { name: "ease-state", light: "cubic-bezier(.42,0,.58,1)" },
    { name: "ease-spring", light: "cubic-bezier(.3,1.45,.55,1)" },
    { name: "t-fast", light: "120ms" },
    { name: "t-base", light: "180ms" },
    { name: "t-msg", light: "260ms" },
    { name: "t-panel", light: "300ms" },
    { name: "rise", light: "10px" },
    { name: "msg-scale", light: ".985" },
    { name: "pop-scale", light: ".96" },
    { name: "react-from", light: ".55" },
    { name: "drawer-shift", light: "28px" }
];

/** What reduced motion changes: no lift, no scale, no slide, and shorter entrances. */
const reducedMotion: Array<[string, string]> = [
    ["rise", "0px"],
    ["drawer-shift", "0px"],
    ["msg-scale", "1"],
    ["pop-scale", "1"],
    ["react-from", "1"],
    ["t-msg", "140ms"],
    ["t-panel", "160ms"]
];

/**
 * The values the design writes once in a component's place: radii, small shadows, and every
 * duration in its motion table. A component may hold none of them, so each is named in the sheet.
 * A reduced value is what the sheet sets when reduced motion is asked for: 0ms where the design
 * says the motion is skipped or a stagger drops away.
 */
type Named = { name: string, value: string, reduced?: string };

const namedLiterals: Named[] = [
    // Radii.
    { name: "r-bubble", value: "20px" },
    { name: "r-tail", value: "6px" },
    { name: "r-pop", value: "14px" },
    { name: "r-item", value: "9px" },
    { name: "r-dialog", value: "18px" },
    { name: "r-sheet", value: "22px" },
    { name: "r-composer", value: "22px" },
    { name: "r-editor", value: "18px" },
    { name: "r-quote-top", value: "14px" },
    { name: "r-quote-bottom", value: "6px" },
    { name: "r-quote-ghost", value: "16px" },
    { name: "r-bar", value: "18px" },
    { name: "r-room-ic", value: "11px" },
    { name: "r-hdr-ic", value: "12px" },
    { name: "r-org", value: "10px" },
    { name: "r-search", value: "10px" },
    { name: "r-option", value: "10px" },
    { name: "r-intro", value: "14px" },
    { name: "r-details", value: "16px" },
    { name: "r-radio-ic", value: "10px" },
    { name: "r-empty-sm", value: "14px" },
    { name: "r-empty", value: "16px" },
    { name: "r-tip", value: "8px" },

    // Small shadows.
    { name: "shadow-day", value: "0 2px 10px -4px rgba(13,17,23,.18)" },
    { name: "shadow-strip", value: "0 6px 12px -10px rgba(13,17,23,.25)" },
    { name: "shadow-composer", value: "0 1px 2px rgba(13,17,23,.04)" },
    { name: "shadow-knob", value: "0 1px 2px rgba(13,17,23,.25)" },
    { name: "shadow-thumb", value: "0 0 0 1px var(--chat-line), 0 1px 2px rgba(13,17,23,.08)" },

    // Durations.
    { name: "d-flash", value: "2.2s" },
    { name: "d-row-flash", value: "1.2s" },
    { name: "d-rx-pop", value: "340ms" },
    { name: "d-roll", value: "220ms" },
    { name: "d-badge-in", value: "260ms" },
    { name: "d-pop-remove", value: "130ms" },
    { name: "d-tools-show", value: "90ms" },
    { name: "d-tools-move", value: "140ms" },
    { name: "d-tools-hide", value: "80ms" },
    { name: "d-hover-rest", value: "60ms" },
    { name: "d-hover-grace", value: "220ms" },
    { name: "d-glyph", value: "140ms" },
    { name: "d-glyph-press", value: "60ms" },
    { name: "d-dialog-in", value: "240ms" },
    { name: "d-toast-in", value: "240ms" },
    { name: "d-toast-out", value: "160ms" },
    { name: "d-toast-life", value: "2.6s" },
    { name: "d-view", value: "220ms", reduced: "0ms" },
    { name: "d-tab-thumb", value: "240ms", reduced: "0ms" },
    { name: "d-stagger", value: "26ms", reduced: "0ms" },
    { name: "d-row-collapse", value: "200ms", reduced: "0ms" },
    { name: "d-fd-pop", value: "420ms", reduced: "0ms" },
    { name: "d-saved-hold", value: "1.6s" },
    { name: "d-sent-hold", value: "2.2s" },
    { name: "d-divider-fade", value: "600ms" },
    { name: "d-read-fade", value: "400ms" },
    { name: "d-pulse", value: "1.2s" },
    { name: "d-sweep", value: "1.1s" },
    { name: "d-spin", value: ".7s" },
    { name: "d-refresh-spin", value: ".8s" },
    { name: "d-skeleton", value: "1.6s" },
    { name: "d-tip-delay", value: "500ms" },
    { name: "d-tip-delay-rx", value: "350ms" },
    { name: "d-search-people", value: "250ms" },
    { name: "d-search-channels", value: "200ms" },
    { name: "d-lookup-line", value: "300ms" }
];

// #endregion The tokens, as data

describe("the chat token sheet", () => {
    let sheet: Sheet;

    beforeAll(() => {
        sheet = readSheet();
    });

    // A stylesheet reaches the page only when the block links it, so the link is what proves the
    // tokens reach every chat component. The fingerprint makes a browser fetch a changed sheet.
    test("is linked by the Chat block, fingerprinted", () => {
        const block = readFileSync(blockPath, "utf8");

        expect(block).toMatch(/AddCssLink\(\s*RequestContext\.ResolveRockUrl\(\s*"~\/Styles\/Blocks\/Communication\/Chat\/chat\.css"\s*\),\s*true\s*\)/);
    });

    test.each(tokens.map(t => [t.name, t] as const))("declares --chat-%s with its light value", (_name, token) => {
        const values = valuesOf(sheet, `--chat-${token.name}`, isLight);

        expect(values.length).toBeGreaterThan(0);
        expect(values.some(v => v.includes(normalize(token.light)))).toBe(true);
        if (token.rock) {
            expect(values.some(v => v.includes(`var(--${token.rock},`))).toBe(true);
        }
        if (token.computed) {
            expect(values.some(v => v.includes(`var(--chat-${token.name}-light,`))).toBe(true);
        }
    });

    const themed = tokens.filter(t => t.dark !== undefined);

    test.each(themed.map(t => [t.name, t] as const))("gives --chat-%s its dark value when the theme attribute is dark", (_name, token) => {
        const values = valuesOf(sheet, `--chat-${token.name}`, isDarkExplicit);

        expect(values.some(v => v.includes(normalize(token.dark!)))).toBe(true);
        if (token.darkRock) {
            expect(values.some(v => v.includes(`var(--${token.darkRock},`))).toBe(true);
        }
        if (token.computed) {
            expect(values.some(v => v.includes(`var(--chat-${token.name}-dark,`))).toBe(true);
        }
    });

    test.each(themed.map(t => [t.name, t] as const))("gives --chat-%s its dark value when the system asks for dark and the page follows it", (_name, token) => {
        for (const condition of [isDarkUnthemed, isDarkFollowingSystem]) {
            const values = valuesOf(sheet, `--chat-${token.name}`, condition);

            expect(values.some(v => v.includes(normalize(token.dark!)))).toBe(true);
            if (token.computed) {
                expect(values.some(v => v.includes(`var(--chat-${token.name}-dark,`))).toBe(true);
            }
        }
    });

    test.each(reducedMotion)("sets --chat-%s to %s when reduced motion is asked for", (name, value) => {
        const values = valuesOf(sheet, `--chat-${name}`, isReducedMotion);

        expect(values).toContain(normalize(value));
    });

    test("leaves the shared durations alone under reduced motion, since hovers and fades still help", () => {
        expect(valuesOf(sheet, "--chat-t-fast", isReducedMotion).every(v => v === "120ms")).toBe(true);
        expect(valuesOf(sheet, "--chat-t-base", isReducedMotion).every(v => v === "180ms")).toBe(true);
    });

    test("outlines a focused control 2px in brand ink at a 2px offset, which reads on white for any church color", () => {
        const focus = sheet.rules.filter(r => r.selector.includes(".chat-shell") && r.selector.includes(":focus-visible") && r.declarations.has("outline"));

        expect(focus.length).toBeGreaterThan(0);
        const outline = normalize(focus[0].declarations.get("outline")!);
        expect(outline).toContain("2px");
        expect(outline).toContain("solid");
        expect(outline).toContain("var(--chat-brand-ink");
        expect(normalize(focus[0].declarations.get("outline-offset") ?? "")).toBe("2px");
    });

    test("outlines focus on :focus too, for the oldest Safari Rock supports, which has no :focus-visible", () => {
        // Safari before 15.4 ignores a rule it cannot read, so the outline on :focus-visible alone
        // never shows there. Browsers that can tell a keyboard focus apart take the outline back
        // from a focus a click made.
        const onFocus = sheet.rules.find(r => r.media === "" && r.selector.includes(".chat-shell") && /:focus(?![-\w])/.test(r.selector)
            && !r.selector.includes(":not(") && r.declarations.has("outline"));
        const takenBack = sheet.rules.find(r => r.media.includes("@supportsselector(:focus-visible)") && r.selector.includes(".chat-shell")
            && r.selector.includes(":focus:not(:focus-visible)"));

        expect(normalize(onFocus?.declarations.get("outline") ?? "")).toContain("var(--chat-brand-ink");
        expect(normalize(onFocus?.declarations.get("outline-offset") ?? "")).toBe("2px");
        expect(normalize(takenBack?.declarations.get("outline") ?? "")).toMatch(/^(none|0)$/);
    });

    test("resets box sizing under the chat's root, so Rock's Bootstrap cannot change how a box measures", () => {
        const reset = sheet.rules.filter(r => r.selector.includes(".chat-shell") && normalize(r.declarations.get("box-sizing") ?? "") === "border-box");

        expect(reset.length).toBeGreaterThan(0);
    });

    test("names every animation with the chat prefix, since Rock's own global animation names would otherwise apply", () => {
        expect(sheet.keyframes.length).toBeGreaterThan(0);
        for (const name of sheet.keyframes) {
            expect(name).toMatch(/^chat-/);
        }
    });

    test("styles nothing outside the chat: every rule that sets a real property sits under a chat class", () => {
        // A rule that only declares tokens may sit on the page's root, where a theme condition reads;
        // anything else in an unscoped sheet would restyle Rock's own page.
        const leaking = sheet.rules
            .filter(r => [...r.declarations.keys()].some(property => !property.startsWith("--")))
            .flatMap(r => splitTopLevel(r.selector, ","))
            .filter(part => !part.includes(".chat-"));

        expect(leaking).toEqual([]);
    });
    test.each(namedLiterals.map(n => [n.name, n.value, n] as const))("names --chat-%s as %s, so no component writes it", (_name, _value, named) => {
        const values = valuesOf(sheet, `--chat-${named.name}`, isLight);

        expect(values).toContain(normalize(named.value));
    });

    test.each(namedLiterals.filter(n => n.reduced !== undefined).map(n => [n.name, n.reduced!] as const))("sets --chat-%s to %s when reduced motion is asked for", (name, value) => {
        expect(valuesOf(sheet, `--chat-${name}`, isReducedMotion)).toContain(normalize(value));
    });

    test("follows the system into dark only on a Rock theme that has a dark mode", () => {
        // Rock's older themes set no theme attribute and have no dark mode, so a page on one of them
        // matches "no theme attribute" while staying light. The block marks its root when it finds
        // the newer theme's variables, and the system's dark is read only under that mark.
        const systemDark = sheet.rules.filter(r => r.media.includes("prefers-color-scheme:dark") && [...r.declarations.keys()].some(p => p.startsWith("--chat-")));
        const unmarked = systemDark.flatMap(r => splitTopLevel(r.selector, ",")).filter(part => !/\[data-rock-theme=["']?v2["']?\]/.test(part));

        expect(systemDark.length).toBeGreaterThan(0);
        expect(unmarked).toEqual([]);
    });

    test("turns dark whenever the theme attribute says dark, whatever theme the page uses", () => {
        // A page that sets the attribute has asked for dark outright, so the mark is not needed.
        const explicit = sheet.rules.filter(isDarkExplicit).flatMap(r => splitTopLevel(r.selector, ",")).filter(part => /\[theme=["']?dark["']?\]/.test(part));

        expect(explicit.some(part => !part.includes("data-rock-theme"))).toBe(true);
    });
});

// The components' own style blocks, read as text: what jsdom cannot lay out or animate is checked
// as the rules that do it.
describe("the components' own styles", () => {
    const components = resolve(__dirname, "../../../../../src/Communication/Chat/ChatShell/components");
    const feedPath = resolve(components, "messageFeed.partial.obs");
    const rowPath = resolve(components, "messageRow.partial.obs");

    /** The body of a keyframes block in a component, by its name. */
    function keyframesBody(path: string, name: string): string | null {
        const source = readFileSync(path, "utf8");
        const start = source.search(new RegExp(`@keyframes\\s+${name}\\s*\\{`));
        if (start < 0) {
            return null;
        }

        let depth = 0;
        for (let i = source.indexOf("{", start); i < source.length; i++) {
            depth += source[i] === "{" ? 1 : source[i] === "}" ? -1 : 0;
            if (depth === 0) {
                return source.slice(start, i + 1);
            }
        }

        return null;
    }

    /** Each selector of a rule on its own, its spacing collapsed, beside the rule it came from. */
    function selectorsOf(sheet: Sheet): { selector: string, rule: Rule }[] {
        return sheet.rules.flatMap(rule => splitTopLevel(rule.selector, ",").map(selector => ({ selector: selector.replace(/\s+/g, " "), rule })));
    }

    // An own message grows from the corner it is anchored to: a bubble from its bottom right, by
    // the sender's side; Community's full-width row from its bottom left, where its text starts. A
    // row-wide origin on the right would slide the text sideways as it scales.
    test("an own message rises from its own corner in each look", () => {
        const origins = selectorsOf(readSheet(rowPath))
            .filter(({ selector, rule }) => rule.media === "" && selector.includes(".is-arriving") && rule.declarations.has("transform-origin"))
            .map(({ selector, rule }) => ({ selector, origin: normalize(rule.declarations.get("transform-origin") ?? "") }));

        expect(origins.filter(o => !o.selector.includes(".is-conversational") && !o.selector.includes(".is-community"))).toEqual([]);
        expect(origins.filter(o => o.selector.includes(".is-conversational")).map(o => o.origin)).toEqual(["bottomright"]);
        expect(origins.filter(o => o.selector.includes(".is-community")).map(o => o.origin)).toEqual(["0100%"]);
        expect(origins.every(o => o.selector.includes(".is-own"))).toBe(true);
    });

    // A send shows at reduced strength while it is on its way and eases to full as it settles,
    // rather than snapping, in the bubble and in Community's text alike.
    test.each([".chat-bubble", ".chat-cm-text"])("%s eases its strength as a send settles", selector => {
        const rule = selectorsOf(readSheet(rowPath)).find(s => s.rule.media === "" && s.selector === selector)?.rule;
        const transitions = splitTopLevel(rule?.declarations.get("transition") ?? "", ",").map(normalize);

        expect(transitions).toContain(normalize("opacity var(--chat-t-base) var(--chat-ease-state)"));
    });

    test("a short conversation sits at the bottom of the feed, by the composer", () => {
        const feed = readSheet(feedPath);
        const spacer = feed.rules.find(r => r.media === "" && r.selector === ".chat-feed::before");

        expect(normalize(spacer?.declarations.get("flex") ?? "")).toMatch(/^1/);
        expect(spacer?.declarations.has("content")).toBe(true);
    });

    test("the sticky day pill steps to its place under the unread strip rather than animating top", () => {
        const feed = readSheet(feedPath);
        const moving = feed.rules.filter(r => r.selector.includes("chat-day-pill") && /(^|,|\s)top(\s|$)/.test(r.declarations.get("transition") ?? ""));

        expect(moving).toEqual([]);
    });

    /** The transitions a component's rule for a selector declares outside any media query, each normalized. */
    function transitionsOf(path: string, selector: string, media = ""): string[] {
        const rule = selectorsOf(readSheet(path)).find(s => s.rule.media === media && s.selector === selector)?.rule;
        return splitTopLevel(rule?.declarations.get("transition") ?? "", ",").map(normalize);
    }

    /** A declaration a component's rule for a selector makes under a condition, normalized. */
    function declarationOf(path: string, selector: string, property: string, where: (rule: Rule) => boolean = r => r.media === ""): string {
        const rule = selectorsOf(readSheet(path)).find(s => where(s.rule) && s.selector === selector)?.rule;
        return normalize(rule?.declarations.get(property) ?? "");
    }

    // The unread strip and Jump to latest come and go as the design does: the opacity on the
    // state curve, the movement on the entrance curve, both over t-base, and leaving the way they
    // came rather than vanishing. Each is always drawn and switched by its on state, so the same
    // transition runs both ways; it is set out of sight only once its fade has ended.
    test.each([
        ["the unread strip", ".chat-unread-strip", "unread-strip", /chat-strip-in|chat-strip-fade/],
        ["Jump to latest", ".chat-jump", "jump-latest", /chat-jump-in/]
    ])("%s fades and moves in and out, rather than appearing and vanishing", (_name, selector, testId, entranceOnly) => {
        const source = readFileSync(feedPath, "utf8");
        // The element's opening tag, read past a ">" inside a quoted binding.
        const element = new RegExp(`<[a-z]+(?:[^>"]|"[^"]*")*?data-testid="${testId}"(?:[^>"]|"[^"]*")*>`).exec(source)?.[0] ?? "";
        expect(element).not.toBe("");
        expect(element).not.toContain("v-show");
        expect(element).not.toContain("v-if");

        const transitions = transitionsOf(feedPath, selector);
        expect(transitions).toContain(normalize("opacity var(--chat-t-base) var(--chat-ease-state)"));
        expect(transitions).toContain(normalize("transform var(--chat-t-base) var(--chat-ease-in)"));
        // Visible from the first moment of the way in and to the last of the way out.
        expect(transitions).toContain(normalize("visibility var(--chat-t-base) linear"));
        expect(declarationOf(feedPath, selector, "opacity")).toBe("0");
        expect(declarationOf(feedPath, selector, "visibility")).toBe("hidden");

        expect(declarationOf(feedPath, `${selector}.is-on`, "opacity")).toBe("1");
        expect(declarationOf(feedPath, `${selector}.is-on`, "visibility")).toBe("visible");
        // No entrance-only animation is left to play beside the transition.
        expect(source).not.toMatch(entranceOnly);
    });

    test("the unread strip comes from 4px above and Jump to latest from the rise below at the pop scale", () => {
        expect(declarationOf(feedPath, ".chat-unread-strip", "transform")).toBe(normalize("translateY(-4px)"));
        expect(declarationOf(feedPath, ".chat-unread-strip.is-on", "transform")).toBe("none");
        expect(declarationOf(feedPath, ".chat-jump", "transform")).toBe(normalize("translate(-50%, var(--chat-rise)) scale(var(--chat-pop-scale))"));
        expect(declarationOf(feedPath, ".chat-jump.is-on", "transform")).toBe(normalize("translate(-50%, 0)"));
    });

    test("under reduced motion the unread strip fades in and out without sliding", () => {
        expect(declarationOf(feedPath, ".chat-unread-strip", "transform", isReducedMotion)).toBe("none");
    });

    // A conversation opened in place of another rises 6px into view over the view time on the
    // entrance curve; under reduced motion a switch is not animated at all.
    test("a conversation switch enters its feed from 6px below, and not at all under reduced motion", () => {
        const animation = declarationOf(feedPath, ".chat-feed.is-entering", "animation");
        expect(animation).toContain(normalize("var(--chat-d-view) var(--chat-ease-in)"));

        const name = animation.split(/\s|var\(/)[0];
        const frames = normalize(keyframesBody(feedPath, name) ?? "");
        expect(frames).toContain("opacity:0");
        expect(frames).toContain(normalize("transform: translateY(6px)"));

        expect(declarationOf(feedPath, ".chat-feed.is-entering", "animation", isReducedMotion)).toBe("none");
    });

    test("a block image keeps its 16 to 7 shape in a browser without aspect-ratio, by a padding box", () => {
        const row = readSheet(rowPath);
        const fallback = row.rules.filter(r => r.media.includes("@supportsnot(aspect-ratio"));
        const box = fallback.find(r => /chat-block-image\)?$/.test(r.selector));
        const image = fallback.find(r => r.selector.includes("chat-block-image img"));

        expect(normalize(box?.declarations.get("padding-top") ?? "")).toBe("43.75%");
        expect(normalize(image?.declarations.get("position") ?? "")).toBe("absolute");
    });
});
