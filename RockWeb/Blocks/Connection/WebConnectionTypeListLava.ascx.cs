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
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;

using Newtonsoft.Json;

using Rock;
using Rock.Attribute;
using Rock.ClientService.Connection.ConnectionType;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Model.Connection.ConnectionType.Options;
using Rock.Security;
using Rock.Utility;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Attribute = Rock.Model.Attribute;

namespace RockWeb.Blocks.Connection
{
    [DisplayName( "Connection Type List" )]
    [Category( "Connection > WebView" )]
    [Description( "Displays the list of connection types." )]
    [IconCssClass( "fa fa-list" )]

    #region Block Attributes
    [CodeEditorField( "Type Template",
        Order = 1,
        Key = AttributeKey.TypeTemplate,
        Description = @"This Lava template will be used to display the Connection Types.
                         <i>(Note: The Lava will include the following merge fields:
                            <p><strong>ConnectionTypes, DetailPage, ConnectionRequestCounts, SumTotalConnectionRequests</strong>)</p>
                         </i>",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 400,
        IsRequired = false,
        DefaultValue = Lava.ConnectionTypes
        )]
    [LinkedPage( "Detail Page",
        Description = "Page to link to when user taps on a connection type. ConnectionTypeGuid is passed in the query string.",
        Order = 2,
        Key = AttributeKey.DetailPage
         )]
    #endregion

    [Rock.SystemGuid.BlockTypeGuid( "887F66AF-944F-4959-87F0-087E3999BAC3" )]
    public partial class WebConnectionTypeListLava : RockBlock
    {
        #region Default Lava
        private static class Lava
        {
            public const string ConnectionTypes = @"
/-
   This is the default lava template for the block

   Available Lava Fields:
       ConnectionTypes
       DetailPage (page GUID)
       ConnectionRequestCounts (a dictionary with key of Type Id and value is the count; where count is either the total count or count of requests assigned to the individual)
       SumTotalConnectionRequests (a sum total of all the counts from that dictionary)
-/
<style>
    .card:hover {
      transform: scale(1.01);
      box-shadow: 0 10px 20px rgba(0,0,0,.12), 0 4px 8px rgba(0,0,0,.06);
    }
</style>
{% if SumTotalConnectionRequests > 0 %}
    {% for connectionType in ConnectionTypes %}
        {% assign typeId = connectionType.Id | ToString %}
        {% assign count = ConnectionRequestCounts[typeId] | AsInteger %}

        {% if count > 0 %}
            <a href='{{ DetailPage | Default:'0' | PageRoute }}?ConnectionTypeGuid={{ connectionType.Guid }}' stretched-link>
                <div class='card mb-2'>
                    <div class='card-body'>
                      <div class='row pt-2' style='height:60px;'>
                            <div class='col-xs-2 col-md-1 mx-auto'>
                                <i class='{{ connectionType.IconCssClass }} text-muted' style=';font-size:30px;'></i>
                            </div>
                            <div class='col-xs-8 col-md-10 pl-md-0 mx-auto'>
                                <span class='text-color'><strong>{{ connectionType.Name }}</strong></span>
                                </br>
                                <span class='text-muted'><small>{{ connectionType.Description }}</small></span>
                            </div>
                            <div class='col-xs-1 col-md-1 mx-auto text-right'>
                                <span class='badge badge-pill badge-primary bg-blue-500'><small>{{ count }}</small></span>
                            </div>
                        </div>
                    </div>
                    <span class=""badge badge-pill badge-info small"">{{ count }}</span>
                </div>
                </div>
               </a>
        {% endif %}
    {% endfor %}
{% else %}
    <div class=""alert alert-info"">No connection requests currently available</div>
{% endif %}";

        }
        #endregion Lava

        #region Attribute Keys
        private static class AttributeKey
        {
            public const string TypeTemplate = "TypeTemplate";
            public const string DetailPage = "DetailPage";
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the detail page unique identifier.
        /// </summary>
        /// <value>
        /// The detail page unique identifier.
        /// </value>
        protected Guid? DetailPageGuid => GetAttributeValue( AttributeKey.DetailPage ).AsGuidOrNull();

        /// <summary>
        /// Gets the opportunity template.
        /// </summary>
        /// <value>
        /// The opportunity template.
        /// </value>
        protected string TypeTemplate => GetAttributeValue( AttributeKey.TypeTemplate );
        #endregion Properties

        #region Base Control Events
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
        }

        protected override void OnLoad( EventArgs e )
        {
            GetConnectionTypes();

            base.OnLoad( e );
        }
        #endregion Base Control Events

        #region Methods
        /// <summary>
        /// Gets the connection types view model that can be sent to the client.
        /// </summary>
        private void GetConnectionTypes()
        {
            using ( var rockContext = new RockContext() )
            {
                // Get the connection types.
                var connectionTypeService = new ConnectionTypeService( rockContext );
                var clientTypeService = new ConnectionTypeClientService( rockContext, CurrentPerson );
                var filterOptions = new ConnectionTypeQueryOptions
                {
                    IncludeInactive = true
                };
                var connectionTypesQuery = connectionTypeService.GetConnectionTypesQuery();

                var connectionTypes = connectionTypeService.GetViewAuthorizedConnectionTypes( connectionTypesQuery, CurrentPerson );

                // Get the various counts to make available to the Lava template.
                // The conversion of the value to a dictionary is a temporary work-around
                // until we have a way to mark external types as lava safe.
                var connectionTypeIds = connectionTypes.Select( t => t.Id ).ToList();
                var requestCounts = clientTypeService.GetConnectionTypeCounts( connectionTypeIds );
                var connectionRequestCounts = new Dictionary<string, string>();
                var sumTotalConnectionRequests = 0;

                foreach ( var typeId in connectionTypeIds )
                {
                    // For now, show TotalCount since there is no way to toggle between the two.
                    // In the future, an options control should be added to allow seeing all vs "only my"
                    //if ( CurrentPerson != null )
                    //{
                    //    sumTotalConnectionRequests += requestCounts[typeId].AssignedToYouCount;
                    //    connectionRequestCounts.Add( typeId.ToString(), requestCounts[typeId].AssignedToYouCount.ToString() );
                    //}
                    //else
                    //{
                    sumTotalConnectionRequests += requestCounts[typeId].TotalCount;
                        connectionRequestCounts.Add( typeId.ToString(), requestCounts[typeId].TotalCount.ToString() );
                    //}
                }

                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );

                mergeFields.AddOrReplace( "ConnectionTypes", connectionTypes );
                mergeFields.AddOrReplace( "DetailPage", DetailPageGuid.ToString() );
                mergeFields.AddOrReplace( "ConnectionRequestCounts", connectionRequestCounts );
                mergeFields.AddOrReplace( "SumTotalConnectionRequests", sumTotalConnectionRequests );

                var content = TypeTemplate
                    .ResolveMergeFields( mergeFields )
                    .ResolveClientIds( upConnectionSelectLava.ClientID );

                lContent.Text = content;
            }
        }
        #endregion Methods
    }
}