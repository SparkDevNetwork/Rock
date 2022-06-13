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

/**
 * Is the value a valid URL?
 * @param val
 */
export function isUrl(val: unknown): boolean {
    if (typeof val === "string") {
        // https://www.regextester.com/1965
        // Modified from link above to support urls like "http://localhost:6229/Person/1/Edit" (Url does not have a period)
        const re = /^(http[s]?:\/\/)?[^\s(["<,>]*\.?[^\s[",><]*$/;
        return re.test(val);
    }

    return false;
}

/**
 * Make the URL safe to use for redirects. Basically, this strips off any
 * protocol and hostname from the URL and ensures it's not a javascript:
 * url or anything like that.
 * 
 * @param url The URL to be made safe to use with a redirect.
 *
 * @returns A string that is safe to assign to window.location.href.
 */
export function makeUrlRedirectSafe(url: string): string {
    try {
        // If this can't be parsed as a url, such as "/page/123" it will throw
        // an error which will be handled by the next section.
        const u = new URL(url);

        // If the protocol isn't an HTTP or HTTPS, then it is most likely
        // a dangerous URL.
        if (u.protocol !== "http:" && u.protocol !== "https:") {
            return "/";
        }

        // Try again incase they did something like "http:javascript:alert('hi')".
        return makeUrlRedirectSafe(`${u.pathname}${u.search}`);
    }
    catch {
        // If the URL contains a : but could not be parsed as a URL then it
        // is not valid, so return "/" so they get redirected to home page.
        if (url.indexOf(":") !== -1) {
            return "/";
        }

        // Otherwise consider it safe to use.
        return url;
    }
}
