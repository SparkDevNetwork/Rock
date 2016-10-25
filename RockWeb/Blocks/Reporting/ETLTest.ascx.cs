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
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using System.Data.SqlClient;
using Rock.Reporting;
using System.Data;

namespace RockWeb.Blocks.Reporting
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "ETL Test" )]
    [Category( "Reporting" )]
    [Description( "ETL Testing" )]
    public partial class ETLTest : Rock.Web.UI.RockBlock
    {
        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                // added for your convenience

                // to show the created/modified by date time details in the PanelDrawer do something like this:
                // pdAuditDetails.SetEntity( <YOUROBJECT>, ResolveRockUrl( "~" ) );
            }
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {

        }

        #endregion

        #region Methods

        // helper functional methods (like BindGrid(), etc.)

        #endregion

        protected void btnGo_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var personEntityTypeId = EntityTypeCache.GetId<Rock.Model.Person>();
            var personEntityFields = EntityHelper.GetEntityFields( typeof( Rock.Model.Person ), true, true );
            //var createTableSQL = string.Empty;

            List<string> populateTableFrom = new List<string>();
            populateTableFrom.Add( "dbo.Person p" );
            List<string> populateTableSelect = new List<string>();
            populateTableSelect.Add( "\n  p.Id as [PersonId]" );
            const string blankString = "<blank>";
            List<string> hashColumns = new List<string>();

            foreach ( var personProperty in personEntityFields.Where( a => a.FieldKind == FieldKind.Property ).OrderBy( a => !a.IsPreviewable ).ThenBy( a => a.Title ) )
            {
                populateTableSelect.Add( string.Format( "  p.{0}", personProperty.Name ) );

                // TODO: If this column should be included as a hash column
                hashColumns.Add( string.Format( "p.{0}", personProperty.Name ) );


                if ( personProperty.FieldType.Guid == Rock.SystemGuid.FieldType.DEFINED_VALUE.AsGuid())
                {
                    var definedType = DefinedTypeCache.Read( personProperty.FieldConfig["definedtype"].Value.AsInteger() );
                    var definedTypePropertyWithoutSuffix = personProperty.Name.Replace( "ValueId", string.Empty );

                    // isnull(dvConnectionStatusValue.Value, '<blank>') [ConnectionStatusValueValue],
                    populateTableSelect.Add( string.Format( "  isnull(dv{0}.Value, '{1}') [{2}]", definedTypePropertyWithoutSuffix, blankString, definedType.Name ) );

                    // TODO: If this column should be included as a hash column
                    hashColumns.Add( string.Format( "dv{0}.Value", definedTypePropertyWithoutSuffix ) );
                    
                    // LEFT OUTER JOIN DefinedValue dvConnectionStatusValue ON dvConnectionStatusValue.Id = p.ConnectionStatusValueId
                    populateTableFrom.Add( string.Format("  LEFT OUTER JOIN DefinedValue dv{0} ON dv{0}.Id = p.{1}", definedTypePropertyWithoutSuffix, personProperty.Name) );
                }
            }

            // OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1167) attribute_1167
            var attributeFromFormatRegular = "  OUTER APPLY( SELECT TOP 1 av.{0} FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = {1} ) attribute_{1}";

            // ,attribute_174.ValueAsDateTime AS [attribute_BaptismDate]
            var attributeSelectFormatRegular = "  attribute_{0}.{1} as [attribute_{2}]";

            /*  OUTER APPLY (
                SELECT TOP 1 av.Value [Attribute.Value], dv.Value [DefinedValue.Value]
                FROM dbo.AttributeValue av
                left JOIN DefinedValue dv on TRY_CONVERT(uniqueidentifier, av.Value) = dv.[Guid]
                WHERE av.EntityId = p.Id 
                    AND AttributeId = 719
                ) attribute_719
            */
            var attributeFromFormatDefinedValue =
                @"  OUTER APPLY( 
    SELECT TOP 1 
        av.Value [Attribute.Value],
        dv.Value [DefinedValue.Value]
    FROM dbo.AttributeValue av 
    JOIN DefinedValue dv on TRY_CONVERT(UNIQUEIDENTIFIER, av.Value) = dv.[Guid]
    WHERE av.EntityId = p.Id and AttributeId = {1} 
    ) attribute_{1}";


            // ,attribute_719.[Attribute.Value] AS [attribute_SourceofVisit]
            //  ,attribute_719.[DefinedValue.Value] AS[attribute_SourceofVisitDV]
            var attributeSelectFormatDefinedValue =
                @"  attribute_{0}.[Attribute.Value] AS [attribute_{2}],
  isnull(attribute_{0}.[DefinedValue.Value], '{1}') AS [attribute_{2}DV]";

            foreach ( var personAttribute in personEntityFields.Where( a => a.FieldKind == FieldKind.Attribute ).OrderBy( a => !a.IsPreviewable ).ThenBy( a => a.Title ) )
            {
                if ( personAttribute.AttributeGuid.HasValue )
                {
                    var attributeCache = AttributeCache.Read( personAttribute.AttributeGuid.Value );
                    if ( attributeCache != null )
                    {
                        if ( personAttribute.FieldType.Guid == Rock.SystemGuid.FieldType.DEFINED_VALUE.AsGuid() )
                        {
                            var definedType = DefinedTypeCache.Read( personAttribute.FieldConfig["definedtype"].Value.AsInteger() );
                            populateTableSelect.Add( string.Format( attributeSelectFormatDefinedValue, attributeCache.Id, blankString, attributeCache.Key ) );
                            
                            // TODO: If this column should be included as a hash column
                            hashColumns.Add( string.Format( "attribute_{0}.[Attribute.Value]", attributeCache.Id ) );
                            hashColumns.Add( string.Format( "attribute_{0}.[DefinedValue.Value]", attributeCache.Id ) );

                            populateTableFrom.Add( string.Format( attributeFromFormatDefinedValue, attributeCache.FieldType.Field.AttributeValueFieldName, attributeCache.Id ) );
                        }
                        else
                        {
                            populateTableSelect.Add( string.Format( attributeSelectFormatRegular, attributeCache.Id, attributeCache.FieldType.Field.AttributeValueFieldName, attributeCache.Key ) );
                            populateTableFrom.Add( string.Format( attributeFromFormatRegular, attributeCache.FieldType.Field.AttributeValueFieldName, attributeCache.Id ) );

                            // TODO: If this column should be included as a hash column
                            hashColumns.Add( string.Format( "attribute_{0}.{1}", attributeCache.Id, attributeCache.FieldType.Field.AttributeValueFieldName ) );
                        }
                    }
                }
            }

            var historyHash = "CONVERT(varchar(max), HASHBYTES('SHA2_512', (select CONCAT(" + hashColumns.AsDelimited( "," ) + ") [Hash] for xml raw)), 2)";
            var flattenedPersonSQL = "SELECT " + historyHash + " [HistoryHash],\n" + populateTableSelect.AsDelimited( "," + Environment.NewLine );
            flattenedPersonSQL += Environment.NewLine;
            flattenedPersonSQL += "FROM " + populateTableFrom.AsDelimited( Environment.NewLine );

            lSql.Text = flattenedPersonSQL;

            /*var dataTable = DbService.GetDataTable( flattenedPersonSQL, CommandType.Text, new Dictionary<string, object>() );
            foreach (var row in dataTable.Rows )
            {

            }*/

        }
    }
}