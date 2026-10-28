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
// The sign-in mark as the browser keeps it: a cookie Rock's sign-out expires by name, read among
// the page's other cookies, and written to last no longer than the sign-in it marks.
import { readFileSync } from "fs";
import { resolve } from "path";
import { browserSignInMark, signInMarkCookieName } from "../../../../src/Communication/Chat/ChatShell/browserShell.partial";

/** Every cookie the page writes, as written, while the test runs. */
let written: string[] = [];

/** What the page's cookies read as. */
let cookies = "";

beforeEach(() => {
    written = [];
    cookies = "";
    Object.defineProperty(document, "cookie", {
        configurable: true,
        get: () => cookies,
        set: (value: string) => void written.push(value)
    });
});

afterEach(() => {
    delete (document as unknown as { cookie?: string }).cookie;
});

describe("the sign-in mark", () => {
    test("is the cookie Rock's sign-out expires, by the very name the server keeps", () => {
        const helper = readFileSync(resolve(__dirname, "../../../../../Rock/Communication/Chat/Platform/Session/ChatSessionHelper.cs"), "utf8");
        const serverName = /SignedInCookieName\s*=\s*"([^"]+)"/.exec(helper)?.[1];

        expect(serverName).toBeTruthy();
        expect(signInMarkCookieName).toBe(serverName);
    });

    test("is set only when the page carries a cookie of exactly its name", () => {
        const mark = browserSignInMark(true);

        cookies = "";
        expect(mark.isSet()).toBe(false);

        cookies = `other=1; ${signInMarkCookieName}_old=1; x${signInMarkCookieName}=1`;
        expect(mark.isSet()).toBe(false);

        cookies = `other=1; ${signInMarkCookieName}=1; last=2`;
        expect(mark.isSet()).toBe(true);

        cookies = `${signInMarkCookieName}=1`;
        expect(mark.isSet()).toBe(true);
    });

    test("lasts a year, at the site's root, for a sign-in kept across browser restarts", () => {
        browserSignInMark(true).set();

        expect(written).toHaveLength(1);
        expect(written[0]).toMatch(new RegExp(`^${signInMarkCookieName}=1; path=/; max-age=31536000; SameSite=Lax`));
    });

    test("ends with the browser session for a sign-in that does, so closing the browser forgets the drafts", () => {
        browserSignInMark(false).set();

        expect(written).toHaveLength(1);
        expect(written[0]).toMatch(new RegExp(`^${signInMarkCookieName}=1; path=/; SameSite=Lax`));
        expect(written[0]).not.toContain("max-age");
        expect(written[0]).not.toContain("expires");
    });
});
