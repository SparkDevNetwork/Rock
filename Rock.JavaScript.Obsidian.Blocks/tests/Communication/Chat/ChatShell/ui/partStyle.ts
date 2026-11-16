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
// A shared part's own styles, read as text. jsdom applies no scoped style, so what a part looks
// like is read from its file: each part takes every value from the token sheet, and its motions
// from the sheet's one set, which is what makes reduced motion and dark mode reach it.
import { readFileSync } from "fs";
import { resolve } from "path";

const uiFolder = resolve(__dirname, "../../../../../src/Communication/Chat/ChatShell/ui");

/** The keyframes the token sheet defines, which a part's animation may name. */
const sheetKeyframes = (): Set<string> => {
    const sheet = readFileSync(resolve(__dirname, "../../../../../../RockWeb/Styles/Blocks/Communication/Chat/chat.css"), "utf8");
    return new Set([...sheet.matchAll(/@keyframes\s+([\w-]+)/g)].map(match => match[1]));
};

/** A part's style blocks, comments removed, whitespace in selectors kept single. */
export function styleOf(file: string): string {
    const text = readFileSync(resolve(uiFolder, file), "utf8");

    return [...text.matchAll(/<style[^>]*>([\s\S]*?)<\/style>/g)]
        .map(match => match[1])
        .join("\n")
        .replace(/\/\*[\s\S]*?\*\//g, "");
}

/** The declarations of every rule whose selector list holds the selector exactly, joined. */
export function ruleOf(style: string, selector: string): string {
    const bodies: string[] = [];

    for (const match of style.matchAll(/([^{}]+)\{([^{}]*)\}/g)) {
        const selectors = match[1].split(",").map(one => one.trim().replace(/\s+/g, " "));
        if (selectors.includes(selector)) {
            bodies.push(match[2]);
        }
    }

    return bodies.join(";");
}

/** Each `@media` block of a style, its query and its rules, braces matched so nested rules stay whole. */
function mediaBlocks(style: string): Array<{ query: string, start: number, end: number, body: string }> {
    const blocks: Array<{ query: string, start: number, end: number, body: string }> = [];

    for (const match of style.matchAll(/@media\s*([^{]+)\{/g)) {
        const open = (match.index ?? 0) + match[0].length;
        let depth = 1;
        let at = open;
        while (at < style.length && depth > 0) {
            depth += style[at] === "{" ? 1 : style[at] === "}" ? -1 : 0;
            at++;
        }
        blocks.push({ query: match[1].trim(), start: match.index ?? 0, end: at, body: style.slice(open, at - 1) });
    }

    return blocks;
}

/** The rules inside every `@media` block of a style whose query is exactly the one given, joined. */
export function mediaOf(style: string, query: string): string {
    return mediaBlocks(style).filter(block => block.query === query).map(block => block.body).join("\n");
}

/** A style with every `@media` block taken out: the rules that hold on every screen. */
export function outsideMedia(style: string): string {
    let rest = style;
    for (const block of mediaBlocks(style).reverse()) {
        rest = rest.slice(0, block.start) + rest.slice(block.end);
    }

    return rest;
}

/**
 * What a part's styles may not hold: a keyframe of its own, a reduced-motion rule of its own
 * (the sheet's tokens and its one set carry reduced motion), a reach into another component, a
 * forced priority, capitals forced on its words, or an animation outside the sheet's set.
 */
export function partRuleBreaks(file: string): string[] {
    const style = styleOf(file);
    const breaks: string[] = [];
    const known = sheetKeyframes();

    if (/@keyframes/.test(style)) {
        breaks.push("keyframes of its own");
    }
    if (/prefers-reduced-motion/.test(style)) {
        breaks.push("a reduced-motion rule of its own");
    }
    if (/:deep\(|:global\(/.test(style)) {
        breaks.push("a reach into another component");
    }
    if (/!important/.test(style)) {
        breaks.push("!important");
    }
    if (/text-transform:\s*uppercase/.test(style)) {
        breaks.push("forced capitals");
    }
    // A decorative loop plays through the loop stop, var(--chat-loop-stop, its name); the name is the set's.
    for (const match of style.matchAll(/animation(?:-name)?:\s*(?:var\(--chat-loop-stop,\s*)?([\w-]+)/g)) {
        if (match[1] !== "none" && !known.has(match[1])) {
            breaks.push(`an animation outside the set: ${match[1]}`);
        }
    }
    for (const match of style.matchAll(/border(?:-[a-z]+)?:\s*([\d.]+)px solid/g)) {
        if (match[1] !== "1") {
            breaks.push(`a ${match[1]}px border, where a line is one pixel`);
        }
    }

    return breaks;
}
