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
// The chat's text boxes, read as text. A box is a wrapper that draws the focus ring and a bare
// input inside it that draws nothing, and the chat sheet owns both. Rock's page styles give every
// text input a border and glow of its own while it is focused, at a weight the chat's own rules
// for one input tie with, so each input drew a second ring inside the wrapper's. The sheet's rule
// outweighs Rock's, and this spec proves every text input in the chat is drawn through it.
import { readdirSync, readFileSync, statSync } from "fs";
import { join, resolve } from "path";

const sheetPath = resolve(__dirname, "../../../../../../RockWeb/Styles/Blocks/Communication/Chat/chat.css");
const shellFolder = resolve(__dirname, "../../../../../src/Communication/Chat/ChatShell");

/** Every .obs file under a folder. */
function obsFiles(folder: string): string[] {
    return readdirSync(folder).flatMap(name => {
        const path = join(folder, name);
        return statSync(path).isDirectory() ? obsFiles(path) : name.endsWith(".obs") ? [path] : [];
    });
}

/** The body of the sheet's rule that names this selector, alone or first in a list, or null when it has none. */
function ruleBody(sheet: string, selector: string): string | null {
    const escaped = selector.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
    const match = new RegExp(`(?:^|[}/\\s])${escaped}\\s*(?:,[^{}]*)?\\{([^}]*)\\}`, "m").exec(sheet);
    return match ? match[1] : null;
}

/** A selector's weight as ids, then classes, attributes and pseudo-classes, then types. */
function specificity(selector: string): [number, number, number] {
    const bare = selector.replace(/::?[\w-]+\([^)]*\)/g, "");
    const classes = (bare.match(/\.[\w-]+|\[[^\]]*\]|:(?!:)[\w-]+/g) ?? []).length;
    const types = (bare.replace(/\.[\w-]+|\[[^\]]*\]|::?[\w-]+/g, " ").match(/(?:^|[\s>+~])[a-z][\w-]*/gi) ?? []).length;
    return [0, classes, types];
}

/**
 * Every input in a template with the class lists of the elements around it, outermost first. The
 * tags are read with their quoted attributes whole, since a binding can hold a ">".
 */
function inputsOf(template: string): { tag: string, holders: string[] }[] {
    const inputs: { tag: string, holders: string[] }[] = [];
    const stack: string[] = [];
    const tags = /<(\/?)([A-Za-z][\w.-]*)((?:[^>"']|"[^"]*"|'[^']*')*?)(\/?)>/g;
    const voids = ["input", "img", "br", "hr"];
    let match: RegExpExecArray | null;

    while ((match = tags.exec(template)) !== null) {
        const classes = /(?:^|\s)class="([^"]*)"/.exec(match[3])?.[1] ?? "";
        if (match[1]) {
            stack.pop();
        }
        else if (match[2].toLowerCase() === "input") {
            inputs.push({ tag: match[0], holders: [...stack] });
        }
        else if (!match[4] && !voids.includes(match[2].toLowerCase())) {
            stack.push(classes);
        }
    }

    return inputs;
}

describe("the chat's text boxes", () => {
    const sheet = readFileSync(sheetPath, "utf8");

    it("draw a text input bare in every state, at a weight that outranks Rock's focused input", () => {
        // Rock's core sheet: input[type="text"]:focus, textarea:focus { border; outline; box-shadow }.
        const rock = specificity("input[type=\"text\"]:focus");

        for (const selector of [".chat-shell .chat-field-input", ".chat-shell .chat-field-input:focus"]) {
            const body = ruleBody(sheet, selector);
            expect(body).not.toBeNull();
            expect(body).toMatch(/border:\s*0\b/);
            expect(body).toMatch(/outline:\s*0\b/);
            expect(body).toMatch(/box-shadow:\s*none/);
        }

        const focused = specificity(".chat-shell .chat-field-input:focus");
        expect(focused[1]).toBeGreaterThan(rock[1]);
    });

    it("draw the one ring on the wrapper, under focus within", () => {
        const body = ruleBody(sheet, ".chat-shell .chat-field:focus-within");
        expect(body).not.toBeNull();
        expect(body).toMatch(/border-color:\s*var\(--chat-focus-line\)/);
        expect(body).toMatch(/box-shadow:\s*0 0 0 3px var\(--chat-brand-ring\)/);
    });

    it("route every text input in the chat through the shared rule, inside a ringed wrapper", () => {
        const offenders: string[] = [];
        let found = 0;

        for (const path of obsFiles(shellFolder)) {
            const text = readFileSync(path, "utf8");
            const template = text.slice(0, text.indexOf("<style") === -1 ? undefined : text.indexOf("<style"));

            for (const { tag, holders } of inputsOf(template)) {
                const type = /\btype="([^"]*)"/.exec(tag)?.[1] ?? "text";
                // A file picker, a radio and a checkbox are not text boxes, and the people picker's
                // input is the one a combobox role marks, drawn inside its field.
                if (["file", "radio", "checkbox", "hidden", "range", "button", "submit"].includes(type)) {
                    continue;
                }

                found++;
                // The ring is drawn by a wrapper: a chat-field box, or the rename field's setting
                // group, whose inner box the sheet rings by its own selector. A bare input has none.
                const isRename = /\bchat-rn-in\b/.test(tag);
                const isRinged = holders.some(classes => isRename ? /\bchat-rn-grp\b/.test(classes) : /\bchat-field\b/.test(classes));
                if (!/\bchat-field-input\b/.test(tag) || !isRinged) {
                    offenders.push(path);
                }
            }
        }

        // The sidebar's search, the directory's, the emoji picker's, the members filter, the
        // leader search, the people picker's field and the rename field.
        expect(found).toBe(7);
        // The rename field's ring is the sheet's second selector in the wrapper's rule.
        expect(ruleBody(sheet, ".chat-shell .chat-field:focus-within")).not.toBeNull();
        expect(sheet).toMatch(/\.chat-shell \.chat-rn-grp \.chat-sg:focus-within\s*\{/);
        expect(offenders).toEqual([]);
    });

    it("leave the wrapper's ring and the input's reset to the chat sheet, never a component's own style", () => {
        const offenders: string[] = [];

        for (const path of obsFiles(shellFolder)) {
            const text = readFileSync(path, "utf8");
            const styleStart = text.indexOf("<style");
            if (styleStart === -1) {
                continue;
            }
            const styles = text.slice(styleStart, text.indexOf("</style>", styleStart));

            // A rule for a wrapper's :focus-within that draws a box-shadow is a second ring, other
            // than the composer's own box, which is not a text input.
            for (const rule of styles.match(/[^{}]+:focus-within[^{}]*\{[^}]*\}/g) ?? []) {
                if (/box-shadow:\s*0 0 0 3px var\(--chat-brand-ring\)/.test(rule) && !/chat-composer-box/.test(rule)) {
                    offenders.push(`${path}: ${rule.trim().split("{")[0].trim()}`);
                }
            }

            // An input rule that clears the border or outline is a reset the shared rule owns.
            for (const rule of styles.match(/[^{}]*\binput\b[^{}]*\{[^}]*\}/g) ?? []) {
                if (/\b(?:outline|border):\s*0\b/.test(rule) && !/radio|checkbox/.test(rule)) {
                    offenders.push(`${path}: ${rule.trim().split("{")[0].trim()}`);
                }
            }
        }

        expect(offenders).toEqual([]);
    });
});
