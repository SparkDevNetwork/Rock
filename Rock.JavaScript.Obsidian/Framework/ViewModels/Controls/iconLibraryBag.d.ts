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

/* eslint-disable @typescript-eslint/naming-convention */

export type IconLibraryBag = {
    /** A list of icon definitions that are part of this icon set. */
    Icons?: IconDefinitionBag[] | null;

    /**
     * A CSS class that is added with the Rock.ViewModels.Controls.IconDefinitionBag.StyleClass
     * required to the icon element to cause the icon to show.
     */
    StyleClassPrefix?: string | null;
};

export type IconBag = {
    /** The SVG markup for the icon for preview purposes in the icon picker. */
    IconSvg?: string | null;

    /** A list of search terms that can be used to find this icon. */
    SearchTerms?: string[] | null;

    /** A CSS class added to the icon element to cause the correct icon to display. */
    StyleClass?: string | null;

    /** The name of the icon */
    Title?: string | null;
};
