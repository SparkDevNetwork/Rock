// The chat draws every color and every timing from one token sheet, which is what lets dark mode
// and any church's color reach every screen at once. This gate fails when a chat component holds
// a literal of its own: a hex, rgb(), rgba(), hsl(), hsla() or named color, a duration in ms or s,
// or a cubic-bezier() curve. It reads a component's style block, its inline styles, bound or not,
// and its script, where a style object, a style property or an animation can carry the same
// literal by another road. It also refuses a comment beside a template's root: the built block
// keeps template comments, so one there makes the component a fragment, and the classes, test ids
// and scoped rules its parent gives it are dropped, which the test renderer never shows. For the
// same reason it refuses a comment kept directly in a component's slot content or a Teleport,
// which Vue's production runtime loses track of once patched, so the next render that empties the
// slot throws and leaves the slot's old content in the page. The keywords transparent, currentColor and inherit are not colors, and
// a color-mix() of tokens is allowed; a color-mix() with a literal in it is caught by the literal.
//
// Two places are exempt, because they are where the values live: the token sheet, chat.css, which
// the block links from RockWeb's Styles folder, and the color arithmetic region of
// brand.partial.ts, which works out the church color's tokens in script since the browsers Rock
// supports cannot mix colors in CSS. The rest of that file is held to the rule like any other, and
// without the region it is read whole. A stylesheet anywhere else in the gate's reach is read
// whole, as a style block.
//
// It also refuses a rule outside the icon that sets an icon's width, height (by their logical
// names too) or stroke width, the token sheet included, and an inline style on an icon that does.
// Such a rule and the icon's own size rule weigh the same, so which one draws is decided by the
// order the built styles load in, which no test sees; a caller sizes an icon through
// --chat-ic-size and --chat-ic-stroke, which the icon reads. A rule is on an icon when the last
// compound of its selector names one, by its class or as the svg every icon draws.
//
//   node check-styles.mjs                checks the chat block, everything under its folder and the token sheet
//   node check-styles.mjs --root <dir>   checks every component under a folder, which is how the
//                                        test proves the gate bites on its fixtures
import { readdirSync, readFileSync } from 'node:fs';
import { basename, relative, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

import { runRatchet } from './known-debt.mjs';

const repoRoot = fileURLToPath(new URL('../../', import.meta.url));

const chatFolder = 'Rock.JavaScript.Obsidian.Blocks/src/Communication/Chat';
const chatBlock = `${chatFolder}/chatShell.obs`;
const shellFolder = `${chatFolder}/ChatShell`;
const tokenSheet = 'RockWeb/Styles/Blocks/Communication/Chat/chat.css';

/** Where literals may live, by file name: the whole file, or only the region named. */
const exempt = new Map([
  ['chat.css', null],
  ['brand.partial.ts', 'Color arithmetic'],
]);

/** The one file whose rules size an icon's drawing. */
const iconOwner = 'icon.partial.obs';

/**
 * What names an icon in one compound selector: its class, its class as an attribute, or the svg
 * every icon draws. And the properties only the icon sets on it, logical sizes included.
 */
const ICON_CLASS = /\.chat-ic(?![\w-])/;
const ICON_ATTRIBUTE = /\[\s*class\s*~=\s*(["']?)chat-ic\1\s*\]/;
const ICON_TYPE = /^(?:[\w*-]*\|)?svg(?![\w-])/i;
const ICON_SIZE = /^((min-|max-)?(width|height|inline-size|block-size)|stroke-width)$/;

/** A tag whose children are slot content or teleported: a component, a dynamic one, or a Teleport. */
const SLOT_OWNER = /^<(?:[A-Z][\w]*|component|teleport|transition|transition-group|keep-alive)(?![\w-])/;

/** A template that is slot content: `#name`, `#[name]` or `v-slot`. */
const SLOT_DIRECTIVE = /\s(?:#|v-slot)/;

/** Functional pseudo-classes whose arguments are the element itself, as opposed to :has() and :not(). */
const SAME_ELEMENT = /^(?::deep|::v-deep|:slotted|:global|:is|:where|:matches|:-webkit-any)$/i;

/**
 * Splits a selector at the separators found outside brackets, parentheses and quotes, so a comma
 * or a space inside :is(...) or [title="a b"] is left alone. Returns each part with where it starts.
 */
function splitOutside(text, isSeparator) {
  const parts = [];
  let depth = 0;
  let quote = null;
  let from = 0;
  for (let i = 0; i < text.length; i++) {
    const c = text[i];
    if (quote) {
      if (c === quote) quote = null;
    } else if (c === '"' || c === '\'') {
      quote = c;
    } else if (c === '(' || c === '[') {
      depth++;
    } else if (c === ')' || c === ']') {
      depth--;
    } else if (depth === 0 && isSeparator(c)) {
      parts.push(text.slice(from, i));
      from = i + 1;
    }
  }
  parts.push(text.slice(from));
  return parts;
}

/**
 * Whether a selector's rule lands on an icon. Only its last compound names the element the rule
 * styles: an icon named before a combinator is something that element sits in or follows, and one
 * inside :has() or :not() is something it holds or is not. Inside :deep(), :is() and their kind the
 * argument is the element itself, so its own last compounds are read the same way.
 */
function isIconSelector(selector) {
  return splitOutside(selector, (c) => c === ',').some((one) => {
    const compounds = splitOutside(one.trim(), (c) => /[\s>+~]/.test(c)).filter((part) => part !== '');
    const last = compounds[compounds.length - 1] ?? '';

    let own = '';
    for (let i = 0; i < last.length; i++) {
      const pseudo = /^::?[\w-]+\(/.exec(last.slice(i));
      if (!pseudo) {
        own += last[i];
        continue;
      }
      // The argument runs to the parenthesis that closes this one.
      let depth = 0;
      let end = i + pseudo[0].length - 1;
      for (; end < last.length; end++) {
        if (last[end] === '(') depth++;
        else if (last[end] === ')' && --depth === 0) break;
      }
      const name = pseudo[0].slice(0, -1);
      if (SAME_ELEMENT.test(name) && isIconSelector(last.slice(i + pseudo[0].length, end))) return true;
      i = end;
    }

    return ICON_CLASS.test(own) || ICON_ATTRIBUTE.test(own) || ICON_TYPE.test(own);
  });
}

/** A style binding or attribute that sizes what it is on, in CSS's names or a style object's. */
const SIZE_IN_STYLE = /(^|[\s{,;'"])((min-|max-)?(width|height|inline-size|block-size)|stroke-width)['"]?\s*:/;

/**
 * The text with an exempt region's characters blanked and its line breaks kept, so the rest is
 * scanned as it is and every line number still holds. A region without both markers blanks nothing.
 */
function withoutRegion(text, name) {
  const open = text.indexOf(`// #region ${name}`);
  const close = text.indexOf(`// #endregion ${name}`, open);
  if (open < 0 || close < 0) return text;
  return text.slice(0, open) + text.slice(open, close).replace(/[^\n]/g, ' ') + text.slice(close);
}

// The CSS named colors. A word from this list standing alone in a style value is a color; the
// same word inside a longer name, such as a token or a keyframe name, is not.
const NAMED_COLORS = [
  'aliceblue', 'antiquewhite', 'aqua', 'aquamarine', 'azure', 'beige', 'bisque', 'black',
  'blanchedalmond', 'blue', 'blueviolet', 'brown', 'burlywood', 'cadetblue', 'chartreuse',
  'chocolate', 'coral', 'cornflowerblue', 'cornsilk', 'crimson', 'cyan', 'darkblue', 'darkcyan',
  'darkgoldenrod', 'darkgray', 'darkgreen', 'darkgrey', 'darkkhaki', 'darkmagenta',
  'darkolivegreen', 'darkorange', 'darkorchid', 'darkred', 'darksalmon', 'darkseagreen',
  'darkslateblue', 'darkslategray', 'darkslategrey', 'darkturquoise', 'darkviolet', 'deeppink',
  'deepskyblue', 'dimgray', 'dimgrey', 'dodgerblue', 'firebrick', 'floralwhite', 'forestgreen',
  'fuchsia', 'gainsboro', 'ghostwhite', 'gold', 'goldenrod', 'gray', 'green', 'greenyellow',
  'grey', 'honeydew', 'hotpink', 'indianred', 'indigo', 'ivory', 'khaki', 'lavender',
  'lavenderblush', 'lawngreen', 'lemonchiffon', 'lightblue', 'lightcoral', 'lightcyan',
  'lightgoldenrodyellow', 'lightgray', 'lightgreen', 'lightgrey', 'lightpink', 'lightsalmon',
  'lightseagreen', 'lightskyblue', 'lightslategray', 'lightslategrey', 'lightsteelblue',
  'lightyellow', 'lime', 'limegreen', 'linen', 'magenta', 'maroon', 'mediumaquamarine',
  'mediumblue', 'mediumorchid', 'mediumpurple', 'mediumseagreen', 'mediumslateblue',
  'mediumspringgreen', 'mediumturquoise', 'mediumvioletred', 'midnightblue', 'mintcream',
  'mistyrose', 'moccasin', 'navajowhite', 'navy', 'oldlace', 'olive', 'olivedrab', 'orange',
  'orangered', 'orchid', 'palegoldenrod', 'palegreen', 'paleturquoise', 'palevioletred',
  'papayawhip', 'peachpuff', 'peru', 'pink', 'plum', 'powderblue', 'purple', 'rebeccapurple',
  'red', 'rosybrown', 'royalblue', 'saddlebrown', 'salmon', 'sandybrown', 'seagreen', 'seashell',
  'sienna', 'silver', 'skyblue', 'slateblue', 'slategray', 'slategrey', 'snow', 'springgreen',
  'steelblue', 'tan', 'teal', 'thistle', 'tomato', 'turquoise', 'violet', 'wheat', 'white',
  'whitesmoke', 'yellow', 'yellowgreen',
];

// A hash glued to a word or to an entity's ampersand, as in "abc#123" or "&#1234;", starts no color.
const HEX = /(?<![\w&])#[0-9a-f]{3,8}(?![\w-])/i;
const COLOR_FUNCTION = /(?<![\w-])(rgba?|hsla?)\(/i;
const CURVE = /cubic-bezier\(/i;
const DURATION = /(?<![\w.-])\d*\.?\d+m?s(?![\w-])/i;
const NAMED = new RegExp(`(?<![\\w-])(${NAMED_COLORS.join('|')})(?![\\w-])`, 'i');
// A script string reads as a CSS value when it holds a length, a token, a border style or a
// declaration, which is what separates "0 0 0 2px red" from a sentence that mentions red.
const READS_AS_CSS = new RegExp([
  /(?<![\w.])-?\d*\.?\d+(px|r?em|%|vh|vw|ch|deg)(?![\w-])/.source,
  /var\(--/.source,
  /\b(solid|dashed|dotted|double|inset|outset)\b/.source,
  /(^|;)\s*(color|background|border|outline|fill|stroke|[a-z]+-[a-z-]+)\s*:/.source,
].join('|'), 'i');
const WHOLE_NAMED = new RegExp(`^(${NAMED_COLORS.join('|')})$`, 'i');

/** The line a character offset falls on, counting from one. */
function lineAt(text, offset) {
  let line = 1;
  for (let i = 0; i < offset; i++) {
    if (text[i] === '\n') line++;
  }
  return line;
}

/** What a CSS value holds that a component may not, or null when it is clean. */
function cssLiteral(value) {
  if (HEX.test(value)) return `a hex color (${HEX.exec(value)[0]})`;
  if (COLOR_FUNCTION.test(value)) return `a ${COLOR_FUNCTION.exec(value)[1]}() color`;
  if (NAMED.test(value)) return `a named color (${NAMED.exec(value)[1]})`;
  if (CURVE.test(value)) return 'a cubic-bezier() curve';
  if (DURATION.test(value)) return `a duration (${DURATION.exec(value)[0]})`;
  return null;
}

/** What a string literal in script holds that a component may not, or null when it is clean. */
function scriptLiteral(content) {
  const trimmed = content.trim();
  // A hex is a color wherever it sits in a string, alone or inside a longer value. Words such as
  // "Issue #1234" are refused too: no rule on the string's shape tells them from a gradient or a
  // pair of colors, and a sentence can be written without a hash and digits.
  const hex = HEX.exec(content);
  if (hex) return `a hex color (${hex[0]})`;
  if (WHOLE_NAMED.test(trimmed)) return `a named color (${trimmed})`;
  if (NAMED.test(content) && READS_AS_CSS.test(content)) return `a named color (${NAMED.exec(content)[1]})`;
  if (COLOR_FUNCTION.test(content)) return `a ${COLOR_FUNCTION.exec(content)[1]}() color`;
  if (CURVE.test(content)) return 'a cubic-bezier() curve';
  if (DURATION.test(content)) return `a duration (${DURATION.exec(content)[0]})`;
  return null;
}

/**
 * Walks a style block's declarations, skipping selectors and at-rule preludes, so an id selector
 * that happens to read as hex is never taken for a color. Reports the line each literal is on.
 */
function scanCss(file, text, start, end, hits, { isLiteralAllowed = false, isIconOwner = false } = {}) {
  const css = text.slice(start, end).replace(/\/\*[\s\S]*?\*\//g, (comment) => comment.replace(/[^\n]/g, ' '));
  const stack = [];
  let segmentStart = 0;
  let selector = '';

  const declaration = (from, to) => {
    const body = css.slice(from, to);
    const colon = body.indexOf(':');
    if (colon < 0) return;
    const property = body.slice(0, colon).trim().toLowerCase();
    const value = body.slice(colon + 1);
    const valueOffset = from + colon + 1 + value.search(/\S|$/);
    const found = isLiteralAllowed ? null : cssLiteral(value);
    if (found) {
      hits.push(`${file}:${lineAt(text, start + valueOffset)}: ${found}`);
    }
    if (!isIconOwner && ICON_SIZE.test(property) && isIconSelector(selector)) {
      hits.push(`${file}:${lineAt(text, start + valueOffset)}: an icon's ${property} set against the icon's own rules; set --chat-ic-size or --chat-ic-stroke on what holds it`);
    }
  };

  for (let i = 0; i < css.length; i++) {
    const c = css[i];
    if (c === '{') {
      const prelude = css.slice(segmentStart, i).trim();
      const isGroup = prelude.startsWith('@') && !/^@(font-face|page)/.test(prelude);
      stack.push(isGroup ? 'group' : 'rule');
      if (!isGroup) selector = prelude;
      segmentStart = i + 1;
    } else if (c === '}') {
      if (stack[stack.length - 1] === 'rule') declaration(segmentStart, i);
      stack.pop();
      segmentStart = i + 1;
    } else if (c === ';' && stack[stack.length - 1] === 'rule') {
      declaration(segmentStart, i);
      segmentStart = i + 1;
    }
  }
}

/**
 * Reads each string literal in script, skipping comments, and refuses one that is a color, a
 * duration or a curve. A number given as an animation's duration is refused too, since a script
 * animation reads its timing from the sheet like everything else.
 */
function scanScript(file, text, start, end, hits) {
  const reported = new Set();
  const report = (offset, what) => {
    const line = lineAt(text, offset);
    if (!reported.has(line)) {
      reported.add(line);
      hits.push(`${file}:${line}: ${what}`);
    }
  };

  let i = start;
  while (i < end) {
    const c = text[i];
    const next = text[i + 1];
    if (c === '/' && next === '/') {
      while (i < end && text[i] !== '\n') i++;
    } else if (c === '/' && next === '*') {
      const close = text.indexOf('*/', i + 2);
      i = close < 0 ? end : close + 2;
    } else if (c === '"' || c === "'" || c === '`') {
      let j = i + 1;
      while (j < end && text[j] !== c) {
        if (text[j] === '\\') j++;
        // A plain string ends at its line; only a template string runs on.
        if (text[j] === '\n' && c !== '`') break;
        j++;
      }
      const found = scriptLiteral(text.slice(i + 1, j));
      if (found) report(i, found);
      i = j + 1;
    } else {
      i++;
    }
  }

  const script = text.slice(start, end);
  for (const match of script.matchAll(/animate\([^\n]*?\bduration\s*:\s*\d/g)) {
    report(start + match.index, 'a duration given to a script animation');
  }
}

/**
 * Reads a template's style attributes, a static one as CSS and a bound one as script, and the
 * comments the built block keeps where they change what renders.
 */
function scanTemplate(file, text, start, end, hits) {
  const template = text.slice(start, end);
  // The root's own indent is the first element's; a comment at that indent sits beside it.
  const rootIndent = /\n([ \t]*)<(?!!--)/.exec(template)?.[1];
  if (rootIndent !== undefined) {
    for (const match of template.matchAll(/\n([ \t]*)<!--/g)) {
      if (match[1] === rootIndent) {
        hits.push(`${file}:${lineAt(text, start + match.index + 1)}: a comment beside the template's root, which makes the built component a fragment; put it inside the root`);
      }
    }
  }

  // Vue's production runtime patches slot content and a Teleport's children without the element
  // of a comment the built block keeps, so a later render that empties the slot or moves the
  // Teleport throws removing it, stops halfway, and leaves the old content in the page. A
  // comment's parent is the nearest open tag above it at a lesser indent. The compiler drops a
  // comment written straight inside a component beside its `<template #name>` slots, so only one
  // among implicit default content, in a slot template, or in a Teleport is kept.
  const lines = template.split('\n');
  const lineStarts = [];
  lines.reduce((at, line) => {
    lineStarts.push(at);
    return at + line.length + 1;
  }, 0);
  const indentOf = (line) => /^[ \t]*/.exec(line)[0].length;
  const openTagAt = (index) => template.slice(lineStarts[index], template.indexOf('>', lineStarts[index]) + 1);
  const isSlotTemplate = (index) => /^<template(?![\w-])/.test(lines[index].trimStart()) && SLOT_DIRECTIVE.test(openTagAt(index));
  /** Whether the component opened on a line has `<template #name>` slots, which make its other comments dropped. */
  const hasSlotTemplates = (parent, childIndent) => {
    for (let down = parent + 1; down < lines.length; down++) {
      const indent = indentOf(lines[down]);
      if (indent <= indentOf(lines[parent]) && lines[down].trimStart().startsWith('</')) return false;
      if (indent === childIndent && isSlotTemplate(down)) return true;
    }
    return false;
  };
  lines.forEach((line, index) => {
    if (!line.trimStart().startsWith('<!--')) return;
    const indent = indentOf(line);
    for (let up = index - 1; up >= 0; up--) {
      const above = lines[up].trimStart();
      if (indentOf(lines[up]) >= indent || !above.startsWith('<') || above.startsWith('</') || above.startsWith('<!--')) continue;
      if ((SLOT_OWNER.test(above) && !hasSlotTemplates(up, indent)) || isSlotTemplate(up)) {
        hits.push(`${file}:${lineAt(text, start + lineStarts[index])}: a comment directly in slot content or a Teleport, which the production runtime cannot remove once patched; put it above the component or inside an element`);
      }
      return;
    }
  });

  // Either quote may wrap an attribute, so the value runs to the quote that opened it.
  for (const match of template.matchAll(/(^|\s)(:style|v-bind:style|style)\s*=\s*(["'])([\s\S]*?)\3/g)) {
    const value = match[4];
    const valueStart = start + match.index + match[0].length - value.length - 1;
    if (match[2] === 'style') {
      const found = cssLiteral(value);
      if (found) hits.push(`${file}:${lineAt(text, valueStart)}: ${found} in an inline style`);
    } else {
      scanScript(file, text, valueStart, valueStart + value.length, hits);
    }
  }

  // An inline style on an icon weighs more than the icon's own rules, so it sizes the icon past
  // them; a caller passes --chat-ic-size and --chat-ic-stroke there instead.
  for (const tag of template.matchAll(/<(Icon|svg)(?![\w-])([^>]*)>/g)) {
    for (const match of tag[2].matchAll(/(^|\s)(:style|v-bind:style|style)\s*=\s*(["'])([\s\S]*?)\3/g)) {
      const words = match[4].replace(/([a-z])([A-Z])/g, '$1-$2').toLowerCase();
      if (SIZE_IN_STYLE.test(words)) {
        const valueStart = start + tag.index + tag[1].length + 1 + match.index + match[0].length - match[4].length - 1;
        hits.push(`${file}:${lineAt(text, valueStart)}: an icon's size set in its inline style; set --chat-ic-size or --chat-ic-stroke on what holds it`);
      }
    }
  }
}

/**
 * Every literal a chat file holds that it should take from the token sheet, and every icon it
 * sizes against the icon's own rules, as `file:line: what`.
 */
export function scan(file, text) {
  const name = basename(file);
  // A file exempt whole may hold literals, but sizes an icon by the same rule as any other.
  const isLiteralAllowed = exempt.has(name) && exempt.get(name) === null;
  if (exempt.has(name) && !isLiteralAllowed) {
    text = withoutRegion(text, exempt.get(name));
  }
  const cssRules = { isLiteralAllowed, isIconOwner: name === iconOwner };

  const hits = [];
  if (file.endsWith('.css')) {
    scanCss(file, text, 0, text.length, hits, cssRules);
    return hits;
  }
  if (isLiteralAllowed) return hits;
  if (file.endsWith('.ts')) {
    scanScript(file, text, 0, text.length, hits);
    return hits;
  }

  for (const match of text.matchAll(/<style[^>]*>([\s\S]*?)<\/style>/g)) {
    const start = match.index + match[0].indexOf('>') + 1;
    scanCss(file, text, start, start + match[1].length, hits, cssRules);
  }
  for (const match of text.matchAll(/<script[^>]*>([\s\S]*?)<\/script>/g)) {
    const start = match.index + match[0].indexOf('>') + 1;
    scanScript(file, text, start, start + match[1].length, hits);
  }
  const templateStart = text.indexOf('<template');
  const templateEnd = text.lastIndexOf('</template>');
  if (templateStart >= 0 && templateEnd > templateStart) {
    scanTemplate(file, text, templateStart, templateEnd, hits);
  }

  return hits;
}

/** A script comparing an animation's name to a string, the string. */
const NAME_READ = /animationName\s*===?\s*(['"])([\w-]+)\1/g;

/**
 * Across files: a keyframe defined in a scoped style block is renamed by the build, so a script
 * that finds an animation by that exact name never finds it in the page, though the test renderer,
 * which keeps the name, does. Every name a chat script compares exactly is refused where a scoped
 * block defines it; defined in the token sheet, it keeps its name.
 */
export function scanKeyframeNames(files) {
  const read = new Set();
  for (const { text } of files) {
    for (const match of text.matchAll(NAME_READ)) read.add(match[2]);
  }
  const hits = [];
  for (const { file, text } of files) {
    if (!file.endsWith('.obs')) continue;
    for (const style of text.matchAll(/<style[^>]*\bscoped\b[^>]*>([\s\S]*?)<\/style>/g)) {
      const start = style.index + style[0].indexOf('>') + 1;
      for (const keyframes of style[1].matchAll(/@keyframes\s+([\w-]+)/g)) {
        if (read.has(keyframes[1])) {
          hits.push(`${file}:${lineAt(text, start + keyframes.index)}: keyframes ${keyframes[1]}, which a script finds by name, defined in a scoped block`);
        }
      }
    }
  }
  return hits;
}

// #region Convictions held by the ratchet

/**
 * The style rules the ratchet holds, each with the reason a reader here needs. A ratchet rule may
 * keep the findings recorded for a file in known-debt.json and no more; a report rule only counts.
 */
export const STYLE_RULES = [
  { id: 'S1', mode: 'ratchet', reason: 'a font size is one of the sizes the token sheet names, so the type scale changes in one place' },
  { id: 'S2', mode: 'ratchet', reason: 'a corner is one of the radii the token sheet names, so buttons, fields and surfaces stay one family' },
  { id: 'S3', mode: 'ratchet', reason: 'a layer is one of the layers the token sheet names, so what floats over what is decided once; auto, -1, 0 and 1 only order siblings' },
  { id: 'S4', mode: 'ratchet', reason: 'how far a thing moves while it animates is a token, so every rise and slide travels the same distance' },
  { id: 'S5', mode: 'ratchet', reason: 'the chat has one set of animations, in the token sheet, so a motion is changed once for every screen' },
  { id: 'S6', mode: 'ratchet', reason: 'a component never restyles the inside of another; a part that should look different changes itself, for every screen' },
  { id: 'S7', mode: 'ratchet', reason: '!important ends the order the styles are written in, so the next change has to win by force too' },
  { id: 'S8', mode: 'ratchet', reason: 'reduced motion is honoured once, in the token sheet, so no screen forgets it and none answers it differently' },
  { id: 'S9', mode: 'ratchet', reason: 'a token nothing reads is a value a reader has to keep in mind for nothing' },
  { id: 'S10', mode: 'report', reason: 'a token read once may be a value with a name and no second caller' },
  { id: 'S11', mode: 'report', reason: 'a class the token sheet styles that no chat file uses may be a rule left behind' },
  { id: 'S12', mode: 'ratchet', reason: 'a part owns its look: a rule outside it that names a class only that part renders is a screen restyling the part' },
];

/** Whether a file is the token sheet, which holds tokens and the shared rules. */
function isSheet(file) {
  return basename(file) === 'chat.css';
}

/**
 * A function from a character offset to its line, counting from one, which finds the line by a
 * binary search over the line starts so a file with many findings is read once.
 */
export function lineFinder(text) {
  const starts = [0];
  for (let i = 0; i < text.length; i++) if (text[i] === '\n') starts.push(i + 1);
  return (offset) => {
    let low = 0;
    let high = starts.length - 1;
    while (low < high) {
      const middle = (low + high + 1) >> 1;
      if (starts[middle] <= offset) low = middle;
      else high = middle - 1;
    }
    return low + 1;
  };
}

/** The text with every character in the range blanked and its line breaks kept. */
function blank(text, from, to) {
  return text.slice(0, from) + text.slice(from, to).replace(/[^\n]/g, ' ') + text.slice(to);
}

/**
 * Each string literal in a stretch of script, skipping comments: its quote, its content and where
 * the content starts. A plain string ends at its line; a template string runs on.
 */
export function* scriptStrings(text, start = 0, end = text.length) {
  let i = start;
  while (i < end) {
    const c = text[i];
    const next = text[i + 1];
    if (c === '/' && next === '/') {
      while (i < end && text[i] !== '\n') i++;
    } else if (c === '/' && next === '*') {
      const close = text.indexOf('*/', i + 2);
      i = close < 0 ? end : close + 2;
    } else if (c === '"' || c === "'" || c === '`') {
      let j = i + 1;
      while (j < end && text[j] !== c) {
        if (text[j] === '\\') j++;
        if (text[j] === '\n' && c !== '`') break;
        j++;
      }
      yield { quote: c, value: text.slice(i + 1, j), offset: i + 1 };
      i = j + 1;
    } else {
      i++;
    }
  }
}

/** A stretch of script with its comments blanked, so a pattern never matches what a comment says. */
function scriptCode(text, start, end) {
  let out = text;
  let i = start;
  while (i < end) {
    const c = text[i];
    const next = text[i + 1];
    if (c === '/' && next === '/') {
      const close = text.indexOf('\n', i);
      const to = close < 0 || close > end ? end : close;
      out = blank(out, i, to);
      i = to;
    } else if (c === '/' && next === '*') {
      const close = text.indexOf('*/', i + 2);
      const to = close < 0 ? end : close + 2;
      out = blank(out, i, to);
      i = to;
    } else if (c === '"' || c === "'" || c === '`') {
      let j = i + 1;
      while (j < end && text[j] !== c) {
        if (text[j] === '\\') j++;
        if (text[j] === '\n' && c !== '`') break;
        j++;
      }
      i = j + 1;
    } else {
      i++;
    }
  }
  return out;
}

/**
 * Where a chat file's code sits: its style blocks, its script blocks and its template, as offsets.
 * A stylesheet is one style region and a script file one script region.
 */
export function regionsOf(file, text) {
  if (file.endsWith('.css')) return { styles: [{ start: 0, end: text.length }], scripts: [], template: null };
  if (file.endsWith('.ts')) return { styles: [], scripts: [{ start: 0, end: text.length }], template: null };

  const inner = (pattern) => [...text.matchAll(pattern)].map((match) => {
    const start = match.index + match[0].indexOf('>') + 1;
    return { start, end: start + match[1].length };
  });
  const templateStart = text.indexOf('<template');
  const templateEnd = text.lastIndexOf('</template>');
  return {
    styles: inner(/<style[^>]*>([\s\S]*?)<\/style>/g),
    scripts: inner(/<script[^>]*>([\s\S]*?)<\/script>/g),
    template: templateStart >= 0 && templateEnd > templateStart ? { start: templateStart, end: templateEnd } : null,
  };
}

/**
 * A chat file with every comment blanked, in whichever language each region speaks, and its
 * line breaks kept, so a pattern reads only code and every offset still holds.
 */
export function codeOf(file, text) {
  const { styles, scripts, template } = regionsOf(file, text);
  let out = text;
  for (const { start, end } of styles) {
    for (const match of text.slice(start, end).matchAll(/\/\*[\s\S]*?\*\//g)) out = blank(out, start + match.index, start + match.index + match[0].length);
  }
  for (const { start, end } of scripts) {
    const code = scriptCode(text, start, end);
    out = out.slice(0, start) + code.slice(start, end) + out.slice(end);
  }
  if (template) {
    for (const match of text.slice(template.start, template.end).matchAll(/<!--[\s\S]*?-->/g)) {
      out = blank(out, template.start + match.index, template.start + match.index + match[0].length);
    }
  }
  return out;
}

/**
 * Walks a stretch of CSS whose comments are already blanked, calling back for every rule's prelude
 * and every declaration, with whether the declaration sits inside keyframes.
 */
function walkCss(css, start, end, { onPrelude = () => {}, onDeclaration = () => {} }) {
  const stack = [];
  let segmentStart = start;
  let selector = '';

  const declaration = (from, to) => {
    const body = css.slice(from, to);
    const colon = body.indexOf(':');
    if (colon < 0) return;
    const value = body.slice(colon + 1);
    onDeclaration({
      property: body.slice(0, colon).trim().toLowerCase(),
      value: value.trim(),
      offset: from + colon + 1 + value.search(/\S|$/),
      selector,
      isInKeyframes: stack.includes('keyframes'),
    });
  };

  for (let i = start; i < end; i++) {
    const c = css[i];
    if (c === '{') {
      const prelude = css.slice(segmentStart, i).trim();
      const offset = segmentStart + css.slice(segmentStart, i).search(/\S|$/);
      const isKeyframes = /^@(-webkit-)?keyframes\b/i.test(prelude);
      const isGroup = prelude.startsWith('@') && !/^@(font-face|page)/.test(prelude);
      stack.push(isKeyframes ? 'keyframes' : isGroup ? 'group' : 'rule');
      if (!isGroup) selector = prelude;
      onPrelude({ prelude, offset, isRule: !isGroup && !stack.slice(0, -1).includes('keyframes') });
      segmentStart = i + 1;
    } else if (c === '}') {
      if (stack[stack.length - 1] === 'rule') declaration(segmentStart, i);
      stack.pop();
      segmentStart = i + 1;
    } else if (c === ';' && stack[stack.length - 1] === 'rule') {
      declaration(segmentStart, i);
      segmentStart = i + 1;
    }
  }
}

/** A token's name stripped from each var(), so a number in a token's name is never read as a literal. */
function withoutTokenNames(value) {
  return value.replace(/var\(\s*--[\w-]+/g, 'var(');
}

const LENGTH = /(?<![\w.#-])-?\d*\.?\d+(px|r?em|%|pt|vw|vh|vmin|vmax|ch|ex|lh)(?![\w-])/i;
const SIZE_KEYWORD = /(?<![\w-])(xx-small|x-small|small|medium|large|x-large|xx-large|xxx-large|smaller|larger)(?![\w-])/i;
const NOT_A_VALUE = /^(inherit|initial|unset|revert|0)$/i;
const SAFE_LAYER = /^(auto|-1|0|1|inherit|initial|unset|revert)$/i;
const RADIUS = /^border(-(top|bottom)-(left|right)|-(start|end)-(start|end))?-radius$/;

/** The ratchet rule a declaration breaks with its literal, or null. Shared by style blocks, inline styles and script. */
function literalRule(property, rawValue) {
  const value = rawValue.replace(/\s*!important\s*$/i, '').trim();
  if (NOT_A_VALUE.test(value)) return null;
  const bare = withoutTokenNames(value);
  if (property === 'font-size' && (LENGTH.test(bare) || SIZE_KEYWORD.test(bare))) return { rule: 'S1', what: `a literal font size (${value})` };
  if (property === 'font' && LENGTH.test(bare)) return { rule: 'S1', what: `a literal font size in the font shorthand (${value})` };
  if (RADIUS.test(property) && LENGTH.test(bare)) return { rule: 'S2', what: `a literal radius (${value})` };
  if (property === 'z-index' && !SAFE_LAYER.test(value) && /-?\d+/.test(bare)) return { rule: 'S3', what: `a literal layer (${value})` };
  return null;
}

/** A px distance other than zero, as keyframes or a script animation hold it, or null. */
function nonZeroPx(value) {
  for (const match of withoutTokenNames(value).matchAll(/(?<![\w.#-])-?(\d*\.?\d+)px(?![\w-])/gi)) {
    if (Number(match[1]) !== 0) return match[0];
  }
  return null;
}

/** A script's style property in CSS's own spelling: fontSize, "font-size" and font-size alike. */
function cssName(name) {
  return name.replace(/[A-Z]/g, (letter) => `-${letter.toLowerCase()}`);
}

/** A template string's static text, each ${...} standing for a value computed at run time. */
function staticText(value) {
  return value.replace(/\$\{[^}]*\}/g, 'X');
}

const SCRIPT_STYLE = /(["'`]?)(fontSize|font-size|font|borderRadius|border-radius|border(?:Top|Bottom)(?:Left|Right)Radius|border-(?:top|bottom)-(?:left|right)-radius|zIndex|z-index)\1\s*(?:=(?!=)|:)\s*(?:(["'`])((?:\\.|(?!\3)[^\\\n])*)\3|(-?\d+(?:\.\d+)?)(?![\w.]))/g;
const SET_PROPERTY = /setProperty\(\s*(["'])([\w-]+)\1\s*,\s*(["'`])((?:\\.|(?!\3)[^\\\n])*)\3(\s*,\s*["']important["'])?/g;

/** What a stretch of script, or a bound style in the template, holds against the style rules. */
function scriptStyleFindings(file, text, start, end, add) {
  const code = scriptCode(text, start, end).slice(start, end);

  for (const match of code.matchAll(SCRIPT_STYLE)) {
    if (/[\w$-]/.test(code[match.index - 1] ?? '')) continue;
    const value = match[4] !== undefined ? staticText(match[4]) : match[5];
    const found = literalRule(cssName(match[2]), value);
    if (found) add(found.rule, start + match.index, found.what);
  }
  for (const match of code.matchAll(SET_PROPERTY)) {
    const found = literalRule(match[2], staticText(match[4]));
    if (found) add(found.rule, start + match.index, found.what);
    if (match[5]) add('S7', start + match.index, 'an important priority set from script');
  }
  for (const string of scriptStrings(text, start, end)) {
    if (/!important/i.test(string.value)) add('S7', string.offset, '!important in a script string');
  }

  // A script animation's keyframes are the first argument to animate(): the array up to its bracket.
  for (const match of code.matchAll(/\.animate\(\s*\[/g)) {
    let depth = 0;
    let close = match.index + match[0].length - 1;
    for (; close < code.length; close++) {
      if (code[close] === '[') depth++;
      else if (code[close] === ']' && --depth === 0) break;
    }
    const from = start + match.index + match[0].length - 1;
    for (const string of scriptStrings(text, from, start + close)) {
      const px = nonZeroPx(staticText(string.value));
      if (px) {
        add('S4', start + match.index, `a ${px} distance in a script animation's keyframes`);
        break;
      }
    }
  }
}

/** What a stretch of CSS holds against the style rules S1 to S8. */
function cssFindings(file, text, start, end, add) {
  const css = text.slice(0, start) + text.slice(start, end).replace(/\/\*[\s\S]*?\*\//g, (comment) => comment.replace(/[^\n]/g, ' ')) + text.slice(end);
  const sheet = isSheet(file);

  walkCss(css, start, end, {
    onPrelude({ prelude, offset }) {
      if (!sheet && /^@(-webkit-)?keyframes\b/i.test(prelude)) add('S5', offset, `keyframes outside the token sheet (${prelude})`);
      if (!sheet && /^@media\b/i.test(prelude) && /prefers-reduced-motion/i.test(prelude)) add('S8', offset, 'a reduced-motion query outside the token sheet');
      if (/:deep\(|::v-deep|:global\(|>>>|\/deep\//.test(prelude)) add('S6', offset, `a selector reaching into another component (${prelude.replace(/\s+/g, ' ')})`);
    },
    onDeclaration({ property, value, offset, isInKeyframes }) {
      if (/!important/i.test(value)) add('S7', offset, `!important on ${property}`);
      if (property.startsWith('--')) return;
      if (isInKeyframes) {
        const px = nonZeroPx(value);
        if (px) add('S4', offset, `a ${px} distance inside keyframes`);
      }
      const found = literalRule(property, value);
      if (found) add(found.rule, offset, found.what);
    },
  });
}

/** What a template's inline styles hold: a static one read as CSS declarations, a bound one as script. */
function templateFindings(file, text, start, end, add) {
  const template = text.slice(start, end);
  for (const match of template.matchAll(/(^|\s)(:style|v-bind:style|style)\s*=\s*(["'])([\s\S]*?)\3/g)) {
    const valueStart = start + match.index + match[0].length - match[4].length - 1;
    if (match[2] !== 'style') {
      scriptStyleFindings(file, text, valueStart, valueStart + match[4].length, add);
      continue;
    }
    let at = valueStart;
    for (const piece of splitOutside(match[4], (c) => c === ';')) {
      const colon = piece.indexOf(':');
      if (colon >= 0) {
        const property = piece.slice(0, colon).trim().toLowerCase();
        const value = piece.slice(colon + 1);
        if (/!important/i.test(value)) add('S7', at, `!important on ${property} in an inline style`);
        const found = literalRule(property, value);
        if (found) add(found.rule, at, `${found.what} in an inline style`);
      }
      at += piece.length + 1;
    }
  }
}

/**
 * The compounds a selector's rule lands on: the last compound of each selector in its list, and
 * inside :deep(), :is() and their kind the argument's own last compounds, since the argument is
 * the element itself. A compound named before a combinator, or inside :has() or :not(), is
 * something the element sits in, follows, holds or is not.
 */
export function styledCompounds(selector) {
  const out = [];
  for (const one of splitOutside(selector, (c) => c === ',')) {
    const compounds = splitOutside(one.trim(), (c) => /[\s>+~]/.test(c)).filter((part) => part !== '');
    const last = compounds[compounds.length - 1] ?? '';

    let own = '';
    for (let i = 0; i < last.length; i++) {
      const pseudo = /^::?[\w-]+\(/.exec(last.slice(i));
      if (!pseudo) {
        own += last[i];
        continue;
      }
      let depth = 0;
      let end = i + pseudo[0].length - 1;
      for (; end < last.length; end++) {
        if (last[end] === '(') depth++;
        else if (last[end] === ')' && --depth === 0) break;
      }
      const name = pseudo[0].slice(0, -1);
      if (SAME_ELEMENT.test(name)) out.push(...styledCompounds(last.slice(i + pseudo[0].length, end)));
      i = end;
    }
    out.push(own);
  }
  return out;
}

/** Every class a template puts on an element: from class attributes and the strings of bound ones. */
function templateClasses(text, template) {
  const classes = new Set();
  const markup = text.slice(template.start, template.end);
  for (const match of markup.matchAll(/\sclass\s*=\s*(["'])([^"']*)\1/g)) {
    for (const name of match[2].split(/\s+/)) if (name) classes.add(name);
  }
  for (const match of markup.matchAll(/\s(?::class|v-bind:class)\s*=\s*(["'])([\s\S]*?)\1/g)) {
    for (const string of match[2].matchAll(/['`]([\w\s-]+)['`]/g)) {
      for (const name of string[1].split(/\s+/)) if (name) classes.add(name);
    }
  }
  return classes;
}

/**
 * Across files: S9 and S10 count every read of each token the sheet defines; S11 finds the chat
 * classes the sheet styles that no other chat file names; S12 finds a rule outside a part naming a
 * class only that part renders.
 */
function crossFileFindings(files, push) {
  const codes = files.map(({ file, text }) => ({ file, text, code: codeOf(file, text) }));

  // Tokens: defined by a declaration in the sheet, read by var() anywhere or by a script string
  // that is the token's whole name, other than a style binding's key or the name setProperty
  // writes. A name built from a prefix and a value read at run time reads the whole family.
  const defined = new Map();
  const reads = new Map();
  const families = new Set();
  for (const { file, text, code } of codes) {
    if (!isSheet(file)) continue;
    walkCss(code, 0, code.length, {
      onDeclaration({ property, offset }) {
        if (property.startsWith('--') && !defined.has(property)) defined.set(property, { file, text, offset });
      },
    });
  }
  const read = (name) => reads.set(name, (reads.get(name) ?? 0) + 1);
  for (const { code } of codes) {
    for (const match of code.matchAll(/var\(\s*(--[\w-]+)/g)) read(match[1]);
    for (const match of code.matchAll(/(["'`])(--[\w-]+)\1/g)) {
      const after = code.slice(match.index + match[0].length);
      const before = code.slice(Math.max(0, match.index - 20), match.index);
      if (/^\s*:/.test(after) || /setProperty\(\s*$/.test(before)) continue;
      read(match[2]);
    }
    for (const match of code.matchAll(/(--[\w-]*-)(?:\$\{|["'`]\s*\+)/g)) {
      // The name setProperty writes is a definition, not a read.
      if (!/setProperty\(\s*["'`]$/.test(code.slice(Math.max(0, match.index - 20), match.index))) families.add(match[1]);
    }
  }
  for (const [name, { file, text, offset }] of defined) {
    if ([...families].some((prefix) => name.startsWith(prefix))) continue;
    const count = reads.get(name) ?? 0;
    const line = lineFinder(text)(offset);
    if (count === 0) push({ rule: 'S9', file, line, what: `${name} is defined and never read` });
    else if (count === 1) push({ rule: 'S10', file, line, what: `${name} is read once` });
  }

  // Classes the sheet styles that no other chat file names. A class built from a prefix at run time
  // is reported too: the same prefixes build element ids, so reading them as classes would hide
  // most of the sheet, and a report that names a few live classes costs a reader a search.
  const named = new Set();
  for (const { file, code } of codes) {
    if (isSheet(file)) continue;
    for (const match of code.matchAll(/(?<![\w-])chat-[\w-]*/g)) named.add(match[0]);
  }
  for (const { file, text, code } of codes) {
    if (!isSheet(file)) continue;
    const lineOf = lineFinder(text);
    const seen = new Set();
    walkCss(code, 0, code.length, {
      onPrelude({ prelude, offset, isRule }) {
        if (!isRule) return;
        for (const match of prelude.matchAll(/\.(chat-[\w-]+)/g)) {
          const name = match[1];
          if (seen.has(name) || named.has(name)) continue;
          seen.add(name);
          push({ rule: 'S11', file, line: lineOf(offset), what: `.${name} is styled here and used by no chat file` });
        }
      },
    });
  }

  // Parts' classes: a class a part's template renders that no other template renders is the part's.
  const renderedBy = new Map();
  for (const { file, text } of codes) {
    if (!file.endsWith('.obs')) continue;
    const { template } = regionsOf(file, text);
    if (!template) continue;
    for (const name of templateClasses(text, template)) {
      if (!renderedBy.has(name)) renderedBy.set(name, new Set());
      renderedBy.get(name).add(file);
    }
  }
  const owner = new Map();
  for (const [name, renderers] of renderedBy) {
    const [only] = renderers;
    if (renderers.size === 1 && /\/ui\/[^/]+\.obs$/.test(only)) owner.set(name, only);
  }
  for (const { file, text, code } of codes) {
    const lineOf = lineFinder(text);
    for (const { start, end } of regionsOf(file, text).styles) {
      walkCss(code, start, end, {
        onPrelude({ prelude, offset, isRule }) {
          if (!isRule || prelude.startsWith('@')) return;
          const parts = new Set();
          for (const compound of styledCompounds(prelude)) {
            for (const match of compound.matchAll(/\.([\w-]+)/g)) {
              const part = owner.get(match[1]);
              if (part && part !== file) parts.add(`.${match[1]} (${basename(part)})`);
            }
          }
          if (parts.size > 0) push({ rule: 'S12', file, line: lineOf(offset), what: `restyles a part from outside it: ${[...parts].join(', ')}` });
        },
      });
    }
  }
}

/**
 * Everything the chat files hold against the ratchet's style rules, as findings of rule, file,
 * line and what. Reads each file alone for S1 to S8, then all of them together for S9 to S12.
 */
export function findStyleDebt(files) {
  const findings = [];
  for (const { file, text } of files) {
    const lineOf = lineFinder(text);
    const add = (rule, offset, what) => findings.push({ rule, file, line: lineOf(offset), what });
    const { styles, scripts, template } = regionsOf(file, text);
    for (const { start, end } of styles) cssFindings(file, text, start, end, add);
    for (const { start, end } of scripts) scriptStyleFindings(file, text, start, end, add);
    if (template) templateFindings(file, text, template.start, template.end, add);
  }
  crossFileFindings(files, (found) => findings.push(found));
  return findings;
}

// #endregion Convictions held by the ratchet

/** Every component and script file under a folder, as paths from the repository root. */
export function walk(folder) {
  const out = [];
  for (const entry of readdirSync(folder, { withFileTypes: true })) {
    const full = resolve(folder, entry.name);
    if (entry.isDirectory()) out.push(...walk(full));
    else if (/\.(obs|ts|css)$/.test(entry.name)) out.push(relative(repoRoot, full).replace(/\\/g, '/'));
  }
  return out;
}

/**
 * The files the gate reads: the chat block, everything under its folder and the token sheet. The
 * folder is walked rather than listed from git, so a component is held to the rule before it is
 * first committed.
 */
export function styleFiles() {
  return [chatBlock, tokenSheet, ...walk(resolve(repoRoot, shellFolder))].sort();
}

const args = process.argv.slice(2);
const invokedDirectly = process.argv[1] && resolve(process.argv[1]) === fileURLToPath(import.meta.url);

if (invokedDirectly) {
  const rootFlag = args.indexOf('--root');
  const files = rootFlag >= 0 && args[rootFlag + 1] ? walk(resolve(args[rootFlag + 1])) : styleFiles();
  // One kind of line ending, which keeps every line number and lets a pattern stop at a line feed.
  const read = files.map((file) => ({ file, text: readFileSync(resolve(repoRoot, file), 'utf8').replace(/\r\n/g, '\n') }));

  const hits = [];
  for (const { file, text } of read) hits.push(...scan(file, text));
  hits.push(...scanKeyframeNames(read));

  const isRatchetFailed = runRatchet({ title: 'style convictions', rules: STYLE_RULES, findings: findStyleDebt(read), args });

  if (hits.length > 0) {
    console.error('style literals, kept comments or icon sizes in chat components:');
    for (const hit of hits) console.error(`  ${hit}`);
    console.error('\nName a value in the token sheet, RockWeb/Styles/Blocks/Communication/Chat/chat.css, and use the token; move a root comment inside the root and a slot comment above its component or into an element; size an icon with --chat-ic-size and --chat-ic-stroke on what holds it.');
  } else {
    console.log(`no style literals: ${files.length} chat files checked`);
  }

  if (hits.length > 0 || isRatchetFailed) process.exit(1);
}
