import '@blocknote/core/style.css';
import './style.css';
import { BlockNoteEditor, BlockNoteSchema, createCodeBlockSpec, defaultBlockSpecs } from "@blocknote/core";
import { SideMenu } from './sideMenu';
import { codeBlockOptions } from './codeOptions';
import { tableBlockSpec, TableHandles } from './table';
import { FormattingToolbar, StyleToolbarButton } from './formattingToolbar';
import { SuggestionMenu } from './suggestionMenu';

export function createEditor(container: HTMLElement): void {
    const schema = BlockNoteSchema.create({
        blockSpecs: {
            ...defaultBlockSpecs,
            table: tableBlockSpec,
            codeBlock: createCodeBlockSpec({
                ...codeBlockOptions,
                defaultLanguage: "text",
            }),
        }
    });

    const editor = BlockNoteEditor.create({
        initialContent: [{ "id": "ddf33292-0d29-4696-bb52-07772b5ebd81", "type": "heading", "props": { "backgroundColor": "default", "textColor": "default", "textAlignment": "left", "level": 1, "isToggleable": false }, "content": [{ "type": "text", "text": "This is a big heading.", "styles": {} }], "children": [] }, { "id": "5c4e4046-6d61-4fff-b2ff-a87f71edb053", "type": "heading", "props": { "backgroundColor": "default", "textColor": "default", "textAlignment": "left", "level": 3, "isToggleable": false }, "content": [{ "type": "text", "text": "This is a much smaller heading.", "styles": {} }], "children": [] }, { "id": "471efa80-e1df-4292-8ca9-83c0f51a4a42", "type": "codeBlock", "props": { "language": "csharp" }, "content": [{ "type": "text", "text": "var obj = new object();", "styles": {} }], "children": [] }, { "id": "1294d8c6-9057-44b5-8953-fcc0a1fc30fd", "type": "table", "props": { "textColor": "default" }, "content": { "type": "tableContent", "columnWidths": [null, null], "rows": [{ "cells": [{ "type": "tableCell", "content": [{ "type": "text", "text": "Title", "styles": {} }], "props": { "colspan": 1, "rowspan": 1, "backgroundColor": "default", "textColor": "default", "textAlignment": "left" } }, { "type": "tableCell", "content": [{ "type": "text", "text": "Description", "styles": {} }], "props": { "colspan": 1, "rowspan": 1, "backgroundColor": "default", "textColor": "default", "textAlignment": "left" } }] }, { "cells": [{ "type": "tableCell", "content": [{ "type": "text", "text": "Rock Solid Church", "styles": {} }], "props": { "colspan": 1, "rowspan": 1, "backgroundColor": "default", "textColor": "default", "textAlignment": "left" } }, { "type": "tableCell", "content": [{ "type": "text", "text": "A sample website that is used for testing.", "styles": {} }], "props": { "colspan": 1, "rowspan": 1, "backgroundColor": "default", "textColor": "default", "textAlignment": "left" } }] }, { "cells": [{ "type": "tableCell", "content": [{ "type": "text", "text": "Another title", "styles": {} }], "props": { "colspan": 1, "rowspan": 1, "backgroundColor": "default", "textColor": "default", "textAlignment": "left" } }, { "type": "tableCell", "content": [{ "type": "text", "text": "Some more descriptive text.", "styles": {} }], "props": { "colspan": 1, "rowspan": 1, "backgroundColor": "default", "textColor": "default", "textAlignment": "left" } }] }] }, "children": [] }, { "id": "8fdd0747-54e4-48c1-bdbb-e98d3d2157d0", "type": "bulletListItem", "props": { "backgroundColor": "default", "textColor": "default", "textAlignment": "left" }, "content": [{ "type": "text", "text": "This is a list item.", "styles": {} }], "children": [{ "id": "e3de4f1c-1c26-46a9-a9a7-c711e3d4f2d8", "type": "bulletListItem", "props": { "backgroundColor": "default", "textColor": "default", "textAlignment": "left" }, "content": [{ "type": "text", "text": "This is an indented item.", "styles": {} }], "children": [] }] }, { "id": "9d588516-2789-4432-9765-4973f357e532", "type": "numberedListItem", "props": { "backgroundColor": "default", "textColor": "default", "textAlignment": "left" }, "content": [{ "type": "text", "text": "This is a numbered list.", "styles": {} }], "children": [] }, { "id": "7a18279f-114c-4792-8df7-58514a6340a6", "type": "numberedListItem", "props": { "backgroundColor": "default", "textColor": "default", "textAlignment": "left" }, "content": [{ "type": "text", "text": "And a second item.", "styles": {} }], "children": [] }, { "id": "ba6dcc20-45fc-45a8-b0bd-1a70a5060c5a", "type": "paragraph", "props": { "backgroundColor": "default", "textColor": "default", "textAlignment": "left" }, "content": [], "children": [] }] as any,
        schema: schema,
        tables: {
            headers: true
        },
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

    const editorContainer = document.createElement("div");
    container.prepend(editorContainer);
    editor.mount(editorContainer);

    // editor.onChange(() => console.log(JSON.stringify(editor.document)));
}
