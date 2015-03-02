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
        /// The Block entity type
        /// </summary>
        public const string BLOCK = "D89555CA-9AE4-4D62-8AF1-E5E463C1EF65";

        /// <summary>
        /// The guid for the email communication medium
        /// </summary>
        public const string COMMUNICATION_MEDIUM_EMAIL = "5A653EBE-6803-44B4-85D2-FB7B8146D55D";

        /// <summary>
        /// The guid for the email communication medium
        /// </summary>
        public const string COMMUNICATION_MEDIUM_SMS = "4BC02764-512A-4A10-ACDE-586F71D8A8BD";

        /// <summary>
        /// The guid for the Rock.Model.Group entity.
        /// </summary>
        public const string GROUP = "9BBFDA11-0D22-40D5-902F-60ADFBC88987";

        /// <summary>
        /// The guid for the Rock.Model.GroupMember entity.
        /// </summary>
        public const string GROUP_MEMBER = "49668B95-FEDC-43DD-8085-D2B0D6343C48";

        /// <summary>
        /// The guid for the Rock.Model.MetricCategory entity
        /// </summary>
        public const string METRICCATEGORY = "3D35C859-DF37-433F-A20A-0FFD0FCB9862";

        /// <summary>
        /// The guid for the Rock.Model.Person entity
        /// </summary>
        public const string PERSON = "72657ED8-D16E-492E-AC12-144C5E7567E7";

        /// <summary>
        /// The guid for the Rock.Model.PersonAlias entity
        /// </summary>
        public const string PERSON_ALIAS = "90F5E87B-F0D5-4617-8AE9-EB57E673F36F";

        /// <summary>
        /// The LiquidSelect DataSelect field for Reporting
        /// </summary>
        public const string REPORTING_DATASELECT_LIQUIDSELECT = "C130DC52-CA31-45EE-A4F2-6C53A838EF3D";        
        
        /// <summary>
        /// The guid for the Rock.Model.Schedule entity
        /// </summary>
        public const string SCHEDULE = "0B2C38A7-D79C-4F85-9757-F1B045D32C8A";

        /// <summary>
        /// The Service Job entity type
        /// </summary>
        public const string SERVICE_JOB = "52766196-A72F-4F60-997A-78E19508843D";

        /// <summary>
        /// The guid for the database storage provider entity
        /// </summary>
        public const string STORAGE_PROVIDER_DATABASE = "0AA42802-04FD-4AEC-B011-FEB127FC85CD";

        /// <summary>
        /// The guid for the filesystem storage provider entity
        /// </summary>
        public const string STORAGE_PROVIDER_FILESYSTEM = "A97B6002-454E-4890-B529-B99F8F2F376A";

        /// <summary>
        /// The guid for the Rock.Model.WorkflowType entity
        /// </summary>
        public const string WORKFLOW_TYPE = "C9F3C4A5-1526-474D-803F-D6C7A45CBBAE";

    }
}