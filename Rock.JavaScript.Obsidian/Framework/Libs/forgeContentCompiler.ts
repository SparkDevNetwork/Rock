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

/*
    8/17/2026 - CLAUDE

    The single compiler for Forge Content authored source. Only Rock's
    server-side compile service runs this bundle, inside a page of the headless
    Chromium that Rock already manages for PDF generation. The browser never
    loads a compiler; the block editor sends source to the server and displays
    the result. (Earlier iterations also loaded this bundle in the browser for
    a live preview and ran it in an in-process Jint engine; see
    specs/260814-forge-content-components.md for why both were dropped.)

    Two constraints introduced for the retired Jint host are kept deliberately:

    1. Source map generation is disabled in the compileScript call. The maps
       were always discarded anyway, and keeping generation off avoids the
       source-map-js hazard of regenerating its sort function via
       new Function(fn.toString()) in any engine whose
       Function.prototype.toString does not return source text.

    2. The Vue version comes from @vue/compiler-sfc's own version export (the
       packages version in lockstep) instead of importing "vue". The lib build
       keeps "vue" external, and the blank compile page has no import map to
       resolve it; taking the version from the bundled compiler keeps this
       bundle's System.register dependency array empty and the bundle
       self-contained.

    Compilation notes carried over from the original in-block implementation:
    compileScript(..., { inlineTemplate: true }) compiles the template directly
    into the setup's returned render function, producing clean ES module output
    with no `with` block. That output is then transformed to a SystemJS
    System.register module, because Rock resolves bare @Obsidian/... specifiers
    through a SystemJS import map, not a native browser import map. TypeScript
    (lang="ts") is intentionally not supported: nothing here surfaces type errors
    to the author and the types would be stripped regardless.

    Reason: One compiler implementation, one host: the server.
*/

import { compileScript, compileStyle, parse, rewriteDefault, version as compilerVersion } from "@vue/compiler-sfc";

// #region Types

/** A single parsed top-level import statement. */
type ParsedImport = {
    /** The import clause exactly as written (for example `RockButton` or `{ ref, computed }`). */
    clause: string;

    /** The module specifier (for example `@Obsidian/Controls/rockButton.obs`). */
    specifier: string;
};

/** The result of splitting a block of code into its import statements and remaining body. */
type ImportSplit = {
    imports: ParsedImport[];
    body: string;
};

/** A single binding pulled from an import clause. */
type ImportBinding = {
    /** The local name the binding is assigned to. */
    local: string;

    /** The expression, relative to the SystemJS setter module parameter `_m`, that produces the value. */
    expression: string;
};

/** The result of a successful compile. */
export type ForgeContentCompileResult = {
    /** The SystemJS module string to store and serve to visitors. */
    compiledContent: string;

    /** The Vue version this compile targeted. */
    vueVersion: string;
};

// #endregion Types

// #region Values

/** The local the compiled component is assigned to inside the generated module. */
const componentLocal = "__component";

// #endregion Values

// #region Functions

/**
 * Produces a stable scope id for the source so the compiled render and the compiled
 * scoped styles agree, and so identical content dedupes to one injected style tag.
 *
 * @param source The authored source.
 *
 * @returns A short identifier derived from the source.
 */
function hashScopeId(source: string): string {
    let hash = 5381;

    for (let index = 0; index < source.length; index++) {
        hash = ((hash << 5) + hash + source.charCodeAt(index)) >>> 0;
    }

    return "v" + hash.toString(16);
}

/**
 * Splits a block of code into its top-level import statements and the remaining body.
 *
 * @param code The code to split.
 *
 * @returns The parsed imports and the body with those imports removed.
 */
