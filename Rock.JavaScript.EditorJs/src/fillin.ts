import { API, InlineTool } from "@editorjs/editorjs"
import { v4 as uuidv4 } from "uuid";
import "./fillin.css";

/**
 * The configuration options for the fillin tool.
 */
export interface FillinConfig {
}

/**
 * The fillin tool that allows the user to mark a bit of text as a blank
 * field to be filled in (or revealed).
 */
export class Fillin implements InlineTool {
    /** The API for the EditorJS instance. */
    private api: API;

    /** The configuration data we were initialized with. */
    private config: FillinConfig;

    /** The button that will be used to activate this tool. */
    private button?: HTMLButtonElement;

    /** The HTML tag used by this tool. */
    private static tag = "FILLIN";

    /** The CSS class to apply to the tool. */
    private static CSS = "rock-fillin";

    /** The icon classes to apply. */
    private iconClasses: {
        base: string;
        active: string;
    };

    /**
     * Initializes a new instance of this inline tool.
     * 
     * @param config The initialization options for the tool.
     */
    constructor(config: { config: FillinConfig, api: API }) {
        this.api = config.api;
        this.config = config.config;

        this.iconClasses = {
            base: this.api.styles.inlineToolButton,
            active: this.api.styles.inlineToolButtonActive
        }
    }

    /**
     * Get tool icon's title.
     * 
     * @return {string}
     */
    static get title() {
        return "Fill In";
    }

    /**
     * Sanitizer rule. All CODE html elements should be allowed.
     *
     * @returns The rule that describes when this is allowed.
     */
    static get sanitize() {
        return {
            fillin: {
                class: Fillin.CSS,
                "data-id": true
            },
        };
    }

    /**
     * Indicates if this tool is an inline tool or not.
     */
    static get isInline() {
        return true;
    }

    /**
     * Renders the inline toolbar button.
     * 
     * @returns The HTML element to display in the inline toolbar.
     */
    render() {
        this.button = document.createElement("button");
        this.button.type = "button";
        this.button.classList.add(this.iconClasses.base);
        this.button.innerHTML = this.toolboxIcon;

        return this.button;
    }

    /**
     * Get the tools icon HTML content.
     * 
     * @return A string that describes the icon node.
     */
    private get toolboxIcon() {
        return `<svg width="13" height="11" viewBox="0 0 13 11" xmlns="http://www.w3.org/2000/svg">\n<path d="M8.87472 8.48199L3.40613 8.48187C3.20317 8.48187 3.00766 8.57659 2.86415 8.74518C2.72063 8.91378 2.64 9.14244 2.64 9.38087C2.64 9.6193 2.72063 9.84797 2.86415 10.0166C3.00766 10.1852 3.20231 10.2799 3.40528 10.2799L8.87472 10.28C9.07769 10.28 9.27234 10.1853 9.41585 10.0167C9.55937 9.84809 9.64 9.61942 9.64 9.38099C9.64 9.14257 9.55937 8.9139 9.41585 8.74531C9.27234 8.57671 9.07769 8.48199 8.87472 8.48199Z" />\n<rect width="1.56641" height="10.28"/>\n<rect x="10.7136" width="1.56641" height="10.28"/>\n</svg>`;
    }

    /**
     * Method that accepts selected range and wrap it somehow.
     * 
     * @param range The range of text to be surrounded by code tags.
     */
    public surround(range: Range): void {
        const termWrapper = this.api.selection.findParentTag(Fillin.tag);

        if (termWrapper) {
            this.unwrap(termWrapper);
        } else {
            this.wrap(range);
        }
    }

    /**
     * Updates the state of our button based on the current selection.
     * 
     * @param selection The current selection made by the user.
     * @returns false
     */
    public checkState(selection: Selection): boolean {
        const termTag = this.api.selection.findParentTag(Fillin.tag, Fillin.CSS);

        this.button?.classList.toggle(this.iconClasses.active, !!termTag);

        return false;
    }

    /**
     * Wrap selection with code tag.
     *
     * @param range Selected text range.
     */
    private wrap(range: Range) {
        /**
         * Create a wrapper for highlighting
         */
        let span = document.createElement(Fillin.tag);
        span.classList.add(Fillin.CSS);
        span.dataset.id = uuidv4();

        /**
         * SurroundContent throws an error if the Range splits a non-Text node with only one of its boundary points
         * @see {@link https://developer.mozilla.org/en-US/docs/Web/API/Range/surroundContents}
         *
         * // range.surroundContents(span);
         */
        span.appendChild(range.extractContents());
        range.insertNode(span);

        /**
         * Expand (add) selection to highlighted block
         */
        this.api.selection.expandToTag(span);
    }

    /**
     * Unwrap selection from code tag.
     *
     * @param termWrapper The element that is currently wrapping the text.
     */
    private unwrap(termWrapper: HTMLElement) {
        /**
         * Expand selection to all term-tag
         */
        this.api.selection.expandToTag(termWrapper);

        let sel = <Selection>window.getSelection();
        let range = sel.getRangeAt(0);
        let unwrappedContent = range.extractContents();

        /**
         * Remove empty term-tag
         */
        termWrapper.parentNode?.removeChild(termWrapper);

        /**
         * Insert extracted content
         */
        range.insertNode(unwrappedContent);

        /**
         * Restore selection
         */
        sel.removeAllRanges();
        sel.addRange(range);
    }
}
