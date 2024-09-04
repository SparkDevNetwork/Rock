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
using System;
using System.Collections.Generic;

using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Core
{
    /// <summary>
    /// Provides actions to manage data for the Persisted Dataset features of Rock.
    /// </summary>
    public class PersistedDatasetDataManager
    {
        private static Lazy<PersistedDatasetDataManager> _dataManager = new Lazy<PersistedDatasetDataManager>();
        public static PersistedDatasetDataManager Instance => _dataManager.Value;

        #region Test Data

        /// <summary>
        /// Create a persisted dataset with some basic information about selected people in the sample data set.
        /// </summary>
        /// <param name="datasetGuid"></param>
        /// <param name="accessKey"></param>
        public void AddPersistedDatasetForPersonBasicInfo( Guid datasetGuid, string accessKey )
        {
            var rockContext = new RockContext();

            // Add a Persisted Dataset containing some test people.
            var datasetLava = @"
[
{%- person where:'Guid == `<benJonesGuid>` || Guid == `<billMarbleGuid>` || Guid == `<alishaMarbleGuid>`' iterator:'People' -%}
  {%- for item in People -%}
    {
        `Id`: {{ item.Id | ToJSON }},
        `FirstName`: {{ item.NickName | ToJSON }},
        `LastName`: {{ item.LastName | ToJSON }},
        `FullName`: {{ item.FullName | ToJSON }},
    }
    {%- unless forloop.last -%},{%- endunless -%}
  {%- endfor -%}
{%- endperson -%}
]
";

            datasetLava = datasetLava.Replace( "<benJonesGuid>", TestGuids.TestPeople.BenJones )
                .Replace( "<billMarbleGuid>", TestGuids.TestPeople.BillMarble )
                .Replace( "<alishaMarbleGuid>", TestGuids.TestPeople.AlishaMarble );

            // Create a Persisted Dataset.
            datasetLava = datasetLava.Replace( "`", @"""" );

            var ps = new PersistedDatasetService( rockContext );

            var pds = ps.Get( datasetGuid );
            if ( pds == null )
            {
                pds = new PersistedDataset();

                pds.Guid = datasetGuid;

                ps.Add( pds );
            }

            pds.Name = "Persons";
            pds.AccessKey = accessKey;
            pds.Description = "A persisted dataset created for testing purposes.";
            pds.BuildScriptType = PersistedDatasetScriptType.Lava;
            pds.BuildScript = datasetLava;
            pds.EnabledLavaCommands = "RockEntity";
            pds.EntityTypeId = EntityTypeCache.Get( typeof( Person ), createIfNotFound: false, rockContext ).Id;

            pds.UpdateResultData();

            rockContext.SaveChanges();
        }

        /// <summary>
        /// Create a persisted dataset with some basic information about selected people in the sample data set.
        /// </summary>
        /// <param name="datasetGuid"></param>
        /// <param name="accessKey"></param>
        public void AddDatasetForContentChannelItemInfo( Guid datasetGuid, string accessKey, List<string> contentChannelItemIdentifiers )
        {
            var rockContext = new RockContext();

            var itemGuids = new List<string>();

            var contentChannelItemService = new ContentChannelItemService( rockContext );
            foreach ( var identifier in contentChannelItemIdentifiers )
            {
                var item = contentChannelItemService.GetByIdentifierOrThrow( identifier, "Title" );
                itemGuids.Add( item.Guid.ToString() );
            }

            // Add a Persisted Dataset containing some basic properties of the specified ContentChannelItems.
            var datasetLava = @"
[
{% assign guids = '<guidList>' | Split:',' %}
{%- for guid in guids -%}
    {%- contentchannelitem where:'Guid == ""{{guid}}""' -%}
      {%- assign item = contentchannelitem -%}
        {
            ""Id"": {{ item.Id | ToJSON }},
            ""Title"": {{ item.Title | ToJSON }},
        }
        {%- unless forloop.last -%},{%- endunless -%}
    {%- endcontentchannelitem -%}
{%- endfor -%}
]
";

            datasetLava = datasetLava.Replace( "<guidList>", itemGuids.AsDelimited( "," ) );

            // Create a Persisted Dataset.
            var ps = new PersistedDatasetService( rockContext );

            var pds = ps.Get( datasetGuid );
            if ( pds == null )
            {
                pds = new PersistedDataset();

                pds.Guid = datasetGuid;

                ps.Add( pds );
            }

            pds.Name = accessKey;
            pds.AccessKey = accessKey;
            pds.Description = "A persisted dataset created for testing purposes.";
            pds.BuildScriptType = PersistedDatasetScriptType.Lava;
            pds.BuildScript = datasetLava;
            pds.EnabledLavaCommands = "RockEntity";
            pds.EntityTypeId = EntityTypeCache.Get( typeof( ContentChannelItem ), createIfNotFound: false, rockContext ).Id;

            pds.UpdateResultData();

            rockContext.SaveChanges();
        }

        #endregion
    }
}