function splitImports(code: string): ImportSplit {
    const importRegex = /import\s+([^;]+?)\s+from\s+(["'])([^"']+)\2\s*;?/g;
    const imports: ParsedImport[] = [];
    let body = code;
    let match: RegExpExecArray | null;

    // Collect first, then remove, so removal does not disturb the regex index.
    const matched: { full: string; clause: string; specifier: string }[] = [];
    while ((match = importRegex.exec(code)) !== null) {
        matched.push({ full: match[0], clause: match[1].trim(), specifier: match[3] });
    }

    for (const item of matched) {
        body = body.replace(item.full, "");
        imports.push({ clause: item.clause, specifier: item.specifier });
    }

    return { imports, body };
}

/**
 * Parses an import clause into the set of local bindings it introduces, expressed
 * relative to a SystemJS setter's module parameter `_m`.
 *
 * @param clause The import clause (for example `RockButton`, `{ ref, computed }`, or `* as ns`).
 *
 * @returns The bindings the clause introduces.
 */
function parseClause(clause: string): ImportBinding[] {
    const bindings: ImportBinding[] = [];
    const trimmed = clause.trim();

    const namespaceMatch = trimmed.match(/^\*\s+as\s+(\w+)$/);
    if (namespaceMatch) {
        bindings.push({ local: namespaceMatch[1], expression: "_m" });
        return bindings;
    }

    const braceStart = trimmed.indexOf("{");
    if (braceStart === -1) {
        // Default import only.
        bindings.push({ local: trimmed, expression: "_m.default" });
        return bindings;
    }

    // A default import may precede the named block (for example `Foo, { bar }`).
    const beforeBrace = trimmed.substring(0, braceStart).replace(/,\s*$/, "").trim();
    if (beforeBrace) {
        bindings.push({ local: beforeBrace, expression: "_m.default" });
    }

    const inside = trimmed.substring(braceStart + 1, trimmed.lastIndexOf("}"));
    for (const part of inside.split(",")) {
        const named = part.trim();
        if (!named) {
            continue;
        }

        const asMatch = named.match(/^(\w+)\s+as\s+(\w+)$/);
        if (asMatch) {
            bindings.push({ local: asMatch[2], expression: "_m." + asMatch[1] });
        }
        else {
            bindings.push({ local: named, expression: "_m." + named });
        }
    }

    return bindings;
}

/**
 * Builds the runtime snippet that injects the compiled styles into the current
 * document, guarded so the same content only injects once.
 *
 * @param id The scope id (used for the guard element id).
 * @param css The compiled CSS.
 *
 * @returns A self-invoking style-injection statement.
 */
function buildStyleInjection(id: string, css: string): string {
    const guardId = "ccstyle-" + id;

    return `(function () { var __id = ${JSON.stringify(guardId)}; if (!document.getElementById(__id)) { var __s = document.createElement("style"); __s.id = __id; __s.textContent = ${JSON.stringify(css)}; document.head.appendChild(__s); } })();`;
}

/**
 * Ensures a generated statement block is terminated, so whatever follows it cannot
 * be absorbed into it by automatic semicolon insertion.
 *
 * `compileScript` emits the component as `const __component = { ... }` with no
 * trailing semicolon. JavaScript does NOT insert one before a line starting with
 * `(` or `[`, because those read as a call or an index on the preceding
 * expression, so `const __component = {...}` followed by the style injection's
 * `(function () { ... })();` parses as an attempt to invoke the component object.
 * That produces a module which compiles, stores, and loads, then throws
 * "is not a function" at runtime. Terminating here removes the hazard for every
 * following statement rather than relying on what happens to come next.
 *
 * @param code The generated code block.
 *
 * @returns The block, guaranteed to end in a semicolon when it has content.
 */
function terminateStatement(code: string): string {
    const trimmed = code.trimEnd();

    if (!trimmed || trimmed.endsWith(";")) {
        return trimmed;
    }

    return trimmed + ";";
}

/**
 * Assembles the ES module output (whose imports are all simple top-level forms and
 * whose component has already been rewritten to `const __component = ...`) into a
 * SystemJS `System.register` module string.
 *
 * @param imports The parsed top-level imports.
 * @param body The module body after imports were stripped (hoisted declarations plus
 *  the `const __component = ...` declaration).
 * @param scopeAssign A statement assigning the scope id to the component, or empty.
 * @param styleInject A statement injecting the compiled styles, or empty.
 *
 * @returns The SystemJS module string.
 */
function buildSystemJsModule(imports: ParsedImport[], body: string, scopeAssign: string, styleInject: string): string {
    const specifierOrder: string[] = [];
    const bindingsBySpecifier: Record<string, ImportBinding[]> = {};
    const allLocals: string[] = [];

    for (const parsedImport of imports) {
        if (!bindingsBySpecifier[parsedImport.specifier]) {
            bindingsBySpecifier[parsedImport.specifier] = [];
            specifierOrder.push(parsedImport.specifier);
        }

        for (const binding of parseClause(parsedImport.clause)) {
            bindingsBySpecifier[parsedImport.specifier].push(binding);
            allLocals.push(binding.local);
        }
    }

    const dependencies = specifierOrder.map(specifier => JSON.stringify(specifier)).join(", ");
    const variableDeclaration = allLocals.length ? "var " + allLocals.join(", ") + ";" : "";

    const setters = specifierOrder.map(specifier => {
        const assignments = bindingsBySpecifier[specifier]
            .map(binding => binding.local + " = " + binding.expression + ";")
            .join(" ");
        return "function (_m) { " + assignments + " }";
    }).join(",\n            ");

    return [
        "System.register([" + dependencies + "], function (_export, _context) {",
        "    \"use strict\";",
        "    " + variableDeclaration,
        "    return {",
        "        setters: [",
        "            " + setters,
        "        ],",
        "        execute: function () {",
        terminateStatement(body),
        terminateStatement(scopeAssign),
        terminateStatement(styleInject),
        `_export("default", ${componentLocal});`,
        "        }",
        "    };",
        "});"
    ].join("\n");
}

/**
 * Compiles an authored single-file component into a SystemJS module string.
 *
 * The authored source is a `<template>` block, a `<script setup>` (or legacy options
 * object `<script>`) block in plain JavaScript, and any number of `<style>` blocks
 * (scoped or not).
 *
 * @param source The authored source.
 *
 * @returns The compiled module string and the Vue version it targeted.
 */
export function compileSource(source: string): ForgeContentCompileResult {
    const parsed = parse(source, { filename: "ForgeContent.vue" });

    if (parsed.errors && parsed.errors.length > 0) {
        throw new Error(parsed.errors.map(error => error.message ?? String(error)).join("\n"));
    }

    const descriptor = parsed.descriptor;
    const id = hashScopeId(source);
    const scopeId = "data-v-" + id;

    // inlineTemplate compiles the template into the returned render function using the
    // setup bindings, producing clean module output with no `with` block. Source maps
    // MUST stay disabled: the output maps are discarded, and generating them breaks
    // the Jint host (see the engineering note at the top of this file).
    const compiledScript = compileScript(descriptor, {
        id,
        inlineTemplate: true,
        sourceMap: false,
        templateOptions: { compilerOptions: { sourceMap: false } }
    });
    const content = rewriteDefault(compiledScript.content, componentLocal);

    // Compile every style block, scoping the ones marked scoped to this component.
    let css = "";
    let hasScopedStyle = false;

    for (const style of descriptor.styles) {
        if (style.scoped) {
            hasScopedStyle = true;
        }

        const compiledStyle = compileStyle({
            source: style.content,
            id,
            scoped: !!style.scoped,
            filename: "ForgeContent.vue"
        });

        if (compiledStyle.errors && compiledStyle.errors.length > 0) {
            throw new Error(compiledStyle.errors.map(error => error.message ?? String(error)).join("\n"));
        }

        css += compiledStyle.code + "\n";
    }

    const { imports, body } = splitImports(content);
    const scopeAssign = hasScopedStyle ? `${componentLocal}.__scopeId = ${JSON.stringify(scopeId)};` : "";
    const styleInject = css.trim() ? buildStyleInjection(id, css) : "";

    const compiledContent = buildSystemJsModule(imports, body, scopeAssign, styleInject);

    // Parse the assembled module so any remaining syntax problem surfaces as a compile
    // error (and blocks the save). Constructing the function parses without executing.
    try {
        // eslint-disable-next-line @typescript-eslint/no-implied-eval
        new Function("System", compiledContent);
    }
    catch (e) {
        throw new Error(e instanceof Error ? e.message : "The compiled module could not be parsed.");
    }

    return {
        compiledContent,
        vueVersion: compilerVersion
    };
}

// #endregion Functions
