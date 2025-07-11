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

import { LavaEndpointHttpMethod } from "@Obsidian/Enums/Cms/lavaEndpointHttpMethod";
import { LavaEndpointSecurityMode } from "@Obsidian/Enums/Cms/lavaEndpointSecurityMode";
import { RockCacheabilityBag } from "@Obsidian/ViewModels/Controls/rockCacheabilityBag";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";
import { PublicAttributeBag } from "@Obsidian/ViewModels/Utility/publicAttributeBag";

export type LavaEndpointBag = {
    /** Gets or sets the attributes. */
    attributes?: Record<string, PublicAttributeBag> | null;

    /** Gets or sets the attribute values. */
    attributeValues?: Record<string, string> | null;

    /** Gets or sets a cache control settings. */
    cacheControlHeaderSettings?: RockCacheabilityBag | null;

    /** Gets or sets the code template. */
    codeTemplate?: string | null;

    /** Gets or sets a description of the lava application. */
    description?: string | null;

    /** Gets or sets whether cross-site forgery protection is enabled or not. */
    enableCrossSiteForgeryProtection: boolean;

    /** Gets or sets the enabled Lava commands. */
    enabledLavaCommands?: ListItemBag[] | null;

    /** Gets or sets the http method. */
    httpMethod?: LavaEndpointHttpMethod | null;

    /** Gets or sets the identifier key of this entity. */
    idKey?: string | null;

    /** Gets or sets a value indicating whether this instance is active. */
    isActive: boolean;

    /** Gets or sets the name. */
    name?: string | null;

    /** Gets or sets the rate limit period in seconds. */
    rateLimitPeriodDurationSeconds?: number | null;

    /** Gets or sets the rate limit requests per period. */
    rateLimitRequestPerPeriod?: number | null;

    /** Gets or sets the mode of the security. */
    securityMode?: LavaEndpointSecurityMode | null;

    /** Gets or sets the slug of lava application. */
    slug?: string | null;
};
