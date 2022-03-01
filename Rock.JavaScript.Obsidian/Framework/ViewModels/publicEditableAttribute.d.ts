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

import { Guid } from "../Util/guid";
import { ListItem } from "../ViewModels";

/**
 * Describes the data sent to and from remote systems to allow editing of
 * attributes (not values, the attributes themselves).
 */
export type PublicEditableAttributeViewModel = {
    /**
     * The unique identifier of the existing attribute. If this is a new
     * attribute the value should be null.
     */
    guid?: Guid | null;

    /**
     * The name of the attribute.
     */
    name?: string | null;

    /**
     * The key that identifies the attribute.
     */
    key?: string | null;

    /**
     * The abbreviated name of the attribute.
     */
    abbreviatedName?: string | null;

    /**
     * The description of the attribute. This is usually used in the help
     * bubble when the edit control is rendered.
     */
    description?: string | null;

    /**
     * A value indicating whether this attribute is a system attribute.
     **/
    isSystem?: boolean;

    /**
     * A value indicating whether the attribute is active.
     */
    isActive?: boolean;

    /**
     * A value indicating whether the attribute is publically visible.
     */
    isPublic?: boolean;

    /**
     * A value indicating whether the attribute is required to be filled in
     * when the edit control is rendered on a page.
     */
    isRequired?: boolean;

    /**
     * A value indicating whether the attribute is shown on bulk update screens.
     */
    isShowOnBulk?: boolean;

    /**
     * A value indicating whether the attribute is shown on grids.
     */
    isShowInGrid?: boolean;

    /**
     * The categories the attribute is associated with.
     */
    categories?: ListItem[] | null;

    /**
     * True if this attribute is used for analytics.
     */
    isAnalytic?: boolean;

    /**
     * True if this attribute records analytic history.
     */
    isAnalyticHistory?: boolean;

    /**
     * True if this attribute should be made available when performing searches.
     */
    isAllowSearch?: boolean;

    /**
     * True if changes to this attribute's values should be saved.
     */
    isEnableHistory?: boolean;

    /**
     * True if this attribute is indexed by universal search.
     */
    isIndexEnabled?: boolean;

    /**
     * Any HTML to be rendered before the attribute's edit control.
     */
    preHtml?: string;

    /**
     * Any HTML to be rendered after the attribute's edit control.
     */
    postHtml?: string;

    /**
     * The field type unique identifier that defines the behavior of the
     * attribute. A value of null is only valid when transmitted from the
     * server to remote systems so that it can provide default values in
     * other properties.
     */
    fieldTypeGuid?: Guid | null;

    /**
     * The configuration options for the attribute. These values are translated
     * into Configuration Values on the server.
     */
    configurationOptions?: Record<string, string> | null;

    /**
     * The default value of the attribute.
     */
    defaultValue?: string | null;
};
