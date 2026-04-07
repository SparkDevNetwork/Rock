import type { CodeBlockOptions } from "@blocknote/core";
import type { DynamicImportLanguageRegistration, DynamicImportThemeRegistration, HighlighterGeneric } from "@shikijs/types";
import { createBundledHighlighter, createCssVariablesTheme } from "@shikijs/core";
import { createJavaScriptRegexEngine } from "@shikijs/engine-javascript";

const theme = createCssVariablesTheme({
    name: "css-variables",
    variablePrefix: "--shiki-",
    fontStyle: true,
});

type BundledLanguage = "typescript" | "ts" | "javascript" | "js";
type BundledTheme = "github-light" | "github-dark" | "css-variables";
type Highlighter = HighlighterGeneric<BundledLanguage, BundledTheme>;

const bundledLanguages = {
    // c: () => import("@shikijs/langs-precompiled/c"),
    // cpp: () => import("@shikijs/langs-precompiled/cpp"),
    // "c++": () => import("@shikijs/langs-precompiled/cpp"),
    css: () => import("@shikijs/langs-precompiled/css"),
    // glsl: () => import("@shikijs/langs-precompiled/glsl"),
    // graphql: () => import("@shikijs/langs-precompiled/graphql"),
    // gql: () => import("@shikijs/langs-precompiled/graphql"),
    // haml: () => import("@shikijs/langs-precompiled/haml"),
    html: () => import("@shikijs/langs-precompiled/html"),
    // java: () => import("@shikijs/langs-precompiled/java"),
    javascript: () => import("@shikijs/langs-precompiled/javascript"),
    js: () => import("@shikijs/langs-precompiled/javascript"),
    json: () => import("@shikijs/langs-precompiled/json"),
    // jsonc: () => import("@shikijs/langs-precompiled/jsonc"),
    // jsonl: () => import("@shikijs/langs-precompiled/jsonl"),
    // jsx: () => import("@shikijs/langs-precompiled/jsx"),
    // julia: () => import("@shikijs/langs-precompiled/julia"),
    // jl: () => import("@shikijs/langs-precompiled/julia"),
    lava: () => import("@shikijs/langs-precompiled/liquid"),
    // less: () => import("@shikijs/langs-precompiled/less"),
    markdown: () => import("@shikijs/langs-precompiled/markdown"),
    md: () => import("@shikijs/langs-precompiled/markdown"),
    // mdx: () => import("@shikijs/langs-precompiled/mdx"),
    // php: () => import("@shikijs/langs-precompiled/php"),
    // postcss: () => import("@shikijs/langs-precompiled/postcss"),
    // pug: () => import("@shikijs/langs-precompiled/pug"),
    // jade: () => import("@shikijs/langs-precompiled/pug"),
    // python: () => import("@shikijs/langs-precompiled/python"),
    // py: () => import("@shikijs/langs-precompiled/python"),
    // r: () => import("@shikijs/langs-precompiled/r"),
    // regexp: () => import("@shikijs/langs-precompiled/regexp"),
    // regex: () => import("@shikijs/langs-precompiled/regexp"),
    // sass: () => import("@shikijs/langs-precompiled/sass"),
    // scss: () => import("@shikijs/langs-precompiled/scss"),
    shellscript: () => import("@shikijs/langs-precompiled/shellscript"),
    bash: () => import("@shikijs/langs-precompiled/shellscript"),
    sh: () => import("@shikijs/langs-precompiled/shellscript"),
    shell: () => import("@shikijs/langs-precompiled/shellscript"),
    zsh: () => import("@shikijs/langs-precompiled/shellscript"),
    sql: () => import("@shikijs/langs-precompiled/sql"),
    svelte: () => import("@shikijs/langs-precompiled/svelte"),
    typescript: () => import("@shikijs/langs-precompiled/typescript"),
    ts: () => import("@shikijs/langs-precompiled/typescript"),
    // vue: () => import("@shikijs/langs-precompiled/vue"),
    // "vue-html": () => import("@shikijs/langs-precompiled/vue-html"),
    // wasm: () => import("@shikijs/langs-precompiled/wasm"),
    // wgsl: () => import("@shikijs/langs-precompiled/wgsl"),
    xml: () => import("@shikijs/langs-precompiled/xml"),
    // yaml: () => import("@shikijs/langs-precompiled/yaml"),
    // yml: () => import("@shikijs/langs-precompiled/yaml"),
    // tsx: () => import("@shikijs/langs-precompiled/tsx"),
    // typescriptreact: () => import("@shikijs/langs-precompiled/tsx"),
    // haskell: () => import("@shikijs/langs-precompiled/haskell"),
    // hs: () => import("@shikijs/langs-precompiled/haskell"),
    "c#": () => import("@shikijs/langs-precompiled/csharp"),
    csharp: () => import("@shikijs/langs-precompiled/csharp"),
    cs: () => import("@shikijs/langs-precompiled/csharp"),
    // latex: () => import("@shikijs/langs-precompiled/latex"),
    // lua: () => import("@shikijs/langs-precompiled/lua"),
    // mermaid: () => import("@shikijs/langs-precompiled/mermaid"),
    // mmd: () => import("@shikijs/langs-precompiled/mermaid"),
    // ruby: () => import("@shikijs/langs-precompiled/ruby"),
    // rb: () => import("@shikijs/langs-precompiled/ruby"),
    // rust: () => import("@shikijs/langs-precompiled/rust"),
    // rs: () => import("@shikijs/langs-precompiled/rust"),
    // scala: () => import("@shikijs/langs-precompiled/scala"),
    // Swift does not support pre-compilation right now
    //swift: () => import("@shikijs/langs/swift"),
    // kotlin: () => import("@shikijs/langs-precompiled/kotlin"),
    // kt: () => import("@shikijs/langs-precompiled/kotlin"),
    // kts: () => import("@shikijs/langs-precompiled/kotlin"),
    // "objective-c": () => import("@shikijs/langs-precompiled/objective-c"),
    // objc: () => import("@shikijs/langs-precompiled/objective-c"),
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
    // "github-light": () => import("@shikijs/themes/github-light"),
    // "github-dark": () => import("@shikijs/themes/github-dark"),
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
    createHighlighter: async () => {
        const highlighter = await createHighlighter({
            themes: [theme],//], "github-dark"],
            langs: [],
            langAlias: languageAliases,
        }) as Highlighter;

        const originalCodeToHtml = highlighter.codeToHtml;
        const originalCodeToTokens = highlighter.codeToTokens;

        highlighter.codeToHtml = (code, options) => {
            console.log("options", options);
            return originalCodeToHtml(code, options);
        };

        highlighter.codeToTokens = (code, options) => {
            console.log("codeToTokens", options);
            const a = originalCodeToTokens(code, options);

            console.log("tokens", a);
            return a;
        }

        return highlighter;
    },
} satisfies CodeBlockOptions;
