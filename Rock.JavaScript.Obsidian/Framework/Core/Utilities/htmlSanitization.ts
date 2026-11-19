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

// NOTICE!!!
// This file has a C# version in the Rock project (Rock.Utility.HtmlSanitization).
// If changes are made to one the same changes must be made to the other.
// The Jest tests run the C# test rows against this file.
// NOTICE!!!

import {
    allowedTargets,
    cssExpression,
    cssUrl,
    dropOnlyElements,
    foreignContentBreakoutElements,
    impliedEndElements,
    maxEstimatedNestingDepth,
    maxNestingDepth,
    formMethodNames,
    mathMlTextIntegrationPoints,
    neverAllowedAttributes,
    neverAllowedCssProperties,
    neverPassElements,
    blockedUrlSchemes,
    rejectedCssValueText,
    richOnlyCssProperties,
    richOnlyElements,
    richOnlyEveryTagAttributes,
    richOnlyTagAttributes,
    schemeRegex,
    simpleCssProperties,
    simpleElements,
    simpleEveryTagAttributes,
    simpleTagAttributes,
    svgHtmlIntegrationPoints,
    urlAttributes,
    urlListAttributes,
    urlSchemes,
    voidElements,
    whiteSpaceClass
} from "./htmlSanitizationTables.partial";

/**
 * What the allowlist sanitizer does with an element that the options do not
 * allow. Elements that must never pass are always dropped with their
 * content, whatever the mode.
 */
export const HtmlTagRemovalMode = {
    /** Removes the element together with all of its content. */
    Drop: 0,

    /** Removes the element but keeps its sanitized child nodes in its place. */
    Unwrap: 1
} as const;

/**
 * What the allowlist sanitizer does with an element that the options do not
 * allow. Elements that must never pass are always dropped with their
 * content, whatever the mode.
 */
export type HtmlTagRemovalMode = typeof HtmlTagRemovalMode[keyof typeof HtmlTagRemovalMode];

const htmlNamespace = "http://www.w3.org/1999/xhtml";
const svgNamespace = "http://www.w3.org/2000/svg";
const mathMlNamespace = "http://www.w3.org/1998/Math/MathML";

const documentPrefix = "<!doctype html><html><body>";

/**
 * Input without any of these characters serializes to itself, so it skips
 * the parser.
 */
const markupCharacters = /[<>&\u00a0\0]/;

const leadingWhiteSpace = new RegExp(`^${whiteSpaceClass}+`);
const trailingWhiteSpace = new RegExp(`${whiteSpaceClass}+$`);
const anyWhiteSpace = new RegExp(whiteSpaceClass, "g");

type OptionsState = {
    allowedTags: Set<string>;
    everyTagAttributes: Set<string>;
    tagAttributes: Map<string, Set<string>>;
    areDataAttributesAllowed: boolean;
    urlSchemes: Set<string>;
    cssProperties: Set<string>;
    removalModes: Map<string, HtmlTagRemovalMode>;
    defaultRemovalMode: HtmlTagRemovalMode;
    isReadOnly: boolean;
};

/**
 * The rules live outside the class so that the sanitizer can read them
 * without making them part of the public API.
 */
const optionsStates = new WeakMap<HtmlSanitizeOptions, OptionsState>();

/**
 * The whole configuration for one call to {@link sanitizeHtml}. Every rule is
 * a set of allowed values: anything not allowed is removed. Start from a
 * read-only preset ({@link HtmlSanitizeOptions.simple} or
 * {@link HtmlSanitizeOptions.rich}), edit a copy made with the copy
 * constructor, then call {@link HtmlSanitizeOptions.makeReadOnly} and keep
 * the result in a module-level constant.
 *
 * The fixed rules (elements that never pass, `on*` attributes, blocked URL
 * schemes, never-allowed CSS properties) live in the sanitizer. The edit
 * methods throw when asked to loosen one of them.
 *
 * This API is internal to Rock, and is not subject to the same compatibility
 * standards as public APIs. It may be changed or removed without notice in any
 * release. You should not use this API directly in any plug-ins. Doing so can
 * result in application failures when updating to a new Rock release.
 */
