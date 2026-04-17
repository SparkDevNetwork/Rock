import '@blocknote/core/style.css';
import './style.css';
import { BlockNoteEditor } from "@blocknote/core";
import { SideMenu } from './sideMenu';
import { TableHandles } from './table';
import { FormattingToolbar, StyleToolbarButton } from './formattingToolbar';
import { SuggestionMenu } from './suggestionMenu';
import { FilePanel, uploadBinaryFile } from './filePanel';
import { rockSchema, type RockPartialBlock } from './schema';
import { initialContent } from './initialContent';

export type BlockNoteOptions = {
    fileTypeGuid?: string | null;
};

export function createEditor(container: HTMLElement, options?: BlockNoteOptions): void {
    const editor = BlockNoteEditor.create({
        initialContent: initialContent as RockPartialBlock[],
        schema: rockSchema,
        tables: {
            headers: true
        },
        uploadFile: (file) => uploadBinaryFile(file, options?.fileTypeGuid),
    });

    container.classList.add("bn-container");

    new SideMenu(editor, container);
    new FormattingToolbar(editor, [
        new StyleToolbarButton("ti ti-bold", "bold"),
        new StyleToolbarButton("ti ti-italic", "italic"),
        new StyleToolbarButton("ti ti-underline", "underline"),
        new StyleToolbarButton("ti ti-strikethrough", "strike")
    ]);
    new TableHandles(editor, container);
    new SuggestionMenu(editor, container);
    new FilePanel(editor, container);

    const editorContainer = document.createElement("div");
    container.prepend(editorContainer);
    editor.mount(editorContainer);

    editor.onChange(() => console.log(editor.document));
}
