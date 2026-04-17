import { FilePanelExtension } from "@blocknote/core/extensions";
import type { RockBlockNoteEditor } from "./schema";

export class FilePanel {
    private readonly fileElement: HTMLInputElement;

    private blockId: string | undefined;

    constructor(editor: RockBlockNoteEditor, container: HTMLElement) {
        this.fileElement = document.createElement("input");
        this.fileElement.type = "file";
        this.fileElement.style.display = "none";

        container.appendChild(this.fileElement);

        const filePanelExtension = editor.getExtension(FilePanelExtension)!;

        this.fileElement.addEventListener("change", async () => {
            const blockId = this.blockId;

            if (!this.fileElement.files || this.fileElement.files.length === 0) {
                return;
            }

            if (!editor.uploadFile || !blockId) {
                return;
            }

            const file = this.fileElement.files[0];

            try {
                let updateData = await editor.uploadFile(file, blockId);

                if (typeof updateData === "string") {
                    updateData = {
                        props: {
                            name: file.name,
                            url: updateData,
                        },
                    };
                }

                editor.updateBlock(blockId, updateData);
            }
            catch (e) {
                console.error("File upload failed", e);
                alert("File upload failed");
            }
            finally {
                this.fileElement.value = "";
            }
        });

        filePanelExtension?.store.subscribe((state) => {
            const blockId = state.currentVal;

            this.blockId = blockId;

            if (!blockId) {
                return;
            }

            this.fileElement.value = "";
            this.fileElement.click();
        });
    }
}

export async function uploadBinaryFile(file: File, fileTypeGuid: string | null | undefined): Promise<string | Record<any, any>> {
    const data = new FormData();
    fileTypeGuid = fileTypeGuid ?? "C1142570-8CD6-4A20-83B1-ACB47C1CD377";

    data.append("file", file);

    const response = await fetch(`/FileUploader.ashx?isBinaryFile=1&fileTypeGuid=${fileTypeGuid}`, {
        method: "POST",
        body: data
    });

    if (!response.ok) {
        throw new Error("Network response was not ok");
    }

    const isImage = file.type.startsWith("image/");
    const uploadedFile = await response.json();

    if (uploadedFile.Id && uploadedFile.FileName) {
        return {
            props: {
                url: `/${isImage ? "GetImage" : "GetFile"}.ashx?guid=${uploadedFile.Guid}`,
                name: file.name,
                fileGuid: uploadedFile.Guid,
            }
        };
    }
    else {
        throw new Error("Invalid response from server");
    }
}
