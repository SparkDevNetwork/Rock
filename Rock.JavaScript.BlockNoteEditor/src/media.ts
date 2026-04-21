import { createBlockConfig, createBlockSpec } from "@blocknote/core";

// Media block is a component for inserting Rock RMS Media Elements into the
// editor.

const guidPattern = /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i;

type MediaBlockState = "not-configured" | "configured-valid" | "configured-invalid";

type MediaPickerOption = {
    value: string;
    text: string;
};

type MediaMetadata = {
    name?: string;
    thumbnailUrl?: string;
};

type MediaPickerDomResult = {
    dom: HTMLElement;
    destroy?: () => void;
    ignoreMutation?: (mutation: any) => boolean;
};

function normalizeMediaElementGuid(mediaElementGuid: string): string {
    return mediaElementGuid.trim();
}

function getMediaBlockState(mediaElementGuid: string): MediaBlockState {
    if (!mediaElementGuid) {
        return "not-configured";
    }

    if (guidPattern.test(mediaElementGuid)) {
        return "configured-valid";
    }

    return "configured-invalid";
}

function createPickerField(labelText: string, placeholderText: string): {
    wrapper: HTMLLabelElement;
    select: HTMLSelectElement;
} {
    const wrapper = document.createElement("label");
    wrapper.classList.add("bn-media-block-field");

    const label = document.createElement("span");
    label.classList.add("bn-media-block-label");
    label.textContent = labelText;
    wrapper.append(label);

    const select = document.createElement("select");
    select.classList.add("bn-media-block-select", "form-control");
    select.disabled = true;

    const placeholder = document.createElement("option");
    placeholder.value = "";
    placeholder.textContent = placeholderText;
    select.append(placeholder);

    wrapper.append(select);

    return {
        wrapper,
        select,
    };
}

function setSelectOptions(select: HTMLSelectElement, placeholderText: string, options: MediaPickerOption[]): void {
    select.replaceChildren();

    const placeholder = document.createElement("option");
    placeholder.value = "";
    placeholder.textContent = placeholderText;
    select.append(placeholder);

    for (const option of options) {
        const optionElement = document.createElement("option");
        optionElement.value = option.value;
        optionElement.textContent = option.text;
        select.append(optionElement);
    }
}

function createMediaThumbnailDom(title: string, imgElement: HTMLImageElement | undefined): HTMLElement {
    const fallback = document.createElement("div");
    fallback.classList.add("bn-media-block-thumbnail");

    if (imgElement) {
        fallback.append(imgElement);
    }
    else {
        fallback.innerHTML = "<svg viewBox=\"0 0 1280 720\" aria-hidden=\"true\" focusable=\"false\"></svg>";
    }

    const iconElement = document.createElement("div");
    iconElement.classList.add("bn-media-block-icon");
    iconElement.innerHTML = "<i class=\"ti ti-video\"></i>";
    fallback.appendChild(iconElement);

    const nameElement = document.createElement("div");
    nameElement.classList.add("bn-media-block-thumbnail-title");
    nameElement.textContent = title;
    fallback.append(nameElement);

    return fallback;
}

function createConfiguredMediaBlockBase(mediaElementGuid: string): HTMLDivElement {
    const dom = document.createElement("div");
    dom.classList.add("bn-media-block", "bn-media-block-configured-valid");
    dom.setAttribute("data-rock-media-block", "");
    dom.setAttribute("data-media-element-guid", mediaElementGuid);
    dom.setAttribute("data-media-block-state", "configured-valid");

    const thumbnail = createMediaThumbnailDom("No thumbnail", undefined);
    dom.append(thumbnail);

    return dom;
}

