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
import {
    getSectionComponentHelper,
    importInertHtml
} from "@Obsidian/Controls/Internal/EmailEditor/utils.partial";

/** Markup that becomes an image with an error handler if it is parsed as HTML. */
const injectedImage = "<img src=x onerror=alert(1)>";

/**
 * Parses the markup into a detached element and returns its first child
 * element, the way the email editor gets a component from section markup.
 */
function createComponent(html: string): Element {
    const container = document.createElement("div");
    container.innerHTML = html;

    return container.firstElementChild as Element;
}

/**
 * A style value the way a browser returns it when the declaration uses var():
 * the fallback text is kept as written, quotes and angle brackets included.
 * jsdom drops most var() values, so tests return it through a stub. The
 * custom property name starts with two hyphens, written here as escapes.
 */
const cssVarInjectedImage = `var(\u002d\u002da,'"><img src=x onerror=alert(1)>')`;

/**
 * Makes the element's style return the var() value for the given properties,
 * the way a browser would for a declaration that uses var().
 */
function stubVarStyle(element: Element, properties: string[]): void {
    const style = (element as HTMLElement).style;
    const stub = new Proxy(style, {
        get(target, property) {
            if (typeof property === "string" && properties.includes(property)) {
                return cssVarInjectedImage;
            }

            const value = Reflect.get(target, property, target);

            return typeof value === "function" ? value.bind(target) : value;
        }
    });

    Object.defineProperty(element, "style", { value: stub });
}

/** Imports the HTML and returns it inside a div so it can be queried. */
function importIntoDiv(html: string): HTMLDivElement {
    const div = document.createElement("div");
    div.appendChild(importInertHtml(document, html));

    return div;
}

