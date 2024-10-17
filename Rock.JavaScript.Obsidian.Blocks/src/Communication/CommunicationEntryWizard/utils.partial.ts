import { inject, InjectionKey, Ref } from "vue";
import { AccordionManager, EditorComponentTypeName } from "./types.partial";
import { newGuid } from "@Obsidian/Utility/guid";
import { splitCase, toTitleCase } from "@Obsidian/Utility/stringUtils";

export const AccordionGroupInjectionKey: InjectionKey<AccordionManager> = Symbol("accordion-group");

/** Uses an accordion group if one is set up. */
export function useAccordionGroup(isExpanded: Ref<boolean>): void {
    const accordionKey = newGuid();
    const group = inject(AccordionGroupInjectionKey);
    group?.register(accordionKey, isExpanded);
}

export function getComponentTypeName(element: HTMLElement): EditorComponentTypeName {
    const classList = [...element.classList];

    const map: Record<string, EditorComponentTypeName> = {
        "component-button": "button",
        "component-code": "code",
        "component-divider": "divider",
        "component-image": "image",
        "component-message": "message",
        "component-rsvp": "rsvp",
        "component-section": "section",
        "component-text": "text",
        "component-title": "title",
        "component-video": "video"
    };

    for (const key in map) {
        if (classList.includes(key)) {
            return map[key];
        }
    }

    throw new Error("Unable to get component type for element");
}

export function getComponentIconCssClass(componentTypeName: EditorComponentTypeName): string {
    switch (componentTypeName) {
        case "title":
            return "fa fa-font";
        case "video":
            return "fa fa-video";
        case "button":
            return "fa fa-square";
        case "text":
            return "fa fa-align-left";
        case "divider":
            return "fa fa-ellipsis-h";
        case "message":
            return "fa fa-user";
        case "image":
            return "fa fa-image";
        case "code":
            return "fa fa-code";
        case "rsvp":
            return "fa fa-check-square";
        case "section":
        case "one-column-section":
            return "rk rk-one-column";
        case "two-column-section":
            return "rk rk-two-column";
        case "three-column-section":
            return "rk rk-three-column";
        default:
            console.warn(`Unable to retrieve the icon for the unknown component type: '${componentTypeName}'. Returning the default icon.`);
            return "fa fa-question";
    }
}

export function getComponentTitle(componentTypeName: EditorComponentTypeName): string {
    switch (componentTypeName) {
        case "title":
            return "Title";
        case "video":
            return "Video";
        case "button":
            return "Button";
        case "text":
            return "Paragraph";
        case "divider":
            return "Divider";
        case "message":
            return "Message";
        case "image":
            return "Image";
        case "code":
            return "Lava";
        case "rsvp":
            return "RSVP";
        case "section":
            return "Section";
        case "one-column-section":
            return "One";
        case "two-column-section":
            return "Two";
        case "three-column-section":
            return "Three";
        default:
            console.warn(`Unable to retrieve the title for the unknown component type, '${componentTypeName}'. Returning the default icon.`);
            return toTitleCase(splitCase(componentTypeName).replace("-", " "));
    }
}

/** Add this CSS class to any temporary (runtime) element that should be stripped out when the HTML is retrieved via getHtml(). */
export const rockRuntimeElementCssClass = "rock-runtime-element" as const;

/** Add this CSS class prefix to any temporary CSS class that should be stripped out when the HTML is retrieved via getHTML(). */
export const rockRuntimeClassCssClassPrefix = "rock-runtime-class-" as const;

/** Add this CSS class to any element that should be editable inline when selected in the email designer. */
export const rockRuntimeClassContentEditable = `${rockRuntimeClassCssClassPrefix}content-editable` as const;

export function createComponentElementPlaceholder(document: Document, componentTypeName: EditorComponentTypeName): HTMLElement {
    let componentTypeCssClass: string;
    if (["one-column-section", "two-column-section", "three-column-section"].includes(componentTypeName)) {
        componentTypeCssClass = "component-section";
    }
    else {
        componentTypeCssClass = `component-${componentTypeName}`;
    }

    const placeholderElement = document.createElement("div");
    placeholderElement.classList.add("component", componentTypeCssClass, "gu-transit");
    placeholderElement.dataset.state = "template";
    placeholderElement.innerHTML = `<i class="${getComponentIconCssClass(componentTypeName)}"></i><br>${getComponentTitle(componentTypeName)}`;
    return placeholderElement;
}

