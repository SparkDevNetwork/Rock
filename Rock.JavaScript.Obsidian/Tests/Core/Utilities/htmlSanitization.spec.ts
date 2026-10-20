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
import * as fs from "fs";
import * as path from "path";
import { HtmlSanitizeOptions, HtmlTagRemovalMode, sanitizeHtml } from "../../../Framework/Core/Utilities/htmlSanitization";
import * as tables from "../../../Framework/Core/Utilities/htmlSanitizationTables.partial";

// NOTICE!!!
// The sanitizer rows come from the C# tests in the Rock.Tests project
// (Utility/HtmlAllowListSanitizerTests.cs) and are read at test time, so a
// row added there runs here too. HtmlSanitizeOptionsTests.cs is ported by
// hand below; changes to it must be made here as well.
// NOTICE!!!

const repositoryRoot = path.resolve(__dirname, "../../../..");
const sanitizerTestsPath = path.join(repositoryRoot, "Rock.Tests/Utility/HtmlAllowListSanitizerTests.cs");
const tablesPath = path.join(repositoryRoot, "Rock/Utility/HtmlSanitization/HtmlAllowListTables.cs");

// #region C# source parsing

const simpleEscapes: Record<string, string> = {
    "\"": "\"",
    "'": "'",
    "\\": "\\",
    "0": "\0",
    "a": "\x07",
    "b": "\b",
    "f": "\f",
    "n": "\n",
    "r": "\r",
    "t": "\t",
    "v": "\v"
};

/**
 * Reads the C# string literal that starts at `start`, either a regular
 * literal with escapes or a verbatim `@"..."` literal.
 *
 * @returns The decoded value and the index just past the closing quote.
 */
function readCSharpString(source: string, start: number): [string, number] {
    if (source[start] === "@") {
        let value = "";
        let i = start + 2;

        while (i < source.length) {
            if (source[i] === "\"") {
                if (source[i + 1] !== "\"") {
                    return [value, i + 1];
                }

                i++;
            }

            value += source[i++];
        }

        throw new Error(`Unterminated verbatim string at ${start}.`);
    }

    let value = "";
    let i = start + 1;

    while (i < source.length && source[i] !== "\"") {
        if (source[i] !== "\\") {
            value += source[i++];
            continue;
        }

        const escape = source[i + 1];

        if (escape === "u") {
            value += String.fromCharCode(parseInt(source.substr(i + 2, 4), 16));
            i += 6;
        }
        else if (escape in simpleEscapes) {
            value += simpleEscapes[escape];
            i += 2;
        }
        else {
            throw new Error(`Unsupported escape '\\${escape}' at ${i}.`);
        }
    }

    if (i >= source.length) {
        throw new Error(`Unterminated string at ${start}.`);
    }

    return [value, i + 1];
}

/** Reads every string literal in a piece of C# source, in order. */
function readCSharpStrings(source: string): string[] {
    const values: string[] = [];

    for (let i = 0; i < source.length; i++) {
        if (source[i] === "\"" || (source[i] === "@" && source[i + 1] === "\"")) {
            const [value, next] = readCSharpString(source, i);
            values.push(value);
            i = next - 1;
        }
    }

    return values;
}

type DataRow = {
    method: string;
    index: number;
    input: string;
    optionsName: string;
    expected: string;
};

