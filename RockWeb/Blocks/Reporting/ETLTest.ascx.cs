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
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using System.Data.SqlClient;
using System.Data.Entity;
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

        protected void btnCreateDimPersonSQL_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var personEntityTypeId = EntityTypeCache.GetId<Rock.Model.Person>();
            var personEntityFields = EntityHelper.GetEntityFields( typeof( Rock.Model.Person ), true, true );
            //var createTableSQL = string.Empty;

            List<string> populateTableFrom = new List<string>();
            populateTableFrom.Add( "dbo.Person p" );
            List<string> populateTableSelect = new List<string>();
            populateTableSelect.Add( "  p.Id as [PersonId]" );

            //const string blankString = "<blank>";
            const int blankDefinedValueId = 0;
            List<string> hashColumns = new List<string>();
            List<string> lastModifiedDateTimeColumns = new List<string>();
            lastModifiedDateTimeColumns.Add( "p.ModifiedDateTime" );

            foreach ( var personProperty in personEntityFields.Where( a => a.FieldKind == FieldKind.Property ).OrderBy( a => !a.IsPreviewable ).ThenBy( a => a.Title ) )
            {
                populateTableSelect.Add( string.Format( "  p.{0}", personProperty.Name ) );

                // TODO: If this column should be included as a hash column
                hashColumns.Add( string.Format( "p.{0}", personProperty.Name ) );

                if ( personProperty.FieldType.Guid == Rock.SystemGuid.FieldType.DEFINED_VALUE.AsGuid() )
                {
                    var definedType = DefinedTypeCache.Read( personProperty.FieldConfig["definedtype"].Value.AsInteger() );
                    var definedTypePropertyWithoutSuffix = personProperty.Name.Replace( "ValueId", string.Empty );

                    populateTableSelect.Add( string.Format( "  isnull(dv{0}.Id, {1}) [{2}]", definedTypePropertyWithoutSuffix, blankDefinedValueId, definedType.Name ) );

                    // TODO: If this column should be included as a hash column
                    hashColumns.Add( string.Format( "dv{0}.Id", definedTypePropertyWithoutSuffix ) );

                    // LEFT OUTER JOIN DefinedValue dvConnectionStatusValue ON dvConnectionStatusValue.Id = p.ConnectionStatusValueId
                    populateTableFrom.Add( string.Format( "  LEFT OUTER JOIN DefinedValue dv{0} ON dv{0}.Id = p.{1}", definedTypePropertyWithoutSuffix, personProperty.Name ) );
                }
            }

            var attributeFromFormatRegular = "  OUTER APPLY( SELECT TOP 1 av.{0}, av.ModifiedDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = {1} ) attribute_{1}";

            var attributeSelectFormatRegular = "  attribute_{0}.{1} as [attribute_{2}]";

            var attributeFromFormatDefinedValue =
                @"  OUTER APPLY( 
    SELECT TOP 1 
        av.Value [Attribute.Value],
        av.ModifiedDateTime,
        dv.ID [DefinedValue.Id]
    FROM dbo.AttributeValue av 
    JOIN DefinedValue dv on TRY_CONVERT(UNIQUEIDENTIFIER, av.Value) = dv.[Guid]
    WHERE av.EntityId = p.Id and AttributeId = {1} 
    ) attribute_{1}";

            var attributeSelectFormatDefinedValue =
                @"  attribute_{0}.[Attribute.Value] AS [attribute_{2}],
  isnull(attribute_{0}.[DefinedValue.Id], {1}) AS [attribute_{2}ValueId]";

            foreach ( var personAttribute in personEntityFields.Where( a => a.FieldKind == FieldKind.Attribute ).OrderBy( a => !a.IsPreviewable ).ThenBy( a => a.Title ) )
            {
                if ( personAttribute.AttributeGuid.HasValue )
                {
                    var attributeCache = AttributeCache.Read( personAttribute.AttributeGuid.Value );
                    if ( attributeCache != null )
                    {
                        lastModifiedDateTimeColumns.Add( string.Format( "attribute_{0}.ModifiedDateTime", attributeCache.Id ) );
                        if ( personAttribute.FieldType.Guid == Rock.SystemGuid.FieldType.DEFINED_VALUE.AsGuid() )
                        {
                            var definedType = DefinedTypeCache.Read( personAttribute.FieldConfig["definedtype"].Value.AsInteger() );
                            populateTableSelect.Add( string.Format( attributeSelectFormatDefinedValue, attributeCache.Id, blankDefinedValueId, attributeCache.Key ) );

                            // TODO: If this column should be included as a hash column
                            hashColumns.Add( string.Format( "attribute_{0}.[Attribute.Value]", attributeCache.Id ) );
                            hashColumns.Add( string.Format( "attribute_{0}.[DefinedValue.Id]", attributeCache.Id ) );

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
            var lastModifiedMax = string.Format(
                "(SELECT MAX(ModifiedDateTime) FROM (VALUES {0} ) AS AllModifiedDateTime(ModifiedDateTime))",
                lastModifiedDateTimeColumns.Select( a => "(" + a + ")" ).ToList().AsDelimited( "," )
                );

            var flattenedPersonSQL = "SELECT \n  "
                + historyHash + " [HistoryHash],\n  "
                + lastModifiedMax + " [MostRecentModifiedDateTime],\n"
                + populateTableSelect.AsDelimited( "," + Environment.NewLine );
            flattenedPersonSQL += Environment.NewLine;
            flattenedPersonSQL += "FROM " + populateTableFrom.AsDelimited( Environment.NewLine );

            tbSQL.Text = flattenedPersonSQL;

            /*var dataTable = DbService.GetDataTable( flattenedPersonSQL, CommandType.Text, new Dictionary<string, object>() );
            foreach (var row in dataTable.Rows )
            {
                
            }*/

        }

        protected void btnCreateDimDefinedTypeViews1_Click( object sender, EventArgs e )
        {
            const string dropViewIfExistsFormat = @"
IF EXISTS (
        SELECT *
        FROM sys.VIEWS
        WHERE NAME = '{0}'
        )
BEGIN
    DROP VIEW {0}
END
";

            const string createViewFormat = @"
CREATE VIEW {0}
AS
SELECT dv.Id [{1}Id]
    ,dv.Value [Name]
    ,dv.[Description]
    ,dv.[Order] [SortOrder]
FROM DefinedValue dv
WHERE dv.DefinedTypeId = {2}
";

            var rockContext = new RockContext();
            var definedTypeService = new DefinedTypeService( rockContext );
            foreach ( var definedType in definedTypeService.Queryable().Include( a => a.DefinedValues ).AsNoTracking().ToList() )
            {
                var definedTypeDatabaseName = definedType.Name.RemoveSpecialCharacters();
                string viewName = string.Format( "vAnalytics_Dim_DefinedType_{0}", definedTypeDatabaseName );

                string dropViewIfExistsSQL = string.Format(
                    dropViewIfExistsFormat,
                    viewName
                    );

                string createViewSQL = string.Format(
                    createViewFormat,
                    viewName,
                    definedTypeDatabaseName,
                    definedType.Id
                    );

                rockContext.Database.ExecuteSqlCommand( dropViewIfExistsSQL );
                rockContext.Database.ExecuteSqlCommand( createViewSQL );
            }
        }

        protected void btnCreateDimDefinedTypeViews2_Click( object sender, EventArgs e )
        {
            var definedValuesProperties = typeof( Rock.Model.FinancialTransaction ).GetProperties()
                .Where( a => a.GetCustomAttribute<DefinedValueAttribute>() != null )
                .Select( a => new
                {
                    a.Name,
                    DefinedValueAttribute = a.GetCustomAttribute<DefinedValueAttribute>()
                } ).ToList();

        }
    }
}