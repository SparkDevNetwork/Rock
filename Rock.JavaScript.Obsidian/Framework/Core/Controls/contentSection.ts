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

import { inject, provide, ref, Ref } from "vue";

const sectionHolderKey = Symbol("contentSectionContainerHolder");

/**
 * Interface for a content section. This provides access to details about the
 * section and allows modification of its state.
 *
 * This is an internal interface used by the content section components to manage
 * state. It is not intended for use outside of the content section components.
 */
export interface IContentSectionHolder {
    /** The title of the section. */
    readonly title: Readonly<Ref<string | undefined>>;

    /** The icon for the section, if any. */
    readonly icon: Readonly<Ref<string | undefined>>;

    /** The anchor for the section, if any. This is used to link to the section. */
    readonly anchor: Ref<string | undefined>;

    /**
     * Indicates whether the section is collapsed or expanded.
     * This is used to control the visibility of the section content.
     */
    readonly isCollapsed: Ref<boolean>;
}

/**
 * Interface for a content section container. This is used to manage the collection
 * of content sections in a content section container.
 *
 * This is an internal interface used by the content section components to manage
 * state. It is not intended for use outside of the content section components.
 */
export interface IContentSectionContainerHolder {
    addSection: (section: IContentSectionHolder) => void;
    removeSection: (section: IContentSectionHolder) => void;
}

/**
 * Provides a content section container holder to child components.
 *
 * This is an internal function used by the content section components to manage
 * state. It is not intended for use outside of the content section components.
 *
 * @param holder The content section container holder to provide.
 */
export function provideSectionContainer(holder: IContentSectionContainerHolder): void {
    provide(sectionHolderKey, holder);
}

/**
 * Retrieves the content section container holder from the current context.
 *
 * This is an internal function used by the content section components to manage
 * state. It is not intended for use outside of the content section components.
 *
 * @returns The content section container holder, or a default implementation if not found.
 */
export function useSectionContainer(): IContentSectionContainerHolder {
    return inject(sectionHolderKey) as IContentSectionContainerHolder
        ?? {
        addSection: () => { },
        removeSection: () => { },
    };
}

/**
 * Creates a new content section holder with the specified title and icon.
 *
 * This is an internal function used by the content section components to manage
 * state. It is not intended for use outside of the content section components.
 *
 * @param title The title of the section.
 * @param icon The icon for the section, if any.
 *
 * @returns A new content section holder to be registered with the content section container.
 */
export function createSection(title: Readonly<Ref<string | undefined>>, icon: Readonly<Ref<string | undefined>>): IContentSectionHolder {
    const isCollapsed = ref(false);
    const anchor = ref<string>();

    const holder: IContentSectionHolder = {
        title,
        icon,
        anchor,
        isCollapsed,
    };

    return holder;
}
