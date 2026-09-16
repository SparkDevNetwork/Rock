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

import type { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import type { MobileApplicationStylesBag } from "@Obsidian/ViewModels/Blocks/Mobile/MobileApplicationDetail/mobileApplicationStylesBag";
import type { PageRouteValueBag } from "@Obsidian/ViewModels/Rest/Controls/pageRouteValueBag";

/**
 * The `PagePicker` control's v-model is a `PageRouteValueBag` (page +
 * optional route), but the entity bag stores a plain `ListItemBag` for
 * these page fields since we do not need route information. This pair
 * of converters bridges the two shapes at the v-model boundary.
 */
export function listItemToPageRoute(item: ListItemBag | null | undefined): PageRouteValueBag | null {
    return item ? { page: item, route: null } : null;
}

export function pageRouteToListItem(value: PageRouteValueBag | null | undefined): ListItemBag | null {
    return value?.page ?? null;
}

/**
 * The lock-phone / lock-tablet orientation dropdowns surface
 * `DeviceOrientation.Unknown` (0) as the dropdown's blank item rather
 * than as an explicit "None" option. This pair maps the int value to the
 * empty string both ways so the dropdown's v-model can stay a `string`
 * while the bag stays a `number`.
 */
export function lockedOrientationToString(value: number | null | undefined): string {
    return value && value > 0 ? String(value) : "";
}

export function lockedOrientationFromString(value: string): number {
    const parsed = parseInt(value, 10);
    return isNaN(parsed) ? 0 : parsed;
}

/**
 * Defaults every nullable string field on the styles bag to "" so that
 * v-model bindings on `ColorPicker` / `CodeEditor` (which require a
 * non-null `string`) type-check cleanly. Non-string fields are passed
 * through unchanged. The inferred return type narrows the listed string
 * fields from `string | null | undefined` to `string` — declaring it
 * explicitly would just duplicate the runtime field list, so the lint
 * rule is suppressed for this one function.
 *
 * Long term this whole helper goes away if `ColorPicker.modelValue` is
 * loosened to accept `string | null | undefined` (the bag stores null
 * exactly because a color can be unset).
 */
// eslint-disable-next-line @typescript-eslint/explicit-function-return-type
export function normalizeStyles(input?: MobileApplicationStylesBag | null) {
    const base = input ?? ({} as MobileApplicationStylesBag);
    return {
        ...base,
        activityIndicatorColor: base.activityIndicatorColor ?? "",
        backgroundColor: base.backgroundColor ?? "",
        barBackgroundColor: base.barBackgroundColor ?? "",
        brand: base.brand ?? "",
        brandSoft: base.brandSoft ?? "",
        brandStrong: base.brandStrong ?? "",
        cssStyles: base.cssStyles ?? "",
        danger: base.danger ?? "",
        dangerSoft: base.dangerSoft ?? "",
        dangerStrong: base.dangerStrong ?? "",
        dark: base.dark ?? "",
        headingColor: base.headingColor ?? "",
        info: base.info ?? "",
        infoSoft: base.infoSoft ?? "",
        infoStrong: base.infoStrong ?? "",
        interfaceMedium: base.interfaceMedium ?? "",
        interfaceSoft: base.interfaceSoft ?? "",
        interfaceSofter: base.interfaceSofter ?? "",
        interfaceSoftest: base.interfaceSoftest ?? "",
        interfaceStrong: base.interfaceStrong ?? "",
        interfaceStronger: base.interfaceStronger ?? "",
        interfaceStrongest: base.interfaceStrongest ?? "",
        light: base.light ?? "",
        menuButtonColor: base.menuButtonColor ?? "",
        primary: base.primary ?? "",
        primarySoft: base.primarySoft ?? "",
        primaryStrong: base.primaryStrong ?? "",
        secondary: base.secondary ?? "",
        secondarySoft: base.secondarySoft ?? "",
        secondaryStrong: base.secondaryStrong ?? "",
        success: base.success ?? "",
        successSoft: base.successSoft ?? "",
        successStrong: base.successStrong ?? "",
        textColor: base.textColor ?? "",
        warning: base.warning ?? "",
        warningSoft: base.warningSoft ?? "",
        warningStrong: base.warningStrong ?? ""
    };
}
