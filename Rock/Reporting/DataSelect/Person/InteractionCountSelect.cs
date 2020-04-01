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
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataSelect.Person
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Select the Interaction count of a person" )]
    [Export( typeof( DataSelectComponent ) )]
    [ExportMetadata( "ComponentName", "Select the Interaction count of a person" )]
    public class InteractionCountSelect : DataSelectComponent
    {
        #region Properties

        /// <summary>
        /// Gets the name of the entity type. Filter should be an empty string
        /// if it applies to all entities
        /// </summary>
        /// <value>
        /// The name of the entity type.
        /// </value>
        public override string AppliesToEntityType
        {
            get
            {
                return typeof( Rock.Model.Person ).FullName;
            }
        }

        /// <summary>
        /// Gets the section that this will appear in in the Field Selector
        /// </summary>
        /// <value>
        /// The section.
        /// </value>
        public override string Section
        {
            get
            {
                return base.Section;
            }
        }

        /// <summary>
        /// The PropertyName of the property in the anonymous class returned by the SelectExpression
        /// </summary>
        /// <value>
        /// The name of the column property.
        /// </value>
        public override string ColumnPropertyName
        {
            get
            {
                return "InteractionCount";
            }
        }

        /// <summary>
        /// Gets the type of the column field.
        /// </summary>
        /// <value>
        /// The type of the column field.
        /// </value>
        public override Type ColumnFieldType
        {
            get { return typeof( int ); }
        }

        /// <summary>
        /// Gets the default column header text.
        /// </summary>
        /// <value>
        /// The default column header text.
        /// </value>
        public override string ColumnHeaderText
        {
            get
            {
                return "Interaction Count";
            }
        }

        /// <summary>
        /// Comma-delimited list of the Entity properties that should be used for Sorting. Normally, you should leave this as null which will make it sort on the returned field
        /// To disable sorting for this field, return string.Empty;
        /// </summary>
        /// <param name="selection"></param>
        /// <returns></returns>
        /// <value>
        /// The sort expression.
        /// </value>
        public override string SortProperties( string selection )
        {
            // disable sorting on this column since it is an IEnumerable
            return string.Empty;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        /// <value>
        /// The title.
        /// </value>
        public override string GetTitle( Type entityType )
        {
            return "Interaction Count";
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="entityIdProperty">The entity identifier property.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override Expression GetExpression( RockContext context, MemberExpression entityIdProperty, string selection )
        {
            string[] selectionValues = selection.Split( '|' );
            if ( selectionValues.Length >= 4 )
            {
                var interactionQry = new InteractionService( context ).Queryable();
                Guid interactionChannelGuid = selectionValues[0].AsGuid();
                var interactionComponentGuid = selectionValues[1].AsGuidOrNull();

                if ( interactionComponentGuid.HasValue )
                {
                    interactionQry = interactionQry.Where( xx => xx.InteractionComponent.Guid == interactionComponentGuid.Value );
                }
                else
                {
                    interactionQry = interactionQry.Where( xx => xx.InteractionComponent.InteractionChannel.Guid == interactionChannelGuid );
                }

                string operation = string.Empty;
                operation = selectionValues[2];

                if ( !string.IsNullOrEmpty( operation ) )
                {
                    interactionQry = interactionQry.Where( xx => xx.Operation == operation );
                }

                string slidingDelimitedValues = selectionValues[3].Replace( ',', '|' );
                var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( slidingDelimitedValues );

                if ( dateRange.Start.HasValue )
                {
                    interactionQry = interactionQry.Where( i => i.InteractionDateTime >= dateRange.Start.Value );
                }

                if ( dateRange.End.HasValue )
                {
                    interactionQry = interactionQry.Where( i => i.InteractionDateTime <= dateRange.End.Value );
                }

                var qry = new PersonService( context ).Queryable()
                   .Select( p => interactionQry.Where( l => l.PersonAlias.PersonId == p.Id )
                   .Count() );

                Expression selectExpression = SelectExpressionExtractor.Extract( qry, entityIdProperty, "p" );

                return selectExpression;
            }

            return null;
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <param name="parentControl"></param>
        /// <returns></returns>
        public override System.Web.UI.Control[] CreateChildControls( System.Web.UI.Control parentControl )
        {

            RockDropDownList ddlInteractionChannel = new RockDropDownList();
            ddlInteractionChannel.ID = parentControl.ID + "_ddlInteractionChannel";
            ddlInteractionChannel.Label = "Interaction Channel";
            ddlInteractionChannel.Required = true;
            ddlInteractionChannel.AutoPostBack = true;
            ddlInteractionChannel.SelectedIndexChanged += ddlInteractionChannel_SelectedIndexChanged;
            parentControl.Controls.Add( ddlInteractionChannel );

            var interactionChannelService = new InteractionChannelService( new RockContext() );
            var noteTypes = interactionChannelService.Queryable().OrderBy( a => a.Name ).Select( a => new
            {
                a.Id,
                a.Name
            } ).ToList();

            ddlInteractionChannel.Items.Clear();
            ddlInteractionChannel.Items.Add( new ListItem() );
            ddlInteractionChannel.Items.AddRange( noteTypes.Select( a => new ListItem( a.Name, a.Id.ToString() ) ).ToArray() );

            int? selectedInteractionChannelId = parentControl.Page.Request.Params[ddlInteractionChannel.UniqueID].AsIntegerOrNull();
            ddlInteractionChannel.SetValue( selectedInteractionChannelId );


            RockDropDownList ddlInteractionComponent = new RockDropDownList();
            ddlInteractionComponent.ID = parentControl.ID + "_ddlInteractionComponent";
            ddlInteractionComponent.Label = "Interaction Component";
            ddlInteractionComponent.EnhanceForLongLists = true;
            parentControl.Controls.Add( ddlInteractionComponent );

            PopulateInteractionComponent( selectedInteractionChannelId, ddlInteractionComponent );
            RockTextBox tbOperation = new RockTextBox();
            tbOperation.Label = "Operation";
            tbOperation.ID = parentControl.ID + "_tbOperation";
            parentControl.Controls.Add( tbOperation );


            SlidingDateRangePicker slidingDateRangePicker = new SlidingDateRangePicker();
            slidingDateRangePicker.ID = parentControl.ID + "_slidingDateRangePicker";
            slidingDateRangePicker.AddCssClass( "js-sliding-date-range" );
            slidingDateRangePicker.Label = "Date Range";
            slidingDateRangePicker.Help = "The date range of the interactions";
            parentControl.Controls.Add( slidingDateRangePicker );

            return new System.Web.UI.Control[] { ddlInteractionChannel, ddlInteractionComponent, tbOperation, slidingDateRangePicker };
        }

        /// <summary>
        /// Populates the group roles.
        /// </summary>
        /// <param name="interactionChannelId">The interaction channel identifier.</param>
        /// <param name="ddlInteractionComponent">The Interaction Component.</param>
        private void PopulateInteractionComponent( int? interactionChannelId, RockDropDownList ddlInteractionComponent )
        {
            if ( interactionChannelId.HasValue )
            {
                var interactionComponentService = new InteractionComponentService( new RockContext() );
                var interactionComponents = interactionComponentService.Queryable()
                                    .Where( a => a.InteractionChannelId == ( interactionChannelId ?? 0 ) )
                                    .OrderBy( a => a.Name ).
                                    Select( a => new
                                    {
                                        a.Id,
                                        a.Name
                                    } ).ToList();

                ddlInteractionComponent.Items.Clear();
                ddlInteractionComponent.Items.Add( new ListItem() );
                ddlInteractionComponent.Items.AddRange( interactionComponents.Select( a => new ListItem( a.Name, a.Id.ToString() ) ).ToArray() );
                ddlInteractionComponent.Visible = interactionComponents.Count > 0;
            }
            else
            {
                ddlInteractionComponent.Visible = false;
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlInteractionChannel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlInteractionChannel_SelectedIndexChanged( object sender, EventArgs e )
        {
            var ddlInteractionChannel = sender as RockDropDownList;
            if ( ddlInteractionChannel != null )
            {
                var ddlInteractionComponent = ddlInteractionChannel.Parent.FindControl( ddlInteractionChannel.ID.Replace( "_ddlInteractionChannel", "_ddlInteractionComponent" ) ) as RockDropDownList;
                if ( ddlInteractionComponent != null )
                {
                    int? interactionChannelId = ddlInteractionChannel.SelectedValueAsId();
                    PopulateInteractionComponent( interactionChannelId, ddlInteractionComponent );
                }
            }
        }

        /// <summary>
        /// Renders the controls.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="controls">The controls.</param>
        public override void RenderControls( System.Web.UI.Control parentControl, System.Web.UI.HtmlTextWriter writer, System.Web.UI.Control[] controls )
        {
            base.RenderControls( parentControl, writer, controls );
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override string GetSelection( System.Web.UI.Control[] controls )
        {
            var ddlInteractionChannel = ( controls[0] as RockDropDownList );
            var ddlInteractionComponent = ( controls[1] as RockDropDownList );
            var tbOperation = ( controls[2] as RockTextBox );
            var slidingDateRangePicker = ( controls[3] as SlidingDateRangePicker );

            int interactionChannelId = ddlInteractionChannel.SelectedValueAsId() ?? 0;
            var rockContext = new RockContext();
            var interactionChannel = new InteractionChannelService( rockContext ).Get( interactionChannelId );
            Guid? interactionChannelGuid = null;
            if ( interactionChannel != null )
            {
                interactionChannelGuid = interactionChannel.Guid;
            }

            int interactionComponentId = ddlInteractionComponent.SelectedValueAsId() ?? 0;
            var interactionComponent = new InteractionComponentService( rockContext ).Get( interactionComponentId );
            Guid? interactionComponentGuid = null;
            if ( interactionComponent != null )
            {
                interactionComponentGuid = interactionComponent.Guid;
            }

            // convert pipe to comma delimited
            var delimitedValues = slidingDateRangePicker.DelimitedValues.Replace( "|", "," );

            return $"{interactionChannelGuid.ToString()}|{interactionComponentGuid}|{tbOperation.Text}|{delimitedValues}";
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( System.Web.UI.Control[] controls, string selection )
        {
            string[] selectionValues = selection.Split( '|' );
            if ( selectionValues.Length >= 1 )
            {
                Guid interactionChannelGuid = selectionValues[0].AsGuid();
                var rockContext = new RockContext();
                var interactionChannel = new InteractionChannelService( rockContext ).Get( interactionChannelGuid );
                var ddlInteractionChannel = ( controls[0] as RockDropDownList );
                ddlInteractionChannel.SetValue( interactionChannel != null ? interactionChannel.Id : ( int? ) null );

                ddlInteractionChannel_SelectedIndexChanged( ddlInteractionChannel, new EventArgs() );

                if ( selectionValues.Length >= 2 )
                {
                    var interactionComponentGuid = selectionValues[1].AsGuidOrNull();
                    if ( interactionComponentGuid.HasValue )
                    {
                        RockDropDownList ddlInteractionComponent = ( controls[1] as RockDropDownList );
                        var interactionComponent = new InteractionComponentService( rockContext ).Get( interactionComponentGuid.Value );
                        ddlInteractionComponent.SetValue( interactionComponent != null ? interactionComponent.Id : ( int? ) null );
                    }
                }

                RockTextBox tbOperation = controls[2] as RockTextBox;
                if ( selectionValues.Length >= 3 )
                {
                    tbOperation.Text = selectionValues[2];
                }

                SlidingDateRangePicker slidingDateRangePicker = controls[3] as SlidingDateRangePicker;
                if ( selectionValues.Length >= 4 )
                {
                    slidingDateRangePicker.DelimitedValues = selectionValues[3].Replace( ',', '|' );
                }
            }
        }

        #endregion
    }
}