export function createComponentElement(document: Document, componentTypeName: EditorComponentTypeName): HTMLElement {
    let componentTypeCssClass: string;
    if (["one-column-section", "two-column-section", "three-column-section"].includes(componentTypeName)) {
        componentTypeCssClass = "component-section";
    }
    else {
        componentTypeCssClass = `component-${componentTypeName}`;
    }

    const componentElement = document.createElement("div");
    componentElement.classList.add("component", componentTypeCssClass);
    componentElement.dataset.state = "component";

    switch (componentTypeName) {
        case "title":
            componentElement.innerHTML = `<h1 class="${rockRuntimeClassContentEditable}">Title</h1>`;
            break;
        case "video":
            componentElement.innerHTML = `<a href=""><img src="/Assets/Images/video-placeholder.jpg" style="width: 100%;" data-imgcsswidth="full"></a>`;
            break;
        case "button":
            componentElement.classList.add("v2");
            componentElement.innerHTML = `<table class="button-outerwrap" border="0" cellpadding="0" cellspacing="0" width="100%" style="min-width:100%;">
                <tbody>
                    <tr>
                        <td valign="top" align="center" class="button-innerwrap">
                            <table border="0" cellpadding="0" cellspacing="0" class="button-shell">
                                <tbody>
                                    <tr>
                                        <td align="center" valign="middle" class="button-content" style="border-radius: 3px;background-color:#2baadf">
                                            <a class="button-link ${rockRuntimeClassContentEditable}" title="Push Me" href="http://" target="_blank" rel="noopener noreferrer" style="display: inline-block; font-weight: bold; letter-spacing: normal; line-height: 100%; text-align: center; text-decoration: none; color: #ffffff;background-color: #2baadf; padding: 15px; border: 1px solid #2baadf; border-radius: 3px;">Push Me</a>
                                        </td>
                                    </tr>
                                </tbody>
                            </table>
                        </td>
                    </tr>
                </tbody>
            </table>`;
            break;
        case "text":
            componentElement.classList.add(rockRuntimeClassContentEditable);
            componentElement.innerHTML = `<p>Can't wait to see what you have to say!</p>`;
            break;
        case "divider":
            componentElement.innerHTML = `<hr />`;
            break;
        case "message":
            componentElement.classList.add(rockRuntimeClassContentEditable);
            componentElement.innerText = "Message";
            break;
        case "image":
            componentElement.innerHTML = `<img src="/Assets/Images/image-placeholder.jpg" style="width: 100%;" data-imgcsswidth="full" alt="">`;
            break;
        case "code":
            componentElement.classList.add(rockRuntimeClassContentEditable);
            componentElement.innerHTML = `Add your code here...`;
            break;
        case "rsvp":
            componentElement.innerHTML = `<table class="rsvp-outerwrap" border="0" cellpadding="0" width="100%" style="min-width:100%;">
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
                                                            <a class="rsvp-accept-link ${rockRuntimeClassContentEditable}" title="Accept" href="http://" target="_blank" rel="noopener noreferrer" style="font-weight: bold; letter-spacing: normal; line-height: 100%; text-align: center; text-decoration: none; color: #FFFFFF;">Accept</a>
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
                                                            <a class="rsvp-decline-link ${rockRuntimeClassContentEditable}" title="Decline" href="http://" target="_blank" style="font-weight: bold; letter-spacing: normal; line-height: 100%; text-align: center; text-decoration: none; color: #FFFFFF;">Decline</a>
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
            break;
        case "section":
        case "one-column-section":
            componentElement.innerHTML = `<div class="dropzone"></div>`;
            break;
        case "two-column-section":
            componentElement.innerHTML = `<table class="row" width="100%">
                <tbody>
                    <tr>
                        <td class="dropzone columns large-6 small-12 first" width="50%" valign="top"></td>
                        <td class="dropzone columns large-6 small-12 last" width="50%" valign="top"></td>
                    </tr>
                </tbody>
            </table>`;
            break;
        case "three-column-section":
            componentElement.innerHTML = `<table class="row" width="100%">
                <tbody>
                    <tr>
                        <td class="dropzone columns large-4 small-12 first" width="33%" valign="top"></td>
                        <td class="dropzone columns large-4 small-12" width="34%" valign="top"></td>
                        <td class="dropzone columns large-4 small-12 last" width="33%" valign="top"></td>
                    </tr>
                </tbody>
            </table>`;
            break;
        default:
            throw new Error(`Unknown typeName: ${componentTypeName}`);
    }

    return componentElement;
}