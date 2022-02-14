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
import { PublicAttributeValue } from "./publicAttributeValue";

export type PublicEditableAttributeValue = PublicAttributeValue & {
    /** The key that identifies this attribute on the entity. */
    key: string;

    /** Indicates if this attribute value is required for saving. */
    isRequired: boolean;

    /** The help text that describes this attribute value. */
    description: string;

    /** Configuration values for how to display and edit this value. */
    configurationValues?: Record<string, string> | null;
};
