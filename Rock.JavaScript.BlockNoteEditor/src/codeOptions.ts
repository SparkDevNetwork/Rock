import type { DynamicImportLanguageRegistration, DynamicImportThemeRegistration } from "@shikijs/types";
import { createBundledHighlighter, createCssVariablesTheme } from "@shikijs/core";
import { createJavaScriptRegexEngine } from "@shikijs/engine-javascript";

const theme = createCssVariablesTheme({
    name: "css-variables",
    variablePrefix: "--shiki-",
    fontStyle: true,
});

type BundledLanguage = "typescript" | "ts" | "javascript" | "js";
type BundledTheme = "css-variables";

const bundledLanguages = {
    css: () => import("@shikijs/langs-precompiled/css"),
    html: () => import("@shikijs/langs-precompiled/html"),
    javascript: () => import("@shikijs/langs-precompiled/javascript"),
    js: () => import("@shikijs/langs-precompiled/javascript"),
    json: () => import("@shikijs/langs-precompiled/json"),
    lava: () => import("@shikijs/langs-precompiled/liquid"),
    markdown: () => import("@shikijs/langs-precompiled/markdown"),
    md: () => import("@shikijs/langs-precompiled/markdown"),
    shellscript: () => import("@shikijs/langs-precompiled/shellscript"),
    bash: () => import("@shikijs/langs-precompiled/shellscript"),
    sh: () => import("@shikijs/langs-precompiled/shellscript"),
    shell: () => import("@shikijs/langs-precompiled/shellscript"),
    zsh: () => import("@shikijs/langs-precompiled/shellscript"),
    sql: () => import("@shikijs/langs-precompiled/sql"),
    svelte: () => import("@shikijs/langs-precompiled/svelte"),
    typescript: () => import("@shikijs/langs-precompiled/typescript"),
    ts: () => import("@shikijs/langs-precompiled/typescript"),
    xml: () => import("@shikijs/langs-precompiled/xml"),
    "c#": () => import("@shikijs/langs-precompiled/csharp"),
    csharp: () => import("@shikijs/langs-precompiled/csharp"),
    cs: () => import("@shikijs/langs-precompiled/csharp"),
} as Record<BundledLanguage, DynamicImportLanguageRegistration>;

const supportedLanguages: Record<string, { name: string, aliases: string[] }> = {
    text: {
        name: "Plain Text",
        aliases: ["text", "txt", "plain"],
    },
    c: {
        name: "C",
        aliases: ["c"],
    },
    cpp: {
        name: "C++",
        aliases: ["cpp", "c++"],
    },
    css: {
        name: "CSS",
        aliases: ["css"],
    },
    lava: {
        name: "Lava",
        aliases: ["lava"],
    },
    glsl: {
        name: "GLSL",
        aliases: ["glsl"],
    },
    graphql: {
        name: "GraphQL",
        aliases: ["graphql", "gql"],
    },
    haml: {
        name: "Ruby Haml",
        aliases: ["haml"],
    },
    html: {
        name: "HTML",
        aliases: ["html"],
    },
    java: {
        name: "Java",
        aliases: ["java"],
    },
    javascript: {
        name: "JavaScript",
        aliases: ["javascript", "js"],
    },
    json: {
        name: "JSON",
        aliases: ["json"],
    },
    jsonc: {
        name: "JSON with Comments",
        aliases: ["jsonc"],
    },
    jsonl: {
        name: "JSON Lines",
        aliases: ["jsonl"],
    },
    jsx: {
        name: "JSX",
        aliases: ["jsx"],
    },
    julia: {
        name: "Julia",
        aliases: ["julia", "jl"],
    },
    less: {
        name: "Less",
        aliases: ["less"],
    },
    markdown: {
        name: "Markdown",
        aliases: ["markdown", "md"],
    },
    mdx: {
        name: "MDX",
        aliases: ["mdx"],
    },
    php: {
        name: "PHP",
        aliases: ["php"],
    },
    postcss: {
        name: "PostCSS",
        aliases: ["postcss"],
    },
    pug: {
        name: "Pug",
        aliases: ["pug", "jade"],
    },
    python: {
        name: "Python",
        aliases: ["python", "py"],
    },
    r: {
        name: "R",
        aliases: ["r"],
    },
    regexp: {
        name: "RegExp",
        aliases: ["regexp", "regex"],
    },
    sass: {
        name: "Sass",
        aliases: ["sass"],
    },
    scss: {
        name: "SCSS",
        aliases: ["scss"],
    },
    shellscript: {
        name: "Shell",
        aliases: ["shellscript", "bash", "sh", "shell", "zsh"],
    },
    sql: {
        name: "SQL",
        aliases: ["sql"],
    },
    svelte: {
        name: "Svelte",
        aliases: ["svelte"],
    },
    typescript: {
        name: "TypeScript",
        aliases: ["typescript", "ts"],
    },
    vue: {
        name: "Vue",
        aliases: ["vue"],
    },
    "vue-html": {
        name: "Vue HTML",
        aliases: ["vue-html"],
    },
    wasm: {
        name: "WebAssembly",
        aliases: ["wasm"],
    },
    wgsl: {
        name: "WGSL",
        aliases: ["wgsl"],
    },
    xml: {
        name: "XML",
        aliases: ["xml"],
    },
    yaml: {
        name: "YAML",
        aliases: ["yaml", "yml"],
    },
    tsx: {
        name: "TSX",
        aliases: ["tsx", "typescriptreact"],
    },
    haskell: {
        name: "Haskell",
        aliases: ["haskell", "hs"],
    },
    csharp: {
        name: "C#",
        aliases: ["c#", "csharp", "cs"],
    },
    latex: {
        name: "LaTeX",
        aliases: ["latex"],
    },
    lua: {
        name: "Lua",
        aliases: ["lua"],
    },
    mermaid: {
        name: "Mermaid",
        aliases: ["mermaid", "mmd"],
    },
    ruby: {
        name: "Ruby",
        aliases: ["ruby", "rb"],
    },
    rust: {
        name: "Rust",
        aliases: ["rust", "rs"],
    },
    scala: {
        name: "Scala",
        aliases: ["scala"],
    },
    swift: {
        name: "Swift",
        aliases: ["swift"],
    },
    kotlin: {
        name: "Kotlin",
        aliases: ["kotlin", "kt", "kts"],
    },
    "objective-c": {
        name: "Objective C",
        aliases: ["objective-c", "objc"],
    },
};

const languageAliases: Record<string, string> = {};

for (const key in supportedLanguages) {
    const lang = supportedLanguages[key];

    for (const langAlias of lang.aliases) {
        if (!bundledLanguages[langAlias as BundledLanguage]) {
            languageAliases[langAlias] = "plaintext";
        }
    }
}

const bundledThemes = {
    "css-variables": () => Promise.resolve({ default: theme }),
} as Record<BundledTheme, DynamicImportThemeRegistration>;

const createHighlighter = createBundledHighlighter<
    BundledLanguage,
    BundledTheme
>({
    langs: bundledLanguages,
    themes: bundledThemes,
    engine: () => createJavaScriptRegexEngine(),
});

export const codeBlockOptions = {
    defaultLanguage: "text",
    supportedLanguages: supportedLanguages,
    createHighlighter: async () => await createHighlighter({
        themes: [theme],
        langs: [],
        langAlias: languageAliases,
    }),
};
