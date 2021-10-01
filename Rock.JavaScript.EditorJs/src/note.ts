import { API, BlockTool, BlockToolConstructorOptions, PasteEvent, SanitizerConfig } from "@editorjs/editorjs";
import { v4 as uuidv4 } from 'uuid';

export interface NoteData {
    id?: string;
    note?: string;
}

export interface NoteConfig {
    placeholder?: string;
}

export class Note implements BlockTool {
    /**
     * Tool's Toolbox settings
     */
    public static get toolbox() {
        return {
            icon: '<svg width="16" height="16" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 448 512"><path d="M448 348.106V80c0-26.51-21.49-48-48-48H48C21.49 32 0 53.49 0 80v351.988c0 26.51 21.49 48 48 48h268.118a48 48 0 0 0 33.941-14.059l83.882-83.882A48 48 0 0 0 448 348.106zm-128 80v-76.118h76.118L320 428.106zM400 80v223.988H296c-13.255 0-24 10.745-24 24v104H48V80h352z"/></svg>',
            title: "Note"
        }
    };

    /**
     * Determines if this blocks supports line breaks inside its content.
     *
     * @returns true if line breaks should be allowed, false otherwise.
     */
    public static get enableLineBreaks() {
        return true;
    }

    private api: API;
    private placeholder: string;
    private id!: string;
    private holderNode: HTMLElement;
    private textAreaNode!: HTMLTextAreaElement;

    public sanitize?: SanitizerConfig;

    public get data(): NoteData {
        return {
            id: this.id,
            note: this.holderNode.querySelector("textarea")?.value || ""
        };
    }

    public set data(value: NoteData) {
        this.id = value.id || uuidv4();
        this.textAreaNode.value = value.note || "";
    }

    constructor(config: BlockToolConstructorOptions<NoteData, NoteConfig>) {
        this.api = config.api;
        this.placeholder = config.config?.placeholder || "Enter note";
        this.holderNode = this.drawView();
        this.data = config.data;
    }

    private drawView(): HTMLElement {
        const div = document.createElement("div");
        const textarea = document.createElement("textarea");

        div.classList.add(this.api.styles.block, "ce-note");
        textarea.classList.add("ce-note__textarea", this.api.styles.input);
        textarea.placeholder = this.placeholder;
        div.appendChild(textarea);

        this.textAreaNode = textarea;

        return div;
    }

    render(): HTMLElement {
        return this.holderNode;
    }

    save(block: HTMLElement): NoteData {
        return {
            note: block.querySelector("textarea")?.value || ""
        };
    }
}
