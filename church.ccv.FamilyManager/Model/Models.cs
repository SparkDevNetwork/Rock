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

namespace church.ccv.FamilyManager.Models
{
    [Serializable]
    public class PersonAttribute
    {
        public enum FamilyRole
        {
            Adult,
            Child,
            Both
        }
        public FamilyRole Filter;
        public bool Required;
        public Rock.Model.Attribute Attribute;
    }

    [Serializable]
    public class CoreData
    {
        public int FamilyManagerVersion;

        public List<Campus> Campuses;

        public DefinedValue ConfigTemplate;
        public List<DefinedValue> MaritalStatus;
        public List<DefinedValue> SchoolGrades;
        public List<DefinedValue> SourceOfVisit;

        public Rock.Model.Attribute FirstTimeVisit;
        public List<PersonAttribute> PersonAttributes;

        public GroupTypeRole FamilyMember_Child_GroupRole;
        public GroupTypeRole FamilyMember_Adult_GroupRole;
        public GroupTypeRole CanCheckIn_GroupRole;
        public GroupTypeRole AllowedCheckInBy_GroupRole;
    }
}