describe("importInertHtml", () => {
    it("returns nodes owned by the target document", () => {
        const targetDocument = document.implementation.createHTMLDocument("");
        const fragment = importInertHtml(targetDocument, "<p>Hello</p>");

        expect(fragment.ownerDocument).toBe(targetDocument);
        expect(fragment.firstChild?.ownerDocument).toBe(targetDocument);
    });

    it("removes event handler attributes", () => {
        const div = importIntoDiv(`<img src="x" onerror="alert(1)" ONLOAD="alert(2)"><a href="https://example.com" onclick="alert(3)">Link</a>`);

        const img = div.querySelector("img") as HTMLImageElement;
        expect(img.getAttribute("src")).toBe("x");
        expect(img.hasAttribute("onerror")).toBe(false);
        expect(img.hasAttribute("onload")).toBe(false);
        expect(div.querySelector("a")?.hasAttribute("onclick")).toBe(false);
        expect(div.querySelector("a")?.getAttribute("href")).toBe("https://example.com");
    });

    it("removes script URLs and srcdoc attributes", () => {
        const div = importIntoDiv(`
            <a id="a1" href="javascript:alert(1)">1</a>
            <a id="a2" href=" JaVa&#x09;Script:alert(1)">2</a>
            <a id="a3" href="vbscript:msgbox(1)">3</a>
            <a id="a4" href="data:text/html,<script>alert(1)</script>">4</a>
            <div id="d1" srcdoc="<script>alert(1)</script>"></div>
            <a id="a5" href="data:image/png;base64,AAAA">5</a>`);

        expect(div.querySelector("#a1")?.hasAttribute("href")).toBe(false);
        expect(div.querySelector("#a2")?.hasAttribute("href")).toBe(false);
        expect(div.querySelector("#a3")?.hasAttribute("href")).toBe(false);
        expect(div.querySelector("#a4")?.hasAttribute("href")).toBe(false);
        expect(div.querySelector("#d1")?.hasAttribute("srcdoc")).toBe(false);
        expect(div.querySelector("#a5")?.getAttribute("href")).toBe("data:image/png;base64,AAAA");
    });

    it("removes active elements", () => {
        const div = importIntoDiv(`
            <script>alert(1)</script>
            <iframe src="https://example.com"></iframe>
            <object data="x"></object>
            <embed src="x">
            <base href="https://example.com/">
            <meta http-equiv="refresh" content="0;url=https://example.com">
            <svg><a><animate attributeName="href" to="javascript:alert(1)"></animate><set attributeName="href" to="javascript:alert(1)"></set></a></svg>
            <p>Kept</p>`);

        expect(div.querySelector("script, iframe, object, embed, base, meta, animate, set")).toBeNull();
        expect(div.querySelector("p")?.textContent).toBe("Kept");
    });

    it("removes elements whose content is parsed differently when scripting is enabled", () => {
        const div = importIntoDiv(`
            <noscript><p title="</noscript>${injectedImage}"></p></noscript>
            <template><img src="x" onerror="alert(1)"></template>
            <noembed>x</noembed>
            <noframes>x</noframes>
            <xmp>x</xmp>
            <p id="kept">Kept</p>`);

        expect(div.querySelector("noscript, template, noembed, noframes, xmp")).toBeNull();
        expect(div.querySelector("[onerror]")).toBeNull();
        expect(div.querySelector("#kept")?.textContent).toBe("Kept");
    });

    it("removes plaintext elements", () => {
        const div = importIntoDiv(`<p>Kept</p><plaintext>${injectedImage}`);

        expect(div.querySelector("plaintext")).toBeNull();
        expect(div.querySelector("p")?.textContent).toBe("Kept");
    });

    it("keeps Lava and table markup unchanged", () => {
        const html = `<div class="component component-text" data-version="v17.3-alpha"><table role="presentation" width="100%"><tbody><tr><td style="padding: 4px;"><a href="{{ 'Global' | Attribute:'PublicApplicationRoot' }}page/1?p={{ Person.Id }}&amp;x=1">Hi {{ Person.NickName }}</a>{% if Person %}<b>Yes</b>{% endif %}</td></tr></tbody></table></div>`;

        const div = importIntoDiv(html);

        expect(div.innerHTML).toBe(html);
    });

    it.each([
        ["form namespace confusion with an SVG animation handler", `<form><math><mtext></form><form><mglyph><style></math><svg><animate onbegin=alert(1) attributeName=x dur=1s>`],
        ["table namespace confusion with an image handler", `<math><mtext><table><mglyph><style><img src=x onerror=alert(1)>`],
        ["form namespace confusion with an image handler", `<form><math><mtext></form><form><mglyph><style></math><img src=x onerror=alert(1)>`]
    ])("stays clean after a serialize and reparse: %s", (_name, html) => {
        const imported = importIntoDiv(html);
        const serialized = imported.innerHTML;
        const activeSelector = "[onbegin], [onerror], animate, set, img, script";

        // Reparse as markup set with innerHTML, the way a section is dropped or reloaded.
        const fragmentDocument = document.implementation.createHTMLDocument("");
        const reparsedContainer = fragmentDocument.createElement("div");
        reparsedContainer.innerHTML = serialized;

        expect(reparsedContainer.querySelector(activeSelector)).toBeNull();

        // Reparse as a whole document, the way the editor's getHtml writes it.
        const reparsedDocument = document.implementation.createHTMLDocument("");
        reparsedDocument.open();
        reparsedDocument.write(`<!DOCTYPE html><html><body>${serialized}</body></html>`);
        reparsedDocument.close();

        expect(reparsedDocument.querySelector(activeSelector)).toBeNull();
        expect(imported.querySelector("math, svg, mglyph, malignmark")).toBeNull();
    });

    it("keeps Lava and table markup stable after a serialize and reparse", () => {
        const html = `<table role="presentation"><tbody><tr><td><a href="https://example.com/?a=1&amp;b=2">{{ Person.NickName }}</a>{% if Person %}<b>Yes</b>{% endif %}<style>p { color: red; }</style></td></tr></tbody></table>`;

        const serialized = importIntoDiv(html).innerHTML;

        expect(serialized).toBe(html);
        expect(importIntoDiv(serialized).innerHTML).toBe(html);
    });

    it.each(["pre", "textarea", "listing"])("keeps %s content with many leading newlines", tagName => {
        const html = `<${tagName}>\n\n\n\n\n\ncode</${tagName}>`;

        const div = importIntoDiv(html);
        const serialized = div.innerHTML;

        expect(div.querySelector(tagName)?.textContent).toContain("code");
        expect(importIntoDiv(serialized).innerHTML).toBe(serialized);
    });
});

describe("email editor v0 component migrations", () => {
    beforeAll(() => {
        // Browsers make CSSStyleDeclaration iterable but jsdom does not.
        const prototype = CSSStyleDeclaration.prototype as unknown as Record<symbol, unknown>;
        if (!prototype[Symbol.iterator]) {
            prototype[Symbol.iterator] = Array.prototype[Symbol.iterator];
        }
    });

    it("section migration keeps var() style values inside attributes", () => {
        const component = createComponent(`<div class="component component-section"><div class="dropzone"></div></div>`);
        stubVarStyle(component, ["backgroundColor", "padding", "borderRadius", "borderStyle", "borderWidth", "borderColor"]);
        stubVarStyle(component.querySelector(".dropzone") as Element, ["backgroundColor", "borderRadius", "borderStyle", "borderWidth", "borderColor", "padding", "textAlign"]);

        const migrated = getSectionComponentHelper().migrate(component);

        expect(migrated.querySelector("img, [onerror]")).toBeNull();
        expect(migrated.querySelector(".dropzone")).not.toBeNull();
    });
});
