// The chat draws every color and every timing from one token sheet, which is what lets dark mode
// and any church's color reach every screen at once. This gate fails when a chat component holds
// a literal of its own: a hex, rgb(), rgba(), hsl(), hsla() or named color, a duration in ms or s,
// or a cubic-bezier() curve. It reads a component's style block, its inline styles, bound or not,
// and its script, where a style object, a style property or an animation can carry the same
// literal by another road. It also refuses a comment beside a template's root: the built block
// keeps template comments, so one there makes the component a fragment, and the classes, test ids
// and scoped rules its parent gives it are dropped, which the test renderer never shows. The keywords transparent, currentColor and inherit are not colors, and
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

/** Reads a template's style attributes, a static one as CSS and a bound one as script, and its root comments. */
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

/** Every component and script file under a folder, as paths from the repository root. */
function walk(folder) {
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

  const hits = [];
  for (const file of files) {
    hits.push(...scan(file, readFileSync(resolve(repoRoot, file), 'utf8')));
  }

  if (hits.length > 0) {
    console.error('style literals, root comments or icon sizes in chat components:');
    for (const hit of hits) console.error(`  ${hit}`);
    console.error('\nName a value in the token sheet, RockWeb/Styles/Blocks/Communication/Chat/chat.css, and use the token; move a root comment inside the root; size an icon with --chat-ic-size and --chat-ic-stroke on what holds it.');
    process.exit(1);
  }

  console.log(`no style literals: ${files.length} chat files checked`);
}
