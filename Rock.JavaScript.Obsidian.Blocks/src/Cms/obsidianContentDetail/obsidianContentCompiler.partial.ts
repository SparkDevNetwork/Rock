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
    7/22/2026 - CLAUDE

    The in-browser compile pipeline for the Obsidian Content block. This runs ONLY
    in the administrator's edit path (never for a plain visitor). It compiles an
    authored template-plus-script source into a SystemJS module string that later
    loads through Rock's existing loader (so the authored `@Obsidian/...` imports
    resolve through the import map exactly like any other Obsidian block).

    Why SystemJS and not native ESM: Rock resolves bare `@Obsidian/...` specifiers
    through a SystemJS import map, not a native browser import map, so the stored
    module must be in SystemJS `System.register` format.

    Why FUNCTION mode (not module mode): the shipped compiler asset is the browser
    build of `@vue/compiler-dom`, which only supports the default "function" codegen
    mode - it rejects "module" mode and `prefixIdentifiers` at compile time. Function
    mode emits a render function body that (a) references the Vue runtime through a
    free `Vue` variable and (b) uses a `with (_ctx)` block. We therefore wrap the
    emitted code in a non-strict IIFE that receives the Vue runtime namespace, and we
    deliberately do NOT mark the generated module strict (a `with` statement is illegal
    in strict mode). The Vue template compiler still runs only here, at save time; the
    stored module is finished JavaScript that visitors load without any compiler.

    Reason: The browser Vue compiler only supports function mode; confine it to edit.
*/

import { loadJavaScriptAsync } from "@Obsidian/Utility/page";
import { version as vueVersion } from "vue";

// #region Types

/** The browser build of `@vue/compiler-dom` exposed as a global by the edit-only compiler asset. */
type VueCompilerDom = {
    compile: (template: string, options: Record<string, unknown>) => {
        code: string;
        errors?: { message?: string; code?: number }[];
    };
};

declare global {
    interface Window {
        VueCompilerDOM?: VueCompilerDom;
    }
}

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
export type ObsidianContentCompileResult = {
    /** The SystemJS module string to store and serve to visitors. */
    compiledContent: string;

    /** The Vue version this compile targeted. */
    vueVersion: string;
};

// #endregion Types

// #region Values

/** The URL of the edit-only compiler asset (a browser build of `@vue/compiler-dom`). */
const compilerAssetUrl = "/Obsidian/Libs/vueCompiler.js";

/** The private local the whole Vue runtime namespace is bound to for the render IIFE. */
const vueRuntimeLocal = "__vueRuntime__";

// #endregion Values

// #region Functions

/**
 * Lazily loads the template compiler asset. This is called only when the block
 * enters edit mode, and never for a plain viewer.
 */
export async function loadCompilerAsync(): Promise<void> {
    const loaded = await loadJavaScriptAsync(compilerAssetUrl, () => !!window.VueCompilerDOM);

    if (!loaded || !window.VueCompilerDOM) {
        throw new Error("The template compiler could not be loaded.");
    }
}

/**
 * Extracts the inner text of the first `<template>` or `<script>` block from the source.
 *
 * @param source The authored source.
 * @param tag The block tag to extract (`template` or `script`).
 *
 * @returns The trimmed inner text, or an empty string if the block is absent.
 */
function extractBlock(source: string, tag: string): string {
    const regex = new RegExp("<" + tag + "[^>]*>([\\s\\S]*?)<\\/" + tag + ">", "i");
    const match = source.match(regex);

    return match ? match[1].trim() : "";
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
 * Assembles a SystemJS `System.register` module from the author's imports, the
 * function-mode render code, and the component options body.
 *
 * @param imports The author's parsed top-level imports.
 * @param renderCode The function-mode code emitted by the Vue compiler. It ends with
 *  `return function render(...) { ... }` and references a free `Vue`.
 * @param componentBody The component options declaration (`const __component = { ... };`).
 *
 * @returns The SystemJS module string.
 */
function buildSystemJsModule(imports: ParsedImport[], renderCode: string, componentBody: string): string {
    const specifierOrder: string[] = [];
    const bindingsBySpecifier: Record<string, ImportBinding[]> = {};
    const allLocals: string[] = [];

    const addBinding = (specifier: string, binding: ImportBinding): void => {
        if (!bindingsBySpecifier[specifier]) {
            bindingsBySpecifier[specifier] = [];
            specifierOrder.push(specifier);
        }

        bindingsBySpecifier[specifier].push(binding);
        allLocals.push(binding.local);
    };

    for (const parsedImport of imports) {
        for (const binding of parseClause(parsedImport.clause)) {
            addBinding(parsedImport.specifier, binding);
        }
    }

    // The function-mode render code references a free `Vue`. Bind the whole vue
    // namespace to a private local and pass it into the render IIFE. This is added
    // even when the author already imports from vue, and a single deduped `vue`
    // dependency serves both.
    addBinding("vue", { local: vueRuntimeLocal, expression: "_m" });

    const dependencies = specifierOrder.map(specifier => JSON.stringify(specifier)).join(", ");
    const variableDeclaration = allLocals.length ? "var " + allLocals.join(", ") + ";" : "";

    const setters = specifierOrder.map(specifier => {
        const assignments = bindingsBySpecifier[specifier]
            .map(binding => binding.local + " = " + binding.expression + ";")
            .join(" ");
        return "function (_m) { " + assignments + " }";
    }).join(",\n            ");

    // NOTE: this module is intentionally NOT strict mode. The function-mode render
    // emitted by the browser Vue compiler uses a `with (_ctx)` block, which is illegal
    // in strict mode, so the render IIFE below must run sloppy.
    return [
        "System.register([" + dependencies + "], function (_export, _context) {",
        "    " + variableDeclaration,
        "    return {",
        "        setters: [",
        "            " + setters,
        "        ],",
        "        execute: function () {",
        "            var render = (function (Vue) {",
        renderCode,
        "            })(" + vueRuntimeLocal + ");",
        componentBody,
        "            __component.render = render;",
        "            _export(\"default\", __component);",
        "        }",
        "    };",
        "});"
    ].join("\n");
}

/**
 * Compiles an authored template-plus-script source into a SystemJS module string.
 *
 * The authored source is a `<template>` block plus a `<script>` block whose body is
 * a plain component options object (`export default { ... }`) with optional simple
 * top-level imports (for example of `@Obsidian/...` controls).
 *
 * @param source The authored source.
 *
 * @returns The compiled module string and the Vue version it targeted.
 */
export function compileSource(source: string): ObsidianContentCompileResult {
    if (!window.VueCompilerDOM) {
        throw new Error("The template compiler has not been loaded.");
    }

    const template = extractBlock(source, "template");
    const script = extractBlock(source, "script");

    if (!template) {
        throw new Error("The source must contain a <template> block.");
    }

    const { imports: scriptImports, body: scriptBody } = splitImports(script);

    // Function mode is the only mode the browser compiler build supports.
    const compiled = window.VueCompilerDOM.compile(template, {});

    if (compiled.errors && compiled.errors.length > 0) {
        const message = compiled.errors
            .map(error => error.message ?? ("Vue compiler error code " + error.code))
            .join("\n");
        throw new Error(message);
    }

    // `export default { ... }` -> `const __component = { ... }`.
    const componentBody = scriptBody.replace(/export\s+default\s+/, "const __component = ");

    return {
        compiledContent: buildSystemJsModule(scriptImports, compiled.code, componentBody),
        vueVersion: vueVersion
    };
}

// #endregion Functions
