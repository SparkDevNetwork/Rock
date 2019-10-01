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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataFilter.Person
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Filter people basaed on Interactions" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Interactions Filter" )]
    public class InteractionsFilter : DataFilterComponent
    {
        #region Properties

        /// <summary>
        /// Gets the entity type that filter applies to.
        /// </summary>
        /// <value>
        /// The entity that filter applies to.
        /// </value>
        public override string AppliesToEntityType
        {
            get { return typeof( Rock.Model.Person ).FullName; }
        }

        /// <summary>
        /// Gets the section.
        /// </summary>
        /// <value>
        /// The section.
        /// </value>
        public override string Section
        {
            get { return "Additional Filters"; }
        }

        #endregion

        #region Public Methods

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
            return "Interactions";
        }

        /// <summary>
        /// Formats the selection on the client-side.  When the filter is collapsed by the user, the Filterfield control
        /// will set the description of the filter to whatever is returned by this property.  If including script, the
        /// controls parent container can be referenced through a '$content' variable that is set by the control before 
        /// referencing this property.
        /// </summary>
        /// <value>
        /// The client format script.
        /// </value>
        public override string GetClientFormatSelection( Type entityType )
        {
            return @"
function() {
  
  var result = 'Interactions';

  var interactionChannel = $('.js-interaction-channel option:selected', $content).text();
  if (interactionChannel) {
     result = result + 'In Interaction Channel:' + interactionChannel;

     var interactionComponent = $('.js-interaction-component option:selected', $content).text();
     if (interactionComponent) {
        result = result + ', In Interaction Component:' + interactionComponent;
     }

    var containsText = $('.js-tbOperation', $content).val();
    if(containsText!==''){
        result = result + ', With operation:' + containsText;
    }

    var dateRangeText = $('.js-slidingdaterange-text-value', $content).val();
    if(dateRangeText !== ''){
        result = result + ', date range:' + dateRangeText;
    }
  }

  return result;
}
";
        }

        /// <summary>
        /// Formats the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override string FormatSelection( Type entityType, string selection )
        {
            string result = "Interactions";
            string[] selectionValues = selection.Split( '|' );
            if ( selectionValues.Length >= 2 )
            {
                var rockContext = new RockContext();
                var interactionChannel = new InteractionChannelService( rockContext ).Get( selectionValues[0].AsGuid() );

                if ( interactionChannel != null )
                {
                    result = string.Format( "In Interaction Channel: {0}", interactionChannel.Name );

                    var interactionComponentGuid = selectionValues[1].AsGuidOrNull();
                    if ( interactionComponentGuid.HasValue )
                    {
                        var interactionComponent = new InteractionComponentService( rockContext ).Get( interactionComponentGuid.Value );
                        if ( interactionComponent != null )
                        {
                            result += string.Format( ", in Interaction Component: {0}", interactionComponent.Name );
                        }
                    }

                    var operation = selectionValues[2];
                    if ( !string.IsNullOrEmpty( operation ) )
                    {
                        result += string.Format( ", with operation: {0}", operation );
                    }

                    if ( !string.IsNullOrEmpty( selectionValues[3] ) )
                    {
                        SlidingDateRangePicker fakeSlidingDateRangePicker = new SlidingDateRangePicker();
                        var formattedDelimitedValues = SlidingDateRangePicker.FormatDelimitedValues( selectionValues[3].Replace( ',', '|' ) );
                        if ( !string.IsNullOrEmpty( formattedDelimitedValues ) )
                        {
                            result += string.Format( ", date range: {0}", formattedDelimitedValues );
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// The ddlInteractionChannel RockDropDownList
        /// </summary>
        private RockDropDownList ddlInteractionChannel = null;

        /// <summary>
        /// The ddlInteractionComponent RockDropDownList
        /// </summary>
        private RockDropDownList ddlInteractionComponent = null;

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            ddlInteractionChannel = new RockDropDownList();
            ddlInteractionChannel.ID = filterControl.ID + "_ddlInteractionChannel";
            ddlInteractionChannel.Label = "Interaction Channel";
            ddlInteractionChannel.CssClass = "js-interaction-channel";
            ddlInteractionChannel.Required = true;
            ddlInteractionChannel.AutoPostBack = true;
            ddlInteractionChannel.EnhanceForLongLists = true;
            ddlInteractionChannel.SelectedIndexChanged += ddlInteractionChannel_SelectedIndexChanged;
            filterControl.Controls.Add( ddlInteractionChannel );

            var interactionChannelService = new InteractionChannelService( new RockContext() );
            var interactionChannels = interactionChannelService.Queryable().OrderBy( a => a.Name ).Select( a => new
            {
                a.Id,
                a.Name
            } ).ToList();

            ddlInteractionChannel.Items.Clear();
            ddlInteractionChannel.Items.Add( new ListItem() );
            ddlInteractionChannel.Items.AddRange( interactionChannels.Select( a => new ListItem( a.Name, a.Id.ToString() ) ).ToArray() );

            int? selectedInteractionChannelId = filterControl.Page.Request.Params[ddlInteractionChannel.UniqueID].AsIntegerOrNull();
            ddlInteractionChannel.SetValue( selectedInteractionChannelId );

            ddlInteractionComponent = new RockDropDownList();
            ddlInteractionComponent.ID = filterControl.ID + "_ddlInteractionComponent";
            ddlInteractionComponent.Label = "Interaction Component";
            ddlInteractionComponent.CssClass = "js-interaction-component";
            ddlInteractionComponent.EnhanceForLongLists = true;
            filterControl.Controls.Add( ddlInteractionComponent );

            PopulateInteractionComponent( selectedInteractionChannelId, ddlInteractionComponent );

            RockTextBox tbOperation = new RockTextBox();
            tbOperation.Label = "Operation";
            tbOperation.ID = filterControl.ID + "_tbOperation";
            tbOperation.CssClass = "js-tbOperation";
            filterControl.Controls.Add( tbOperation );

            SlidingDateRangePicker slidingDateRangePicker = new SlidingDateRangePicker();
            slidingDateRangePicker.ID = filterControl.ID + "_slidingDateRangePicker";
            slidingDateRangePicker.AddCssClass( "js-sliding-date-range" );
            slidingDateRangePicker.Label = "Date Range";
            slidingDateRangePicker.Help = "The date range of the interactions";
            filterControl.Controls.Add( slidingDateRangePicker );

            return new Control[4] { ddlInteractionChannel, ddlInteractionComponent, tbOperation, slidingDateRangePicker };
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
                                    .Where( a => a.ChannelId == ( interactionChannelId ?? 0 ) )
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
            int? interactionChannelId = ddlInteractionChannel.SelectedValueAsId();
            PopulateInteractionComponent( interactionChannelId, ddlInteractionComponent );

        }

        /// <summary>
        /// Renders the controls.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="filterControl">The filter control.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="controls">The controls.</param>
        public override void RenderControls( Type entityType, FilterField filterControl, HtmlTextWriter writer, Control[] controls )
        {
            base.RenderControls( entityType, filterControl, writer, controls );
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override string GetSelection( Type entityType, Control[] controls )
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
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( Type entityType, Control[] controls, string selection )
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

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="serviceInstance">The service instance.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override Expression GetExpression( Type entityType, IService serviceInstance, ParameterExpression parameterExpression, string selection )
        {
            string[] selectionValues = selection.Split( '|' );
            if ( selectionValues.Length >= 4 )
            {
                var rockContext = ( RockContext ) serviceInstance.Context;
                var interactionQry = new InteractionService( rockContext ).Queryable();
                Guid interactionChannelGuid = selectionValues[0].AsGuid();
                var interactionComponentGuid = selectionValues[1].AsGuidOrNull();

                if ( interactionComponentGuid.HasValue )
                {
                    interactionQry = interactionQry.Where( xx => xx.InteractionComponent.Guid == interactionComponentGuid.Value );
                }
                else
                {
                    interactionQry = interactionQry.Where( xx => xx.InteractionComponent.Channel.Guid == interactionChannelGuid );
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

                var innerQry = interactionQry.Select( xx => xx.PersonAlias.PersonId ).AsQueryable();

                var qry = new PersonService( ( RockContext ) serviceInstance.Context ).Queryable()
                    .Where( p => innerQry.Any( xx => xx == p.Id ) );

                Expression extractedFilterExpression = FilterExpressionExtractor.Extract<Rock.Model.Person>( qry, parameterExpression, "p" );

                return extractedFilterExpression;
            }

            return null;
        }

        #endregion
    }
}