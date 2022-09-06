import { CompilerError, SFCDescriptor, SFCScriptBlock, SFCTemplateCompileResults } from "vue/compiler-sfc";
import * as compiler from "vue/compiler-sfc";
import { customAlphabet } from "nanoid";

const nanoid = customAlphabet("abcdefghijklmnopqrstuvwxyz01234567890", 24);

/**
 * Compiles the script tag into a string that can be later inserted into the
 * actual TypeScript file.
 * 
 * @param descriptor The description of the parsed SFC.
 * @param id The identifier used to scope this SFC.
 * 
 * @returns An object that contains the script code.
 */
function compileScript(descriptor: SFCDescriptor, id: string): { result: SFCScriptBlock, code: string } {
    // Attempt to compile the script portion of the SFC.
    const scriptResult = compiler.compileScript(descriptor, {
        id: id,
        isProd: true,
        inlineTemplate: false
    });

    const scriptCode = compiler.rewriteDefault(
        scriptResult.content,
        "_sfc_main",
        scriptResult.lang === "ts" ? ["typescript"] : undefined
    );

    return {
        result: scriptResult,
        code: scriptCode
    };
}

/**
 * Compilies the HTML template into pure JavaScript that can be inserted into
 * the TypeScript file for final compilation.
 * 
 * @param descriptor The description of the parsed SFC.
 * @param id The identifier used to scope this SFC.
 * @param script The extracted script information from the SFC.
 * 
 * @returns The results of the template compilation.
 */
function compileTemplate(descriptor: SFCDescriptor, id: string, script: SFCScriptBlock): SFCTemplateCompileResults | undefined {
    // Get the template block of the SFC.
    const block = descriptor.template;
    if (!block) {
        console.error("Missing template element.");
        return undefined;
    }

    const lang = descriptor.scriptSetup?.lang || descriptor.script?.lang;
    const hasScoped = descriptor.styles.some((s) => s.scoped);

    const result = compiler.compileTemplate({
        filename: descriptor.filename,
        id: id,
        scoped: hasScoped,
        source: descriptor.template?.content ?? "",
        slotted: descriptor.slotted,
        isProd: true,
        inMap: block.src ? undefined : block.map,
        ssr: false,
        ssrCssVars: descriptor.cssVars,
        preprocessLang: block.lang,
        compilerOptions: {
            scopeId: hasScoped ? `data-v-${id}` : undefined,
            expressionPlugins: lang === "ts" ? ["typescript"] : [],
            bindingMetadata: script.bindings,
        }
    });

    if (result.errors.length > 0) {
        writeErrors(result.errors, descriptor.filename);
        return undefined;
    }

    return result;
}

/**
 * Compile the style tags into a single string. This handles scoped slots.
 * 
 * @param descriptor The description of the parsed SFC.
 * @param id The identifier used to scope this SFC.
 * 
 * @returns A string that contains all the contents from the style tags.
 */
function compileStyle(descriptor: SFCDescriptor, id: string): string | undefined {
    const styleStrings: string[] = [];

    for (const i in descriptor.styles) {
        const style = compiler.compileStyle({
            filename: descriptor.filename,
            id: `data-v-${id}`,
            isProd: true,
            source: descriptor.styles[i].content,
            scoped: descriptor.styles[i].scoped
        });

        // Check if there were any errors parsing the template.
        if (style.errors.length > 0) {
            writeErrors(style.errors, descriptor.filename);
            return undefined;
        }

        styleStrings.push(style.code);
    }

    return styleStrings.join("\n");
}

/**
 * Writes an array of errors to the console.
 * 
 * @param errors The errors to be written to the output.
 * @param filename The name of the file that generated the error.
 */
function writeErrors(errors: (string | CompilerError | SyntaxError)[], filename: string): void {
    for (const error of errors) {
        if (typeof error === "string") {
            console.error(error);
        }
        else {
            const compilerError = error as CompilerError;

            if (compilerError.loc) {
                console.error(`error SFC0001: File '${filename}' at ${compilerError.loc.start.line}:${compilerError.loc.start.column}. ${compilerError.message}`);
            }
            else {
                console.error(`error SFC0001: File '${filename}'. ${error.message}`);
            }
        }
    }
}

/**
 * Translates a Vue SFC file into a pure TypeScript file for the compiler.
 * 
 * @param source The source of the SFC file.
 * @param filename The full path and filename to the SFC file.
 * 
 * @returns A string that contains the SFC file in a format that TypeScript can understand.
 */
export function translateVue(source: string, filename: string): string | undefined {
    const id = nanoid();

    // Parse the template.
    const { descriptor, errors } = compiler.parse(source, {
        filename: filename,
    });

    // Check if there were any errors parsing the template.
    if (errors.length > 0) {
        writeErrors(errors, filename);
        return undefined;
    }

    const hasScoped = descriptor.styles.some((s) => s.scoped);
    const { code: scriptCode, result: scriptResult } = compileScript(descriptor, id);
    let templateCode = "";

    if (descriptor.template) {
        const template = compileTemplate(descriptor, id, scriptResult);

        if (!template) {
            return undefined;
        }

        // Insert "@ts-ignore" before each line so TypeScript doesn't complain.
        templateCode = (template.code ?? "").replace("\r\n", "\n").split("\n").map(l => `// @ts-ignore\n${l}`).join("\n");
    }

    const style = compileStyle(descriptor, id);

    // Start defining the output code and properties.
    const output: string[] = [scriptCode, templateCode];
    const attachedProps: [string, string][] = [];

    // If we have a template then attach it.
    if (descriptor.template) {
        attachedProps.push(["render", "render"]);
    }

    // If we are scoped then we need to insert the scope identifier.
    if (hasScoped) {
        attachedProps.push([`__scopeId`, JSON.stringify(`data-v-${id}`)]);
    }

    // Generate the style tag to be inserted in the head.
    if (style) {
        output.push(`const __sfc_style = document.createElement("style");
__sfc_style.textContent = \`${style.trim()}\`;
document.head.appendChild(__sfc_style);`);
    }

    // If we don't have any attached properties then just export the script.
    if (!attachedProps.length) {
        output.push("export default _sfc_main;");
    }
    else {
        output.push(`const exportHelper = _sfc_main;
for (const [key, val] of [${attachedProps.map(([key, val]) => `["${key}",${val}]`).join(",")}]) {
  // @ts-ignore
  exportHelper[key] = val;
}
export default exportHelper;`);
    }

    const finalCode = output.join("\n");

    return finalCode;
}
