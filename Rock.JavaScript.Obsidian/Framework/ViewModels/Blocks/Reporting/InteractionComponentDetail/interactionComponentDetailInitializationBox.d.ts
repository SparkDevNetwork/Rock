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

/** The box that contains all the initialization information for the Interaction Component Detail block. */
export type InteractionComponentDetailInitializationBox = {
    /** Gets or sets the name of the component. */
    componentName?: string | null;

    /** Gets or sets the content. The content is generated using either the Component's LavaTemplate of the block's DefaultLavaTemplate attribute. */
    content?: string | null;

    /**
     * Gets or sets the error message. A non-empty value indicates that
     * an error is preventing the block from being displayed.
     */
    errorMessage?: string | null;

    /** Gets or sets the navigation urls. */
    navigationUrls?: Record<string, string> | null;

    /** Gets or sets the security grant token. */
    securityGrantToken?: string | null;
};
