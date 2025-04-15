// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//

import { html2canvas } from "@Obsidian/Libs/html2canvas";
import { BinaryFiletype } from "@Obsidian/SystemGuids/binaryFiletype";
import { Category } from "@Obsidian/SystemGuids/category";
import { EmailSection } from "@Obsidian/SystemGuids/emailSection";
import { HttpResult } from "@Obsidian/Types/Utility/http";
import { post, uploadBinaryFile } from "@Obsidian/Utility/http";
import { Enumerable } from "@Obsidian/Utility/linq";
import { isPromise } from "@Obsidian/Utility/promiseUtils";
import { EmailEditorDeleteEmailSectionOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/emailEditorDeleteEmailSectionOptionsBag";
import { EmailEditorEmailSectionBag } from "@Obsidian/ViewModels/Rest/Controls/emailEditorEmailSectionBag";
import { EmailEditorGetEmailSectionOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/emailEditorGetEmailSectionOptionsBag";
import { EmailEditorRegisterRsvpRecipientsOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/emailEditorRegisterRsvpRecipientsOptionsBag";
import { EmailEditorAttendanceOccurrenceBag } from "@Obsidian/ViewModels/Rest/Controls/emailEditorAttendanceOccurrenceBag";
import { EmailEditorGetAttendanceOccurrenceOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/emailEditorGetAttendanceOccurrenceOptionsBag";
import { EmailEditorGetFutureAttendanceOccurrencesOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/emailEditorGetFutureAttendanceOccurrencesOptionsBag";
import { EmailEditorCreateAttendanceOccurrenceOptionsBag } from "@Obsidian/ViewModels/Rest/Controls/emailEditorCreateAttendanceOccurrenceOptionsBag";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { Guid } from "@Obsidian/Types";
import { findComponentInnerWrappers, getImageComponentHelper, getSectionComponentHelper, getTextComponentHelper, getTitleComponentHelper } from "./utils.partial";

type ElementBinaryFileInfo = {
    binaryFile: ListItemBag | null | undefined;
};

type CreateBinaryFileFromElementOptions = {
    /** The source HTML element. */
    element: HTMLElement;
    /** The desired file name. */
    fileName: string;
    /** Optional file processor. */
    fileProcessor?: (file: File) => Promise<File> | File;
    /** Optional element width. */
    elementWidth?: number;
    /** The binary filetype guid. */
    binaryFileTypeGuid: Guid;
};

/**
 * Creates a binary file image from an HTML element.
 *
 * @param options `options.element` must exist in the DOM.
 */
export async function createBinaryFileImageFromElement(
    options: CreateBinaryFileFromElementOptions
): Promise<ElementBinaryFileInfo> {
    // Convert the element to an image.
    const canvas = await html2canvas(options.element, {
        // Set to null for transparent background.
        backgroundColor: null,

        // Turn on logging if needed but remember to turn it back off.
        logging: false,

        windowWidth: options.elementWidth
    }) as HTMLCanvasElement;

    const blob = await new Promise<Blob | null>(resolve => {
        return canvas.toBlob(blob => resolve(blob), "image/png");
    });

    // Upload the thumbnail file.
    let binaryFile: ListItemBag | null | undefined;

    if (blob) {
        // Create the thumbnail file.
        let file = new File([blob], options.fileName, { type: "image/png" });

        if (options.fileProcessor) {
            const res = options.fileProcessor(file);

            if (isPromise(res)) {
                file = await res;
            }
            else {
                file = res;
            }
        }

        // Upload the thumbnail file.
        binaryFile = await uploadBinaryFile(
            file,
            options.binaryFileTypeGuid,
            {
                isTemporary: true,
                progress: () => { }
            }
        );
    }

    return {
        binaryFile
    };
}

export async function createEmailSection(bag: EmailEditorEmailSectionBag): Promise<HttpResult<EmailEditorEmailSectionBag>> {
    return await post<EmailEditorEmailSectionBag>("/api/v2/Controls/EmailEditorCreateEmailSection", undefined, bag);
}

export async function getEmailSection(bag: EmailEditorGetEmailSectionOptionsBag): Promise<HttpResult<EmailEditorEmailSectionBag>> {
    return await post<EmailEditorEmailSectionBag>("/api/v2/Controls/EmailEditorGetEmailSection", undefined, bag);
}

export async function getAllEmailSections(): Promise<HttpResult<EmailEditorEmailSectionBag[]>> {
    return await post<EmailEditorEmailSectionBag[]>("/api/v2/Controls/EmailEditorGetAllEmailSections");
}

export async function updateEmailSection(bag: EmailEditorEmailSectionBag): Promise<HttpResult<EmailEditorEmailSectionBag>> {
    return await post<EmailEditorEmailSectionBag>("/api/v2/Controls/EmailEditorUpdateEmailSection", undefined, bag);
}

export async function deleteEmailSection(options: EmailEditorDeleteEmailSectionOptionsBag): Promise<HttpResult<void>> {
    return await post("/api/v2/Controls/EmailEditorDeleteEmailSection", undefined, options);
}

function useTemporaryElement(document: Document, html: string, similarElementSelector: string, callback: (tempElement: HTMLElement) => void): void {
    const tempElement = document.createElement("div");

    try {
        // Generate the element from the email section source markup.
        tempElement.innerHTML = html;

        // Add the clone temporarily to the get html2canvas to work.

        // To get the most accurate thumbnail image,
        // the temporary element should be
        // added where a similar section has been placed.
        const tempElementContent = tempElement.textContent ?? "";
        const bestElement = Enumerable.from(document.querySelectorAll(similarElementSelector))
            .select(e => {
                if (e.textContent && e.textContent.includes(tempElementContent)) {
                    // Text content matches.
                    return {
                        element: e,
                        rank: 0
                    };
                }
                else if (!e.textContent && !tempElementContent) {
                    // Neither has text content.
                    return {
                        element: e,
                        rank: 1
                    };
                }
                else {
                    // Text content doesn't match, but they are both section components.
                    return {
                        element: e,
                        rank: 2
                    };
                }
            })
            .orderBy(e => e.rank)
            .firstOrDefault();

        if (bestElement?.element.parentNode) {
            // Place it after the best matching element.
            bestElement.element.parentNode.insertBefore(tempElement, bestElement.element.nextSibling);
        }
        else {
            // Place it at the end of the document.
            document.body.append(tempElement);
        }

        callback(tempElement);
    }
    finally {
        tempElement.remove();
    }
}

async function createEmailSectionAndThumbnail(document: Document, bag: Omit<EmailEditorEmailSectionBag, "thumbnailBinaryFile">): Promise<HttpResult<EmailEditorEmailSectionBag>> {
    return await new Promise<HttpResult<EmailEditorEmailSectionBag>>(resolve => {
        useTemporaryElement(
            document,
            bag.sourceMarkup ?? "",
            ".component-section",
            async tempElement => {
                const { binaryFile: thumbnailBinaryFile } = await createBinaryFileImageFromElement({
                    element: tempElement,
                    fileName: `${bag.name!.replace(" ", "_")}.png`,
                    binaryFileTypeGuid: BinaryFiletype.CommunicationImage
                });

                const result = await createEmailSection({
                    ...bag,
                    thumbnailBinaryFile
                });

                resolve(result);
            }
        );
    });
}

export async function createStarterSections(document: Document): Promise<HttpResult<EmailEditorEmailSectionBag[]>> {
    const starterSectionsCategory: ListItemBag = {
        value: Category.EmailSectionStarterSections,
        text: "Starter Sections"
    } as const;

    const sectionComponentHelper = getSectionComponentHelper();
    const imageComponentHelper = getImageComponentHelper();
    const titleComponentHelper = getTitleComponentHelper();
    const textComponentHelper = getTextComponentHelper();

    const starterHeroSectionComponent = sectionComponentHelper.createComponentElement("section");
    const elements = sectionComponentHelper.getElements(starterHeroSectionComponent);
    if (elements) {
        elements.marginWrapper.table.setAttribute("data-email-section-guid", "acae542b-51e3-4bb2-99b3-ff420a85d019");
        const column = elements.rowWrapper?.querySelector(".columns") as HTMLTableCellElement;
        if (column) {
            const wrappers = findComponentInnerWrappers(column);
            if (wrappers) {
                wrappers.marginWrapper.borderWrapper.paddingWrapper.td.style.textAlign = "center";
                wrappers.marginWrapper.borderWrapper.paddingWrapper.td.setAttribute("align", "center");
                wrappers.marginWrapper.borderWrapper.paddingWrapper.td.style.padding = "8px";

                const dropzone = wrappers.marginWrapper.borderWrapper.paddingWrapper.td.querySelector(".dropzone") as HTMLElement;

                if (dropzone) {
                    const imageComponent = imageComponentHelper.createComponentElement();
                    dropzone.appendChild(imageComponent);

                    const titleComponent = titleComponentHelper.createComponentElement();
                    const titleElements = titleComponentHelper.getElements(titleComponent);
                    if (titleElements) {
                        titleElements.marginWrapper.td.style.padding = "12px 0px 0px";
                        if (titleElements.headingEl) {
                            titleElements.headingEl.innerText = "Item Title 1";
                        }
                    }
                    dropzone.appendChild(titleComponent);

                    const textComponent = textComponentHelper.createComponentElement();
                    const textElements = textComponentHelper.getElements(textComponent);
                    if (textElements?.contentWrapper) {
                        textElements.contentWrapper.innerHTML = `<p style="margin: 0;">Join us in a welcoming community.</p>`;
                    }
                    dropzone.appendChild(textComponent);
                }
            }
        }
    }

    const result1 = await createEmailSectionAndThumbnail(document, {
        guid: EmailSection.StarterHero,
        category: starterSectionsCategory,
        isSystem: true,
        name: "Starter Hero",
        sourceMarkup: starterHeroSectionComponent.outerHTML,
        usageSummary: "Single-column layout with an image, title, and summary. Ideal for a bold, welcoming introduction."
    });

    const starterStandardPromoSectionComponent = sectionComponentHelper.createComponentElement("two-column-section");
    const elements2 = sectionComponentHelper.getElements(starterStandardPromoSectionComponent);
    if (elements2) {
        elements2.marginWrapper.table.setAttribute("data-email-section-guid", "6cbe0906-9a9a-4b67-91af-fabd4936dec9");
        const columns = elements2.rowWrapper?.querySelectorAll(".columns");

        const column1 = columns?.item(0) as HTMLTableCellElement;
        const column2 = columns?.item(1) as HTMLTableCellElement;

        if (column1) {
            const wrappers = findComponentInnerWrappers(column1);
            if (wrappers) {
                wrappers.marginWrapper.borderWrapper.paddingWrapper.td.style.textAlign = "center";
                wrappers.marginWrapper.borderWrapper.paddingWrapper.td.setAttribute("align", "center");
                wrappers.marginWrapper.borderWrapper.paddingWrapper.td.style.padding = "8px";

                const dropzone = wrappers.marginWrapper.borderWrapper.paddingWrapper.td.querySelector(".dropzone") as HTMLElement;

                if (dropzone) {
                    const imageComponent = imageComponentHelper.createComponentElement();
                    dropzone.appendChild(imageComponent);

                    const titleComponent = titleComponentHelper.createComponentElement();
                    const titleElements = titleComponentHelper.getElements(titleComponent);
                    if (titleElements) {
                        titleElements.marginWrapper.td.style.padding = "12px 0px 0px";
                        if (titleElements.headingEl) {
                            titleElements.headingEl.innerText = "Item Title 1";
                        }
                    }
                    dropzone.appendChild(titleComponent);

                    const textComponent = textComponentHelper.createComponentElement();
                    const textElements = textComponentHelper.getElements(textComponent);
                    if (textElements?.contentWrapper) {
                        textElements.contentWrapper.innerHTML = `<p style="margin: 0;">Join us in a welcoming community.</p>`;
                    }
                    dropzone.appendChild(textComponent);
                }
            }
        }

        if (column2) {
            const wrappers = findComponentInnerWrappers(column2);
            if (wrappers) {
                wrappers.marginWrapper.borderWrapper.paddingWrapper.td.style.textAlign = "center";
                wrappers.marginWrapper.borderWrapper.paddingWrapper.td.setAttribute("align", "center");
                wrappers.marginWrapper.borderWrapper.paddingWrapper.td.style.padding = "8px";

                const dropzone = wrappers.marginWrapper.borderWrapper.paddingWrapper.td.querySelector(".dropzone") as HTMLElement;

                if (dropzone) {
                    const imageComponent = imageComponentHelper.createComponentElement();
                    dropzone.appendChild(imageComponent);

                    const titleComponent = titleComponentHelper.createComponentElement();
                    const titleElements = titleComponentHelper.getElements(titleComponent);
                    if (titleElements) {
                        titleElements.marginWrapper.td.style.padding = "12px 0px 0px";
                        if (titleElements.headingEl) {
                            titleElements.headingEl.innerText = "Item Title 2";
                        }
                    }
                    dropzone.appendChild(titleComponent);

                    const textComponent = textComponentHelper.createComponentElement();
                    const textElements = textComponentHelper.getElements(textComponent);
                    if (textElements?.contentWrapper) {
                        textElements.contentWrapper.innerHTML = `<p style="margin: 0;">Join us in a welcoming community.</p>`;
                    }
                    dropzone.appendChild(textComponent);
                }
            }
        }
    }

    const result2 = await createEmailSectionAndThumbnail(document, {
        guid: EmailSection.StarterStandardPromo,
        category: starterSectionsCategory,
        isSystem: true,
        name: "Starter Standard Promo",
        sourceMarkup: starterStandardPromoSectionComponent.outerHTML,
        usageSummary: "Two-column layout with an image, title, and summary. Perfect for promoting multiple offerings side-by-side."
    });

    const starter3ColumnPromoSectionComponent = sectionComponentHelper.createComponentElement("three-column-section");
    const elements3 = sectionComponentHelper.getElements(starter3ColumnPromoSectionComponent);
    if (elements3) {
        elements3.marginWrapper.table.setAttribute("data-email-section-guid", "63c1ebf8-0398-4039-9fba-a99886ed7106");
        const columns = elements3.rowWrapper?.querySelectorAll(".columns");

        const column1 = columns?.item(0) as HTMLTableCellElement;
        const column2 = columns?.item(1) as HTMLTableCellElement;
        const column3 = columns?.item(2) as HTMLTableCellElement;

        if (column1) {
            const wrappers = findComponentInnerWrappers(column1);
            if (wrappers) {
                wrappers.marginWrapper.borderWrapper.paddingWrapper.td.style.textAlign = "center";
                wrappers.marginWrapper.borderWrapper.paddingWrapper.td.setAttribute("align", "center");
                wrappers.marginWrapper.borderWrapper.paddingWrapper.td.style.padding = "8px";

                const dropzone = wrappers.marginWrapper.borderWrapper.paddingWrapper.td.querySelector(".dropzone") as HTMLElement;

                if (dropzone) {
                    const imageComponent = imageComponentHelper.createComponentElement();
                    dropzone.appendChild(imageComponent);

                    const titleComponent = titleComponentHelper.createComponentElement();
                    const titleElements = titleComponentHelper.getElements(titleComponent);
                    if (titleElements) {
                        titleElements.marginWrapper.td.style.padding = "12px 0px 0px";
                        if (titleElements.headingEl) {
                            titleElements.headingEl.innerText = "Item Title 1";
                        }
                    }
                    dropzone.appendChild(titleComponent);

                    const textComponent = textComponentHelper.createComponentElement();
                    const textElements = textComponentHelper.getElements(textComponent);
                    if (textElements?.contentWrapper) {
                        textElements.contentWrapper.innerHTML = `<p style="margin: 0;">Join us in a welcoming community.</p>`;
                    }
                    dropzone.appendChild(textComponent);
                }
            }
        }

        if (column2) {
            const wrappers = findComponentInnerWrappers(column2);
            if (wrappers) {
                wrappers.marginWrapper.borderWrapper.paddingWrapper.td.style.textAlign = "center";
                wrappers.marginWrapper.borderWrapper.paddingWrapper.td.setAttribute("align", "center");
                wrappers.marginWrapper.borderWrapper.paddingWrapper.td.style.padding = "8px";

                const dropzone = wrappers.marginWrapper.borderWrapper.paddingWrapper.td.querySelector(".dropzone") as HTMLElement;

                if (dropzone) {
                    const imageComponent = imageComponentHelper.createComponentElement();
                    dropzone.appendChild(imageComponent);

                    const titleComponent = titleComponentHelper.createComponentElement();
                    const titleElements = titleComponentHelper.getElements(titleComponent);
                    if (titleElements) {
                        titleElements.marginWrapper.td.style.padding = "12px 0px 0px";
                        if (titleElements.headingEl) {
                            titleElements.headingEl.innerText = "Item Title 2";
                        }
                    }
                    dropzone.appendChild(titleComponent);

                    const textComponent = textComponentHelper.createComponentElement();
                    const textElements = textComponentHelper.getElements(textComponent);
                    if (textElements?.contentWrapper) {
                        textElements.contentWrapper.innerHTML = `<p style="margin: 0;">Join us in a welcoming community.</p>`;
                    }
                    dropzone.appendChild(textComponent);
                }
            }
        }

        if (column3) {
            const wrappers = findComponentInnerWrappers(column3);
            if (wrappers) {
                wrappers.marginWrapper.borderWrapper.paddingWrapper.td.style.textAlign = "center";
                wrappers.marginWrapper.borderWrapper.paddingWrapper.td.setAttribute("align", "center");
                wrappers.marginWrapper.borderWrapper.paddingWrapper.td.style.padding = "8px";

                const dropzone = wrappers.marginWrapper.borderWrapper.paddingWrapper.td.querySelector(".dropzone") as HTMLElement;

                if (dropzone) {
                    const imageComponent = imageComponentHelper.createComponentElement();
                    dropzone.appendChild(imageComponent);

                    const titleComponent = titleComponentHelper.createComponentElement();
                    const titleElements = titleComponentHelper.getElements(titleComponent);
                    if (titleElements) {
                        titleElements.marginWrapper.td.style.padding = "12px 0px 0px";
                        if (titleElements.headingEl) {
                            titleElements.headingEl.innerText = "Item Title 3";
                        }
                    }
                    dropzone.appendChild(titleComponent);

                    const textComponent = textComponentHelper.createComponentElement();
                    const textElements = textComponentHelper.getElements(textComponent);
                    if (textElements?.contentWrapper) {
                        textElements.contentWrapper.innerHTML = `<p style="margin: 0;">Join us in a welcoming community.</p>`;
                    }
                    dropzone.appendChild(textComponent);
                }
            }
        }
    }

    const result3 = await createEmailSectionAndThumbnail(document, {
        guid: EmailSection.Starter3ColumnPromo,
        category: starterSectionsCategory,
        isSystem: true,
        name: "Starter 3-Column Promo",
        sourceMarkup: starter3ColumnPromoSectionComponent.outerHTML,
        usageSummary: "3-column layout with an image, title, and summary. Perfect for promoting multiple offerings side-by-side."
    });

    const isAnySuccess = result1.isSuccess || result2.isSuccess || result3.isSuccess;
    let data: EmailEditorEmailSectionBag[] | null = null;
    if (isAnySuccess) {
        data = [];
        if (result1.isSuccess && result1.data) {
            data.push(result1.data);
        }
        if (result2.isSuccess && result2.data) {
            data.push(result2.data);
        }
        if (result3.isSuccess && result3.data) {
            data.push(result3.data);
        }
    }

    const isAnyError = result1.isError || result2.isError || result3.isError;
    let errorMessage: string | null = null;
    if (isAnyError) {

        const errorMessages: string[] = [];
        if (result1.isError && result1.errorMessage) {
            errorMessages.push(result1.errorMessage);
        }
        if (result2.isError && result2.errorMessage) {
            errorMessages.push(result2.errorMessage);
        }
        if (result3.isError && result3.errorMessage) {
            errorMessages.push(result3.errorMessage);
        }

        if (errorMessages.length) {
            errorMessage = errorMessages.join(", ");
        }
    }

    return {
        isError: isAnyError,
        errorMessage,
        isSuccess: isAnySuccess,
        statusCode: result1.statusCode,
        data
    };
}

export async function registerRsvpRecipients(bag: EmailEditorRegisterRsvpRecipientsOptionsBag): Promise<HttpResult<void>> {
    return await post("/api/v2/Controls/EmailEditorRegisterRsvpRecipients", undefined, bag);
}

export async function getAttendanceOccurrence(bag: EmailEditorGetAttendanceOccurrenceOptionsBag): Promise<HttpResult<EmailEditorAttendanceOccurrenceBag>> {
    return await post<EmailEditorAttendanceOccurrenceBag>("/api/v2/Controls/EmailEditorGetAttendanceOccurrence", undefined, bag);
}

export async function getFutureAttendanceOccurrences(bag: EmailEditorGetFutureAttendanceOccurrencesOptionsBag): Promise<HttpResult<ListItemBag[]>> {
    return await post<ListItemBag[]>("/api/v2/Controls/EmailEditorGetFutureAttendanceOccurrences", undefined, bag);
}

export async function createAttendanceOccurrence(bag: EmailEditorCreateAttendanceOccurrenceOptionsBag): Promise<HttpResult<EmailEditorAttendanceOccurrenceBag>> {
    return await post<EmailEditorAttendanceOccurrenceBag>("/api/v2/Controls/EmailEditorCreateAttendanceOccurrence", undefined, bag);
}