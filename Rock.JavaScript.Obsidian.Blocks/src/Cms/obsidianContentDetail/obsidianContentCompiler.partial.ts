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

    The in-browser compile entry point for the Obsidian Content block. This runs
    ONLY in the administrator's edit path (never for a plain visitor).

    8/6/2026 - CLAUDE

    The compile implementation moved to the @Obsidian/Libs/obsidianContentCompiler
    library bundle so the server-side compile service (running the same bundle in a
    Jint engine) and this editor always produce identical output for the same source.
    This file is now a thin loader that keeps the block's original API: the lib is
    still loaded on demand only when the block enters edit mode, so its weight never
    reaches a plain visitor. All compile mechanics and their reasons live in the lib.

    Reason: Delegate to the shared compiler lib so browser and server never drift.
*/

// #region Types

/** The result of a successful compile. Mirrors the shared lib's result shape. */
export type ObsidianContentCompileResult = {
    /** The SystemJS module string to store and serve to visitors. */
    compiledContent: string;

    /** The Vue version this compile targeted. */
    vueVersion: string;
};

/** The exported surface of the shared compiler lib this file delegates to. */
type SharedCompiler = {
    compileSource: (source: string) => ObsidianContentCompileResult;
};

// #endregion Types

// #region Values

/** The loaded compiler lib, cached after the first load. */
let compiler: SharedCompiler | null = null;

// #endregion Values

// #region Functions

/**
 * Lazily loads the shared compiler library bundle. This is called only when the
 * block enters edit mode, and never for a plain viewer.
 */
export async function loadCompilerAsync(): Promise<void> {
    if (compiler) {
        return;
    }

    // Lazily load the compiler library through Rock's loader (this dynamic import is
    // rewritten to a SystemJS import, so the bundle resolves through the import map).
    // This only runs in edit mode, never for a plain viewer.
    const module = await import("@Obsidian/Libs/obsidianContentCompiler");

    if (!module || typeof (module as unknown as SharedCompiler).compileSource !== "function") {
        throw new Error("The single-file-component compiler could not be loaded.");
    }

    compiler = module as unknown as SharedCompiler;
}

/**
 * Compiles an authored single-file component into a SystemJS module string using
 * the shared compiler lib. `loadCompilerAsync` must have completed first.
 *
 * @param source The authored source.
 *
 * @returns The compiled module string and the Vue version it targeted.
 */
export function compileSource(source: string): ObsidianContentCompileResult {
    if (!compiler) {
        throw new Error("The compiler has not been loaded.");
    }

    return compiler.compileSource(source);
}

// #endregion Functions