async function fetchMediaMetadata(mediaElementGuid: string, signal: AbortSignal): Promise<MediaMetadata> {
    const response = await fetch("/api/v2/controls/MediaElementPickerGetMediaElementMetadata", {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
        },
        body: JSON.stringify({ mediaElementGuid }),
        signal,
    });

    if (!response.ok) {
        throw new Error(`Request failed with status ${response.status}`);
    }

    const data = await response.json();

    if (!data || typeof data !== "object") {
        throw new Error("Response payload was not an object.");
    }

    return {
        name: typeof data.name === "string" ? data.name.trim() : undefined,
        thumbnailUrl: typeof data.thumbnailUrl === "string" ? data.thumbnailUrl.trim() : undefined,
    };
}

function createConfiguredMediaBlockDom(mediaElementGuidValue: string): MediaPickerDomResult {
    const mediaElementGuid = normalizeMediaElementGuid(mediaElementGuidValue);
    const state = getMediaBlockState(mediaElementGuid);

    if (state === "configured-valid") {
        const dom = createConfiguredMediaBlockBase(mediaElementGuid);
        const abortController = new AbortController();
        let disposed = false;

        void fetchMediaMetadata(mediaElementGuid, abortController.signal)
            .then(metadata => {
                if (disposed || !metadata.thumbnailUrl) {
                    return;
                }

                const image = document.createElement("img");
                image.classList.add("bn-media-block-thumbnail-image");
                image.alt = "Media thumbnail";

                image.addEventListener("error", () => {
                    if (disposed) {
                        return;
                    }

                    dom.replaceChildren(createMediaThumbnailDom("No thumbnail", undefined));
                }, { once: true });

                image.src = metadata.thumbnailUrl;
                dom.replaceChildren(createMediaThumbnailDom(metadata.name ?? "Unknown", image));
            })
            .catch(error => {
                if (disposed || (error instanceof DOMException && error.name === "AbortError")) {
                    return;
                }
            });

        return {
            dom,
            destroy: () => {
                disposed = true;
                abortController.abort();
            },
            ignoreMutation: (mutation) => mutation.target instanceof Node && dom.contains(mutation.target),
        };
    }

    const dom = document.createElement("div");
    dom.classList.add("bn-media-block", `bn-media-block-${state}`);
    dom.setAttribute("data-rock-media-block", "");
    dom.setAttribute("data-media-element-guid", mediaElementGuid);
    dom.setAttribute("data-media-block-state", state);

    const title = document.createElement("div");
    title.classList.add("bn-media-block-title");
    dom.append(title);

    title.textContent = "Media element is invalid";

    const message = document.createElement("div");
    message.classList.add("bn-media-block-message");
    message.textContent = "The configured media element value is not a valid Guid. Delete and recreate this block.";
    dom.append(message);

    const invalidValue = document.createElement("div");
    invalidValue.classList.add("bn-media-block-guid");
    invalidValue.textContent = mediaElementGuid;
    dom.append(invalidValue);

    return {
        dom,
    };
}

function createConfiguredMediaExternalDom(mediaElementGuidValue: string): HTMLDivElement {
    const mediaElementGuid = normalizeMediaElementGuid(mediaElementGuidValue);
    const state = getMediaBlockState(mediaElementGuid);

    if (state === "configured-valid") {
        return createConfiguredMediaBlockBase(mediaElementGuid);
    }

    return createConfiguredMediaBlockDom(mediaElementGuid).dom as HTMLDivElement;
}