export class HtmlSanitizeOptions {
    /**
     * The read-only preset for small snippets: inline formatting, links,
     * paragraphs, line breaks and `div`. No tables, images, `id`, `data-*`
     * or positional CSS.
     */
    public static readonly simple: HtmlSanitizeOptions = new HtmlSanitizeOptions()
        .addGroups(simpleElements, simpleEveryTagAttributes, simpleTagAttributes, urlSchemes, simpleCssProperties)
        .makeReadOnly();

    /**
     * The read-only preset for larger staff-authored templates: everything in
     * {@link HtmlSanitizeOptions.simple} plus headings, lists, HTML5 section
     * elements, tables, images, form controls, SVG icons, `id`, `data-*` and
     * positional, table and flex CSS.
     */
    public static readonly rich: HtmlSanitizeOptions = new HtmlSanitizeOptions(HtmlSanitizeOptions.simple)
        .addGroups(richOnlyElements, richOnlyEveryTagAttributes, richOnlyTagAttributes, [], richOnlyCssProperties)
        .allowDataAttributes()
        .makeReadOnly();

    /**
     * Creates editable options. With no source they allow no elements, so
     * only text is kept. With a source they are an editable copy of it, even
     * when the source is read-only, and share no state with it.
     *
     * @param source The options to copy.
     */
    public constructor(source?: HtmlSanitizeOptions) {
        if (source === null) {
            throw new Error("The source options can not be null.");
        }

        const sourceState = source ? getState(source) : undefined;
        const tagAttributes = new Map<string, Set<string>>();

        sourceState?.tagAttributes.forEach((attributes, tag) => {
            tagAttributes.set(tag, new Set(attributes));
        });

        optionsStates.set(this, {
            allowedTags: new Set(sourceState?.allowedTags),
            everyTagAttributes: new Set(sourceState?.everyTagAttributes),
            tagAttributes,
            areDataAttributesAllowed: sourceState?.areDataAttributesAllowed ?? false,
            urlSchemes: new Set(sourceState?.urlSchemes),
            cssProperties: new Set(sourceState?.cssProperties),
            removalModes: new Map(sourceState?.removalModes),
            defaultRemovalMode: sourceState?.defaultRemovalMode ?? HtmlTagRemovalMode.Drop,
            isReadOnly: false
        });
    }

    /** `true` when every edit method throws. */
    public get isReadOnly(): boolean {
        return getState(this).isReadOnly;
    }

    /**
     * Prevents any further edits. Calling it again is harmless.
     *
     * @returns This instance.
     */
    public makeReadOnly(): this {
        getState(this).isReadOnly = true;

        return this;
    }

    /**
     * Allows an element.
     *
     * @param tag The tag name.
     * @returns This instance.
     * @throws The options are read-only, or `tag` is blank or names an element that must never pass.
     */
    public allowTag(tag: string): this {
        const state = this.getEditableState();
        throwIfBlank(tag, "tag");
        throwIfNeverPass(tag);

        state.allowedTags.add(tag.toLowerCase());

        return this;
    }

    /**
     * Stops allowing an element.
     *
     * @param tag The tag name.
     * @returns This instance.
     * @throws The options are read-only, or `tag` is blank.
     */
    public disallowTag(tag: string): this {
        const state = this.getEditableState();
        throwIfBlank(tag, "tag");

        state.allowedTags.delete(tag.toLowerCase());

        return this;
    }

