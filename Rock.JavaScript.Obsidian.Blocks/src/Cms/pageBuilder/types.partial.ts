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

import { PageBuilderModuleTypeBag } from "@Obsidian/ViewModels/Blocks/Cms/PageBuilder/pageBuilderModuleTypeBag";

/** Tells the page frame that a module type has started dragging from the sidebar. */
export type ModuleTypeDragStartRequest = {
    moduleType: PageBuilderModuleTypeBag;
};

/** Tells the page frame where the pointer is, in coordinates relative to the frame. */
export type ModuleTypeDragOverRequest = {
    clientX: number;
    clientY: number;
};

/** Tells the page frame that the dragged module type was dropped at a point, in coordinates relative to the frame. */
export type ModuleTypeDropRequest = {
    clientX: number;
    clientY: number;
};

/** Tells the page frame that the pointer left it while dragging. */
export type ModuleTypeDragLeaveRequest = {
    type: "MODULE_TYPE_DRAG_LEAVE_REQUEST";
};

/** Tells the page frame that the drag has finished, whether or not it was dropped. */
export type ModuleTypeDragEndRequest = {
    type: "MODULE_TYPE_DRAG_END_REQUEST";
};

/** Tells the page frame to reload the page it is showing. */
export type PageFrameReloadRequest = {
    type: "PAGE_FRAME_RELOAD_REQUEST";
};

/** Tells the page frame to select a Canvas block, as soon as it is on the page. */
export type ModuleSelectRequest = {
    blockId: number;
};

/** Where a module type was dropped, as reported by the page frame. */
export type ModuleDrop = {
    /** The module type that was dropped. */
    moduleType: PageBuilderModuleTypeBag;

    /** The key of the zone it was dropped in. */
    zoneName: string;

    /** The identifier of the block it was dropped in front of, or null to place it last in the zone. */
    beforeBlockId: number | null;
};

/** Where a Canvas block was dragged by its handle, as reported by the page frame. */
export type ModuleMove = {
    /** The identifier of the Canvas block that was moved. */
    blockId: number;

    /** The key of the zone it was dropped in. */
    zoneName: string;

    /** The identifier of the block it was dropped in front of, or null to place it last in the zone. */
    beforeBlockId: number | null;
};
