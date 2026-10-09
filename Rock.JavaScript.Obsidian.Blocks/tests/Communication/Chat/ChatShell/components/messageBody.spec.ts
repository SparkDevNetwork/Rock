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
// A message body, mounted: the server builds the pieces and blocks, and this client draws them.
// A released client lives for years, so anything it was not built with (a piece, a block) is drawn
// by the text the server put beside it, and a link or an image is drawn only from an address this
// client trusts, whatever the server sent.
import { mount, VueWrapper } from "@vue/test-utils";
import MessageBody from "../../../../../src/Communication/Chat/ChatShell/components/messageBody.partial.obs";

function draw(props: { text: string | null, pieces?: unknown, blocks?: unknown }): VueWrapper {
    return mount(MessageBody, { props });
}

const fallback = (text: string): unknown => ({ type: "text", text });

describe("a message body's pieces", () => {
    test("with no pieces, the body is drawn as text", () => {
        const body = draw({ text: "just words", pieces: null });

        expect(body.text()).toBe("just words");
        expect(body.findAll("a")).toHaveLength(0);
    });

    test("text, a mention, a link and an emoji are each drawn as their kind", () => {
        const body = draw({
            text: "raw",
            pieces: [
                { type: "text", text: "Hi " },
                { type: "mention", text: "@Lee Leader", person_alias_guid: "a0000007-0000-4000-8000-000000000000" },
                { type: "text", text: " see " },
                { type: "link", text: "https://x.org/a", url: "https://x.org/a" },
                { type: "text", text: " " },
                { type: "emoji", text: ":thumbsup:", code: "thumbsup", emoji: "👍", label: "Thumbs up" },
                { type: "emoji", text: ":party:", code: "party", image_url: "https://cdn.example/party.png", label: "Party" }
            ]
        });

        expect(body.get("[data-testid='piece-mention']").text()).toBe("@Lee Leader");
        const link = body.get("a");
        expect(link.attributes("href")).toBe("https://x.org/a");
        expect(link.attributes("target")).toBe("_blank");
        expect(link.attributes("rel")).toBe("noopener noreferrer nofollow");
        expect(link.text()).toBe("https://x.org/a");
        expect(body.get("[data-testid='piece-emoji']").text()).toBe("👍");
        expect(body.get("[data-testid='piece-emoji']").attributes("title")).toBe("Thumbs up");
        const image = body.get("img");
        expect(image.attributes("src")).toBe("https://cdn.example/party.png");
        expect(image.attributes("alt")).toBe("Party");
        expect(body.text()).not.toContain("raw");
    });

    test("a piece of a kind this client does not know is drawn as its text", () => {
        const body = draw({ text: "raw", pieces: [{ type: "text", text: "vote: " }, { type: "poll_ref", text: "Lunch?", poll: 7 }] });

        expect(body.text()).toBe("vote: Lunch?");
    });

    test("a link to anything but http or https is drawn as text", () => {
        const body = draw({ text: "raw", pieces: [{ type: "link", text: "click", url: "javascript:alert(1)" }] });

        expect(body.findAll("a")).toHaveLength(0);
        expect(body.text()).toBe("click");
    });

    test("an emoji image not on https is drawn as its text", () => {
        const body = draw({ text: "raw", pieces: [{ type: "emoji", text: ":x:", code: "x", image_url: "http://cdn.example/x.png", label: "X" }] });

        expect(body.findAll("img")).toHaveLength(0);
        expect(body.text()).toBe(":x:");
    });

    test("markup in a piece's text is shown as characters, never as markup", () => {
        const body = draw({ text: "raw", pieces: [{ type: "text", text: "<b>x</b>" }] });

        expect(body.findAll("b")).toHaveLength(0);
        expect(body.text()).toBe("<b>x</b>");
    });
});

describe("a message's blocks", () => {
    test("each kind this client knows is drawn, in place of the body", () => {
        const body = draw({
            text: "Easter signups are open",
            pieces: null,
            blocks: [
                { type: "text", text: "Signups: https://x.org", pieces: [{ type: "text", text: "Signups: " }, { type: "link", text: "https://x.org", url: "https://x.org" }], fallback: fallback("Signups: https://x.org") },
                { type: "image", url: "https://x.org/easter.png", alt: "The cross at dawn", fallback: fallback("Picture") },
                { type: "divider", fallback: fallback("---") },
                { type: "context", text: "Events team", pieces: null, fallback: fallback("Events team") }
            ]
        });

        expect(body.get("[data-testid='block-text'] a").attributes("href")).toBe("https://x.org");
        expect(body.get("[data-testid='block-image'] img").attributes("alt")).toBe("The cross at dawn");
        expect(body.find("[data-testid='block-divider']").exists()).toBe(true);
        expect(body.get("[data-testid='block-context']").text()).toBe("Events team");
        expect(body.text()).not.toContain("Easter signups are open");
        expect(body.text()).not.toContain("---");
    });

    test("a block of a kind this client does not know is drawn as its fallback, and the blocks around it still draw", () => {
        const body = draw({
            text: "Vote now",
            blocks: [
                { type: "context", text: "Before", pieces: null, fallback: fallback("Before") },
                { type: "poll", options: ["a", "b"], fallback: fallback("Vote: a or b") },
                { type: "divider", fallback: fallback("---") }
            ]
        });

        expect(body.get("[data-testid='block-context']").text()).toBe("Before");
        expect(body.get("[data-testid='block-fallback']").text()).toBe("Vote: a or b");
        expect(body.find("[data-testid='block-divider']").exists()).toBe(true);
    });

    test("an image block not on https is drawn as its fallback", () => {
        const body = draw({ text: "x", blocks: [{ type: "image", url: "http://x.org/a.png", alt: "a", fallback: fallback("A picture") }] });

        expect(body.findAll("img")).toHaveLength(0);
        expect(body.get("[data-testid='block-fallback']").text()).toBe("A picture");
    });
});