    /**
     * Allows an attribute on one tag, or on every tag when `tag` is blank.
     *
     * @param tag The tag name, or a blank value for every tag.
     * @param attribute The attribute name.
     * @returns This instance.
     * @throws The options are read-only, or `attribute` is blank, is an `on*`
     * event handler, is `formaction` or `srcdoc`, or is a URL-list attribute
     * (`archive`, `ping`, `srcset`).
     */
    public allowAttribute(tag: string | null | undefined, attribute: string): this {
        const state = this.getEditableState();
        throwIfBlank(attribute, "attribute");

        const name = attribute.toLowerCase();

        if (name.startsWith("on")) {
            throw new Error(`Event handler attributes such as '${attribute}' can never be allowed.`);
        }

        if (neverAllowedAttributes.has(name)) {
            throw new Error(`The '${attribute}' attribute can never be allowed.`);
        }

        if (urlListAttributes.has(name)) {
            throw new Error(`The URL-list attribute '${attribute}' can not be allowed because its entries are not checked.`);
        }

        if (isBlank(tag)) {
            state.everyTagAttributes.add(name);
        }
        else {
            const key = (tag as string).toLowerCase();
            let attributes = state.tagAttributes.get(key);

            if (!attributes) {
                attributes = new Set();
                state.tagAttributes.set(key, attributes);
            }

            attributes.add(name);
        }

        return this;
    }

    /**
     * Stops allowing an attribute on one tag, or on every tag when `tag` is
     * blank. A blank tag also removes the attribute from every tag's own set.
     *
     * @param tag The tag name, or a blank value for every tag.
     * @param attribute The attribute name.
     * @returns This instance.
     * @throws The options are read-only, or `attribute` is blank, or `tag` is
     * not blank and the attribute is allowed on every tag, because one tag can
     * not opt out of an every-tag attribute.
     */
    public disallowAttribute(tag: string | null | undefined, attribute: string): this {
        const state = this.getEditableState();
        throwIfBlank(attribute, "attribute");

        const name = attribute.toLowerCase();

        if (isBlank(tag)) {
            state.everyTagAttributes.delete(name);
            state.tagAttributes.forEach(attributes => attributes.delete(name));

            return this;
        }

        if (state.everyTagAttributes.has(name)) {
            throw new Error(`The attribute '${attribute}' is allowed on every tag, so it can not be disallowed for '${tag}' alone. Disallow it with a blank tag instead.`);
        }

        state.tagAttributes.get((tag as string).toLowerCase())?.delete(name);

        return this;
    }

    /**
     * Allows `data-*` attributes on every allowed tag.
     *
     * @returns This instance.
     * @throws The options are read-only.
     */
    public allowDataAttributes(): this {
        this.getEditableState().areDataAttributesAllowed = true;

        return this;
    }

    /**
     * Stops allowing `data-*` attributes, except those allowed by name.
     *
     * @returns This instance.
     * @throws The options are read-only.
     */
    public disallowDataAttributes(): this {
        this.getEditableState().areDataAttributesAllowed = false;

        return this;
    }

    /**
     * Allows a URL scheme in URL attributes and CSS `url()` values. Relative
     * URLs are always allowed.
     *
     * @param scheme The scheme, without the colon.
     * @returns This instance.
     * @throws The options are read-only, or `scheme` is blank or is a blocked scheme.
     */
    public allowUrlScheme(scheme: string): this {
        const state = this.getEditableState();
        throwIfBlank(scheme, "scheme");

        if (blockedUrlSchemes.has(scheme.toLowerCase())) {
            throw new Error(`The URL scheme '${scheme}' can never be allowed.`);
        }

        state.urlSchemes.add(scheme.toLowerCase());

        return this;
    }

    /**
     * Stops allowing a URL scheme.
     *
     * @param scheme The scheme, without the colon.
     * @returns This instance.
     * @throws The options are read-only, or `scheme` is blank.
     */
    public disallowUrlScheme(scheme: string): this {
        const state = this.getEditableState();
        throwIfBlank(scheme, "scheme");

        state.urlSchemes.delete(scheme.toLowerCase());

        return this;
    }

    /**
     * Allows a CSS property in `style` attributes.
     *
     * @param property The property name.
     * @returns This instance.
     * @throws The options are read-only, or `property` is blank or is a never-allowed property.
     */
    public allowCssProperty(property: string): this {
        const state = this.getEditableState();
        throwIfBlank(property, "property");

        if (neverAllowedCssProperties.has(property.toLowerCase())) {
            throw new Error(`The CSS property '${property}' can never be allowed.`);
        }

        state.cssProperties.add(property.toLowerCase());

        return this;
    }

