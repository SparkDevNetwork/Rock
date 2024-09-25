import { EditorComponentType, EditorComponentTypeName } from "./types.partial";
import { toWordFull } from "@Obsidian/Utility/numberUtils";

export function getComponentOrThrow(componentElement: HTMLElement): EditorComponentType;
export function getComponentOrThrow(type: EditorComponentTypeName): EditorComponentType;
export function getComponentOrThrow(componentElementOrType: EditorComponentTypeName | HTMLElement): EditorComponentType {
    let type: string = "";
    if (typeof componentElementOrType === "string") {
        type = componentElementOrType;
    }
    else {
        type = [...componentElementOrType.classList].find(cls => cls.startsWith("component-"))?.replace("component-", "") ?? "";
    }

    switch (type) {
        case "text":
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
        case "code":
            return lavaComponent;
        case "rsvp":
            return rsvpComponent;
        case "section":
            return sectionComponent;
        case "one-column-section":
            return oneColumnSectionComponent;
        case "two-column-section":
            return twoColumnSectionComponent;
        case "three-column-section":
            return threeColumnSectionComponent;
        default:
            throw "Unknown editor component";
    }
}

export const titleComponent: EditorComponentType = {
    iconCssClass: "fa fa-font",
    title: "Title",
    typeName: "text",

    createComponentElement(d: Document): HTMLElement {
        const el = d.createElement("div");
        el.classList.add("component", `component-${this.typeName}`);
        el.dataset.state = "component";
        el.innerHTML = `<h1>Title</h1>`;
        return el;
    },

    createComponentPlaceholder(d: Document): HTMLElement {
        const ph = d.createElement("div");
        ph.classList.add("component", `component-${this.typeName}`, "gu-transit");
        ph.dataset.state = "template";
        ph.innerHTML = `<i class="${this.iconCssClass}"></i><br>${this.title}`;
        return ph;
    }
};

export const videoComponent: EditorComponentType = {
    iconCssClass: "fa fa-video",
    title: "Video",
    typeName: "video",

    createComponentElement(d: Document): HTMLElement {
        const el = d.createElement("div");
        el.classList.add("component", `component-${this.typeName}`);
        el.dataset.state = "component";
        el.innerHTML = `<a href=""><img src="/Assets/Images/video-placeholder.jpg" style="width: 100%;" data-imgcsswidth="full"></a>`;
        return el;
    },

    createComponentPlaceholder(d: Document): HTMLElement {
        const ph = d.createElement("div");
        ph.classList.add("component", `component-${this.typeName}`, "gu-transit");
        ph.dataset.state = "template";
        ph.innerHTML = `<i class="${this.iconCssClass}"></i><br>${this.title}`;
        return ph;
    }
};