/** Reads every three-string `[DataRow]` of every test method in a C# test file. */
function readDataRows(source: string): DataRow[] {
    const rows: DataRow[] = [];
    let pending: string[][] = [];

    for (const line of source.split(/\r?\n/)) {
        const trimmed = line.trim();
        const method = /^public void (\w+)\(/.exec(trimmed);

        if (trimmed.startsWith("[DataRow(")) {
            const values = readCSharpStrings(trimmed.substring("[DataRow(".length));

            if (values.length !== 3) {
                throw new Error(`Expected 3 strings in: ${trimmed}`);
            }

            pending.push(values);
        }
        else if (method) {
            pending.forEach(([input, optionsName, expected], index) => {
                rows.push({ method: method[1], index, input, optionsName, expected });
            });
            pending = [];
        }
    }

    return rows;
}

function stripLineComments(source: string): string {
    return source.split(/\r?\n/).map(line => line.replace(/\/\/.*$/, "")).join("\n");
}

function sorted(values: Iterable<string>, ignoreCase: boolean): string[] {
    return Array.from(new Set(Array.from(values).map(value => ignoreCase ? value.toLowerCase() : value))).sort();
}

function allMatches(pattern: RegExp, text: string): RegExpExecArray[] {
    const global = new RegExp(pattern.source, "g");
    const matches: RegExpExecArray[] = [];

    for (let match = global.exec(text); match !== null; match = global.exec(text)) {
        matches.push(match);
    }

    return matches;
}

function toCamelCase(name: string): string {
    return name[0].toLowerCase() + name.substring(1);
}

// #endregion

const sanitizerTestsSource = fs.readFileSync(sanitizerTestsPath, "utf8");
const tablesSource = stripLineComments(fs.readFileSync(tablesPath, "utf8"));

const namedOptions: Record<string, HtmlSanitizeOptions> = {
    "Simple": HtmlSanitizeOptions.simple,
    "Rich": HtmlSanitizeOptions.rich,
    "Text": new HtmlSanitizeOptions().makeReadOnly(),
    "SimpleUnwrap": new HtmlSanitizeOptions(HtmlSanitizeOptions.simple)
        .setDefaultRemovalMode(HtmlTagRemovalMode.Unwrap)
        .makeReadOnly(),
    "RichUnwrap": new HtmlSanitizeOptions(HtmlSanitizeOptions.rich)
        .setDefaultRemovalMode(HtmlTagRemovalMode.Unwrap)
        .makeReadOnly(),
    "SimpleUnwrapBlink": new HtmlSanitizeOptions(HtmlSanitizeOptions.simple)
        .setRemovalMode("blink", HtmlTagRemovalMode.Unwrap)
        .makeReadOnly(),
    "NoteText": new HtmlSanitizeOptions()
        .allowTag("br")
        .allowTag("div")
        .allowTag("span")
        .allowAttribute("span", "class")
        .allowAttribute("span", "data-identifier")
        .setDefaultRemovalMode(HtmlTagRemovalMode.Unwrap)
        .makeReadOnly(),
    "SimpleFont": new HtmlSanitizeOptions(HtmlSanitizeOptions.simple)
        .allowTag("font")
        .allowAttribute("font", "color")
        .makeReadOnly(),
    "SimpleCite": new HtmlSanitizeOptions(HtmlSanitizeOptions.simple)
        .allowAttribute("blockquote", "cite")
        .makeReadOnly(),
    "SimpleNoTitle": new HtmlSanitizeOptions(HtmlSanitizeOptions.simple)
        .allowAttribute("a", "title")
        .disallowAttribute("", "title")
        .makeReadOnly(),
    "RichVideo": new HtmlSanitizeOptions(HtmlSanitizeOptions.rich)
        .allowTag("video")
        .allowAttribute("video", "poster")
        .makeReadOnly(),
    "RichNoImg": new HtmlSanitizeOptions(HtmlSanitizeOptions.rich)
        .disallowTag("img")
        .makeReadOnly(),
    "RichNoImgAlt": new HtmlSanitizeOptions(HtmlSanitizeOptions.rich)
        .disallowAttribute("img", "alt")
        .makeReadOnly(),
    "RichNoData": new HtmlSanitizeOptions(HtmlSanitizeOptions.rich)
        .disallowDataAttributes()
        .makeReadOnly(),
    "RichMath": new HtmlSanitizeOptions(HtmlSanitizeOptions.rich)
        .allowTag("math")
        .allowTag("mi")
        .allowTag("mrow")
        .makeReadOnly(),
    "SimpleNoMailto": new HtmlSanitizeOptions(HtmlSanitizeOptions.simple)
        .disallowUrlScheme("mailto")
        .makeReadOnly(),
    "SimpleNoColor": new HtmlSanitizeOptions(HtmlSanitizeOptions.simple)
        .disallowCssProperty("color")
        .makeReadOnly()
};

type KnownDifference = {
    method: string;
    input: string;
    optionsName: string;
    expected: string;
    reason: string;
};

/**
 * The C# rows whose TypeScript output is expected to differ, each with its
 * TypeScript output. Only differences that come from the browser's parser
 * belong here: DOMParser parses with scripting off, and it follows newer
 * HTML parsing rules than AngleSharp does.
 */
const knownDifferences: KnownDifference[] = [
    {
        method: "Sanitize_NeverPassElements_AreDroppedWithContent",
        input: "<noscript><p title=\"</noscript><img src=x onerror=alert(1)>\"></noscript>",
        optionsName: "Simple",
        expected: "",
        reason: "noscript: DOMParser parses with scripting off, so the whole payload stays inside noscript."
    },
    {
        method: "Sanitize_NeverPassElements_AreDroppedWithContent",
        input: "<noscript><p title=\"</noscript><img src=x onerror=alert(1)>\"></noscript>",
        optionsName: "RichUnwrap",
        expected: "",
        reason: "noscript: DOMParser parses with scripting off, so the whole payload stays inside noscript."
    },
    {
        method: "Sanitize_ForeignContentVectors_GiveVerifiedOutput",
        input: "<noscript><p title=\"</noscript><img src=x onerror=alert(1)>\">",
        optionsName: "Simple",
        expected: "",
        reason: "noscript: DOMParser parses with scripting off, so the whole payload stays inside noscript."
    },
    {
        method: "Sanitize_ForeignContentVectors_GiveVerifiedOutput",
        input: "<noscript><p title=\"</noscript><img src=x onerror=alert(1)>\">",
        optionsName: "Rich",
        expected: "",
        reason: "noscript: DOMParser parses with scripting off, so the whole payload stays inside noscript."
    },
    {
        method: "Sanitize_ForeignContentVectors_GiveVerifiedOutput",
        input: "<noscript><p title=\"</noscript><img src=x onerror=alert(1)>\">",
        optionsName: "RichUnwrap",
        expected: "",
        reason: "noscript: DOMParser parses with scripting off, so the whole payload stays inside noscript."
    }
];

function findKnownDifference(row: DataRow): KnownDifference | undefined {
    return knownDifferences.find(d => d.method === row.method && d.input === row.input && d.optionsName === row.optionsName);
}

describe("sanitizeHtml", () => {
    const rows = readDataRows(sanitizerTestsSource);

    it("reads the C# data rows", () => {
        expect(rows.length).toBeGreaterThan(200);
    });

    it("has a TypeScript options object for every C# options name", () => {
        const block = /NamedOptions = new Dictionary<string, HtmlSanitizeOptions>\s*\{([\s\S]*?)\n\s*\};/.exec(sanitizerTestsSource);
        const names = allMatches(/\["(\w+)"\]\s*=/, block?.[1] ?? "").map(m => m[1]);

        expect(names.length).toBeGreaterThan(0);
        expect(Object.keys(namedOptions).sort()).toEqual(names.sort());
    });

    it("lists only known differences that match a C# row", () => {
        for (const difference of knownDifferences) {
            expect(rows.some(row => findKnownDifference(row) === difference)).toBe(true);
        }
    });

    describe.each(Array.from(new Set(rows.map(row => row.method))))("%s", method => {
        it.each(rows.filter(row => row.method === method).map(row => [row.index, row] as const))("row %i", (_index, row) => {
            const options = namedOptions[row.optionsName];
            const expected = findKnownDifference(row)?.expected ?? row.expected;

            expect(options).toBeDefined();

            const actual = sanitizeHtml(row.input, options);

            expect(actual).toBe(expected);
            expect(sanitizeHtml(actual, options)).toBe(expected);
        });
    });

    it("Sanitize_UnwrapAcrossAParserScope_MayReshapeOnceOnly", () => {
        const options = namedOptions["RichUnwrap"];

        const first = sanitizeHtml("<p><marquee><p>x</p></marquee></p>", options);
        const second = sanitizeHtml(first, options);

        expect(first).toBe("<p><p>x</p></p>");
        expect(second).toBe("<p></p><p>x</p><p></p>");
        expect(sanitizeHtml(second, options)).toBe(second);
    });

    it("treats U+0085 in url() as whitespace the way the .NET regex does", () => {
        const nextLine = String.fromCharCode(0x85);

        expect(sanitizeHtml(`<span style="background-image: url(${nextLine}'javascript:x')">a</span>`, HtmlSanitizeOptions.simple)).toBe("<span>a</span>");
    });

    const nest = (tag: string, depth: number, content: string): string =>
        `<${tag}>`.repeat(depth) + content + `</${tag}>`.repeat(depth);

    it("Sanitize_NestingAtTheLimit_IsKept", () => {
        const html = nest("b", tables.maxNestingDepth, "x");

        expect(sanitizeHtml(html, HtmlSanitizeOptions.rich)).toBe(html);
    });

    it("Sanitize_NestingPastTheLimit_RemovesTheDeeperElementWithItsContent", () => {
        const expected = nest("b", tables.maxNestingDepth, "");
        const actual = sanitizeHtml(nest("b", tables.maxNestingDepth + 1, "x"), HtmlSanitizeOptions.rich);

        expect(actual).toBe(expected);
        expect(sanitizeHtml(actual, HtmlSanitizeOptions.rich)).toBe(expected);
    });

    it.each([["b", "Rich"], ["font", "RichUnwrap"]])("Sanitize_DeepestParsedNesting_DoesNotOverflow (%s, %s)", (tag, optionsName) => {
        const actual = sanitizeHtml(nest(tag, tables.maxEstimatedNestingDepth, "x"), namedOptions[optionsName]);

        expect(actual).toBe(tag === "b" ? nest("b", tables.maxNestingDepth, "") : "");
    });

    it("Sanitize_VeryDeepNesting_ComesBackAsText", () => {
        const html = nest("b", 50000, "x");

        expect(sanitizeHtml(html, HtmlSanitizeOptions.rich)).toBe(html.replace(/</g, "&lt;").replace(/>/g, "&gt;"));
    });

    const asText = (html: string): string => html.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    const assertSanitized = (html: string, expected: string): void => {
        const actual = sanitizeHtml(html, HtmlSanitizeOptions.rich);

        expect(actual).toBe(expected);
        expect(sanitizeHtml(actual, HtmlSanitizeOptions.rich)).toBe(expected);
    };

    it("Sanitize_AtTheEstimatedDepthLimit_IsParsed", () => {
        assertSanitized(nest("div", tables.maxEstimatedNestingDepth, "x"), nest("div", tables.maxNestingDepth, ""));
    });

    it("Sanitize_PastTheEstimatedDepthLimit_ComesBackAsText", () => {
        const html = nest("div", tables.maxEstimatedNestingDepth + 1, "x");

        assertSanitized(html, asText(html));
    });

    it("Sanitize_PastTheEstimatedDepthLimit_EncodesLikeTheSerializer", () => {
        const tags = "<div>".repeat(tables.maxEstimatedNestingDepth + 1);

        assertSanitized("a & b c\r\nd\re\0f" + tags, "a &amp; b&nbsp;c\nd\nef" + asText(tags));
    });

    it.each(["<div a=\"</div>\">", "<div a='</div>'>", "<div></x>", "<div><!-- </div> -->"])("Sanitize_DepthHiddenByMarkupTricks_IsStillCounted (%s)", repeated => {
        const html = repeated.repeat(tables.maxEstimatedNestingDepth + 1) + "x";

        assertSanitized(html, asText(html));
    });

    it("Sanitize_DepthAfterABangClosedComment_IsStillCounted", () => {
        const html = "<!-- a --!>" + "<div>".repeat(tables.maxEstimatedNestingDepth + 1) + "x";

        assertSanitized(html, asText(html));
    });

    it("Sanitize_ManyUnclosedListItemsAndVoidElements_AreParsed", () => {
        assertSanitized("<ul>" + "<li>a".repeat(2000) + "</ul>", "<ul>" + "<li>a</li>".repeat(2000) + "</ul>");
        assertSanitized("<br>".repeat(2000), "<br>".repeat(2000));
    });

    it("Sanitize_NullInput_ReturnsNull", () => {
        expect(sanitizeHtml(null, HtmlSanitizeOptions.simple)).toBeNull();
        expect(sanitizeHtml(undefined, HtmlSanitizeOptions.simple)).toBeUndefined();
    });
});

describe("HtmlSanitizeOptions", () => {
    const readOnlyError = /read-only/;

    it("Presets_AreReadOnly", () => {
        expect(HtmlSanitizeOptions.simple.isReadOnly).toBe(true);
        expect(HtmlSanitizeOptions.rich.isReadOnly).toBe(true);
    });

    it("NewOptions_AreEditableAndAllowOnlyText", () => {
        const options = new HtmlSanitizeOptions();

        expect(options.isReadOnly).toBe(false);
        expect(sanitizeHtml("a <b>b</b> c", options)).toBe("a  c");
    });

    it("EditMethods_OnReadOnlyOptions_ThrowInvalidOperationException", () => {
        const options = new HtmlSanitizeOptions().makeReadOnly();

        expect(() => options.allowTag("b")).toThrow(readOnlyError);
        expect(() => options.disallowTag("b")).toThrow(readOnlyError);
        expect(() => options.allowAttribute("b", "title")).toThrow(readOnlyError);
        expect(() => options.disallowAttribute("b", "title")).toThrow(readOnlyError);
        expect(() => options.allowDataAttributes()).toThrow(readOnlyError);
        expect(() => options.disallowDataAttributes()).toThrow(readOnlyError);
        expect(() => options.allowUrlScheme("https")).toThrow(readOnlyError);
        expect(() => options.disallowUrlScheme("https")).toThrow(readOnlyError);
        expect(() => options.allowCssProperty("color")).toThrow(readOnlyError);
        expect(() => options.disallowCssProperty("color")).toThrow(readOnlyError);
        expect(() => options.setRemovalMode("font", HtmlTagRemovalMode.Unwrap)).toThrow(readOnlyError);
        expect(() => options.setDefaultRemovalMode(HtmlTagRemovalMode.Unwrap)).toThrow(readOnlyError);
    });

    it("EditMethods_OnPresets_ThrowInvalidOperationException", () => {
        expect(() => HtmlSanitizeOptions.simple.allowTag("font")).toThrow(readOnlyError);
        expect(() => HtmlSanitizeOptions.rich.disallowTag("img")).toThrow(readOnlyError);
    });

    it("MakeReadOnly_ReturnsSameInstanceAndCanBeCalledAgain", () => {
        const options = new HtmlSanitizeOptions();

        expect(options.makeReadOnly()).toBe(options);
        expect(options.makeReadOnly()).toBe(options);
        expect(options.isReadOnly).toBe(true);
    });

    it("CopyOfPreset_IsEditableAndDoesNotChangePreset", () => {
        const copy = new HtmlSanitizeOptions(HtmlSanitizeOptions.simple);

        expect(copy.isReadOnly).toBe(false);

        copy.allowTag("font").allowAttribute("b", "data-x").setDefaultRemovalMode(HtmlTagRemovalMode.Unwrap);

        expect(sanitizeHtml("<font>a</font><b data-x=\"1\">b</b><blink>c</blink>", copy)).toBe("<font>a</font><b data-x=\"1\">b</b>c");
        expect(sanitizeHtml("<font>a</font><b data-x=\"1\">b</b><blink>c</blink>", HtmlSanitizeOptions.simple)).toBe("<b>b</b>");
    });

    it("CopyOfCopy_SharesNoTagAttributeSets", () => {
        const source = new HtmlSanitizeOptions(HtmlSanitizeOptions.simple).allowAttribute("span", "data-identifier");
        const copy = new HtmlSanitizeOptions(source).disallowAttribute("span", "data-identifier");

        expect(sanitizeHtml("<span data-identifier=\"1\">a</span>", source)).toBe("<span data-identifier=\"1\">a</span>");
        expect(sanitizeHtml("<span data-identifier=\"1\">a</span>", copy)).toBe("<span>a</span>");
    });

    it("CopyConstructor_NullSource_ThrowsArgumentNullException", () => {
        expect(() => new HtmlSanitizeOptions(null as unknown as HtmlSanitizeOptions)).toThrow();
    });

    it("Sanitize_MakesOptionsReadOnly", () => {
        const options = new HtmlSanitizeOptions().allowTag("b");

        expect(sanitizeHtml("<b>x</b>", options)).toBe("<b>x</b>");
        expect(options.isReadOnly).toBe(true);
    });

    it("Sanitize_FastPathInput_StillMakesOptionsReadOnly", () => {
        const options = new HtmlSanitizeOptions();

        expect(sanitizeHtml("plain", options)).toBe("plain");
        expect(options.isReadOnly).toBe(true);
    });

    it("Sanitize_NullOptions_ThrowsArgumentNullException", () => {
        const nullOptions = null as unknown as HtmlSanitizeOptions;

        expect(() => sanitizeHtml("<b>x</b>", nullOptions)).toThrow();
        expect(() => sanitizeHtml(null, nullOptions)).toThrow();
    });

    it.each([
        "script", "SCRIPT", "style", "iframe", "object", "embed", "form", "link", "meta", "base", "noscript", "template",
        "frame", "frameset", "applet", "xmp", "noembed", "noframes", "plaintext", "title", "animate", "set", "foreignObject", "annotation-xml"
    ])("NeverPassElement_CanNotBeAllowedOrGivenARemovalMode %s", tag => {
        const options = new HtmlSanitizeOptions();

        expect(() => options.allowTag(tag)).toThrow();
        expect(() => options.setRemovalMode(tag, HtmlTagRemovalMode.Unwrap)).toThrow();
        expect(() => options.setRemovalMode(tag, HtmlTagRemovalMode.Drop)).toThrow();
    });

    it.each(["svg", "MATH"])("ForeignContentRoot_CanBeAllowedButNotGivenARemovalMode %s", tag => {
        const options = new HtmlSanitizeOptions();

        expect(options.allowTag(tag)).toBe(options);
        expect(() => options.setRemovalMode(tag, HtmlTagRemovalMode.Unwrap)).toThrow();
        expect(() => options.setRemovalMode(tag, HtmlTagRemovalMode.Drop)).toThrow();
    });

    it.each(["onclick", "OnMouseOver", "on", "formaction", "FormAction", "srcdoc", "SRCDOC", "archive", "ping", "srcset"])("AllowAttribute_FixedRuleAttribute_ThrowsArgumentException %s", attribute => {
        const options = new HtmlSanitizeOptions();

        expect(() => options.allowAttribute("img", attribute)).toThrow();
        expect(() => options.allowAttribute("", attribute)).toThrow();
    });

    it.each(["javascript", "JavaScript", "vbscript", "file", "data"])("AllowUrlScheme_BlockedScheme_ThrowsArgumentException %s", scheme => {
        expect(() => new HtmlSanitizeOptions().allowUrlScheme(scheme)).toThrow();
    });

    it.each(["behavior", "BEHAVIOR", "-moz-binding"])("AllowCssProperty_NeverAllowedProperty_ThrowsArgumentException %s", property => {
        expect(() => new HtmlSanitizeOptions().allowCssProperty(property)).toThrow();
    });

    it.each([null, "", "  "])("BlankName_ThrowsArgumentException %p", blank => {
        const options = new HtmlSanitizeOptions();
        const name = blank as string;

        expect(() => options.allowTag(name)).toThrow();
        expect(() => options.disallowTag(name)).toThrow();
        expect(() => options.setRemovalMode(name, HtmlTagRemovalMode.Unwrap)).toThrow();
        expect(() => options.allowAttribute("span", name)).toThrow();
        expect(() => options.allowAttribute("", name)).toThrow();
        expect(() => options.disallowAttribute("span", name)).toThrow();
        expect(() => options.disallowAttribute("", name)).toThrow();
        expect(() => options.allowUrlScheme(name)).toThrow();
        expect(() => options.disallowUrlScheme(name)).toThrow();
        expect(() => options.allowCssProperty(name)).toThrow();
        expect(() => options.disallowCssProperty(name)).toThrow();
    });

    it.each([null, "", "  "])("AllowAttribute_BlankTag_AllowsOnEveryTag %p", tag => {
        const options = new HtmlSanitizeOptions().allowTag("b").allowTag("i").allowAttribute(tag, "data-x");

        expect(sanitizeHtml("<b data-x=\"1\">a</b><i data-x=\"2\">b</i>", options)).toBe("<b data-x=\"1\">a</b><i data-x=\"2\">b</i>");
    });

    it.each([null, "", "  "])("DisallowAttribute_BlankTag_RemovesFromEveryTagAndEachTag %p", tag => {
        const options = new HtmlSanitizeOptions(HtmlSanitizeOptions.simple)
            .allowAttribute("a", "title")
            .disallowAttribute(tag, "title");

        expect(sanitizeHtml("<a href=\"/x\" title=\"t\">a</a><b title=\"t\">b</b>", options)).toBe("<a href=\"/x\">a</a><b>b</b>");
    });

    it("DisallowAttribute_TagWithEveryTagAttribute_ThrowsArgumentException", () => {
        const options = new HtmlSanitizeOptions(HtmlSanitizeOptions.simple);

        expect(() => options.disallowAttribute("a", "title")).toThrow(/every tag/);
        expect(() => options.disallowAttribute("span", "CLASS")).toThrow(/every tag/);
    });

    it("UndefinedRemovalMode_ThrowsArgumentOutOfRangeException", () => {
        const options = new HtmlSanitizeOptions();
        const undefinedMode = 2 as HtmlTagRemovalMode;

        expect(() => options.setRemovalMode("font", undefinedMode)).toThrow(/not a defined value/);
        expect(() => options.setDefaultRemovalMode(undefinedMode)).toThrow(/not a defined value/);
    });
});

describe("htmlSanitizationTables", () => {
    const tableValues = tables as unknown as Record<string, unknown>;
    const svgBase = readCSharpStrings(/CreateSvgSet\( params string\[\] geometryAttributes \)[\s\S]*?CreateSet\(([^)]*)\)/.exec(tablesSource)?.[1] ?? "");
    const csharpSets = new Map<string, string[]>();

    for (const match of allMatches(/HashSet<string> (\w+) = CreateSet\(([^;]*)\);/, tablesSource)) {
        csharpSets.set(match[1], readCSharpStrings(match[2]));
    }

    it("MaxNestingDepth matches", () => {
        expect(tables.maxNestingDepth).toBe(Number(/const int MaxNestingDepth = (\d+);/.exec(tablesSource)?.[1]));
    });

    it("MaxEstimatedNestingDepth matches", () => {
        expect(tables.maxEstimatedNestingDepth).toBe(Number(/const int MaxEstimatedNestingDepth = (\d+);/.exec(tablesSource)?.[1]));
    });

    it("reads the C# tables", () => {
        expect(svgBase.length).toBeGreaterThan(0);
        expect(csharpSets.size).toBeGreaterThan(15);
    });

    it.each(Array.from(csharpSets.keys()))("%s matches", name => {
        const set = tableValues[toCamelCase(name)];

        expect(set).toBeInstanceOf(Set);
        expect(sorted(set as Set<string>, true)).toEqual(sorted(csharpSets.get(name) as string[], true));
    });

    it.each(["SimpleTagAttributes", "RichOnlyTagAttributes"])("%s matches", name => {
        const body = new RegExp(`Dictionary<string, HashSet<string>> ${name} = [^{]*\\{([\\s\\S]*?)\\};`).exec(tablesSource)?.[1] ?? "";
        const expected: Record<string, string[]> = {};

        for (const entry of allMatches(/\["(\w+)"\] = (CreateSet|CreateSvgSet)\(([^)]*)\)/, body)) {
            const attributes = readCSharpStrings(entry[3]);
            expected[entry[1].toLowerCase()] = sorted(entry[2] === "CreateSvgSet" ? svgBase.concat(attributes) : attributes, true);
        }

        const actual: Record<string, string[]> = {};
        (tableValues[toCamelCase(name)] as Map<string, Set<string>>).forEach((attributes, tag) => {
            actual[tag] = sorted(attributes, true);
        });

        expect(Object.keys(expected).length).toBeGreaterThan(0);
        expect(actual).toEqual(expected);
    });

    it("FormMethodNames matches, case-sensitively", () => {
        const body = /FormMethodNames = new HashSet<string>\( StringComparer\.Ordinal \)\s*\{([\s\S]*?)\};/.exec(tablesSource)?.[1] ?? "";
        const expected = readCSharpStrings(body);

        expect(expected.length).toBeGreaterThan(50);
        expect(sorted(tables.formMethodNames, false)).toEqual(sorted(expected, false));
    });

    it("RejectedCssValueText matches", () => {
        const body = /RejectedCssValueText = \{([^}]*)\}/.exec(tablesSource)?.[1] ?? "";

        expect(Array.from(tables.rejectedCssValueText)).toEqual(readCSharpStrings(body));
    });

    it("the regexes match", () => {
        const pattern = (name: string): string => readCSharpStrings(new RegExp(`Regex ${name} = new Regex\\( (@"(?:[^"]|"")*")`).exec(tablesSource)?.[1] ?? "")[0];

        expect(tables.schemeRegex.source).toBe(pattern("SchemeRegex"));
        expect(tables.schemeRegex.flags).toBe("i");
        expect(tables.cssExpression.source).toBe(pattern("CssExpression"));
        expect(tables.cssExpression.flags).toBe("");
        expect(tables.cssUrl.source).toBe(pattern("CssUrl").split("(?>\\s*)").join(`${tables.whiteSpaceClass}*`));
    });

    it("every TypeScript set has a C# counterpart", () => {
        const csharpNames = new Set(Array.from(csharpSets.keys()).map(toCamelCase).concat(["simpleTagAttributes", "richOnlyTagAttributes", "formMethodNames"]));
        const typeScriptNames = Object.keys(tableValues).filter(name => tableValues[name] instanceof Set || tableValues[name] instanceof Map);

        expect(typeScriptNames.filter(name => !csharpNames.has(name))).toEqual([]);
    });
});
