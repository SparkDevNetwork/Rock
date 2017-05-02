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

namespace church.ccv.ClientApp.Models
{
    [Serializable]
    public class GroupRegModel
    {
        public int RequestedGroupId;

        public string FirstName;
        public string LastName;
        public string SpouseName;
        public string Email;
        public string Phone;
    }

    [Serializable]
    public class RegAccountData
    {
        public string FirstName;
        public string LastName;
        public string Email;

        public string CellPhoneNumber;

        public string Username;
        public string Password;
    }

    [Serializable]
    public class GroupInfo
    {
        public string Description { get; set; }
        public string LeaderInformation { get; set; }
        public string Children { get; set; }
        public int CoachPhotoId { get; set; }
        public Guid GroupPhotoGuid { get; set; }
    }
}
