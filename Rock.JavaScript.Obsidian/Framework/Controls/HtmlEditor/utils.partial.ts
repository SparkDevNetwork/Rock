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

import { Editor } from "@Obsidian/Libs/tinymce";

/** Gets a button element from the toolbar. This should only be called after the editor is initialized. */
export function getToolbarButton(tooltip: string, parent?: HTMLElement | undefined): HTMLElement | null | undefined {
    // The only identifier TinyMCE adds to a toolbar button is the title (tooltip) attribute.
    // This function will need to be updated if a newer version of the TinyMCE library
    // is used and does things differently (doesn't add the tox-tbtn class or set the title attribute).
    return (parent ?? document).querySelector(`.tox-tbtn[title='${tooltip}']`) as HTMLElement;
}

/** Sets the enabled/disabled state of the editor. */
export function setEditorEnabled(editor: Editor, isEnabled: boolean): void {
    // Enable/disable the toolbar and border.
    editor.ui?.setEnabled(isEnabled);

    // Enable/disable the content.
    const editorBody = editor.getBody();
    if (editorBody) {
        editorBody.setAttribute("contenteditable", `${isEnabled}`);
    }
}

/** Extracts <style> elements from an HTML string. */
function getStyleSheetStrings(html: string): string[] {
    return html.match(/<style.*>((?!<\/style>).)*<\/style>/sg) ??  [];
}

/** Scopes CSS style rules by adding a prefix to each rule selector. */
function scopeCSSStyleRule(cssRule: CSSStyleRule, cssRuleSelectorPrefix: string): string {
    cssRule.selectorText = cssRule.selectorText
        // Split multiple CSS rule selectors; e.g., `*, body, p`
        .split(",")
        // Trim white space from each selector.
        .map(selector => selector.trim())
        // Omit empty selectors.
        .filter(selector => !!selector)
        // Add prefixes to selectors that don't already have them.
        .map(selector => {
            if (!selector.includes(cssRuleSelectorPrefix)) {
                return `${cssRuleSelectorPrefix} ${selector}`;
            }
            else {
                return selector;
            }
        })
        // Join the selectors back together.
        .join(", ");

    return cssRule.cssText;
}

/**
 * Scopes CSS rules.
 * This also works for CSS media queries.
 * @example
 * // if cssRuleSelectorPrefix === "#someid" then
 * p {
 *   color: red;
 * }
 * // will become
 * #someid p {
 *   color: red;
 * }
 */
function scopeCSSRules(cssRules: CSSRuleList, cssRuleSelectorPrefix: string): string[] {
    const rulesCssText: string[] = [];

    for (let i = 0; i < cssRules.length; i++) {
        const rule = cssRules[i];
        if (rule instanceof CSSStyleRule) {
            scopeCSSStyleRule(rule, cssRuleSelectorPrefix);
        }
        else if (rule instanceof CSSMediaRule) {
            scopeCSSRules(rule.cssRules, cssRuleSelectorPrefix);
        }
        else {
            // This isn't a CSS rule so no prefix necessary?
        }

        // Keep track of the updated CSS rules.
        // These will be joined later when replacing the <style> tag with the scoped version.
        rulesCssText.push(rule.cssText);
    }

    return rulesCssText;
}

/**
 * Adds a prefix to every CSS rule selector in the provided HTML <style> elements.
 */
export function scopeStyleSheets(html: string, scopeIdentifier: string): string {
    if (!html) {
        return html;
    }

    const styleSheetStrings = getStyleSheetStrings(html);
    styleSheetStrings.forEach((styleSheetString: string) => {
        // Convert the <style> HTML string to a CSSStyleSheet object.
        const cssStyleSheet = document.createElement("style");
        const openingTag = styleSheetString.match(/^<style.*>/)?.[0] ?? "<style>";
        cssStyleSheet.innerHTML = styleSheetString.replace(openingTag, "").replace("</style>", "").replace(/\n\s+/g, "\n");
        document.body.appendChild(cssStyleSheet);
        const {sheet} = cssStyleSheet;
        document.body.removeChild(cssStyleSheet);

        // Add the prefix to each CSS rule selector.
        if (sheet) {
            const rulesCssText = scopeCSSRules(sheet.cssRules, scopeIdentifier);

            // Replace the <style> tag with the new stylesheet.
            html = html.replace(styleSheetString, `${openingTag}\n${rulesCssText.join("\n")}\n</style>`);
        }
    });

    return html;
}

/** Returns the HTML with scoped style sheets removed. */
export function unscopeStyleSheets(html: string, cssRuleSelectorPrefix: string): string {
    if (!html) {
        return html;
    }

    const styleSheetStrings = getStyleSheetStrings(html);

    styleSheetStrings.forEach((styleSheetString: string) => {
        const styleSheetStringWithoutPrefixes = styleSheetString.replace(new RegExp(`${cssRuleSelectorPrefix}[\t ]+`, "g"), "").replace(new RegExp(cssRuleSelectorPrefix, "g"), "");
        html = html.replace(styleSheetString, styleSheetStringWithoutPrefixes);
    });

    return html;
}