import EditorJSCode, { CodeData as EditorJSCodeData, CodeConfig as EditorJSCodeConfig } from "@editorjs/code";
import { API } from "@editorjs/editorjs";

/**
 * The data for the custom code tool.
 */
export interface CodeData extends EditorJSCodeData {
    /** The language code that should be used for syntax highlighting. */
    lang?: string;
}

/**
 * A dictionary of all the supported languages. The key is the short language
 * code and the value is the user friendly text.
 */
const supportedLanguages: Record<string, string> = {
    bash: "Bash",
    c: "C",
    csharp: "C#",
    cpp: "C++",
    coffee: "Coffee",
    coffeescript: "Coffee Script",
    css: "CSS",
    html: "HTML",
    javascript: "JavaScript",
    json: "JSON",
    markdown: "Markdown",
    objectivec: "Objective-C",
    perl: "Perl",
    php: "PHP",
    plsql: "PL/SQL",
    powershell: "PowerShell",
    py: "Python",
    rb: "Ruby",
    regex: "Regular Expression",
    ruby: "Ruby",
    sass: "SASS",
    scss: "SCSS",
    shell: "Shell",
    sql: "SQL",
    svg: "SVG",
    swift: "Swift",
    ts: "TypeScript",
    xaml: "XAML",
    xml: "XML",
    yaml: "YAML"
};

/**
 * Custom code editor that extends the standard code tool in EditorJS. Provides
 * additional option to select the language of the code being entered.
 */
export class Code extends EditorJSCode {
    /** The HTML node that provides the user with the option to select the language. */
    private languageNode?: HTMLSelectElement;

    /**
     * Creates a new instance of the code tool.
     * 
     * @param config The initialization options for the tool.
     */
    constructor(config: { data: CodeData, config: EditorJSCodeConfig, api: API, readOnly: boolean }) {
        super(config);

        const holder = <HTMLDivElement>(<any>this).nodes.holder;

        this.languageNode = this.createLanguageNode();
        this.languageNode.selectedIndex = this.findLanguageIndex(config.data?.lang || "");
        holder.appendChild(this.languageNode);
    }

    /**
     * Create the HTML node that will let the user pick the language.
     * 
     * @returns The HTML node that will be used for picking the language.
     */
    createLanguageNode(): HTMLSelectElement {
        const languageNode = document.createElement("select");
        languageNode.setAttribute("class", "form-control");

        let option = document.createElement("option");
        option.value = "";
        option.text = "Select Language";
        languageNode.options.add(option);

        const languageKeys = Object.keys(supportedLanguages);
        for (let i = 0; i < languageKeys.length; i++) {
            const key = languageKeys[i];

            option = document.createElement("option");
            option.value = key;
            option.text = supportedLanguages[key];
            languageNode.options.add(option);
        }

        return languageNode;
    }

    /**
     * Gets the save data for the code tool.
     * 
     * @param codeWrapper The HTML element that contains all our data.
     * @returns An object that contains all the data required to save this block.
     */
    public save(codeWrapper: HTMLDivElement) {
        const data: CodeData = {
            ...super.save(codeWrapper)
        };

        const languageNode = codeWrapper.querySelector("select");
        if (languageNode !== null) {
            data.lang = languageNode.options[languageNode.selectedIndex].value;
        }

        return data;
    }

    /**
     * Validates the data to see if it should be included in the saved data
     * stream.
     * 
     * @param savedData The data that was returned by the save method.
     * @returns True if the data is valid and should be saved.
     */
    public validate(savedData: CodeData) {
        return savedData.code !== "";
    }

    /**
     * Returns the tool's private data.
     *
     * @returns The private data for the tool.
     */
    get data() {
        return super.data;
    }

    /**
     * Sets the tools private data.
     *
     * @param data Saved tool data
     */
    set data(data: CodeData) {
        super.data = data;

        if (this.languageNode !== undefined) {
            this.languageNode.selectedIndex = this.findLanguageIndex(data?.lang || "");
        }
    }

    /**
     * Finds the index of the specified language identifier in the select HTML
     * node.
     * 
     * @param lang The language identifier.
     * @returns The index of the language identifier or 0 if not found.
     */
    findLanguageIndex(lang: string) {
        if (this.languageNode !== undefined) {
            for (let i = 0; i < this.languageNode.options.length; i++) {
                if (this.languageNode?.options[i].value === lang) {
                    return i;
                }
            }
        }

        return 0;
    }
}