    /**
     * Stops allowing a CSS property.
     *
     * @param property The property name.
     * @returns This instance.
     * @throws The options are read-only, or `property` is blank.
     */
    public disallowCssProperty(property: string): this {
        const state = this.getEditableState();
        throwIfBlank(property, "property");

        state.cssProperties.delete(property.toLowerCase());

        return this;
    }

    /**
     * Sets what happens to one element when it is not allowed.
     *
     * @param tag The tag name.
     * @param mode The removal mode.
     * @returns This instance.
     * @throws The options are read-only, or `tag` is blank, names an element
     * that must never pass, names `svg` or `math` (always dropped when not
     * allowed), or `mode` is not a defined value.
     */
    public setRemovalMode(tag: string, mode: HtmlTagRemovalMode): this {
        const state = this.getEditableState();
        throwIfBlank(tag, "tag");
        throwIfNeverPass(tag);
        throwIfUndefinedMode(mode);

        if (dropOnlyElements.has(tag.toLowerCase())) {
            throw new Error(`The element '${tag}' is always removed with its content when it is not allowed, so its removal mode can not be set.`);
        }

        state.removalModes.set(tag.toLowerCase(), mode);

        return this;
    }

    /**
     * Sets what happens to elements that are not allowed and have no mode of
     * their own. The default is {@link HtmlTagRemovalMode.Drop}.
     *
     * @param mode The removal mode.
     * @returns This instance.
     * @throws The options are read-only, or `mode` is not a defined value.
     */
    public setDefaultRemovalMode(mode: HtmlTagRemovalMode): this {
        const state = this.getEditableState();
        throwIfUndefinedMode(mode);

        state.defaultRemovalMode = mode;

        return this;
    }

    /**
     * Adds groups of preset data through the edit methods, so the presets
     * pass the same fixed-rule checks as any caller's options.
     */
    private addGroups(tags: Iterable<string>, everyTagAttributes: Iterable<string>, tagAttributes: ReadonlyMap<string, Iterable<string>>, schemes: Iterable<string>, cssProperties: Iterable<string>): this {
        Array.from(tags).forEach(tag => this.allowTag(tag));
        Array.from(everyTagAttributes).forEach(attribute => this.allowAttribute("", attribute));
        tagAttributes.forEach((attributes, tag) => {
            Array.from(attributes).forEach(attribute => this.allowAttribute(tag, attribute));
        });
        Array.from(schemes).forEach(scheme => this.allowUrlScheme(scheme));
        Array.from(cssProperties).forEach(property => this.allowCssProperty(property));

        return this;
    }

    private getEditableState(): OptionsState {
        const state = getState(this);

        if (state.isReadOnly) {
            throw new Error("These HTML sanitize options are read-only. Edit a copy made with the copy constructor instead.");
        }

        return state;
    }
}

function getState(options: HtmlSanitizeOptions): OptionsState {
    const state = optionsStates.get(options);

    if (!state) {
        throw new Error("The options were not created by the HtmlSanitizeOptions constructor.");
    }

    return state;
}

function isBlank(value: string | null | undefined): boolean {
    return value === null || value === undefined || trim(value) === "";
}

function throwIfBlank(value: string | null | undefined, parameterName: string): void {
    if (isBlank(value)) {
        throw new Error(`The ${parameterName} can not be null or blank.`);
    }
}

function throwIfNeverPass(tag: string): void {
    if (neverPassElements.has(tag.toLowerCase())) {
        throw new Error(`The element '${tag}' is always removed with its content and can not be configured.`);
    }
}

function throwIfUndefinedMode(mode: HtmlTagRemovalMode): void {
    if (mode !== HtmlTagRemovalMode.Drop && mode !== HtmlTagRemovalMode.Unwrap) {
        throw new Error(`The removal mode '${mode}' is not a defined value.`);
    }
}

