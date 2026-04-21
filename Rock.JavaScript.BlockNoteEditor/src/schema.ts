import { BlockNoteSchema, createCodeBlockSpec, defaultBlockSpecs } from "@blocknote/core";
import { mediaBlockSpec } from "./media";
import { tableBlockSpec } from "./table";
import { codeBlockOptions } from "./codeOptions";

export const rockSchema = BlockNoteSchema.create({
    blockSpecs: {
        ...defaultBlockSpecs,
        media: mediaBlockSpec,
        table: tableBlockSpec,
        codeBlock: createCodeBlockSpec({
            ...codeBlockOptions,
            defaultLanguage: "text",
        }),
    }
});

export type RockBlock = typeof rockSchema.Block;
export type RockBlockNoteEditor = typeof rockSchema.BlockNoteEditor;
export type RockPartialBlock = typeof rockSchema.PartialBlock;
