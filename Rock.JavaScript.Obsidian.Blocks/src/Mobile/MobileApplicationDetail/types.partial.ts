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

export const enum NavigationUrlKey {
    LayoutDetailPage = "LayoutDetailPage",
    LayoutDetailEditPage = "LayoutDetailEditPage",
    PageDetailPage = "PageDetailPage",
    DeepLinkDetailPage = "DeepLinkDetailPage",
    DeepLinkDetailEditPage = "DeepLinkDetailEditPage",
    ParentPage = "ParentPage"
}

export const enum PageParameterKey {
    SiteId = "SiteId",
    Tab = "Tab"
}

/**
 * The string identifiers for each tab in the Mobile Application Detail
 * block. Values are deliberately stable across releases — child detail
 * pages navigate back here with `?Tab=Layouts` (etc.) and the Application
 * tab also defaults to this string when no tab is requested.
 */
export const enum MobileApplicationTab {
    Application = "Application",
    Styles = "Styles",
    Layouts = "Layouts",
    Pages = "Pages",
    DeepLinks = "Deep Links"
}

/**
 * The mobile style framework values shared between the C# `MobileStyleFramework`
 * enum and the Vue side. Mirrors the integer values used by the server.
 */
export const enum MobileStyleFrameworkValue {
    Legacy = 0,
    Blended = 1,
    Standard = 2
}

/**
 * The mobile shell type values shared between the C#
 * `Rock.Common.Mobile.Enums.ShellType` enum (compiled into the
 * Rock.Common.Mobile assembly) and the Vue side. Mirrors the integer
 * values that flow through the bag's `applicationType` field.
 */
export const enum ShellTypeValue {
    Blank = 0,
    Flyout = 1,
    Tabbed = 2
}
