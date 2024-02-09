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

/**
 * Class SystemCommunicationPreviewBag.
 * Implements the Rock.ViewModels.Utility.EntityBagBase
 */
export type SystemCommunicationPreviewInitializationBox = {
    /** Gets or sets the ID */
    id: number | null;

    /** Gets or sets the title. */
    title?: string | null;

    /** Gets or sets the email address. */
    email?: string | null;

    /** Gets or sets the From property. */
    from?: string | null;

    /** Gets or sets the FromName property. */
    fromName?: string | null;

    /** Gets or sets the email subject. */
    subject?: string | null;

    /** Gets or sets the email body. */
    body?: string | null;

    /** Gets or sets the publication date. */
    date?: string | null;

    /** Gets or sets the has send date flag. */
    hasSendDate?: boolean;

    /** Gets or sets the target person ID. */
    targetPersonId?: number | null;

    /** Gets or sets the publication date. */
    publicationDate?: string | null;
};
