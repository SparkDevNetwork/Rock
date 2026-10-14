import assert = require("assert");
import { makeUrlRedirectSafe } from "../../Framework/Utility/url";

describe("makeUrlRedirectSafe Suite", () => {
    it("Rejects invalid URLs", () => {
        const urls = [
            "javascript:alert('xss')",
            "javascript:javascript:alert('xss')",
            " JavaScript:alert('xss')",
            "java\tscript:alert('xss')",
            "vbscript:msgbox(1)",
            "data:text/html,<script>alert('xss')</script>",
            "file:///c:/x",
            "ftp://localhost/x",
            "myapp://localhost/x",

            // Can't be parsed by the browser.
            "http://[",
            "https://unsafe.com\u0000.localhost"
        ];

        for (const url of urls) {
            assert.strictEqual(makeUrlRedirectSafe(url), "/", url);
        }
    });

    it("Keeps links to another site on this site", () => {
        const origin = window.location.origin;
        const host = window.location.host;
        const urls: [string, string][] = [
            ["https://unsafe.com", `${origin}/`],
            ["https://unsafe.com/page/1?id=1", `${origin}/page/1?id=1`],
            ["https://unsafe.com/page/1#notes", `${origin}/page/1`],
            ["https:/unsafe.com", `${origin}/`],

            // Protocol-relative and backslash links.
            ["//unsafe.com", `${origin}/`],
            ["/\\unsafe.com", `${origin}/`],
            ["\\\\unsafe.com", `${origin}/`],
            ["https://x.com//unsafe.com", `${origin}//unsafe.com`],
            ["https://x.com/\\unsafe.com", `${origin}//unsafe.com`],

            // Characters the browser removes before reading the link.
            ["/\t/unsafe.com", `${origin}/`],
            ["/\n/unsafe.com", `${origin}/`],
            ["/\r/unsafe.com", `${origin}/`],
            [" //unsafe.com", `${origin}/`],
            ["\u0001//unsafe.com", `${origin}/`],

            // Text before "@" is user info, not the host.
            ["https://trusted.com@unsafe.com", `${origin}/`],
            [`https://${host}@unsafe.com`, `${origin}/`]
        ];

        for (const [url, expected] of urls) {
            assert.strictEqual(makeUrlRedirectSafe(url), expected, url);
        }
    });

    it("Accepts valid URLs", () => {
        const origin = window.location.origin;
        const urls = [
            "/page/123",
            "/page/admin?id=1",
            "/page/123?id=1&val=True",
            "/page/123?returnUrl=//unsafe.com",
            "/page/123?time=10:30",
            "page/1",
            "?id=1",
            "",
            `${origin}/page/123`,
            `${origin}/page/123#notes`,
            `${origin.replace("//", "//someone@")}/page/123`,

            // These are considered safe because they are still encoded, so
            // they will be treated as a filename by the browser.
            "javascript%3Aalert%28%27xss%27%29",
            "javascript%253Aalert%28%27xss%27%29",
            "/%09/unsafe.com",
            "/%2F%2Funsafe.com",
            "%252F%252Funsafe.com",
            "/&#47;unsafe.com",

            // A control character after the start is encoded as part of the
            // path.
            "/\u0000/unsafe.com"
        ];

        for (const url of urls) {
            assert.strictEqual(makeUrlRedirectSafe(url), url, url);
        }
    });
});