/** Trims the characters .NET's `string.Trim()` trims. */
function trim(value: string): string {
    return value.replace(leadingWhiteSpace, "").replace(trailingWhiteSpace, "");
}

// #region Sanitizer

/**
 * What the walk does with one node: `remove` it with its content, `keep` it
 * as it is, `clean` its attributes and walk its children, or `unwrap` it by
 * walking its children and then replacing it with them.
 */
type NodeAction = "remove" | "keep" | "clean" | "unwrap";

/**
 * Sanitizes HTML against an allowlist. The input is parsed as a document body
 * into an inert document, everything the options and the fixed rules do not
 * allow is removed, and the body content is serialized.
 *
 * This does not replace server-side sanitization: use it only for HTML that
 * exists only in the browser.
 *
 * This API is internal to Rock, and is not subject to the same compatibility
 * standards as public APIs. It may be changed or removed without notice in any
 * release. You should not use this API directly in any plug-ins. Doing so can
 * result in application failures when updating to a new Rock release.
 *
 * @param html The HTML to sanitize.
 * @param options The allow rules. They become read-only.
 * @returns The sanitized HTML, or the input when it is `null`, `undefined` or empty.
 * @throws `options` is `null` or `undefined`.
 */
export function sanitizeHtml(html: string | null | undefined, options: HtmlSanitizeOptions): string | null | undefined {
    if (options === null || options === undefined) {
        throw new Error("The options can not be null.");
    }

    options.makeReadOnly();

    if (!html || !markupCharacters.test(html)) {
        return html;
    }

    if (isTooDeeplyNested(html)) {
        return encodeAsText(html);
    }

    // A DOMParser document loads nothing and runs nothing, unlike an element
    // of the live page, which would fetch images and run onerror handlers.
    const body = new DOMParser().parseFromString(documentPrefix + html, "text/html").body;

    sanitizeChildNodes(body, getState(options), 1);

    return body.innerHTML;
}

/**
 * Estimates, without parsing, whether the input nests elements deeper than
 * {@link maxEstimatedNestingDepth}. The scan follows the tokenizer closely
 * enough that markup can not hide its depth: end tags close only an open
 * element of the same name, quoted attribute values are skipped, and comments
 * end where the parser ends them. It overestimates rather than underestimates.
 */
function isTooDeeplyNested(html: string): boolean {
    const openElements: string[] = [];
    const openCounts = new Map<string, number>();
    let position = html.indexOf("<");

    while (position >= 0 && position < html.length) {
        const next = position + 1 < html.length ? html[position + 1] : "\0";

        if (next === "!") {
            position = skipComment(html, position);
        }
        else if (next === "?") {
            position = skipPast(html, ">", position + 2);
        }
        else if (next === "/" && position + 2 < html.length && isAsciiLetter(html[position + 2])) {
            const nameEnd = findTagNameEnd(html, position + 2);
            const name = html.substring(position + 2, nameEnd).toLowerCase();

            if ((openCounts.get(name) ?? 0) > 0) {
                while (openElements.length > 0) {
                    const top = openElements.pop() as string;

                    openCounts.set(top, (openCounts.get(top) ?? 1) - 1);

                    if (top === name) {
                        break;
                    }
                }
            }

            position = skipTag(html, nameEnd);
        }
        else if (isAsciiLetter(next)) {
            const nameEnd = findTagNameEnd(html, position + 1);
            const name = html.substring(position + 1, nameEnd).toLowerCase();

            if (!voidElements.has(name) && !impliedEndElements.has(name)) {
                openElements.push(name);
                openCounts.set(name, (openCounts.get(name) ?? 0) + 1);

                if (openElements.length > maxEstimatedNestingDepth) {
                    return true;
                }
            }

            position = skipTag(html, nameEnd);
        }
        else {
            position++;
        }

        position = position < html.length ? html.indexOf("<", position) : -1;
    }

    return false;
}

