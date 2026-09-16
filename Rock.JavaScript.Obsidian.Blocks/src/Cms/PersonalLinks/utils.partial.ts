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

import { PersonalLinksDataBag } from "@Obsidian/ViewModels/Blocks/Cms/PersonalLinks/personalLinksDataBag";
import { QuickReturnGroup, QuickReturnItem } from "./types.partial";

const personalLinksDataKey = "personalLinksData";

/**
 * Reads the cached personal-links payload from localStorage, returning null
 * if no cache exists or the stored value is corrupt.
 */
export function getPersonalLinksData(): PersonalLinksDataBag | null {
    try {
        const raw = localStorage.getItem(personalLinksDataKey);
        return raw ? JSON.parse(raw) as PersonalLinksDataBag : null;
    }
    catch {
        return null;
    }
}

/**
 * Writes the personal-links payload to localStorage so the popover can render
 * from cache before the next server fetch.
 */
export function savePersonalLinksData(data: PersonalLinksDataBag): void {
    localStorage.setItem(personalLinksDataKey, JSON.stringify(data));
}

/**
 * Loads the person's quick-return items from localStorage, sorts them by
 * typeOrder then most-recent, and groups them by type for rendering.
 */
export function getQuickReturns(storageKey: string): QuickReturnGroup[] {
    if (!storageKey) {
        return [];
    }

    let items: QuickReturnItem[] = [];
    try {
        const raw = localStorage.getItem(storageKey);
        items = raw ? JSON.parse(raw) as QuickReturnItem[] : [];
    }
    catch {
        return [];
    }

    const sorted = items.slice().sort((a, b) => {
        const byTypeOrder = a.typeOrder - b.typeOrder;
        if (byTypeOrder) {
            return byTypeOrder;
        }
        return new Date(b.createdDateTime).getTime() - new Date(a.createdDateTime).getTime();
    });

    const groups = new Map<string, QuickReturnItem[]>();
    for (const item of sorted) {
        const existing = groups.get(item.type);
        if (existing) {
            existing.push(item);
        }
        else {
            groups.set(item.type, [item]);
        }
    }

    const result: QuickReturnGroup[] = [];
    for (const [type, groupItems] of groups) {
        result.push({ type, items: groupItems });
    }
    return result;
}

/**
 * Positions the popover under the bookmark button: top lines up with the
 * button's bottom edge, left is centered under the button and clamped to the
 * viewport so the popover never overflows off-screen.
 */
export function calculatePopoverPosition(buttonEl: HTMLElement, popoverWidth: number): { top: number; left: number } {
    const rect = buttonEl.getBoundingClientRect();
    const buttonCenter = rect.left + (rect.width / 2);
    const leftMax = window.innerWidth - popoverWidth;
    const left = Math.max(0, Math.min(buttonCenter - (popoverWidth / 2), leftMax));
    return {
        top: rect.bottom,
        left
    };
}
