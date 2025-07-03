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

import { Liquid } from "@Obsidian/Libs/liquidjs";

const engine = new Liquid({
    cache: true
});

const hasLavaCommandFieldsRegex: RegExp = /\{%.+%\}/;
const hasLavaShortcodeFieldsRegex: RegExp = /\{\[.+\]\}/;

export const LavaTagHtmlCommentRegex: RegExp = /<!--\s*rock-lava-tag-(\w+)(?:\s+([\s\S]*?))?-->/;
const lavaTagHtmlCommentGlobalRegex: RegExp = new RegExp(LavaTagHtmlCommentRegex.source, "g");
const lavaTagRegex: RegExp = /\{%-?\s*(\w+)(?:\s+([\s\S]*?))?-?%\}/;
const lavaTagGlobalRegex: RegExp = new RegExp(lavaTagRegex.source, "g");

export function resolveMergeFields(template: string, mergeFields: Record<string, unknown>): string {
    const tpl = engine.parse(template);

    return engine.renderSync(tpl, mergeFields);
}

/** Determines whether the string potentially has lava command {% %} fields in it. */
export function hasLavaCommandFields(template: string): boolean {
    return hasLavaCommandFieldsRegex.test(template);
}

/** Determines whether the string potentially has lava tag HTML comments `<!-- rock-lava-tag-<name> [args] -->` in it. */
export function hasLavaTagHtmlComments(template: string): boolean {
    return LavaTagHtmlCommentRegex.test(template);
}

/** Determines whether the string potentially has lava shortcode {[ ]} fields in it. */
export function hasLavaShortcodeFields(template: string): boolean {
    return hasLavaShortcodeFieldsRegex.test(template);
}

/**
 * Replaces Lava tags in a template with HTML comments to prevent them from breaking HTML structure.
 *
 * @param template The template string containing Lava tags `{% %}`.
 * @returns The modified template string with Lava tags replaced by HTML comments.
 *
 * @example
 * replaceLavaTagsWithHtmlComments(`{% raw %}`); // `<!-- rock-lava-tag-raw -->`
 * replaceLavaTagsWithHtmlComments(`{% if Person.IsMember %}`); // `<!-- rock-lava-tag-if Person.IsMember -->`
 * replaceLavaTagsWithHtmlComments(`{% assign name = 'Ted Decker' %}`); // `<!-- rock-lava-tag-assign name = 'Ted Decker' -->`
 */
export function replaceLavaTagsWithHtmlComments(template: string): string {
    return template.replace(
        lavaTagGlobalRegex,
        "<!-- rock-lava-tag-$1 $2-->"
    );
}

/**
 * Replaces Lava HTML comments in a template with tags to prevent them from breaking HTML structure.
 *
 * @param template The template string containing Lava HTML comments `<!-- rock-lava-tag-<name> [args] -->`.
 * @returns The modified template string with Lava tags replaced by HTML comments.
 *
 * @example
 * replaceLavaTagsWithHtmlComments(`<!-- rock-lava-tag-raw -->`); // `{% raw %}`
 * replaceLavaTagsWithHtmlComments(`<!-- rock-lava-tag-if Person.IsMember -->`); // `{% if Person.IsMember %}`
 * replaceLavaTagsWithHtmlComments(`<!-- rock-lava-tag-assign name = 'Ted Decker' -->`); // `{% assign name = 'Ted Decker' %}`
 */
export function replaceLavaHtmlCommentsWithTags(template: string): string {
    return template.replace(
        lavaTagHtmlCommentGlobalRegex,
        "{% $1 $2%}"
    );
}