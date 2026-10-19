import {
    getButtonComponentHelper,
    getCodeComponentHelper,
    getImageComponentHelper,
    getRsvpComponentHelper,
    getTitleComponentHelper,
    getVideoComponentHelper,
    importInertHtml
} from "@Obsidian/Controls/Internal/EmailEditor/utils.partial";

/** Markup that becomes an image with an error handler if it is parsed as HTML. */
const injectedImage = "<img src=x onerror=alert(1)>";

/** The same markup, entity encoded so it is plain text or a plain attribute value. */
const encodedInjectedImage = "&lt;img src=x onerror=alert(1)&gt;";

/**
 * Parses the markup into a detached element and returns its first child
 * element, the way the email editor gets a component from section markup.
 */
function createComponent(html: string): Element {
    const container = document.createElement("div");
    container.innerHTML = html;

    return container.firstElementChild as Element;
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
});

describe("email editor v0 component migrations", () => {
    beforeAll(() => {
        // Browsers make CSSStyleDeclaration iterable but jsdom does not.
        const prototype = CSSStyleDeclaration.prototype as unknown as Record<symbol, unknown>;
        if (!prototype[Symbol.iterator]) {
            prototype[Symbol.iterator] = Array.prototype[Symbol.iterator];
        }
    });

    it("title migration keeps encoded heading text as text", () => {
        const component = createComponent(`<div class="component component-title"><h1>${encodedInjectedImage}</h1></div>`);

        const migrated = getTitleComponentHelper().migrate(component);

        expect(migrated.querySelector("img")).toBeNull();
        expect(migrated.querySelector("h1")?.textContent).toBe(injectedImage);
    });

    it("button migration keeps the title attribute and text as values", () => {
        const component = createComponent(`<div class="component component-button"><table class="button-innerwrap" align="left"><tbody><tr><td><table class="button-shell"><tbody><tr><td class="button-content"><a class="button-link" href="https://example.com/" title="&quot;&gt;${encodedInjectedImage}">${encodedInjectedImage}</a></td></tr></tbody></table></td></tr></tbody></table></div>`);

        const migrated = getButtonComponentHelper().migrate(component);
        const link = migrated.querySelector("a.button-link");

        expect(migrated.querySelector("img")).toBeNull();
        expect(link?.getAttribute("title")).toBe(`">${injectedImage}`);
        expect(link?.textContent).toBe(injectedImage);
        expect(link?.getAttribute("href")).toBe("https://example.com/");
    });

    it("image migration keeps the alt attribute as a value", () => {
        const component = createComponent(`<div class="component component-image"><a href="https://example.com/"><img src="/Assets/Images/x.jpg" alt="&quot;&gt;${encodedInjectedImage}"></a></div>`);

        const migrated = getImageComponentHelper().migrate(component);
        const images = migrated.querySelectorAll("img");

        expect(images.length).toBe(1);
        expect(images[0].getAttribute("alt")).toBe(`">${injectedImage}`);
        expect(images[0].getAttribute("src")).toBe("/Assets/Images/x.jpg");
        expect(migrated.querySelector("a")?.getAttribute("href")).toBe("https://example.com/");
    });

    it("video migration keeps the link and image attributes as values", () => {
        const component = createComponent(`<div class="component component-video"><a href="https://example.com/?a=1&amp;b=&quot;&gt;${encodedInjectedImage}"><img src="/Assets/Images/video.jpg" style="width: 100%;"></a></div>`);

        const migrated = getVideoComponentHelper().migrate(component);
        const images = migrated.querySelectorAll("img");

        expect(images.length).toBe(1);
        expect(images[0].getAttribute("src")).toBe("/Assets/Images/video.jpg");
        expect(migrated.querySelector("a")?.getAttribute("href")).toBe(`https://example.com/?a=1&b=">${injectedImage}`);
    });

    it("rsvp migration keeps link text as text", () => {
        const component = createComponent(`<div class="component component-rsvp"><table class="rsvp-innerwrap" align="left"><tbody><tr><td><a class="rsvp-accept-link" href="https://example.com/a">${encodedInjectedImage}</a><a class="rsvp-decline-link" href="https://example.com/d">${encodedInjectedImage}</a></td></tr></tbody></table></div>`);

        const migrated = getRsvpComponentHelper().migrate(component);

        expect(migrated.querySelector("img")).toBeNull();
        expect(migrated.querySelector("a.rsvp-accept-link")?.textContent).toBe(injectedImage);
        expect(migrated.querySelector("a.rsvp-accept-link")?.getAttribute("title")).toBe(injectedImage);
        expect(migrated.querySelector("a.rsvp-decline-link")?.textContent).toBe(injectedImage);
        expect(migrated.querySelector("a.rsvp-accept-link")?.getAttribute("href")).toBe("https://example.com/a");
    });

    it("code migration keeps the existing content", () => {
        const component = createComponent(`<div class="component component-code"><p title="a &lt; b">{{ Person.NickName }}</p><b>Bold</b></div>`);

        const migrated = getCodeComponentHelper().migrate(component);
        const content = migrated.querySelector(".content-wrapper-for-code");

        expect(content?.innerHTML).toBe(`<p title="a < b">{{ Person.NickName }}</p><b>Bold</b>`);
    });
});
