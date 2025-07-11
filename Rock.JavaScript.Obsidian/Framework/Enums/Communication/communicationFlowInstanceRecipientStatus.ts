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

/** The recipient status in a Communication Flow Instance. */
export const CommunicationFlowInstanceRecipientStatus = {
    /** The recipient is still active to receive communications in the communication flow instance. */
    Active: 1,

    /** The recipient has unsubscribed from the communication flow instance and will no longer receive communications. */
    Unsubscribe: 2
} as const;

/** The recipient status in a Communication Flow Instance. */
export const CommunicationFlowInstanceRecipientStatusDescription: Record<number, string> = {
    1: "Active",

    2: "Unsubscribe"
};

/** The recipient status in a Communication Flow Instance. */
export type CommunicationFlowInstanceRecipientStatus = typeof CommunicationFlowInstanceRecipientStatus[keyof typeof CommunicationFlowInstanceRecipientStatus];
