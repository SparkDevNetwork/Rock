import { BlockToolConstructorOptions } from "@editorjs/editorjs";
import EditorEmbed, { EmbedData, EmbedConfig } from "@editorjs/embed";

export class Embed extends EditorEmbed {
    constructor(config: BlockToolConstructorOptions<EmbedData, EmbedConfig>) {
        super(config);
    }

    static prepare(config: { config: EmbedConfig }) {
        /* Inject our MP4 video handler unless they have requested it not be used. */
        config.config = config.config || {};
        config.config.services = config.config.services || {};
        if (config.config.services.video === undefined || config.config.services.video === true) {
            config.config.services.video = {
                regex: /(https?:\/\/.*\.(mp4|m4v|mov))/,
                embedUrl: '<%= remote_id %>',
                html: "<video controls></video>",
                height: 300,
                width: 600
            };
        }

        EditorEmbed.prepare({
            config: config.config
        });
    }

    render() {
        const container = super.render();
        if (container.childElementCount === 0) {
            return container;
        }

        /* Reformat the container to be responsive. */
        const iframeContainer = document.createElement("div");
        iframeContainer.classList.add("embed-responsive", "embed-responsive-16by9");

        const iframe = <HTMLElement>container.querySelector("iframe") || container.querySelector("video");
        iframe.classList.add("embed-responsive-item");
        iframe.removeAttribute("style");
        iframe.removeAttribute("width");
        iframe.removeAttribute("height");

        iframe.before(iframeContainer);
        iframe.remove();
        iframeContainer.appendChild(iframe);

        return container;
    }
}