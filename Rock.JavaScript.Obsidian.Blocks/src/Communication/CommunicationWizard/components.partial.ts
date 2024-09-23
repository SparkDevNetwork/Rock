import { EditorComponent, EditorComponentType } from "./types.partial";

export function getComponentOrThrow(type: EditorComponentType): EditorComponent {
    switch (type) {
        case "title":
            return titleComponent;
        case "video":
            return videoComponent;
        case "button":
            return buttonComponent;
        case "paragraph":
            return paragraphComponent;
        case "divider":
            return dividerComponent;
        case "message":
            return messageComponent;
        case "image":
            return imageComponent;
        case "lava":
            return lavaComponent;
        case "rsvp":
            return rsvpComponent;
        default:
            throw "Unknown editor component";
    }
}

export const titleComponent: EditorComponent = {
    iconCssClass: "fa fa-font",
    name: "Title",
    type: "title",

    createElement(d: Document): HTMLElement {
        const el = d.createElement("div");
        el.classList.add("component", "component-text");
        el.dataset.state = "component";
        el.innerHTML = `<h1>Title</h1>`;
        return el;
    },

    createPlaceholder(d: Document): HTMLElement {
        const ph = d.createElement("div");
        ph.style.border = "1px solid gray";
        ph.innerHTML = `<i class="${this.iconCssClass}" style="font-family: 'rock-application' !important; speak: none; font-style: normal; font-weight: normal; font-variant: normal; text-transform: none; line-height: 1; display: inline-block; -webkit-font-smoothing: antialiased; -moz-osx-font-smoothing: grayscale;"></i><br/>${this.name} (Placeholder)`;
        return ph;
    }
};

export const videoComponent: EditorComponent = {
    iconCssClass: "fa fa-video",
    name: "Video",
    type: "video",

    createElement(d: Document): HTMLElement {
        const el = d.createElement("div");
        el.classList.add("component", "component-video");
        el.dataset.state = "component";
        el.innerHTML = `<a href=""><img src="/Assets/Images/video-placeholder.jpg" style="width: 100%;" data-imgcsswidth="full"></a>`;
        return el;
    },

    createPlaceholder(d: Document): HTMLElement {
        const ph = d.createElement("div");
        ph.innerText = `${this.name} (Placeholder)`;
        return ph;
    }
};

export const buttonComponent: EditorComponent = {
    iconCssClass: "fa fa-square",
    name: "Button",
    type: "button",

    createElement(d: Document): HTMLElement {
        const el = d.createElement("div");
        el.classList.add("component", "component-button", "v2");
        el.dataset.state = "component";
        el.innerHTML = `<table class="button-outerwrap" border="0" cellpadding="0" cellspacing="0" width="100%" style="min-width:100%;">
    <tbody>
        <tr>
            <td valign="top" align="center" class="button-innerwrap">
                <table border="0" cellpadding="0" cellspacing="0" class="button-shell">
                    <tbody>
                        <tr>
                            <td align="center" valign="middle" class="button-content" style="border-radius: 3px;background-color:#2baadf">
                                <a class="button-link" title="Push Me" href="http://" target="_blank" rel="noopener noreferrer" style="display: inline-block; font-weight: bold; letter-spacing: normal; line-height: 100%; text-align: center; text-decoration: none; color: #ffffff;background-color: #2baadf; padding: 15px; border: 1px solid #2baadf; border-radius: 3px;">Push Me</a>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </td>
        </tr>
    </tbody>
</table>`;
        return el;
    },

    createPlaceholder(d: Document): HTMLElement {
        const ph = d.createElement("div");
        ph.innerText = `${this.name} (Placeholder)`;
        return ph;
    }
};

export const paragraphComponent: EditorComponent = {
    iconCssClass: "fa fa-align-left",
    name: "Paragraph",
    type: "paragraph",

    createElement(d: Document): HTMLElement {
        const el = d.createElement("div");
        el.classList.add("component", "component-paragraph");
        el.dataset.state = "component";
        el.innerHTML = `<p>Can't wait to see what you have to say!</p>`;
        return el;
    },

    createPlaceholder(d: Document): HTMLElement {
        const ph = d.createElement("div");
        ph.innerText = `${this.name} (Placeholder)`;
        return ph;
    }
};

