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

import { Guid } from "@Obsidian/Types";
import { HttpResult } from "@Obsidian/Types/Utility/http";
import { BulkUpdateBag } from "@Obsidian/ViewModels/Blocks/Crm/BulkUpdate/bulkUpdateBag";
import { BulkUpdatePersonBag } from "@Obsidian/ViewModels/Blocks/Crm/BulkUpdate/bulkUpdatePersonBag";
import { GroupRolesResponseBag } from "@Obsidian/ViewModels/Blocks/Crm/BulkUpdate/groupRolesResponseBag";
import { PublicAttributeBag } from "@Obsidian/ViewModels/Utility/publicAttributeBag";

/**
 * Canonical keys for {@link BulkUpdateBag.updatedFields}. Mirror of server-side
 * `BulkUpdateProcessor.UpdatedFieldKey`
 */
export const UpdatedFieldKey = {
    title: "title",
    suffix: "suffix",
    gender: "gender",
    maritalStatus: "maritalStatus",
    grade: "grade",
    graduationYear: "graduationYear",
    campus: "campus",
    connectionStatus: "connectionStatus",
    recordStatus: "recordStatus",
    recordSource: "recordSource",
    communicationPreference: "communicationPreference",
    isEmailActive: "isEmailActive",
    emailPreference: "emailPreference",
    emailNote: "emailNote",
    following: "following",
    reviewReason: "reviewReason",
    reviewReasonNote: "reviewReasonNote",
    systemNote: "systemNote",
} as const;

/**
 * The set of valid keys for {@link BulkUpdateBag.updatedFields}.
 */
export type UpdatedFieldKeyName = keyof typeof UpdatedFieldKey;

/**
 * The fully-keyed toggle map sent in {@link BulkUpdateBag.updatedFields}.
 */
export type UpdatedFields = Record<UpdatedFieldKeyName, boolean>;

/**
 * A single segment of a change-summary line. Chip segments are the dynamic
 * entity / value names, rendered as inline monospace chips on the confirmation
 * screen; plain segments are the surrounding sentence text.
 */
export type ChangeSegment = {
    /** The text to display for this segment. */
    text: string;

    /** Whether this segment is a chip (a dynamic entity or value name). */
    isChip: boolean;
};

/**
 * One change-summary line, expressed as an ordered list of segments so the
 * template can style chips without unsafe HTML interpolation.
 */
export type ChangeLine = ChangeSegment[];

/**
 * Represents a wrapper for an attribute that can be bulk-updated.
 */
export type AttributeUpdateItem = {
    /** The actual attribute configuration bag. */
    attribute: PublicAttributeBag;

    /** The current assigned value for this attribute. */
    value: string;

    /** Whether this attribute is selected to be updated. */
    isActive: boolean;
};

/**
 * Typed invoker for the Bulk Update block's `[BlockAction]` methods. Each
 * method wraps a single server action with the correct request shape and
 * response type so call sites stay free of magic strings and casts.
 *
 * @see useInvokeBulkUpdateBlockAction
 */
export type BulkUpdateBlockActionInvoker = {
    /**
     * Fetches name + photo for a person to add to the update list. Keyed on
     * the picker's `modelValue.value` (the person's primary alias GUID).
     */
    getUpdatePerson(personAliasGuid: Guid): Promise<HttpResult<BulkUpdatePersonBag>>;

    /**
     * Projects the graduation year by applying the chosen grade's offset to
     * the system's current graduation year. Returns null on a grade with no
     * numeric offset.
     */
    getGraduationYearFromGrade(gradeValueGuid: Guid): Promise<HttpResult<number>>;

    /**
     * Loads the role dropdown source for the picked group. Includes the
     * group's group-type GUID so the Vue layer can scope follow-up lookups
     * (e.g. member attributes) to the same type.
     */
    getGroupRoles(groupGuid: Guid): Promise<HttpResult<GroupRolesResponseBag>>;

    /**
     * Submits the bulk-update payload and kicks off the server-side processor on a
     * background task. Returns immediately on success; progress and the final
     * completion status are delivered via the `TaskActivityProgressTopic` RealTime
     * topic that the caller has already subscribed to. The `sessionId` is the
     * subscriber's `topic.connectionId`, used by the server to route progress
     * events back to this browser.
     */
    save(bag: BulkUpdateBag, sessionId: string): Promise<HttpResult<unknown>>;

    /**
     * Loads the group member attribute set (filtered to `ShowOnBulk`) for
     * the picked group's group-type. Used by the Update-in-Group panel to
     * drive the per-attribute opt-in editor.
     */
    getGroupMemberAttributes(groupGuid: Guid): Promise<HttpResult<PublicAttributeBag[]>>;

    /**
     * Loads the step attribute set (filtered to `ShowOnBulk`) for the
     * picked step type. Used by the Add Step and Update Step panels.
     */
    getStepAttributes(stepTypeGuid: Guid): Promise<HttpResult<PublicAttributeBag[]>>;
};
