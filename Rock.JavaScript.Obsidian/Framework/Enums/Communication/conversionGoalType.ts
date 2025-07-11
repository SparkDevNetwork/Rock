//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the Rock.CodeGeneration project
//     Changes to this file will be lost when the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
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

/** A conversion goal type for a Communication Flow. This is used to track the success of a Communication Flow in achieving its goals. */
export const ConversionGoalType = {
    /** Recipients have completed a Workflow form. This means they initiated a workflow and it was marked Completed. */
    CompletedForm: 1,

    /** Recipients have registered for an event. */
    Registered: 2,

    /** Recipients have joined a Group of a given Group Type. */
    JoinedGroupType: 3,

    /** Recipients have joined a Group. */
    JoinedGroup: 4,

    /** Recipients have entered a Data View. */
    EnteredDataView: 5,

    /** Recipients have taken a step in a Workflow. */
    TookStep: 6
} as const;

/** A conversion goal type for a Communication Flow. This is used to track the success of a Communication Flow in achieving its goals. */
export const ConversionGoalTypeDescription: Record<number, string> = {
    1: "Completed Form",

    2: "Registered",

    3: "Joined Group Type",

    4: "Joined Group",

    5: "Entered Data View",

    6: "Took Step"
};

/** A conversion goal type for a Communication Flow. This is used to track the success of a Communication Flow in achieving its goals. */
export type ConversionGoalType = typeof ConversionGoalType[keyof typeof ConversionGoalType];