/**
 * Skips a comment or other `<!` construct the way the parser ends it: a
 * comment at the first `-->` or `--!>` (or at once for `<!-->` and `<!--->`),
 * and anything else, such as a doctype, at the first `>`.
 */
function skipComment(html: string, start: number): number {
    if (!html.startsWith("<!--", start)) {
        return skipPast(html, ">", start + 2);
    }

    if (html.startsWith(">", start + 4)) {
        return start + 5;
    }

    if (html.startsWith("->", start + 4)) {
        return start + 6;
    }

    return Math.min(skipPast(html, "-->", start + 4), skipPast(html, "--!>", start + 4));
}

/**
 * Skips the rest of a start or end tag, stepping over quoted attribute values
 * so a `>` or tag inside one is not mistaken for markup.
 */
function skipTag(html: string, position: number): number {
    while (position < html.length) {
        const c = html[position];

        if (c === ">") {
            return position + 1;
        }

        position++;

        if (c !== "=") {
            continue;
        }

        while (position < html.length && isHtmlWhiteSpace(html[position])) {
            position++;
        }

        if (position < html.length && (html[position] === "\"" || html[position] === "'")) {
            const close = html.indexOf(html[position], position + 1);

            position = close < 0 ? html.length : close + 1;
        }
    }

    return html.length;
}

/** Finds the end of a tag name: the first whitespace, `/` or `>`. */
function findTagNameEnd(html: string, position: number): number {
    while (position < html.length && html[position] !== "/" && html[position] !== ">" && !isHtmlWhiteSpace(html[position])) {
        position++;
    }

    return position;
}

/** Finds the position after the next occurrence of `value`, or the end of the input. */
function skipPast(html: string, value: string, position: number): number {
    const index = position < html.length ? html.indexOf(value, position) : -1;

    return index < 0 ? html.length : index + value.length;
}

function isAsciiLetter(c: string): boolean {
    return (c >= "a" && c <= "z") || (c >= "A" && c <= "Z");
}

function isHtmlWhiteSpace(c: string): boolean {
    return c === "\t" || c === "\n" || c === "\f" || c === "\r" || c === " ";
}

/**
 * Encodes input that is too deeply nested to parse as plain text, exactly as
 * the serializer would write a text node holding it, so sanitizing the result
 * again returns the same string.
 */
function encodeAsText(html: string): string {
    let result = "";

    for (let i = 0; i < html.length; i++) {
        const c = html[i];

        switch (c) {
            case "&":
                result += "&amp;";
                break;

            case "<":
                result += "&lt;";
                break;

            case ">":
                result += "&gt;";
                break;

            case "\u00a0":
                result += "&nbsp;";
                break;

            case "\0":
                break;

            case "\r":
                result += "\n";
                if (html[i + 1] === "\n") {
                    i++;
                }

                break;

            default:
                result += c;
                break;
        }
    }

    return result;
}

/**
 * Sanitizes the child nodes of `parent`, depth first. `depth` is how deep the
 * children are nested below the body; the body's own children are at 1.
 */
function sanitizeChildNodes(parent: Element, options: OptionsState, depth: number): void {
    for (const node of Array.from(parent.childNodes)) {
        if (node.nodeType === Node.ELEMENT_NODE && depth > maxNestingDepth) {
            parent.removeChild(node);
            continue;
        }

        switch (getNodeAction(node, parent, options)) {
            case "keep":
                break;

            case "clean":
                sanitizeAttributes(node as Element, options);
                sanitizeChildNodes(node as Element, options, depth + 1);
                break;

            case "unwrap":
                sanitizeChildNodes(node as Element, options, depth + 1);
                while (node.firstChild) {
                    parent.insertBefore(node.firstChild, node);
                }

                parent.removeChild(node);
                break;

            case "remove":
                parent.removeChild(node);
                break;
        }
    }
}

