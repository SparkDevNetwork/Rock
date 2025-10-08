declare module "@editorjs/attaches" {
    import { TunesMenuConfig } from "@editorjs/editorjs/types/tools";
    import { BlockTool, BlockToolConstructorOptions, SanitizerConfig } from "@editorjs/editorjs";

    export interface AttachesConfig {
        endpoint?: string,
        field?: string,
        types?: string,
        errorMessage?: string,
        uploader?: {
            uploadByFile?: Function,
        },
    }

    export interface UploadResponseFormat {
        success: boolean,
        file: AttachesFileData
    }

    export interface AttachesFileData {
        url: string,
        size?: number,
        extension?: string,
        name: string,
    }

    export interface AttachesToolData {
        title?: string,

        file?: AttachesFileData,
    }

    export default class Attaches implements BlockTool {
        nodes: {
            wrapper?: HTMLElement;
        };
        uploader: {
            uploadByFile?: (file: File, options?: { onPreview?: () => void }) => Promise<UploadResponseFormat>;
            onUpload: (response: UploadResponseFormat) => void;
            onError: (message: string) => void;
        };

        constructor(config: BlockToolConstructorOptions<AttachesToolData, AttachesConfig>);

        get CSS(): Record<string, string>;
        sanitize?: SanitizerConfig;

        renderSettings(): TunesMenuConfig;
        save(blockWrapper: HTMLDivElement): AttachesToolData;
        render(): HTMLElement;

        showFileData(): void;
    }
}
