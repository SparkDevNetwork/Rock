// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using System.Data.Entity;
using church.ccv.FamilyManager.Models;

namespace church.ccv.FamilyManager
{
    // Class containing GUIDS not defined in core within Rock that Family Manager needs.
    public static class Guids
    {
        // Defined Types
        public const string SOURCE_OF_VISIT_DEFINED_TYPE = "54C1EC6C-3DE8-42F4-9445-6A2F91C16B08";
        public const string FIRST_TIME_VISIT_DEFINED_TYPE = "655D6FBA-F8C0-4919-9E31-C1C936653555";

        // Groups
        public const string FAMILY_MANAGER_AUTHORIZED_GROUP = "D832E933-1972-4482-B24D-6AF0AC6BDF20";
    }
}