function getNodeAction(node: Node, parent: Element, options: OptionsState): NodeAction {
    if (node.nodeType === Node.TEXT_NODE) {
        return "keep";
    }

    // Comments, processing instructions, doctypes and any other node kind.
    if (node.nodeType !== Node.ELEMENT_NODE) {
        return "remove";
    }

    const element = node as Element;
    const name = element.localName.toLowerCase();

    if (neverPassElements.has(name) || !isNamespaceValid(element, parent)) {
        return "remove";
    }

    if (options.allowedTags.has(name)) {
        return "clean";
    }

    // Unwrapping inside SVG or MathML could move children into a namespace
    // the parser would not have put them in.
    if (element.namespaceURI !== htmlNamespace) {
        return "remove";
    }

    const mode = options.removalModes.get(name) ?? options.defaultRemovalMode;

    return mode === HtmlTagRemovalMode.Unwrap ? "unwrap" : "remove";
}

/**
 * Determines whether an element is in a namespace its parent could have
 * produced, the way DOMPurify checks namespaces. This catches trees that a
 * browser parses differently once serialized, which is how
 * namespace-confusion attacks get past sanitizers.
 */
function isNamespaceValid(element: Element, parent: Element): boolean {
    const parentNamespace = parent.namespaceURI;
    const parentName = parent.localName.toLowerCase();
    const name = element.localName.toLowerCase();

    if (element.namespaceURI === htmlNamespace) {
        if (parentNamespace === svgNamespace) {
            return svgHtmlIntegrationPoints.has(parentName);
        }

        if (parentNamespace === mathMlNamespace) {
            return mathMlTextIntegrationPoints.has(parentName);
        }

        return true;
    }

    if (foreignContentBreakoutElements.has(name)) {
        return false;
    }

    if (element.namespaceURI === svgNamespace) {
        if (parentNamespace === svgNamespace) {
            return true;
        }

        return name === "svg"
            && (parentNamespace === htmlNamespace || mathMlTextIntegrationPoints.has(parentName));
    }

    if (element.namespaceURI === mathMlNamespace) {
        return parentNamespace === mathMlNamespace
            || (name === "math" && parentNamespace === htmlNamespace);
    }

    return false;
}

function sanitizeAttributes(element: Element, options: OptionsState): void {
    const tag = element.localName.toLowerCase();

    for (const attribute of Array.from(element.attributes)) {
        const value = getSafeAttributeValue(tag, attribute.name, attribute.value, options);

        if (value === null) {
            element.removeAttributeNode(attribute);
        }
        else if (value !== attribute.value) {
            attribute.value = value;
        }
    }

    if (element.getAttribute("target")?.toLowerCase() === "_blank") {
        element.setAttribute("rel", "noopener noreferrer");
    }
}

/**
 * Gets the value an attribute keeps after sanitizing, or `null` to remove
 * the attribute.
 */
function getSafeAttributeValue(tag: string, attributeName: string, value: string, options: OptionsState): string | null {
    const name = attributeName.toLowerCase();

    if (name.startsWith("on")
        || !isAttributeAllowed(tag, name, options)
        || value.indexOf("&{") >= 0) {
        return null;
    }

    if (urlAttributes.has(name)) {
        return getSafeUrl(value, options);
    }

    if (name === "style") {
        return getSafeStyle(value, options);
    }

    if (name === "target") {
        return allowedTargets.has(value.toLowerCase()) ? value : null;
    }

    if (name === "name" || name === "id") {
        return isSafeFormMemberName(value) ? value : null;
    }

    return value;
}

function isAttributeAllowed(tag: string, name: string, options: OptionsState): boolean {
    return options.everyTagAttributes.has(name)
        || options.tagAttributes.get(tag)?.has(name) === true
        || (options.areDataAttributesAllowed && name.startsWith("data-"));
}

/**
 * Determines whether a `name` or `id` value can not hide a method of, or
 * inject a value into, the page-wide WebForms form.
 */
function isSafeFormMemberName(value: string): boolean {
    return value.indexOf("$") < 0
        && !value.startsWith("__")
        && !formMethodNames.has(value);
}

