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
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;
using Rock.Net;
using Rock.Security;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Rest.Controls;
using Rock.ViewModels.Utility;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataSelect.Person
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Selects Interaction Date for a Person" )]
    [Export( typeof( DataSelectComponent ) )]
    [ExportMetadata( "ComponentName", "Select the first or last Interaction date of a person" )]
    [Rock.SystemGuid.EntityTypeGuid( "F6F153F3-B901-4796-A0EB-D055B234EED4" )]
    public class InteractionDateSelect : DataSelectComponent
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
                return "InteractionDate";
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
            get { return typeof( DateTime? ); }
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
                return "Interaction Date";
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

        #region Configuration

        /// <inheritdoc/>
        public override DynamicComponentDefinitionBag GetComponentDefinition( Type entityType, string selection, RockContext rockContext, RockRequestContext requestContext )
        {
            var options = new Dictionary<string, string>();

            var interactionChannelOptions = new InteractionChannelService( rockContext ).Queryable()
                .OrderBy( a => a.Name )
                .Select( a => new ListItemBag { Value = a.Guid.ToString(), Text = a.Name } )
                .ToList();

            options.Add( "interactionChannelOptions", interactionChannelOptions.ToCamelCaseJson( false, true ) );

            string[] selectionValues = selection.Split( '|' );
            if ( selectionValues.Length >= 1 )
            {
                Guid? interactionChannelGuid = selectionValues[0].AsGuidOrNull();

                if ( interactionChannelGuid.HasValue )
                {
                    var interactionComponentOptions = new InteractionComponentService( rockContext ).Queryable()
                        .Where( a => a.InteractionChannel.Guid == interactionChannelGuid )
                        .OrderBy( a => a.Name )
                        .Select( a => new ListItemBag { Value = a.Guid.ToString(), Text = a.Name } )
                        .ToList();

                    options.Add( "interactionComponentOptions", interactionComponentOptions.ToCamelCaseJson( false, true ) );
                }
            }

            var selectionModeOptions = Enum.GetValues( typeof( FirstLastInteraction ) )
                .Cast<FirstLastInteraction>()
                .Select( a => new ListItemBag
                {
                    Value = a.ConvertToInt().ToString(),
                    Text = a.ConvertToString()
                } )
                .ToList();

            options.Add( "selectionModeOptions", selectionModeOptions.ToCamelCaseJson( false, true ) );

            return new DynamicComponentDefinitionBag
            {
                Url = requestContext.ResolveRockUrl( "~/Obsidian/Reporting/DataSelects/Person/interactionDateSelect.obs" ),
                Options = options
            };
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetObsidianComponentData( Type entityType, string selection, RockContext rockContext, RockRequestContext requestContext )
        {
            var data = new Dictionary<string, string>();
            string[] selectionValues = selection.Split( '|' );

            if ( selectionValues.Length >= 1 )
            {
                data.Add( "interactionChannel", selectionValues[0] );
            }

            if ( selectionValues.Length >= 2 )
            {
                data.Add( "interactionComponent", selectionValues[1] );
            }

            if ( selectionValues.Length >= 3 )
            {
                data.Add( "operation", selectionValues[2] );
            }

            if ( selectionValues.Length >= 4 )
            {
                data.Add( "selectionMode", selectionValues[3] );
            }

            return data;
        }

        /// <inheritdoc/>
        public override string GetSelectionFromObsidianComponentData( Type entityType, Dictionary<string, string> data, RockContext rockContext, RockRequestContext requestContext )
        {
            var interactionChannel = data.GetValueOrDefault( "interactionChannel", string.Empty );
            var interactionComponent = data.GetValueOrDefault( "interactionComponent", string.Empty );
            var operation = data.GetValueOrDefault( "operation", string.Empty );
            var selectionMode = data.GetValueOrDefault( "selectionMode", "0" );

            return $"{interactionChannel}|{interactionComponent}|{operation}|{selectionMode}";
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> ExecuteComponentRequest( Dictionary<string, string> request, SecurityGrant securityGrant, RockContext rockContext, RockRequestContext requestContext )
        {
            var action = request.GetValueOrNull( "action" );
            var options = request.GetValueOrNull( "options" )?.FromJsonOrNull<InteractionCountSelectGetComponentsOptionsBag>();

            if ( action == "GetComponents" && options != null && options.InteractionChannelGuid != null )
            {
                var interactionComponentOptions = new InteractionComponentService( rockContext ).Queryable()
                    .Where( a => a.InteractionChannel.Guid == options.InteractionChannelGuid )
                    .OrderBy( a => a.Name )
                    .Select( a => new ListItemBag { Value = a.Guid.ToString(), Text = a.Name } )
                    .ToList();

                return new Dictionary<string, string> { ["interactionComponentOptions"] = interactionComponentOptions.ToCamelCaseJson( false, true ) };
            }

            return null;
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
            return "Interaction Date";
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

                var selectionMode = selectionValues[3].ConvertToEnum<FirstLastInteraction>();

                IQueryable<DateTime?> qry;
                if ( selectionMode == FirstLastInteraction.First )
                {
                    qry = new PersonService( context ).Queryable().Select( p => interactionQry.Where( l => l.PersonAlias.PersonId == p.Id )
                   .Min( l => ( DateTime? ) l.InteractionDateTime ) );
                }
                else
                {
                    qry = new PersonService( context ).Queryable().Select( p => interactionQry.Where( l => l.PersonAlias.PersonId == p.Id )
                  .Max( l => ( DateTime? ) l.InteractionDateTime ) );
                }

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


            RockRadioButtonList rblSelectionMode = new RockRadioButtonList();
            rblSelectionMode.ID = parentControl.ID + "rblSelectionMode";
            rblSelectionMode.Label = "Selection Mode";
            rblSelectionMode.BindToEnum<FirstLastInteraction>();
            rblSelectionMode.SetValue( FirstLastInteraction.First.ConvertToInt() );
            parentControl.Controls.Add( rblSelectionMode );

            return new System.Web.UI.Control[] { ddlInteractionChannel, ddlInteractionComponent, tbOperation, rblSelectionMode };
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
            var rblSelectionMode = ( controls[3] as RockRadioButtonList );

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

            return $"{interactionChannelGuid.ToString()}|{interactionComponentGuid}|{tbOperation.Text}|{rblSelectionMode.SelectedValue}";
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

                RockRadioButtonList rblSelectionMode = controls[3] as RockRadioButtonList;
                if ( selectionValues.Length >= 4 )
                {
                    rblSelectionMode.SetValue( selectionValues[3] );
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public enum FirstLastInteraction
        {
            /// <summary>
            /// The first interaction
            /// </summary>
            First,

            /// <summary>
            /// The last interaction
            /// </summary>
            Last
        }

        #endregion
    }
}
