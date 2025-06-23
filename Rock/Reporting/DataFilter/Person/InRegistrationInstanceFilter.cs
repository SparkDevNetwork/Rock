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
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Net;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;
using Rock.Web.UI.Controls;

/*
    06/24/2020 - MSB

    This filter needs to stay in this namespace unless a migration is created and tested to move the data view filters over to the
    new entity type that will be created for the new location.

    Reason: DataView Filters
*/

namespace Rock.Reporting.DataSelect.Person
{
    /// <summary>
    /// Filter people on whether they registered (registrar) or were registered (registrant) in the designated registration instance.
    /// </summary>
    [Description( "Filter people on whether they registered (registrar) or were registered (registrant) in the designated registration instance." )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Person In Registration Instance Filter" )]
    [Rock.SystemGuid.EntityTypeGuid( "1F51DA3B-22FE-4093-9DAA-5492B5FB17DA" )]
    public class InRegistrationInstanceFilter : DataFilterComponent
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

        #region Configuration

        /// <inheritdoc/>
        public override DynamicComponentDefinitionBag GetComponentDefinition( Type entityType, string selection, RockContext rockContext, RockRequestContext requestContext )
        {
            return new DynamicComponentDefinitionBag
            {
                Url = requestContext.ResolveRockUrl( "~/Obsidian/Reporting/DataFilters/Person/inRegistrationInstanceFilter.obs" )
            };
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetObsidianComponentData( Type entityType, string selection, RockContext rockContext, RockRequestContext requestContext )
        {
            var data = new Dictionary<string, string> {
                { "selection", selection }, // TODO remove selection
                { "person", "2" },
                { "onWaitList", null }
            };

            if ( selection.IsNullOrWhiteSpace() )
            {
                return data;
            }

            SelectionConfig selectionConfig = SelectionConfig.Parse( selection );

            var registrationTemplate = new RegistrationTemplateService( rockContext ).Get( selectionConfig.RegistrationTemplateId );
            data.Add( "template", registrationTemplate?.ToListItemBag().ToCamelCaseJson( false, true ) );

            var registrationInstance = new RegistrationInstanceService( rockContext ).Get( selectionConfig.RegistrationInstanceGuid ?? Guid.Empty );
            data.Add( "instance", registrationInstance?.ToListItemBag().ToCamelCaseJson( false, true ) );

            data.AddOrReplace( "person", selectionConfig.RegistrationType.ConvertToInt().ToString() );
            data.AddOrReplace( "onWaitList", selectionConfig.OnWaitList?.ToTrueFalse() );

            return data;
        }

        /// <inheritdoc/>
        public override string GetSelectionFromObsidianComponentData( Type entityType, Dictionary<string, string> data, RockContext rockContext, RockRequestContext requestContext )
        {
            SelectionConfig selectionConfig = new SelectionConfig();

            var templateGuid = data.GetValueOrNull( "template" )?.FromJsonOrNull<ListItemBag>()?.Value?.AsGuidOrNull();

            if ( templateGuid.HasValue )
            {
                var template = new RegistrationTemplateService( rockContext ).Get( templateGuid.Value );
                selectionConfig.RegistrationTemplateId = template.Id;
            }
            else
            {
                var template = new RegistrationTemplateService( rockContext ).Queryable().FirstOrDefault();
                selectionConfig.RegistrationTemplateId = template.Id;
            }

            var instanceGuid = data.GetValueOrNull( "instance" )?.FromJsonOrNull<ListItemBag>()?.Value?.AsGuidOrNull();
            selectionConfig.RegistrationInstanceGuid = instanceGuid;

            selectionConfig.RegistrationType = ( RegistrationTypeSpecifier ) data.GetValueOrDefault( "person", "2" ).AsInteger();
            selectionConfig.OnWaitList = data.GetValueOrNull( "onWaitList" )?.AsBooleanOrNull();
            return selectionConfig.ToJson();
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
            return "In Registration Instance";
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
  var result = $('.js-registration-type input:first', $content).is(':checked') ? 'Registrar' : 'Registrant';
  var onWaitList = $('.js-on-wait-list option:selected', $content).val();
  if (onWaitList) {
    if (onWaitList === 'True') {
        onWaitList = ', only wait list';
    } else {
        onWaitList = ', no wait list';
    }
  }
  var registrationInstance = $('.js-registration-instance option:selected', $content);
  if ( registrationInstance.length > 0  && registrationInstance.val() ) {
     result = result + ' in registration instance ""' + registrationInstance.text() + '""' + onWaitList;
  } else {
    var registrationTemplate = $('.js-registration-template option:selected', $content).text();
    result = result + ' in any registration instance of template ""' + registrationTemplate + '""' + onWaitList;
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
            SelectionConfig selectionConfig = SelectionConfig.Parse( selection );

            string filterOptions = selectionConfig.RegistrationType == RegistrationTypeSpecifier.Registrar ? "Registrar" : "Registrant";

            var waitlistFilterStatus = selectionConfig.OnWaitList == null ? string.Empty : Convert.ToBoolean( selectionConfig.OnWaitList ) ? ", only wait list" : ", no wait list";

            var registrationInstance = new RegistrationInstanceService( new RockContext() ).Queryable().Where( a => a.Guid == selectionConfig.RegistrationInstanceGuid ).FirstOrDefault();
            if ( registrationInstance != null )
            {
                return string.Format( "{0} in registration instance '{1}' {2}", filterOptions, registrationInstance.Name, waitlistFilterStatus );
            }
            else
            {
                var registrationTemplate = new RegistrationTemplateService( new RockContext() ).Queryable().Where( t => t.Id == selectionConfig.RegistrationTemplateId ).FirstOrDefault();
                return string.Format( "{0} in any registration instance of template '{1}' {2}", filterOptions, registrationTemplate.Name, waitlistFilterStatus );
            }
        }

#if WEBFORMS

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            var controls = new List<Control>();

            var _ddlRegistrationTemplate = new RockDropDownList();
            _ddlRegistrationTemplate.CssClass = "js-registration-template";
            _ddlRegistrationTemplate.ID = filterControl.ID + "_ddlRegistrationTemplate";
            _ddlRegistrationTemplate.Label = "Registration Template";
            _ddlRegistrationTemplate.DataTextField = "Name";
            _ddlRegistrationTemplate.DataValueField = "Id";
            _ddlRegistrationTemplate.DataSource = new RegistrationTemplateService( new RockContext() ).Queryable()
                .OrderBy( a => a.Name )
                .Select( d => new
                {
                    d.Id,
                    d.Name
                } )
            .ToList();
            _ddlRegistrationTemplate.DataBind();
            _ddlRegistrationTemplate.SelectedIndexChanged += ddlRegistrationTemplate_SelectedIndexChanged;
            _ddlRegistrationTemplate.AutoPostBack = true;
            filterControl.Controls.Add( _ddlRegistrationTemplate );

            // Now add the registration instance picker
            var _ddlRegistrationInstance = new RockDropDownList();
            _ddlRegistrationInstance.CssClass = "js-registration-instance";
            _ddlRegistrationInstance.Label = "Registration Instance";
            _ddlRegistrationInstance.ID = filterControl.ID + "_ddlRegistrationInstance";
            filterControl.Controls.Add( _ddlRegistrationInstance );

            PopulateRegistrationInstanceList( filterControl );

            var _rblRegistrationType = new RockRadioButtonList();
            _rblRegistrationType.CssClass = "js-registration-type";
            _rblRegistrationType.ID = filterControl.ID + "_registrationType";
            _rblRegistrationType.RepeatDirection = RepeatDirection.Horizontal;
            _rblRegistrationType.Label = "Person";
            _rblRegistrationType.Help = "Choose whether to filter by the person who did the registering (registrar) or the person who was registered (registrant).";
            _rblRegistrationType.Items.Add( new ListItem( "Registrar", "1" ) );
            _rblRegistrationType.Items.Add( new ListItem( "Registrant", "2" ) );
            _rblRegistrationType.SelectedValue = "2";
            filterControl.Controls.Add( _rblRegistrationType );

            // Add control for 'on wait list' drop down.
            RockDropDownList ddlOnWaitList = new RockDropDownList();
            ddlOnWaitList.CssClass = "js-on-wait-list";
            ddlOnWaitList.ID = $"{filterControl.ID}_ddlOnWaitList";
            ddlOnWaitList.Label = "On Wait List";
            ddlOnWaitList.Help = "Select 'Yes' to only show people on the wait list. Select 'No' to only show people who are not on the wait list, or leave blank to ignore wait list status.";
            ddlOnWaitList.Items.Add( new ListItem() );
            ddlOnWaitList.Items.Add( new ListItem( "Yes", "True" ) );
            ddlOnWaitList.Items.Add( new ListItem( "No", "False" ) );

            // Set as blank (includes both wait list and regular registrations) by default.
            ddlOnWaitList.SelectedValue = string.Empty;
            filterControl.Controls.Add( ddlOnWaitList );

            return new Control[4] { _ddlRegistrationTemplate, _ddlRegistrationInstance, _rblRegistrationType, ddlOnWaitList };
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlRegistrationTemplate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlRegistrationTemplate_SelectedIndexChanged( object sender, EventArgs e )
        {
            var filterField = ( sender as Control ).FirstParentControlOfType<FilterField>();
            PopulateRegistrationInstanceList( filterField );
        }

        /// <summary>
        /// Populates the registration instance list.
        /// </summary>
        /// <param name="filterField">The filter field.</param>
        private void PopulateRegistrationInstanceList( FilterField filterField )
        {
            var _ddlRegistrationTemplate = filterField.ControlsOfTypeRecursive<RockDropDownList>().FirstOrDefault( a => a.HasCssClass( "js-registration-template" ) );
            var _ddlRegistrationInstance = filterField.ControlsOfTypeRecursive<RockDropDownList>().FirstOrDefault( a => a.HasCssClass( "js-registration-instance" ) );

            var registrationTemplateId = _ddlRegistrationTemplate.SelectedValue.AsInteger();
            if ( registrationTemplateId != 0 )
            {
                _ddlRegistrationInstance.Items.Clear();
                _ddlRegistrationInstance.Items.Add( new ListItem( "- Any -", string.Empty ) );
                foreach ( var item in new RegistrationInstanceService( new RockContext() ).Queryable().Where( r => r.RegistrationTemplateId == registrationTemplateId ).OrderBy( r => r.Name ) )
                {
                    _ddlRegistrationInstance.Items.Add( new ListItem( item.Name, item.Guid.ToString() ) );
                }

                _ddlRegistrationInstance.Visible = _ddlRegistrationInstance.Items.Count > 1;
            }
            else
            {
                _ddlRegistrationInstance.Visible = false;
            }
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
            SelectionConfig selectionConfig = new SelectionConfig();

            var ddlRegistrationTemplate = controls[0] as RockDropDownList;
            var ddlRegistrationInstance = controls[1] as RockDropDownList;
            var rblRegistrationType = controls[2] as RockRadioButtonList;
            var ddlOnWaitList = controls[3] as RockDropDownList;

            selectionConfig.RegistrationTemplateId = ddlRegistrationTemplate.SelectedValue.AsInteger();
            selectionConfig.RegistrationInstanceGuid = ddlRegistrationInstance.SelectedValue.AsGuidOrNull();
            selectionConfig.RegistrationType = ( RegistrationTypeSpecifier ) rblRegistrationType.SelectedValue.AsInteger();
            selectionConfig.OnWaitList = ddlOnWaitList.SelectedValue.AsBooleanOrNull();
            return selectionConfig.ToJson();
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( Type entityType, Control[] controls, string selection )
        {
            SelectionConfig selectionConfig = SelectionConfig.Parse( selection );

            var registrationTemplate = new RegistrationTemplateService( new RockContext() ).Get( selectionConfig.RegistrationTemplateId );
            var ddlRegistrationTemplate = controls[0] as RockDropDownList;
            if ( registrationTemplate != null )
            {
                ddlRegistrationTemplate.SetValue( selectionConfig.RegistrationTemplateId );
            }

            ddlRegistrationTemplate_SelectedIndexChanged( ddlRegistrationTemplate, new EventArgs() );

            var ddlRegistrationInstance = controls[1] as RockDropDownList;
            if ( selectionConfig.RegistrationInstanceGuid != null )
            {
                ddlRegistrationInstance.SetValue( selectionConfig.RegistrationInstanceGuid );
            }
            else
            {
                ddlRegistrationInstance.SetValue( string.Empty );
            }

            var rblRegistrationType = controls[2] as RockRadioButtonList;

            rblRegistrationType.SetValue( selectionConfig.RegistrationType.ConvertToInt() );

            var ddlOnWaitList = controls[3] as RockDropDownList;
            if ( selectionConfig.OnWaitList != null )
            {
                ddlOnWaitList.SetValue( selectionConfig.OnWaitList.ToString() );
            }
            else
            {
                ddlOnWaitList.SetValue( string.Empty );
            }
        }

#endif

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
            var rockContext = ( RockContext ) serviceInstance.Context;

            SelectionConfig selectionConfig = SelectionConfig.Parse( selection );
            if ( selectionConfig == null )
            {
                // The selection configuration is null, so return nothing.
                return Expression.Constant( false );
            }

            IQueryable<RegistrationRegistrant> registrantQuery;
            IQueryable<Registration> registrationQuery;
            IQueryable<Rock.Model.Person> qry;

            if ( selectionConfig.RegistrationType == RegistrationTypeSpecifier.Registrant )
            {
                registrantQuery = new RegistrationRegistrantService( rockContext ).Queryable()
                    .Where( r => r.Registration.RegistrationInstance.RegistrationTemplateId == selectionConfig.RegistrationTemplateId );

                if ( selectionConfig.RegistrationInstanceGuid != null )
                {
                    registrantQuery = registrantQuery.Where( r => r.Registration.RegistrationInstance.Guid == selectionConfig.RegistrationInstanceGuid );
                }

                // If the OnWaitList drop-down is NOT null, filter the registrant query based on registrants' wait list status.
                if ( selectionConfig.OnWaitList != null )
                {
                    registrantQuery = registrantQuery.Where( r => r.OnWaitList == selectionConfig.OnWaitList );
                }

                qry = new PersonService( rockContext ).Queryable()
                    .Where( p => registrantQuery.Where( xx => xx.PersonAlias.PersonId == p.Id ).Count() >= 1 );
            }
            else
            {
                // When the type is the Registrar.
                registrationQuery = new RegistrationService( rockContext ).Queryable()
                    .Where( r => r.RegistrationInstance.RegistrationTemplateId == selectionConfig.RegistrationTemplateId );

                if ( selectionConfig.RegistrationInstanceGuid != null )
                {
                    registrationQuery = registrationQuery.Where( r => r.RegistrationInstance.Guid == selectionConfig.RegistrationInstanceGuid );
                }

                // If the OnWaitList drop-down is NOT null, filter the registration query based on registrants' wait list status.
                if ( selectionConfig.OnWaitList != null )
                {
                    registrationQuery = registrationQuery.SelectMany( r => r.Registrants.Where( reg => reg.OnWaitList == selectionConfig.OnWaitList ) ).Select( r => r.Registration );
                }

                qry = new PersonService( rockContext ).Queryable()
                    .Where( p => registrationQuery.Where( xx => xx.PersonAlias.PersonId == p.Id ).Count() >= 1 );
            }

            Expression result = FilterExpressionExtractor.Extract<Rock.Model.Person>( qry, parameterExpression, "p" );

            return result;
        }

        #endregion Public methods

        /// <summary>
        /// Get and set the filter settings from DataViewFilter.Selection
        /// </summary>
        protected class SelectionConfig
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SelectionConfig"/> class.
            /// </summary>
            public SelectionConfig()
            {
                // Add values to set defaults / populate upon object creation.
            }

            /// <summary>
            /// Gets or sets the registration template ID.
            /// </summary>
            /// <value>
            /// The integer value of the registration template ID.
            /// </value>
            public int RegistrationTemplateId { get; set; }

            /// <summary>
            /// Gets or sets the registration instance guid.
            /// </summary>
            /// <value>
            /// The nullable value of the registration instance guid.
            /// </value>
            public Guid? RegistrationInstanceGuid { get; set; }

            /// <summary>
            /// Gets or sets the registration type.
            /// </summary>
            /// <value>
            /// The <see cref="RegistrationTypeSpecifier"/> of the registration type.
            /// </value>
            public RegistrationTypeSpecifier RegistrationType { get; set; }

            /// <summary>
            /// Gets or sets the 'On Wait List' filter status.
            /// </summary>
            /// <value>
            /// The nullable boolean value of the On Wait List filter.
            /// </value>
            public bool? OnWaitList { get; set; }

            /// <summary>
            /// Parses the specified selection from a JSON or delimited string.  If a delimited string, position 1 is the template, 2 is the registration instance, 3 is the type (registrar or registrant), 4 is the wait list status.
            /// </summary>
            /// <param name="selection">The selection.</param>
            /// <returns></returns>
            public static SelectionConfig Parse( string selection )
            {
                var selectionConfig = selection.FromJsonOrNull<SelectionConfig>();

                // This will only occur when the selection string is not JSON.
                if ( selectionConfig == null )
                {
                    selectionConfig = new SelectionConfig();

                    // If the configuration is a pipe-delimited string, then try to parse it the old-fashioned way.
                    string[] selectionValues = selection.Split( '|' );

                    // Index 0 is the registration template ID.
                    // Index 1 is the instance guid.
                    // Index 2 is the registration type.
                    if ( selectionValues.Count() >= 3 )
                    {
                        selectionConfig.RegistrationTemplateId = selectionValues[0].AsInteger();
                        selectionConfig.RegistrationInstanceGuid = selectionValues[1].AsGuidOrNull();
                        selectionConfig.RegistrationType = ( RegistrationTypeSpecifier ) selectionValues[2].AsInteger();
                    }
                    else
                    {
                        // If there are not at least 3 values in the selection string then it is not a valid selection.
                        return null;
                    }

                    // Index 3 is the 'on wait list' option.
                    if ( selectionValues.Count() >= 4 )
                    {
                        selectionConfig.OnWaitList = selectionValues[3].AsBooleanOrNull();
                    }
                }

                return selectionConfig;
            }
        }

        /// <summary>
        /// Enumeration of registration type.
        /// </summary>
        public enum RegistrationTypeSpecifier
        {
            /// <summary>
            /// When the results include the people who were registered.
            /// </summary>
            Registrant = 2,

            /// <summary>
            /// When the results include the person / people who made the registrations (for others). 
            /// </summary>
            Registrar = 1
        }
    }
}