/**
 * Applies the URL rule, copied from Ganss.Xss `GetSafeIri`.
 *
 * @returns The URL without leading whitespace, or `null` when its scheme is not allowed.
 */
function getSafeUrl(url: string, options: OptionsState): string | null {
    const trimmedUrl = url.replace(leadingWhiteSpace, "");
    const match = schemeRegex.exec(trimmedUrl);

    if (match && !options.urlSchemes.has(match[1].toLowerCase())) {
        return null;
    }

    return trimmedUrl;
}

/**
 * Applies the CSS rule to a `style` attribute value.
 *
 * @returns The kept declarations joined by `"; "`, or `null` when none is kept.
 */
function getSafeStyle(style: string, options: OptionsState): string | null {
    const declarations: string[] = [];

    for (const declaration of splitCssDeclarations(style)) {
        const safeDeclaration = getSafeCssDeclaration(declaration, options);

        if (safeDeclaration !== null) {
            declarations.push(safeDeclaration);
        }
    }

    return declarations.length > 0 ? declarations.join("; ") : null;
}

/**
 * Splits a `style` value on the semicolons outside quotes and brackets.
 * Malformed declarations are skipped, so every returned declaration stands on
 * its own when the kept ones are joined again.
 */
function splitCssDeclarations(style: string): string[] {
    const declarations: string[] = [];
    const closers: string[] = [];
    let quote = "";
    let isMalformed = false;
    let start = 0;

    for (let i = 0; i <= style.length; i++) {
        const isEnd = i === style.length;

        if (isEnd || (style[i] === ";" && quote === "" && closers.length === 0)) {
            // An unclosed quote or bracket, a mismatched bracket or a newline
            // in a string would make the browser end this declaration
            // somewhere other than where we split it.
            if (!isMalformed && quote === "" && closers.length === 0) {
                declarations.push(style.substring(start, i));
            }

            closers.length = 0;
            quote = "";
            isMalformed = false;
            start = i + 1;
            continue;
        }

        const c = style[i];

        if (quote !== "") {
            if (c === quote) {
                quote = "";
            }
            else if (c === "\n" || c === "\r" || c === "\f") {
                isMalformed = true;
                quote = "";
            }
        }
        else if (c === "\"" || c === "'") {
            quote = c;
        }
        else if (c === "(") {
            closers.push(")");
        }
        else if (c === "[") {
            closers.push("]");
        }
        else if (c === "{") {
            closers.push("}");
        }
        else if (c === ")" || c === "]" || c === "}") {
            if (closers.length === 0 || closers.pop() !== c) {
                isMalformed = true;
            }
        }
    }

    return declarations;
}

/**
 * Applies the CSS rule to one declaration, given without its semicolon.
 *
 * @returns The declaration as `property: value`, or `null` when it is removed.
 */
function getSafeCssDeclaration(declaration: string, options: OptionsState): string | null {
    const colonIndex = declaration.indexOf(":");

    if (colonIndex < 0) {
        return null;
    }

    const property = trim(declaration.substring(0, colonIndex)).toLowerCase();
    const value = trim(declaration.substring(colonIndex + 1));

    if (value.length === 0
        || !options.cssProperties.has(property)
        || isFixedPosition(property, value)
        || rejectedCssValueText.some(text => value.indexOf(text) >= 0)
        || cssExpression.test(value)) {
        return null;
    }

    cssUrl.lastIndex = 0;

    for (let match = cssUrl.exec(value); match !== null; match = cssUrl.exec(value)) {
        const url = match[2].replace(anyWhiteSpace, "");

        if (url.length > 0 && getSafeUrl(url, options) === null) {
            return null;
        }
    }

    return `${property}: ${value}`;
}

/**
 * Determines whether a declaration is `position: fixed`, which would pin an
 * element over the whole page wherever it is scrolled.
 */
function isFixedPosition(property: string, value: string): boolean {
    if (property !== "position") {
        return false;
    }

    return trim(value.split("!important").join("")).toLowerCase() === "fixed";
}

// #endregion
