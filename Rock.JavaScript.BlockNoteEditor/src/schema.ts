import { BlockNoteSchema, createCodeBlockSpec, defaultBlockSpecs } from "@blocknote/core";
import { tableBlockSpec } from "./table";
import { codeBlockOptions } from "./codeOptions";
import { createImageBlockSpec } from "./image";

export const rockSchema = BlockNoteSchema.create({
    blockSpecs: {
        ...defaultBlockSpecs,
        table: tableBlockSpec,
        codeBlock: createCodeBlockSpec({
            ...codeBlockOptions,
            defaultLanguage: "text",
        }),
        image: createImageBlockSpec(),
    }
});

export type RockBlock = typeof rockSchema.Block;
export type RockBlockNoteEditor = typeof rockSchema.BlockNoteEditor;
export type RockPartialBlock = typeof rockSchema.PartialBlock;
