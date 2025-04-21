declare module "@editorjs/image" {
    import { BlockTool, BlockToolConstructorOptions } from "@editorjs/editorjs";

    export type TunesMenuConfigItem = {
        name: string
    };

    export type TunesMenuConfig = TunesMenuConfigItem | TunesMenuConfigItem[];

    export interface ImageConfig {
        endpoints?: {
            byFile?: string,
            byUrl?: string
        },
        field?: string,
        types?: string,
        additionalRequestData?: object,
        additionalRequestHeaders?: object,
        captionPlaceholder?: string,
        buttonContent?: string,
        uploader?: {
            uploadByFile?: Function,
            uploadByUrl?: Function
        },
        actions?: Array<object>
    }

    export interface ImageUpload {
        success: boolean,
        file: {
            url: string
        }
    }

    export interface ImageData {
        caption?: string,
        stretched?: boolean
        withBorder?: boolean,
        withBackground?: boolean,

        file?: {
            url: string
        },
    }

    export default class Image implements BlockTool {
        constructor(config: BlockToolConstructorOptions<ImageData, ImageConfig>);

        sanitize?: SanitizerConfig;

        renderSettings(): TunesMenuConfig;
        save(blockWrapper: HTMLDivElement): ImageData;
        render(): HTMLElement;
    }
}