export const dividerComponent: EditorComponent = {
    iconCssClass: "fa fa-ellipsis-h",
    name: "Divider",
    type: "divider",

    createElement(d: Document): HTMLElement {
        const el = d.createElement("div");
        el.classList.add("component", "component-button", "v2");
        el.dataset.state = "component";
        el.innerHTML = `<hr style="margin-top: 20px; margin-bottom: 20px; border: 0; height: 4px; background: #c4c4c4;" />`;
        return el;
    },

    createPlaceholder(d: Document): HTMLElement {
        const ph = d.createElement("div");
        ph.innerText = `${this.name} (Placeholder)`;
        return ph;
    }
};

export const messageComponent: EditorComponent = {
    iconCssClass: "fa fa-user",
    name: "Message",
    type: "message",

    createElement(d: Document): HTMLElement {
        const el = d.createElement("div");
        el.innerText = this.name;
        return el;
    },

    createPlaceholder(d: Document): HTMLElement {
        const ph = d.createElement("div");
        ph.innerText = `${this.name} (Placeholder)`;
        return ph;
    }
};

export const imageComponent: EditorComponent = {
    iconCssClass: "fa fa-image",
    name: "Image",
    type: "image",

    createElement(d: Document): HTMLElement {
        const el = d.createElement("div");
        el.classList.add("component", "component-image");
        el.dataset.state = "component";
        el.innerHTML = `<img src="/Assets/Images/image-placeholder.jpg" style="width: 100%;" data-imgcsswidth="full" alt="">`;
        return el;
    },

    createPlaceholder(d: Document): HTMLElement {
        const ph = d.createElement("div");
        ph.innerText = `${this.name} (Placeholder)`;
        return ph;
    }
};

export const lavaComponent: EditorComponent = {
    iconCssClass: "fa fa-code",
    name: "Lava",
    type: "lava",

    createElement(d: Document): HTMLElement {
        const el = d.createElement("div");
        el.classList.add("component", "component-code");
        el.dataset.state = "component";
        el.innerHTML = `Add your code here...`;
        return el;
    },

    createPlaceholder(d: Document): HTMLElement {
        const ph = d.createElement("div");
        ph.innerText = `${this.name} (Placeholder)`;
        return ph;
    }
};

export const rsvpComponent: EditorComponent = {
    iconCssClass: "fa fa-check-square",
    name: "RSVP",
    type: "rsvp",

    createElement(d: Document): HTMLElement {
        const el = d.createElement("div");
        el.classList.add("component", "component-rsvp");
        el.dataset.state = "component";
        el.innerHTML = `<table class="rsvp-outerwrap" border="0" cellpadding="0" width="100%" style="min-width:100%;">
    <tbody>
        <tr>
            <td style="padding-top:0; padding-right:0; padding-bottom:0; padding-left:0;" valign="top" align="center" class="rsvp-innerwrap">
                <table border="0" cellpadding="0" cellspacing="0">
                    <tbody>
                        <tr>
                            <td>
                                <table border="0" cellpadding="0" cellspacing="0" class="accept-button-shell" style="display: inline-table; border-collapse: separate !important; border-radius: 3px; background-color: #16C98D;">
                                    <tbody>
                                        <tr>
                                            <td align="center" valign="middle" class="rsvp-accept-content" style="font-family: Arial; font-size: 16px; padding: 15px;">
                                                <a class="rsvp-accept-link" title="Accept" href="http://" target="_blank" rel="noopener noreferrer" style="font-weight: bold; letter-spacing: normal; line-height: 100%; text-align: center; text-decoration: none; color: #FFFFFF;">Accept</a>
                                            </td>
                                        </tr>
                                    </tbody>
                                </table>
                            </td>
                            <td style="padding-left: 10px;">
                                <table border="0" cellpadding="0" cellspacing="0" class="decline-button-shell" style="display: inline-table; border-collapse: separate !important; border-radius: 3px; background-color: #D4442E;">
                                    <tbody>
                                        <tr>
                                            <td align="center" valign="middle" class="rsvp-decline-content" style="font-family: Arial; font-size: 16px; padding: 15px;">
                                                <a class="rsvp-decline-link" title="Decline" href="http://" target="_blank" style="font-weight: bold; letter-spacing: normal; line-height: 100%; text-align: center; text-decoration: none; color: #FFFFFF;">Decline</a>
                                            </td>
                                        </tr>
                                    </tbody>
                                </table>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </td>
        </tr>
    </tbody>
</table>
<input type="hidden" class="rsvp-group-id">
<input type="hidden" class="rsvp-occurrence-value">`;
        return el;
    },

    createPlaceholder(d: Document): HTMLElement {
        const ph = d.createElement("div");
        ph.innerText = `${this.name} (Placeholder)`;
        return ph;
    }
};