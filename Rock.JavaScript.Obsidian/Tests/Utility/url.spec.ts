import assert = require("assert");
import { makeUrlRedirectSafe } from "../../Framework/Utility/url";

describe("makeUrlRedirectSafe Suite", () => {
    it("Rejects invalid URLs", () => {
        assert.strictEqual(makeUrlRedirectSafe("javascript:alert('xss')"), "/");
        assert.strictEqual(makeUrlRedirectSafe("javascript:javascript:alert('xss')"), "/");
    });

    it("Accepts valid URLs", () => {
        const urls = [
            "/page/123",
            "/page/admin?id=1",
            "/page/123?id=1&val=True",

            // These are considered safe because they are still encoded, so
            // they will be treated as a filename by the browser.
            "javascript%3Aalert%28%27xss%27%29",
            "javascript%253Aalert%28%27xss%27%29"
        ];

        for (const url of urls) {
            assert.strictEqual(makeUrlRedirectSafe(url), url);
        }
    });
});
