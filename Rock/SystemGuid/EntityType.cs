﻿// <copyright>
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
        /// The guid for the Rock.Model.ConnectionActivityType entity
        /// </summary>
        public const string CONNECTION_ACTIVITY_TYPE = "97B143F0-CB9D-4652-8FF1-FF2FA1EA4945";

        /// <summary>
        /// The guid for the Rock.Model.ConnectionOpportunity entity
        /// </summary>
        public const string CONNECTION_OPPORTUNITY = "79F64363-BC90-4109-9D31-A5EEB397CB2F";
        /// <summary>
        /// The guid for the Rock.Model.ConnectionOpportunityCampus entity
        /// </summary>
        public const string CONNECTION_OPPORTUNITY_CAMPUS = "E656E8B3-12AB-476E-AA63-5F9B76F64A08";
        /// <summary>
        /// The guid for the Rock.Model.ConnectionOpportunityGroup entity
        /// </summary>
        public const string CONNECTION_OPPORTUNITY_GROUP = "CD3F425C-9B36-4433-9C38-D58DE42C9F65";
        /// <summary>
        /// The guid for the Rock.Model.ConnectionOpportunityConnectorGroup entity
        /// </summary>
        public const string CONNECTION_OPPORTUNITY_CONNECTOR_GROUP = "4CB430B1-0F32-482F-9C95-164A09332CC1";
        /// <summary>
        /// The guid for the Rock.Model.ConnectionRequest entity
        /// </summary>
        public const string CONNECTION_REQUEST = "36B0D0C7-8125-48FA-9DA2-729AAA65F718";
        /// <summary>
        /// The guid for the Rock.Model.ConnectionRequestActivity entity
        /// </summary>
        public const string CONNECTION_REQUEST_ACTIVITY = "3248F40D-7661-42CC-AD9B-EF63322937B7";
        /// <summary>
        /// The guid for the Rock.Model.ConnectionRequestWorkflow entity
        /// </summary>
        public const string CONNECTION_REQUEST_WORKFLOW = "C69D1C9F-5521-4C83-8FE9-5044ECC2CE65";
        /// <summary>
        /// The guid for the Rock.Model.ConnectionStatus entity
        /// </summary>
        public const string CONNECTION_STATUS = "F3840C8B-63BF-4F98-AC4A-9336896E589B";
        /// <summary>
        /// The guid for the Rock.Model.ConnectionType entity
        /// </summary>
        public const string CONNECTION_TYPE = "B1E52EAD-65BD-4C4D-BCCD-73368067621D";
        /// <summary>
        /// The guid for the Rock.Model.ConnectionWorkflow entity
        /// </summary>
        public const string CONNECTION_WORKFLOW = "4EB8711F-7301-4699-A223-0505A7CEB20A";

        /// <summary>
        /// The guid for the Rock.Model.DataView entity.
        /// </summary>
        public const string DATAVIEW = "57F8FA29-DCF1-4F74-8553-87E90F234139";

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
        /// The guid for the Rock.Model.MergeTemplate entity
        /// </summary>
        public const string MERGE_TEMPLATE = "CD1DB988-6891-4B0F-8D1B-B0A311A3BC3E";

        /// <summary>
        /// The guid for the Rock.Model.Note entity
        /// </summary>
        public const string NOTE = "53DC1E78-14A5-44DE-903F-6A2CB02164E7";

        /// <summary>
        /// The guid for the Rock.Model.Page entity
        /// </summary>
        public const string PAGE = "E104DCDF-247C-4CED-A119-8CC51632761F";

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

        /// <summary>
        /// The protect my ministry provider
        /// </summary>
        public const string PROTECT_MY_MINISTRY_PROVIDER = "C16856F4-3C6B-4AFB-A0B8-88A303508206";

    }
}