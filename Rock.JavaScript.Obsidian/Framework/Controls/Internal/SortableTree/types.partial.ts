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
 * Minimum shape a node must conform to for use with the SortableTree control.
 */
export interface ISortableTreeNode {
    /** Unique identifier for this node, used for the v-for key. */
    idKey?: string | null;

    /** Display name shown in the row's title region. */
    name?: string | null;

    /** When false, the row is rendered with the .inactive modifier (faded). */
    isActive?: boolean | null;

    /** Child nodes rendered recursively beneath this row. */
    children?: ISortableTreeNode[] | null;
}

/**
 * A button rendered into each row's trailing actions area. Buttons appear on
 * row hover and call `handler` with the row's node when clicked.
 */
export interface ISortableTreeAction {
    /** Icon class for the action button. */
    iconCssClass: string;

    /** Optional tooltip / aria-label for the button. */
    label?: string;

    /** Click handler. Receives the row's node. */
    handler: (node: ISortableTreeNode) => void;

    /**
     * When provided, the action only renders on rows where this returns true.
     */
    isVisible?: (node: ISortableTreeNode) => boolean;
}

/**
 * Payload for the SortableTree `reorder` event.
 * Fired after the move has been applied to the underlying nodes array.
 */
export interface ISortableTreeReorderEvent {
    /** The node that was moved. */
    node: ISortableTreeNode;

    /** The parent node. Null when the moved node is at the top level. */
    parent: ISortableTreeNode | null;

    /** The index of the node within its parent's children before the move. */
    oldIndex: number;

    /** The index of the node within its parent's children after the move. */
    newIndex: number;
}

/**
 * Tunable options for the auto-scroll behavior. All fields are optional;
 * unspecified values fall back to sensible defaults.
 */
export interface IDragAutoScrollOptions {
    /**
     * Distance from the container's edge at which auto-scroll kicks in.
     * Defaults to 50.
     */
    edgeZonePx?: number;

    /**
     * Maximum scroll speed when the cursor is right at the edge. Speed ramps
     * linearly down to zero as the cursor moves away from the edge. Defaults
     * to 12.
     */
    maxSpeedPxPerFrame?: number;
}
