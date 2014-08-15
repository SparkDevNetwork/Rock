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
using System.Web;

namespace Rock.SystemGuid
{
    /// <summary>
    /// System Email Templates
    /// </summary>
    public static class EntityType
    {
        /// <summary>
        /// The database authentication provider
        /// </summary>
        public const string AUTHENTICATION_DATABASE = "4E9B798F-BB68-4C0E-9707-0928D15AB020";

        /// <summary>
        /// The guid for the email communication channel
        /// </summary>
        public const string COMMUNICATION_CHANNEL_EMAIL = "5A653EBE-6803-44B4-85D2-FB7B8146D55D";

        /// <summary>
        /// The guid for the email communication channel
        /// </summary>
        public const string COMMUNICATION_CHANNEL_SMS = "4BC02764-512A-4A10-ACDE-586F71D8A8BD";

        /// <summary>
        /// The guid for the database storage provider entity
        /// </summary>
        public const string STORAGE_PROVIDER_DATABASE = "0AA42802-04FD-4AEC-B011-FEB127FC85CD";

        /// <summary>
        /// The guid for the filesystem storage provider entity
        /// </summary>
        public const string STORAGE_PROVIDER_FILESYSTEM = "A97B6002-454E-4890-B529-B99F8F2F376A";

        /// <summary>
        /// The guid for the Rock.Model.MetricCategory entity
        /// </summary>
        public const string METRICCATEGORY = "3D35C859-DF37-433F-A20A-0FFD0FCB9862";

        /// <summary>
        /// The guid for the Rock.Model.Schedule entity
        /// </summary>
        public const string SCHEDULE = "0B2C38A7-D79C-4F85-9757-F1B045D32C8A";
    }
}