export const buttonComponent: EditorComponentType = {
    iconCssClass: "fa fa-square",
    title: "Button",
    typeName: "button",

    createComponentElement(d: Document): HTMLElement {
        const el = d.createElement("div");
        el.classList.add("component", `component-${this.typeName}`, "v2");
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

    createComponentPlaceholder(d: Document): HTMLElement {
        const ph = d.createElement("div");
        ph.classList.add("component", `component-${this.typeName}`, "v2", "gu-transit");
        ph.dataset.state = "template";
        ph.innerHTML = `<i class="${this.iconCssClass}"></i><br>${this.title}`;
        return ph;
    }
};

export const paragraphComponent: EditorComponentType = {
    iconCssClass: "fa fa-align-left",
    title: "Paragraph",
    typeName: "paragraph",

    createComponentElement(d: Document): HTMLElement {
        const el = d.createElement("div");
        el.classList.add("component", `component-${this.typeName}`);
        el.dataset.state = "component";
        el.innerHTML = `<p>Can't wait to see what you have to say!</p>`;
        return el;
    },

    createComponentPlaceholder(d: Document): HTMLElement {
        const ph = d.createElement("div");
        ph.classList.add("component", `component-${this.typeName}`, "gu-transit");
        ph.dataset.state = "template";
        ph.innerHTML = `<i class="${this.iconCssClass}"></i><br>${this.title}`;
        return ph;
    }
};

export const dividerComponent: EditorComponentType = {
    iconCssClass: "fa fa-ellipsis-h",
    title: "Divider",
    typeName: "divider",

    createComponentElement(d: Document): HTMLElement {
        const el = d.createElement("div");
        el.classList.add("component", `component-${this.typeName}`);
        el.dataset.state = "component";
        el.innerHTML = `<hr style="margin-top: 20px; margin-bottom: 20px; border: 0; height: 4px; background: #c4c4c4;" />`;
        return el;
    },

    createComponentPlaceholder(d: Document): HTMLElement {
        const ph = d.createElement("div");
        ph.classList.add("component", `component-${this.typeName}`, "gu-transit");
        ph.dataset.state = "template";
        ph.innerHTML = `<i class="${this.iconCssClass}"></i><br>${this.title}`;
        return ph;
    }
};

export const messageComponent: EditorComponentType = {
    iconCssClass: "fa fa-user",
    title: "Message",
    typeName: "message",

    createComponentElement(d: Document): HTMLElement {
        const el = d.createElement("div");
        el.classList.add("component", `component-${this.typeName}`);
        el.dataset.state = "component";
        el.innerText = this.title;
        return el;
    },

    createComponentPlaceholder(d: Document): HTMLElement {
        const ph = d.createElement("div");
        ph.classList.add("component", `component-${this.typeName}`, "gu-transit");
        ph.dataset.state = "template";
        ph.innerHTML = `<i class="${this.iconCssClass}"></i><br>${this.title}`;
        return ph;
    }
};

export const imageComponent: EditorComponentType = {
    iconCssClass: "fa fa-image",
    title: "Image",
    typeName: "image",

    createComponentElement(d: Document): HTMLElement {
        const el = d.createElement("div");
        el.classList.add("component", `component-${this.typeName}`);
        el.dataset.state = "component";
        el.innerHTML = `<img src="/Assets/Images/image-placeholder.jpg" style="width: 100%;" data-imgcsswidth="full" alt="">`;
        return el;
    },

    createComponentPlaceholder(d: Document): HTMLElement {
        const ph = d.createElement("div");
        ph.classList.add("component", `component-${this.typeName}`, "gu-transit");
        ph.dataset.state = "template";
        ph.innerHTML = `<i class="${this.iconCssClass}"></i><br>${this.title}`;
        return ph;
    }
};

export const lavaComponent: EditorComponentType = {
    iconCssClass: "fa fa-code",
    title: "Lava",
    typeName: "code",

    createComponentElement(d: Document): HTMLElement {
        const el = d.createElement("div");
        el.classList.add("component", `component-${this.typeName}`);
        el.dataset.state = "component";
        el.innerHTML = `Add your code here...`;
        return el;
    },

    createComponentPlaceholder(d: Document): HTMLElement {
        const ph = d.createElement("div");
        ph.classList.add("component", `component-${this.typeName}`, "gu-transit");
        ph.dataset.state = "template";
        ph.innerHTML = `<i class="${this.iconCssClass}"></i><br>${this.title}`;
        return ph;
    }
};

export const sectionComponent: EditorComponentType & { sectionCount: number; } = {
    get iconCssClass(): string {
        return `rk rk-${toWordFull(this.sectionCount)}-column`;
    },
    title: "Section",
    typeName: "section",
    sectionCount: 1,

    createComponentElement(d: Document): HTMLElement {
        function generateSections(sectionCount: number): string {
            let tds = "";
            const widths = {
                2: ["50%", "50%"],
                3: ["33%", "34%", "33%"],
                4: ["25%", "25%", "25%", "25%"],
                5: ["20%", "20%", "20%", "20%", "20%"],
                6: ["16%", "17%", "17%", "17%", "17%", "16%"],
                7: ["14%", "14%", "15%", "14%", "15%", "14%", "14%"],
                8: ["12%", "12%", "13%", "13%", "13%", "13%", "12%", "12%"],
                9: ["11%", "11%", "11%", "11%", "12%", "11%", "11%", "11%", "11%"],
                10: ["10%", "10%", "10%", "10%", "10%", "10%", "10%", "10%", "10%", "10%"],
                11: ["16%", "17%", "17%", "17%", "17%", "16%"],
                12: ["8%", "9%", "9%", "9%", "9%", "9%", "9%", "9%", "9%", "9%", "9%", "8%"]
            };

            if (widths[sectionCount]) {
                const width = widths[sectionCount];
                const largeCssClass = `large-${12 / sectionCount}`;

                tds += `<td class="dropzone columns ${largeCssClass} small-12 first" width="${width[0]}" valign="top"></td>\n`;
                for (let i = 1; i < sectionCount - 1; i++) {
                    tds += `<td class="dropzone columns ${largeCssClass} small-12" width="${width[i]}" valign="top"></td>\n`;
                }
                tds += `<td class="dropzone columns ${largeCssClass} small-12 last" width="${width[sectionCount - 1]}" valign="top"></td>`;
            }
            else {
                tds = "<td>Invalid section count</td>";
            }

            return tds;
        }

        const el = d.createElement("div");
        el.classList.add("component", "component-section");
        el.dataset.state = "component";
        if (this.sectionCount <= 1) {
            el.innerHTML = `<div class="dropzone"></div>`;
        }
        else {
            el.innerHTML = `<table class="row" width="100%">
        <tbody>
            <tr>
                ${generateSections(this.sectionCount)}
            </tr>
        </tbody>
    </table>`;
        }

        return el;
    },

    createComponentPlaceholder(d: Document): HTMLElement {
        const ph = d.createElement("div");
        ph.classList.add("component", "component-section", "gu-transit");
        ph.dataset.state = "template";
        ph.innerHTML = `<i class="${this.iconCssClass}"></i><br>${this.title}`;
        return ph;
    }
};

export const oneColumnSectionComponent: EditorComponentType = {
    iconCssClass: "rk rk-one-column",
    title: "One",
    typeName: "one-column-section",

    createComponentElement(d: Document): HTMLElement {
        const el = d.createElement("div");
        el.classList.add("component", "component-section");
        el.dataset.state = "component";
        el.innerHTML = `<div class="dropzone"></div>`;
        return el;
    },

    createComponentPlaceholder(d: Document): HTMLElement {
        const ph = d.createElement("div");
        ph.classList.add("component", "component-section", "gu-transit");
        ph.dataset.state = "template";
        ph.innerHTML = `<i class="${this.iconCssClass}"></i><br>${this.title}`;
        return ph;
    }
};

export const twoColumnSectionComponent: EditorComponentType = {
    iconCssClass: "rk rk-two-column",
    title: "Two",
    typeName: "two-column-section",

    createComponentElement(d: Document): HTMLElement {
        const el = d.createElement("div");
        el.classList.add("component", "component-section");
        el.dataset.state = "component";
        el.innerHTML = `<table class="row" width="100%">
    <tbody>
        <tr>
            <td class="dropzone columns large-6 small-12 first" width="50%" valign="top"></td>
            <td class="dropzone columns large-6 small-12 last" width="50%" valign="top"></td>
        </tr>
    </tbody>
</table>`;
        return el;
    },

    createComponentPlaceholder(d: Document): HTMLElement {
        const ph = d.createElement("div");
        ph.classList.add("component", "component-section", "gu-transit");
        ph.dataset.state = "template";
        ph.innerHTML = `<i class="${this.iconCssClass}"></i><br>${this.title}`;
        return ph;
    }
};

export const threeColumnSectionComponent: EditorComponentType = {
    iconCssClass: "rk rk-three-column",
    title: "Three",
    typeName: "three-column-section",

    createComponentElement(d: Document): HTMLElement {
        const el = d.createElement("div");
        el.classList.add("component", "component-section");
        el.dataset.state = "component";
        el.innerHTML = `<table class="row" width="100%">
    <tbody>
        <tr>
            <td class="dropzone columns large-4 small-12 first" width="33%" valign="top"></td>
            <td class="dropzone columns large-4 small-12" width="34%" valign="top"></td>
            <td class="dropzone columns large-4 small-12 last" width="33%" valign="top"></td>
        </tr>
    </tbody>
</table>`;
        return el;
    },

    createComponentPlaceholder(d: Document): HTMLElement {
        const ph = d.createElement("div");
        ph.classList.add("component", "component-section", "gu-transit");
        ph.dataset.state = "template";
        ph.innerHTML = `<i class="${this.iconCssClass}"></i><br>${this.title}`;
        return ph;
    }
};

export const rsvpComponent: EditorComponentType = {
    iconCssClass: "fa fa-check-square",
    title: "RSVP",
    typeName: "rsvp",

    createComponentElement(d: Document): HTMLElement {
        const el = d.createElement("div");
        el.classList.add("component", `component-${this.typeName}`);
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

    createComponentPlaceholder(d: Document): HTMLElement {
        const ph = d.createElement("div");
        ph.classList.add("component", `component-${this.typeName}`, "gu-transit");
        ph.dataset.state = "template";
        ph.innerHTML = `<i class="${this.iconCssClass}"></i><br>${this.title}`;
        return ph;
    }
};