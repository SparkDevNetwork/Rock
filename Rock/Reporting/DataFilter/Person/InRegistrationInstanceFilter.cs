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
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataSelect.Person
{
    /// <summary>
    /// Filter people on whether they registered (registrar) or were registered (registrant) in the designated registration instance.
    /// </summary>
    [Description( "Filter people on whether they registered (registrar) or were registered (registrant) in the designated registration instance." )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Person In Registration Instance Filter" )]
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
        /// The Registration Template picker (drop down list)
        /// </summary>
        private RockDropDownList _ddlRegistrationTemplate = null;

        /// <summary>
        /// The Registration Instance picker (drop down list)
        /// </summary>
        private RockDropDownList _ddlRegistrationInstance = null;

        /// <summary>
        /// The registration type (registrar or registrant) radio button list
        /// </summary>
        private RockRadioButtonList _rblRegistrationType = null;

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
  var result = $('.js-registration-type input:first', $content).is(':checked') ? 'Registrar' : 'Registrant'

  var registrationInstance = $('.js-registration-instance option:selected', $content);
  if ( registrationInstance.length > 0  && registrationInstance.val() ) {
     result = result + ' in registration instance ""' + registrationInstance.text() + '""';
  } else {
    var registrationTemplate = $('.js-registration-template option:selected', $content).text();
    result = result + ' in any registration instance of template ""' + registrationTemplate + '""';
  }

  return result;
}
";
        }

        /// <summary>
        /// Formats the selection. 1 is the template, 2 is the registration instance, 3 is the type (registrar or registrant)
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override string FormatSelection( Type entityType, string selection )
        {
            string result = "Registrant";

            string[] selectionValues = selection.Split( '|' );

            if ( selectionValues.Length >= 3 )
            {
                var registrationType = selectionValues[2].AsIntegerOrNull();
                if ( registrationType == 1 )
                {
                    result = "Registrar";
                }

                var registrationInstanceGuid = selectionValues[1].AsGuid();
                var registrationInstance = new RegistrationInstanceService( new RockContext() ).Queryable().Where( a => a.Guid == registrationInstanceGuid ).FirstOrDefault();
                if ( registrationInstance != null )
                {
                    return string.Format( "{0} in registration instance '{1}'", result, registrationInstance.Name );
                }
                else
                {
                    var registrationTemplateId = selectionValues[0].AsIntegerOrNull() ?? 0;
                    var registrationTemplate = new RegistrationTemplateService( new RockContext() ).Queryable().Where( t => t.Id == registrationTemplateId ).FirstOrDefault();
                    return string.Format( "{0} in any registration instance of template '{1}'", result, registrationTemplate.Name );
                }
            }

            return result;
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            var controls = new List<Control>();
            
            // Add the registration template picker
            int? selectedTemplateId = null;
            if ( _ddlRegistrationTemplate != null )
            {
                selectedTemplateId = _ddlRegistrationTemplate.SelectedValueAsId();
            }

            _ddlRegistrationTemplate = new RockDropDownList();
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
            _ddlRegistrationTemplate.SelectedValue = selectedTemplateId.ToStringSafe();
            filterControl.Controls.Add( _ddlRegistrationTemplate );

            // Now add the registration instance picker
            _ddlRegistrationInstance = new RockDropDownList();
            _ddlRegistrationInstance.CssClass = "js-registration-instance";
            _ddlRegistrationInstance.Label = "Registration Instance";
            _ddlRegistrationInstance.ID = filterControl.ID + "_ddlRegistrationInstance";
            filterControl.Controls.Add( _ddlRegistrationInstance );

            PopulateRegistrationInstanceList( _ddlRegistrationTemplate.SelectedValueAsId () ?? 0  );

            _rblRegistrationType = new RockRadioButtonList();
            _rblRegistrationType.CssClass = "js-registration-type";
            _rblRegistrationType.ID = filterControl.ID + "_registrationType";
            _rblRegistrationType.RepeatDirection = RepeatDirection.Horizontal;
            _rblRegistrationType.Label = "Person";
            _rblRegistrationType.Help = "Choose whether to filter by the person who did the registering (registrar) or the person who was registered (registrant).";
            _rblRegistrationType.Items.Add( new ListItem( "Registrar", "1" ) );
            _rblRegistrationType.Items.Add( new ListItem( "Registrant", "2" ) );
            _rblRegistrationType.SelectedValue = "2";
            filterControl.Controls.Add( _rblRegistrationType );

            return new Control[3] { _ddlRegistrationTemplate, _ddlRegistrationInstance, _rblRegistrationType };
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlRegistrationTemplate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlRegistrationTemplate_SelectedIndexChanged( object sender, EventArgs e )
        {
            int registrationTemplateId = _ddlRegistrationTemplate.SelectedValueAsId() ?? 0;
            PopulateRegistrationInstanceList( registrationTemplateId );
        }

        /// <summary>
        /// Populates the registration instance list.
        /// </summary>
        /// <param name="registrationTemplateId">The registration template identifier.</param>
        private void PopulateRegistrationInstanceList( int registrationTemplateId )
        {
            if ( registrationTemplateId != 0  )
            {
                _ddlRegistrationInstance.Items.Clear();
                _ddlRegistrationInstance.Items.Add( new ListItem( "- Any -", "" ) );
                foreach ( var item in new RegistrationInstanceService( new RockContext() ).Queryable().Where( r => r.RegistrationTemplateId == registrationTemplateId  ).OrderBy( r => r.Name ) )
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
            var ddlRegistrationTemplate = ( controls[0] as RockDropDownList );
            var ddlRegistrationInstance = ( controls[1] as RockDropDownList );
            var rblRegistrationType = ( controls[2] as RockRadioButtonList );

            return string.Format( "{0}|{1}|{2}", ddlRegistrationTemplate.SelectedValue, ddlRegistrationInstance.SelectedValue, rblRegistrationType.SelectedValue );
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
                int registrationTemplateId = selectionValues[0].AsInteger();
                var registrationTemplate = new RegistrationTemplateService( new RockContext() ).Get( registrationTemplateId );
                if ( registrationTemplate  != null )
                {
                    ( controls[0] as RockDropDownList ).SetValue( registrationTemplateId );
                }

                ddlRegistrationTemplate_SelectedIndexChanged( this, new EventArgs() );

                var ddlRegistrationInstance = controls[1] as RockDropDownList;
                if ( selectionValues.Length >= 2 )
                {
                    ddlRegistrationInstance.SetValue( selectionValues[1] );
                }
                else
                {
                    ddlRegistrationInstance.SetValue( string.Empty );
                }

                var rblRegistrationType = controls[2] as RockRadioButtonList;
                if ( selectionValues.Length >= 3 )
                {
                    rblRegistrationType.SetValue( selectionValues[2] );
                }
                else
                {
                    rblRegistrationType.SetValue( "2" );
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
            if ( selectionValues.Length >= 3 )
            {
                int? registrationTemplateId = selectionValues[0].AsIntegerOrNull();
                Guid? registrationInstanceGuid = selectionValues[1].AsGuidOrNull();
                var registrationType = selectionValues[2];

                var rockContext = (RockContext) serviceInstance.Context;

                IQueryable<RegistrationRegistrant> registrantQuery;
                IQueryable<Registration> registrationQuery;
                IQueryable<Rock.Model.Person> qry;

                if ( registrationTemplateId == null )
                {
                    // no registration template id selected, so return nothing
                    return Expression.Constant( false );
                }

                // Registrant
                if ( registrationType == null || registrationType == "2" )
                {
                    registrantQuery = new RegistrationRegistrantService( rockContext ).Queryable()
                        .Where( r => r.Registration.RegistrationInstance.RegistrationTemplateId == registrationTemplateId );

                    if ( registrationInstanceGuid != null )
                    {
                        registrantQuery = registrantQuery.Where( r => r.Registration.RegistrationInstance.Guid == registrationInstanceGuid );
                    }

                    qry = new PersonService( rockContext ).Queryable()
                        .Where( p => registrantQuery.Where( xx => xx.PersonAlias.PersonId == p.Id ).Count() >= 1 );
                }
                // Registrar 
                else
                {
                    registrationQuery = new RegistrationService( rockContext ).Queryable()
                        .Where( r => r.RegistrationInstance.RegistrationTemplateId == registrationTemplateId );

                    if ( registrationInstanceGuid != null )
                    {
                        registrationQuery = registrationQuery.Where( r => r.RegistrationInstance.Guid == registrationInstanceGuid );
                    }
                    
                    qry = new PersonService( rockContext ).Queryable()
                        .Where( p => registrationQuery.Where( xx => xx.PersonAlias.PersonId == p.Id ).Count() >= 1 );
                }
                
                Expression result = FilterExpressionExtractor.Extract<Rock.Model.Person>( qry, parameterExpression, "p" );

                return result;
            }

            return null;
        }

        #endregion
    }
}