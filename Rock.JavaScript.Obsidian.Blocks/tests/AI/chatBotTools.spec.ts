import { parseMarkdown } from "../../src/AI/ChatBot/tools.partial";

function render(markdown: string): HTMLElement {
    const container = document.createElement("div");
    container.innerHTML = parseMarkdown(markdown);

    return container;
}

function getEventHandlerAttributes(container: HTMLElement): string[] {
    return Array.from(container.querySelectorAll("*"))
        .flatMap(element => element.getAttributeNames())
        .filter(name => name.toLowerCase().startsWith("on"));
}

describe("parseMarkdown", () => {
    describe("Raw HTML", () => {
        it("Removes an image with an event handler", () => {
            const container = render(`<img src="https://example.com/a.png" onerror="alert(1)">`);

            expect(container.querySelector("img")).toBeNull();
            expect(getEventHandlerAttributes(container)).toEqual([]);
        });

        it("Removes a javascript link target from a Markdown link", () => {
            const container = render("[Click me](javascript:alert(1))");
            const anchor = container.querySelector("a");

            expect(container.textContent).toContain("Click me");
            expect(anchor?.hasAttribute("href") ?? false).toBe(false);
        });

        it("Removes a javascript link target from a raw anchor", () => {
            const container = render(`<a href="javascript:alert(1)">Click me</a>`);
            const anchor = container.querySelector("a");

            expect(container.textContent).toContain("Click me");
            expect(anchor?.hasAttribute("href") ?? false).toBe(false);
        });

        it("Keeps the text of a raw button but not the button", () => {
            const container = render("<button>TH105-marker</button>");

            expect(container.querySelector("button")).toBeNull();
            expect(container.textContent).toContain("TH105-marker");
        });

        it("Removes inline styles", () => {
            const container = render(`<span style="color: red">Red text</span>`);

            expect(container.querySelector("[style]")).toBeNull();
            expect(container.textContent).toContain("Red text");
        });

        it("Removes classes outside of tables and code", () => {
            const container = render(`<span class="btn btn-danger">Fake button</span>`);

            expect(container.querySelector("[class]")).toBeNull();
            expect(container.textContent).toContain("Fake button");
        });

        it("Replaces the classes on a raw table with the table classes", () => {
            const container = render(`<table class="navbar-fixed-top"><tr><td>Cell</td></tr></table>`);

            expect(container.querySelector("table")?.getAttribute("class")).toBe("table table-bordered table-striped");
        });

        it("Removes focus, screen reader and target attributes but keeps the link", () => {
            const container = render(`<a href="https://example.com/page" target="_top" role="alert" tabindex="1" aria-label="Fake" dir="rtl">Page</a>`);
            const anchor = container.querySelector("a");

            expect(anchor?.getAttribute("href")).toBe("https://example.com/page");
            expect(anchor?.getAttributeNames().filter(name => name !== "href")).toEqual([]);
            expect(anchor?.textContent).toBe("Page");
        });

        it("Removes a class that is not a language from raw code", () => {
            const container = render(`<code class="navbar-fixed-top">Code</code>`);

            expect(container.querySelector("code")?.hasAttribute("class")).toBe(false);
            expect(container.textContent).toContain("Code");
        });
    });

    describe("Markdown images", () => {
        it("Become links to the image", () => {
            const container = render("![Chart](https://example.com/chart.png)");
            const anchor = container.querySelector("a");

            expect(container.querySelector("img")).toBeNull();
            expect(anchor?.getAttribute("href")).toBe("https://example.com/chart.png");
            expect(anchor?.textContent).toBe("Chart");
        });

        it("Use the address as the text when there is no alt text", () => {
            const container = render("![](https://example.com/chart.png)");

            expect(container.querySelector("a")?.textContent).toBe("https://example.com/chart.png");
        });

        it("Show formatted alt text as plain text", () => {
            const container = render("![**Q3** chart](https://example.com/c.png)");

            expect(container.querySelector("strong")).toBeNull();
            expect(container.querySelector("a")?.textContent).toBe("Q3 chart");
        });

        it("Show only the alt text inside a link", () => {
            const container = render("[![Report](https://example.com/chart.png)](https://example.com/page/123)");
            const anchors = container.querySelectorAll("a");

            expect(anchors.length).toBe(1);
            expect(anchors[0].getAttribute("href")).toBe("https://example.com/page/123");
            expect(anchors[0].textContent).toBe("Report");
        });
    });

    describe("Markdown formatting", () => {
        it("Keeps headings, bold and strikethrough", () => {
            const container = render("# Title\n\n**Bold** and ~~gone~~");

            expect(container.querySelector("h1")?.textContent).toBe("Title");
            expect(container.querySelector("strong")?.textContent).toBe("Bold");
            expect(container.querySelector("del")?.textContent).toBe("gone");
        });

        it("Keeps the start of a numbered list", () => {
            const container = render("3. Third\n4. Fourth");

            expect(container.querySelector("ol")?.getAttribute("start")).toBe("3");
            expect(container.querySelectorAll("li").length).toBe(2);
        });

        it("Keeps table classes and column alignment", () => {
            const container = render("| Campus | Count |\n| --- | ---: |\n| Main | 10 |");
            const table = container.querySelector("table");

            expect(table?.getAttribute("class")).toBe("table table-bordered table-striped");
            expect(container.querySelector("th[align='right']")?.textContent).toBe("Count");
            expect(container.querySelector("td[align='right']")?.textContent).toBe("10");
        });

        it("Shows task list items as text", () => {
            const container = render("- [ ] Open\n- [x] Done");
            const items = Array.from(container.querySelectorAll("li")).map(li => li.textContent?.trim());

            expect(container.querySelector("input")).toBeNull();
            expect(items).toEqual(["[ ]  Open", "[x]  Done"]);
        });

        it("Keeps the language class on fenced code", () => {
            const container = render("```html\n<b>{{ Person.FullName }}</b>\n```");
            const code = container.querySelector("pre > code");

            expect(code?.getAttribute("class")).toBe("language-html");
            expect(code?.textContent).toBe("<b>{{ Person.FullName }}</b>\n");
            expect(code?.querySelector("b")).toBeNull();
        });

        it("Shows special characters as typed", () => {
            const container = render("5 < 6 & 7 \"quoted\"");

            expect(container.textContent?.trim()).toBe("5 < 6 & 7 \"quoted\"");
        });
    });
});
