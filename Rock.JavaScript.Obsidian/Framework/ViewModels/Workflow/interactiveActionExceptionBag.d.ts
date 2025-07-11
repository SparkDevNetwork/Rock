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

/** Describes an error that occurred while processing the interactive action. */
export type InteractiveActionExceptionBag = {
    /**
     * The error code that describes the error that occurred. This is
     * specific to each component and has no meaning to Rock itself.
     */
    code: number;

    /**
     * The text that describes the error that occurred. This is intended
     * to be presented to the individual.
     */
    message?: string | null;
};
