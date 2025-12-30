import AttachesTool, { AttachesConfig, AttachesToolData, UploadResponseFormat } from "@editorjs/attaches";
import { BlockToolConstructorOptions, PasteConfig } from "@editorjs/editorjs";

/**
 * The save data for the RockImage block.
 */
interface AttachmentData extends AttachesToolData {
    file?: {
        /** The URL used to display the image. */
        url: string,

        /** The size of the file in bytes. */
        size: number,

        /** The filename extension. */
        extension: string,

        /** The filename with extension. */
        name: string,

        /** The identifier of the image if it was uploaded into Rock. */
        fileGuid?: string;
    }
}

/**
 * The configuration options for the RockImage block.
 */
interface AttachmentConfig extends AttachesConfig {
    /** The binary file type unique identifier to use when uploading. */
    binaryFileType?: string;
};

/**
 * Upload a file to Rock and return the link to view it with.
 *
 * @param file The file to be uploaded.
 */
async function uploadByFile(file: File, fileTypeGuid: string): Promise<UploadResponseFormat> {
    const data = new FormData();

    data.append("file", file);

    return await $.ajax({
        url: `/FileUploader.ashx?isBinaryFile=1&fileTypeGuid=${fileTypeGuid}`,
        type: 'POST',
        contentType: false,
        data: data,
        processData: false,
        cache: false,
        dataType: 'json'
    }).then(function (uploadedFile) {
        if (uploadedFile.Id && uploadedFile.FileName) {
            return {
                success: true,
                file: {
                    url: `/GetFile.ashx?guid=${uploadedFile.Guid}`,
                    name: uploadedFile.FileName as string,
                    title: uploadedFile.FileName as string,
                    size: file.size,
                    fileGuid: uploadedFile.Guid as string
                }
            }
        }
        else {
            return {
                success: false,
                file: {
                    url: "",
                    name: "",
                    fileGuid: ""
                }
            };
        }
    });
}


/**
 * Extended implmeentation of standard EditorJS attaches tool. This is updated
 * to work specifically with Rock.
 */
export class Attachment extends AttachesTool {
    private binaryFileType: string;

    constructor(config: BlockToolConstructorOptions<AttachmentData, AttachmentConfig>) {
        /* Use unsecured file type unless the user specifies one. */
        const binaryFileType = config.config?.binaryFileType || "C1142570-8CD6-4A20-83B1-ACB47C1CD377";

        config.config = config.config || {};

        // Override the uploader to use our custom Rock uploader.
        config.config.uploader = {
            uploadByFile: (file: File) => uploadByFile(file, binaryFileType)
        };

        super(config);

        this.binaryFileType = binaryFileType;
    }

    override get CSS() {
        const css = { ...super.CSS };

        css.wrapper = "structuredcontent-attachment";
        css.wrapperWithFile = "structuredcontent-attachment-withfile";
        css.wrapperLoading = "structuredcontent-attachment-loading";
        css.button = "structuredcontent-attachment-upload";
        css.title = "structuredcontent-attachment-fileinfo-title";
        css.size = "structuredcontent-attachment-fileinfo-size";
        css.downloadButton = "structuredcontent-attachment-download";
        css.fileInfo = "structuredcontent-attachment-fileinfo";
        css.fileIcon = "structuredcontent-attachment-fileicon";
        css.fileIconBackground = "structuredcontent-attachment-fileicon-background";
        css.fileIconLabel = "structuredcontent-attachment-fileicon-label";

        return css;
    }

    override showFileData() {
        super.showFileData();

        const title = this.nodes.wrapper?.querySelector(".structuredcontent-attachment-fileinfo-title");

        if (title) {
            title.setAttribute("spellcheck", "false");
        }

        const downloadButton = this.nodes.wrapper?.querySelector("a");

        if (downloadButton) {
            downloadButton.innerHTML = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="icon icon-tabler icons-tabler-outline icon-tabler-download"><path d="M4 17v2a2 2 0 0 0 2 2h12a2 2 0 0 0 2 -2v-2" /><path d="M7 11l5 5l5 -5" /><path d="M12 4l0 12" /></svg>`;
        }

        const uploadButton = this.nodes.wrapper?.querySelector(".structuredcontent-attachment-upload");

        if (uploadButton) {
            uploadButton.innerHTML = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="icon icon-tabler icons-tabler-outline icon-tabler-paperclip"><path d="M15 7l-6.5 6.5a1.5 1.5 0 0 0 3 3l6.5 -6.5a3 3 0 0 0 -6 -6l-6.5 6.5a4.5 4.5 0 0 0 9 9l6.5 -6.5" /></svg>`;
        }

        const iconBackground = this.nodes.wrapper?.querySelector(".structuredcontent-attachment-fileicon-background");

        if (iconBackground instanceof HTMLElement) {
            iconBackground.style.backgroundColor = "";
        }

        const iconLabel = this.nodes.wrapper?.querySelector(".structuredcontent-attachment-fileicon-label");

        if (iconLabel instanceof HTMLElement) {
            iconLabel.style.backgroundColor = "";
        }
    }

    /**
     * Specify paste substitutes
     *
     * @see {@link https://github.com/codex-team/editor.js/blob/master/docs/tools.md#paste-handling}
     */
    static get pasteConfig(): PasteConfig {
        return {
            tags: [],
            patterns: {},
            files: {
                mimeTypes: [
                    "application/*", // Bug in editorjs doesn't support periods, so we have to accept everything.
                ],
            },
        };
    }

    /**
     * Specify paste handlers
     *
     * @public
     * @see {@link https://github.com/codex-team/editor.js/blob/master/docs/tools.md#paste-handling}
     * @param {CustomEvent} event - editor.js custom paste event
     *                              {@link https://github.com/codex-team/editor.js/blob/master/types/tools/paste-events.d.ts}
     * @returns {void}
     */
    onPaste(event: CustomEvent): void {
        console.log("onPaste", event);
        if (event.type !== "file") {
            return;
        }

        const file = event.detail.file as File;

        this.nodes.wrapper?.classList.add(this.CSS.wrapperLoading, this.CSS.loader);
        const upload = uploadByFile(file, this.binaryFileType);

        upload.then(response => {
            this.uploader.onUpload(response);
        }).catch(errorResponse => {
            const error = errorResponse.body;
            const message = (error && error.message) ? error.message : "File upload failed";

            this.uploader.onError(message);
        })
    }
}
