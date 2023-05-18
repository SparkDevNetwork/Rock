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

/** The style to use when displaying a picker component. */
export const PickerDisplayStyle = {
    /** The component will choose the best layout to use when displaying the items. */
    Auto: "auto",

    /** The component should display the items as a list (radio or checkbox) of items. */
    List: "list",

    /** The component should display in a compact form such as a drop down list. */
    Condensed: "condensed"
} as const;

/** The style to use when displaying a picker component. */
export type PickerDisplayStyle = typeof PickerDisplayStyle[keyof typeof PickerDisplayStyle];