function createMediaPickerDom(block: any, editor: any): MediaPickerDomResult {
    const dom = document.createElement("div");
    dom.classList.add("bn-media-block", "bn-media-block-not-configured");
    dom.setAttribute("data-rock-media-block", "");
    dom.setAttribute("data-media-element-guid", "");
    dom.setAttribute("data-media-block-state", "not-configured");

    const description = document.createElement("div");
    description.classList.add("bn-media-block-message");
    description.textContent = "Select a media account, folder, and media element to configure this block.";
    dom.append(description);

    const status = document.createElement("div");
    status.classList.add("bn-media-block-status");
    dom.append(status);

    const accountField = createPickerField("Media Account", "Loading media accounts...");
    const folderField = createPickerField("Media Folder", "Select a media account first");
    const elementField = createPickerField("Media Element", "Select a media folder first");

    folderField.wrapper.hidden = true;
    elementField.wrapper.hidden = true;

    dom.append(accountField.wrapper, folderField.wrapper, elementField.wrapper);

    let disposed = false;
    const abortControllers = new Set<AbortController>();
    let loadingStatusTimeout: number | undefined;

    const setStatus = (message: string, type: "loading" | "error" | "idle" = "idle") => {
        if (loadingStatusTimeout !== undefined) {
            window.clearTimeout(loadingStatusTimeout);
            loadingStatusTimeout = undefined;
        }

        status.textContent = message;
        status.dataset.state = type;
        status.hidden = !message;
    };

    const setDelayedLoadingStatus = (message: string): void => {
        if (loadingStatusTimeout !== undefined) {
            window.clearTimeout(loadingStatusTimeout);
        }

        status.hidden = true;
        status.textContent = "";
        status.dataset.state = "loading";

        loadingStatusTimeout = window.setTimeout(() => {
            loadingStatusTimeout = undefined;

            if (disposed) {
                return;
            }

            status.textContent = message;
            status.dataset.state = "loading";
            status.hidden = false;
        }, 500);
    };

    const disposeFetches = () => {
        for (const controller of abortControllers) {
            controller.abort();
        }

        abortControllers.clear();
    };

    const runRequest = async (url: string, body?: Record<string, string>) => {
        const controller = new AbortController();
        abortControllers.add(controller);

        try {
            const response = await fetch(url, {
                method: "POST",
                headers: body ? {
                    "Content-Type": "application/json",
                } : undefined,
                body: body ? JSON.stringify(body) : undefined,
                signal: controller.signal,
            });

            if (!response.ok) {
                throw new Error(`Request failed with status ${response.status}`);
            }

            const data = await response.json();

            if (!Array.isArray(data)) {
                throw new Error("Response payload was not an array.");
            }

            return data.map(option => ({
                value: typeof option?.value === "string" ? option.value : "",
                text: typeof option?.text === "string" ? option.text : "",
            })).filter(option => option.value && option.text);
        }
        finally {
            abortControllers.delete(controller);
        }
    };

    const loadAccounts = async () => {
        accountField.select.disabled = true;
        setDelayedLoadingStatus("Loading media accounts...");

        try {
            const options = await runRequest("/api/v2/controls/MediaElementPickerGetMediaAccounts");

            if (disposed) {
                return;
            }

            setSelectOptions(accountField.select, options.length ? "Select a media account" : "No media accounts found", options);
            accountField.select.disabled = options.length === 0;
            setStatus(options.length ? "" : "No media accounts were returned.");
        }
        catch (error) {
            if (disposed || (error instanceof DOMException && error.name === "AbortError")) {
                return;
            }

            accountField.select.disabled = true;
            setSelectOptions(accountField.select, "Unable to load media accounts", []);
            setStatus("Unable to load media accounts.", "error");
        }
    };

    const loadFolders = async (mediaAccountGuid: string) => {
        folderField.wrapper.hidden = false;
        folderField.select.disabled = true;
        setSelectOptions(folderField.select, "Loading media folders...", []);
        elementField.wrapper.hidden = true;
        elementField.select.disabled = true;
        setSelectOptions(elementField.select, "Select a media folder first", []);
        setDelayedLoadingStatus("Loading media folders...");

        try {
            const options = await runRequest("/api/v2/controls/MediaElementPickerGetMediaFolders", { mediaAccountGuid });

            if (disposed) {
                return;
            }

            setSelectOptions(folderField.select, options.length ? "Select a media folder" : "No media folders found", options);
            folderField.select.disabled = options.length === 0;
            setStatus(options.length ? "" : "No media folders were returned.");
        }
        catch (error) {
            if (disposed || (error instanceof DOMException && error.name === "AbortError")) {
                return;
            }

            folderField.select.disabled = true;
            setSelectOptions(folderField.select, "Unable to load media folders", []);
            setStatus("Unable to load media folders.", "error");
        }
    };

    const loadElements = async (mediaFolderGuid: string) => {
        elementField.wrapper.hidden = false;
        elementField.select.disabled = true;
        setSelectOptions(elementField.select, "Loading media elements...", []);
        setDelayedLoadingStatus("Loading media elements...");

        try {
            const options = await runRequest("/api/v2/controls/MediaElementPickerGetMediaElements", { mediaFolderGuid });

            if (disposed) {
                return;
            }

            setSelectOptions(elementField.select, options.length ? "Select a media element" : "No media elements found", options);
            elementField.select.disabled = options.length === 0;
            setStatus(options.length ? "" : "No media elements were returned.");
        }
        catch (error) {
            if (disposed || (error instanceof DOMException && error.name === "AbortError")) {
                return;
            }

            elementField.select.disabled = true;
            setSelectOptions(elementField.select, "Unable to load media elements", []);
            setStatus("Unable to load media elements.", "error");
        }
    };

    accountField.select.addEventListener("change", () => {
        const mediaAccountGuid = accountField.select.value;
        disposeFetches();
        folderField.wrapper.hidden = !mediaAccountGuid;
        elementField.wrapper.hidden = true;

        if (!mediaAccountGuid) {
            folderField.select.disabled = true;
            elementField.select.disabled = true;
            setSelectOptions(folderField.select, "Select a media account first", []);
            setSelectOptions(elementField.select, "Select a media folder first", []);
            setStatus("");
            return;
        }

        void loadFolders(mediaAccountGuid);
    });

    folderField.select.addEventListener("change", () => {
        const mediaFolderGuid = folderField.select.value;
        disposeFetches();
        elementField.wrapper.hidden = !mediaFolderGuid;

        if (!mediaFolderGuid) {
            elementField.select.disabled = true;
            setSelectOptions(elementField.select, "Select a media folder first", []);
            setStatus("");
            return;
        }

        void loadElements(mediaFolderGuid);
    });

    elementField.select.addEventListener("change", () => {
        const mediaElementGuid = normalizeMediaElementGuid(elementField.select.value);

        if (!mediaElementGuid) {
            return;
        }

        editor.updateBlock(block, {
            props: {
                mediaElementGuid,
            },
        });
    });

    void loadAccounts();

    return {
        dom,
        destroy: () => {
            disposed = true;
            if (loadingStatusTimeout !== undefined) {
                window.clearTimeout(loadingStatusTimeout);
                loadingStatusTimeout = undefined;
            }
            disposeFetches();
        },
        ignoreMutation: (mutation) => mutation.target instanceof Node && dom.contains(mutation.target),
    };
}

function createEditorMediaBlockDom(block: any, editor: any): MediaPickerDomResult {
    if (!normalizeMediaElementGuid(block.props.mediaElementGuid)) {
        return createMediaPickerDom(block, editor);
    }

    return createConfiguredMediaBlockDom(block.props.mediaElementGuid);
}

export const createMediaBlockConfig = createBlockConfig(
    () =>
        ({
            type: "media" as const,
            propSchema: {
                mediaElementGuid: {
                    default: "",
                },
            },
            content: "none" as const,
        }) as const,
);

const createMediaBlockSpec = createBlockSpec(
    createMediaBlockConfig,
    {
        parse(element) {
            if (element.tagName === "DIV" && element.hasAttribute("data-rock-media-block")) {
                return {
                    mediaElementGuid: normalizeMediaElementGuid(element.getAttribute("data-media-element-guid") ?? ""),
                };
            }

            return undefined;
        },
        render(block, editor) {
            return createEditorMediaBlockDom(block, editor);
        },
        toExternalHTML(block) {
            return {
                dom: createConfiguredMediaExternalDom(block.props.mediaElementGuid),
            };
        },
    },
);

export const mediaBlockSpec = createMediaBlockSpec();
