// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Workflow;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Control used by RegistrationDetail block to edit a registration's registrant
    /// </summary>
    [ToolboxData( "<{0}:RegistrantEditor runat=server></{0}:RegistrantEditor>" )]
    public class RegistrantEditor : CompositeControl, IHasValidationGroup
    {

        #region Private Fields/Controls

        private HiddenField _hfExpanded;
        private HiddenFieldWithClass _hfTitle;

        private HighlightLabel _hlCost;
        private HiddenField _hfRegistrantGuid;
        private PersonPicker _ppRegistrant;
        private RockLiteral _lCost;
        private CurrencyBox _curCost;

        private LinkButton _lbEditRegistrant;
        private LinkButton _lbDeleteRegistrant;
        private LinkButton _lbSaveRegistrant;
        private LinkButton _lbCancelRegistrant;

        private bool EditMode
        {
            get { return ViewState["EditMode"] as bool? ?? false; }
            set { ViewState["EditMode"] = value; }
        }

        #endregion

        #region Properties
        
        /// <summary>
        /// Gets or sets the forms.
        /// </summary>
        /// <value>
        /// The forms.
        /// </value>
        private List<RegistrantEditorForm> Forms
        {
            get 
            {
                return ViewState["Forms"] as List<RegistrantEditorForm>;
            }
            set 
            { 
                ViewState["Forms"] = value;
                RecreateChildControls();
            }
        }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public string Title
        {
            get
            {
                EnsureChildControls();
                return HttpUtility.HtmlDecode( _hfTitle.Value );
            }
            set
            {
                EnsureChildControls();
                _hfTitle.Value = HttpUtility.HtmlEncode( value );
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="RegistrantEditor"/> is expanded.
        /// </summary>
        /// <value>
        ///   <c>true</c> if expanded; otherwise, <c>false</c>.
        /// </value>
        public bool Expanded
        {
            get
            {
                EnsureChildControls();

                bool expanded = false;
                if ( !bool.TryParse( _hfExpanded.Value, out expanded ) )
                {
                    expanded = false;
                }

                return expanded;
            }

            set
            {
                EnsureChildControls();
                _hfExpanded.Value = value.ToString();
            }
        }

        /// <summary>
        /// Gets or sets the type of the cost label.
        /// </summary>
        /// <value>
        /// The type of the cost label.
        /// </value>
        public LabelType CostLabelType
        {
            get
            {
                EnsureChildControls();
                return _hlCost.LabelType;
            }

            set
            {
                EnsureChildControls();
                _hlCost.LabelType = value;
            }
        }

        /// <summary>
        /// Gets or sets the validation group.
        /// </summary>
        /// <value>
        /// The validation group.
        /// </value>
        public string ValidationGroup
        {
            get
            {
                return ViewState["ValidationGroup"] as string;
            }
            set
            {
                ViewState["ValidationGroup"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the activity type unique identifier.
        /// </summary>
        /// <value>
        /// The activity type unique identifier.
        /// </value>
        public Guid RegistrantGuid
        {
            get
            {
                EnsureChildControls();
                return _hfRegistrantGuid.Value.AsGuid();
            }
        }

        /// <summary>
        /// Gets the person identifier.
        /// </summary>
        /// <value>
        /// The person identifier.
        /// </value>
        public int? PersonId
        {
            get
            {
                EnsureChildControls();
                return _ppRegistrant.PersonId;
            }
            set
            {
                EnsureChildControls();
                _ppRegistrant.PersonId = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the person.
        /// </summary>
        /// <value>
        /// The name of the person.
        /// </value>
        public string PersonName
        {
            get
            {
                EnsureChildControls();
                return _ppRegistrant.PersonName;
            }
            set
            {
                EnsureChildControls();
                _ppRegistrant.PersonName = value;
            }
        }

        /// <summary>
        /// Gets or sets the cost.
        /// </summary>
        /// <value>
        /// The cost.
        /// </value>
        public decimal Cost
        {
            get
            {
                EnsureChildControls();
                return _curCost.Text.AsDecimal();
            }
            set
            {
                EnsureChildControls();
                _curCost.Text = value.ToString();
            }
        }

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            string script = @"
// activity animation
$('.rock-registrant-editor > header').click(function () {
    $(this).siblings('.panel-body').slideToggle();

    if ( $(this).find('.js-header-controls').length ) {
        $(this).find('.js-header-title').slideToggle();
        $(this).find('.js-header-controls').slideToggle();
    }

    $expanded = $(this).children('input.filter-expanded');
    $expanded.val($expanded.val() == 'True' ? 'False' : 'True');

    $('a.view-state > i', this).toggleClass('fa-chevron-down');
    $('a.view-state > i', this).toggleClass('fa-chevron-up');
});

// fix so that certain controls will fire its event, but not the parent event 
$('.js-stop-immediate-propagation').click(function (event) {
    event.stopImmediatePropagation();
});

";
            ScriptManager.RegisterStartupScript( this, typeof( RegistrantEditor ), "RegistrantEditor", script, true );
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Controls.Clear();

            _hfExpanded = new HiddenField();
            _hfExpanded.ID = "_hfExpanded";
            _hfExpanded.Value = "False";
            Controls.Add( _hfExpanded );

            _hfTitle = new HiddenFieldWithClass();
            _hfTitle.ValidateRequestMode = System.Web.UI.ValidateRequestMode.Disabled;
            _hfTitle.ID = "_hfTitle";
            _hfTitle.CssClass = "js-header-title-hidden";
            Controls.Add( _hfTitle );

            _hfRegistrantGuid = new HiddenField();
            _hfRegistrantGuid.ID = this.ID + "_hfRegistrantGuid";
            Controls.Add( _hfRegistrantGuid );

            _hlCost = new HighlightLabel();
            _hlCost.ID = this.ID + "_hlCost";
            _hlCost.LabelType = LabelType.Info;
            Controls.Add( _hlCost );

            _ppRegistrant = new PersonPicker();
            _ppRegistrant.ID = this.ID + "_ppRegistrant";
            _ppRegistrant.Label = "Person";
            _ppRegistrant.Required = true;
            _ppRegistrant.SelectPerson += _ppRegistrant_SelectPerson;
            Controls.Add( _ppRegistrant );

            _lCost = new RockLiteral();
            _lCost.ID = this.ID + "_lCost";
            _lCost.Label = "Cost";
            Controls.Add( _lCost );

            _curCost = new CurrencyBox();
            _curCost.ID = this.ID + "_curCost";
            _curCost.Label = "Cost";
            Controls.Add( _curCost );

            _lbEditRegistrant = new LinkButton();
            _lbEditRegistrant.CausesValidation = false;
            _lbEditRegistrant.ID = this.ID + "_lbEditRegistrant";
            _lbEditRegistrant.Text = "Edit";
            _lbEditRegistrant.CssClass = "btn btn-primary js-action-edit";
            _lbEditRegistrant.Click += lbEditRegistrant_Click;
            Controls.Add( _lbEditRegistrant );

            _lbDeleteRegistrant = new LinkButton();
            _lbDeleteRegistrant.CausesValidation = false;
            _lbDeleteRegistrant.ID = this.ID + "_lbDeleteRegistrant";
            _lbDeleteRegistrant.Text = "Delete";
            _lbDeleteRegistrant.CssClass = "btn btn-link js-action-delete";
            _lbDeleteRegistrant.Click += lbDeleteRegistrant_Click;
            Controls.Add( _lbDeleteRegistrant );

            _lbSaveRegistrant = new LinkButton();
            _lbSaveRegistrant.CausesValidation = true;
            _lbSaveRegistrant.ID = this.ID + "_lbSaveRegistrant";
            _lbSaveRegistrant.Text = "Save";
            _lbSaveRegistrant.CssClass = "btn btn-primary js-action-save";
            _lbSaveRegistrant.Click += lbSaveRegistrant_Click;
            Controls.Add( _lbSaveRegistrant );

            _lbCancelRegistrant = new LinkButton();
            _lbCancelRegistrant.CausesValidation = false;
            _lbCancelRegistrant.ID = this.ID + "_lbCancelRegistrant";
            _lbCancelRegistrant.Text = "Cancel";
            _lbCancelRegistrant.CssClass = "btn btn-link js-action-cancel";
            _lbCancelRegistrant.Click += lbCancelRegistrant_Click;
            Controls.Add( _lbCancelRegistrant );

            if ( Forms != null )
            {
                foreach ( var form in Forms )
                {
                    form.Controls.Clear();

                    foreach ( var field in form.Fields )
                    {
                        if ( field.FieldSource == RegistrationFieldSource.PersonField )
                        {
                            var rlField = new RockLiteral();
                            rlField.ID = this.ID + "_rl" + field.PersonFieldType.ConvertToString( false );
                            rlField.Label = field.PersonFieldType.ConvertToString( true );
                            Controls.Add( rlField );
                            form.Controls.Add( rlField );
                        }
                        else
                        {
                            if ( field.AttributeId.HasValue )
                            {
                                var attribute = AttributeCache.Read( field.AttributeId.Value );
                                if ( attribute != null )
                                {
                                    if ( field.FieldSource == RegistrationFieldSource.RegistrationAttribute )
                                    {
                                        var control = attribute.AddControl( Controls, string.Empty, this.ValidationGroup, false, true, field.IsRequired );
                                        form.Controls.Add( control );
                                    }
                                    else
                                    {
                                        var rlField = new RockLiteral();
                                        rlField.ID = this.ID + "_rl" + attribute.Id.ToString();
                                        rlField.Label = attribute.Name;
                                        Controls.Add( rlField );
                                        form.Controls.Add( rlField );
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                // Section
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "panel panel-widget rock-registrant-editor " + CssClass );
                writer.AddAttribute( HtmlTextWriterAttribute.Id, this.ClientID );
                writer.RenderBeginTag( "section" );

                // Header
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "panel-heading clearfix" );
                writer.RenderBeginTag( "header" );

                // Hidden Field to track expansion
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "filter-expanded" );
                _hfExpanded.RenderControl( writer );

                /* Begin - Title and header controls */
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "pull-left" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                // Hidden Field to track Title
                _hfTitle.RenderControl( writer );

                // Title
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "js-header-title" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "fa fa-person" );
                writer.RenderBeginTag( HtmlTextWriterTag.I );
                writer.RenderEndTag();
                writer.Write( " " );
                writer.Write( Title );
                writer.RenderEndTag();  

                writer.RenderEndTag();  // pull-left

                // Panel Controls
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "pull-right" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                _hlCost.RenderControl( writer );

                // Chevron up/down Button
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "btn btn-link btn-xs view-state" );
                writer.RenderBeginTag( HtmlTextWriterTag.A );
                writer.AddAttribute( HtmlTextWriterAttribute.Class, Expanded ? "fa fa-chevron-up" : "fa fa-chevron-down" );
                writer.RenderBeginTag( HtmlTextWriterTag.I );
                writer.RenderEndTag();
                writer.RenderEndTag();

                writer.RenderEndTag(); // pull-right

                writer.RenderEndTag(); // Header                

                // Body
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "panel-body" );
                if ( !Expanded )
                {
                    writer.AddStyleAttribute( HtmlTextWriterStyle.Display, "none" );
                }
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                RenderRegistrant( writer );

                writer.RenderEndTag();

                writer.RenderEndTag();  // Section
            }
        }

        /// <summary>
        /// Renders the registrant.
        /// </summary>
        /// <param name="writer">The writer.</param>
        private void RenderRegistrant ( HtmlTextWriter writer )
        {
            _hfRegistrantGuid.RenderControl( writer );

            if ( EditMode )
            {
                _ppRegistrant.RenderControl( writer );
                _curCost.RenderControl( writer );
            }
            else
            {
                _lCost.Text = Cost.ToString( "C2" );
                _lCost.RenderControl( writer );
            }

            if ( Forms != null )
            {
                foreach ( var form in Forms )
                {
                    RenderForms( form, writer );
                }
            }

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "actions" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            _lbEditRegistrant.Visible = !EditMode;
            _lbEditRegistrant.RenderControl( writer );

            _lbDeleteRegistrant.Visible = !EditMode;
            _lbDeleteRegistrant.RenderControl( writer );

            _lbSaveRegistrant.Visible = EditMode;
            _lbSaveRegistrant.RenderControl( writer );

            _lbCancelRegistrant.Visible = EditMode;
            _lbCancelRegistrant.RenderControl( writer );

            writer.RenderEndTag();
        }

        private void RenderForms( RegistrantEditorForm form,  HtmlTextWriter writer )
        {
            // panel
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "panel panel-block" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            // Header
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "panel-heading clearfix" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "panel-title pull-left" );
            writer.RenderBeginTag( HtmlTextWriterTag.H1 );
            writer.Write( form.Name );
            writer.RenderEndTag();  // H1.panel-title
            writer.RenderEndTag();  // Div.panel-heading

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "panel-body" );
            writer.RenderBeginTag( HtmlTextWriterTag.H1 );

            foreach( var control in form.Controls)
            {
                control.RenderControl( writer );
            }

            writer.RenderEndTag();  // Div.panel-body

            writer.RenderEndTag();  // Div.panel-block
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the type of the workflow action.
        /// </summary>
        /// <returns></returns>
        public void SetRegistrantFromControl( RegistrationRegistrant registrant )
        {
            EnsureChildControls();

            if ( registrant != null )
            {
                registrant.Cost = _curCost.Text.AsDecimal();

                if ( Forms != null && registrant.PersonAlias != null && registrant.PersonAlias.Person != null )
                {
                    // Get registrant attribute field values
                    if ( registrant.Attributes == null )
                    {
                        registrant.LoadAttributes();
                    }

                    foreach ( var field in Forms
                        .SelectMany( f => f.Fields
                            .Where( a => a.FieldSource == RegistrationFieldSource.RegistrationAttribute ) ) )
                    {
                        var attribute = AttributeCache.Read( field.AttributeId.Value );
                        var control = FindControl( string.Format( "attribute_field_{0}", attribute.Id ) );
                        if ( attribute != null && control != null )
                        {
                            registrant.SetAttributeValue( attribute.Key, attribute.FieldType.Field.GetEditValue( control, attribute.QualifierValues ) );
                        }
                    }
                }

            }

        }

        /// <summary>
        /// Sets the type of the workflow action.
        /// </summary>
        /// <param name="value">The value.</param>
        public void SetControlFromRegistrant( RegistrationRegistrant value, RockContext rockContext )
        {
            EnsureChildControls();

            if ( value != null )
            {
                this.Title = value.ToString();

                _hlCost.Text = value.CostWithFees.ToString( "C2" );
                _hfRegistrantGuid.Value = value.Guid.ToString();
                if ( value.PersonAlias != null )
                {
                    _ppRegistrant.SetValue( value.PersonAlias.Person );
                    this.Title = value.PersonAlias.Person.ToString();
                }
                else
                {
                    _ppRegistrant.SetValue( null );
                    this.Title = "New Registrant";
                }

                _curCost.Text = value.Cost.ToString();

                Person person = null;
                if ( value.PersonAlias != null )
                {
                    person = value.PersonAlias.Person;
                }

                if ( Forms != null && person != null )
                {
                    // Set the values of the person field controls
                    foreach ( var field in Forms
                        .SelectMany( f => f.Fields
                            .Where( a => a.FieldSource == RegistrationFieldSource.PersonField ) ) )
                    {
                        var literalControl = FindControl( this.ID + "_rl" + field.PersonFieldType.ConvertToString( false ) ) as RockLiteral;

                        if ( literalControl != null )
                        {
                            literalControl.Text = string.Empty;

                            DefinedValueCache dvPhone = null;

                            switch ( field.PersonFieldType )
                            {
                                case RegistrationPersonFieldType.FirstName: literalControl.Text = person.NickName; break;
                                case RegistrationPersonFieldType.LastName: literalControl.Text = person.LastName; break;
                                case RegistrationPersonFieldType.Campus:
                                    {
                                        if ( person != null )
                                        {
                                            var family = person.GetFamilies( rockContext ).FirstOrDefault();
                                            if ( family != null && family.Campus != null )
                                            {
                                                literalControl.Text = family.Campus.Name;
                                            }
                                        }
                                        break;
                                    }
                                case RegistrationPersonFieldType.Address:
                                    {
                                        var location = person.GetHomeLocation( rockContext );
                                        if ( location != null )
                                        {
                                            literalControl.Text = location.ToStringSafe();
                                        }
                                        break;
                                    }
                                case RegistrationPersonFieldType.Email: literalControl.Text = person.Email; break;
                                case RegistrationPersonFieldType.Birthdate: literalControl.Text = person.BirthDate.HasValue ? person.BirthDate.Value.ToShortDateString() : string.Empty; break;
                                case RegistrationPersonFieldType.Gender: literalControl.Text = person.Gender.ConvertToString(); break;
                                case RegistrationPersonFieldType.MaritalStatus: literalControl.Text = person.MaritalStatusValue != null ? person.MaritalStatusValue.Value : string.Empty; break;
                                case RegistrationPersonFieldType.MobilePhone:
                                    {
                                        dvPhone = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE );
                                        break;
                                    }
                                case RegistrationPersonFieldType.HomePhone:
                                    {
                                        dvPhone = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME );
                                        break;
                                    }
                                case RegistrationPersonFieldType.WorkPhone:
                                    {
                                        dvPhone = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK );
                                        break;
                                    }
                            }

                            if ( dvPhone != null )
                            {
                                var phoneNumber = new PersonService( rockContext ).GetPhoneNumber( person, dvPhone );
                                if ( phoneNumber != null )
                                {
                                    literalControl.Text = phoneNumber.NumberFormatted;
                                }
                            }
                        }
                    }

                    // Set the values of the person attribute controls
                    var personAttributeFields = Forms
                        .SelectMany( f => f.Fields
                        .Where( a =>
                            a.FieldSource == RegistrationFieldSource.PersonAttribute &&
                            a.AttributeId.HasValue ) );
                    {
                        if ( personAttributeFields.Any() )
                        {
                            if ( person.Attributes == null )
                            {
                                person.LoadAttributes( rockContext );
                            }

                            foreach ( var field in personAttributeFields )
                            {
                                var attribute = AttributeCache.Read( field.AttributeId.Value );
                                var literalControl = FindControl( this.ID + "_rl" + field.AttributeId.Value.ToString() ) as RockLiteral;
                                if ( attribute != null && literalControl != null )
                                {
                                    literalControl.Text = attribute.FieldType.Field.FormatValueAsHtml( null, person.GetAttributeValue( attribute.Key ), attribute.QualifierValues );
                                }
                            }
                        }
                    }

                    // Set the values of the person attribute controls
                    var groupMemberAttributeFields = Forms
                        .SelectMany( f => f.Fields
                        .Where( a =>
                            a.FieldSource == RegistrationFieldSource.GroupMemberAttribute &&
                            a.AttributeId.HasValue ) );
                    {
                        if ( groupMemberAttributeFields.Any() && value.GroupMember != null )
                        {
                            if ( value.GroupMember.Attributes == null )
                            {
                                value.GroupMember.LoadAttributes( rockContext );
                            }

                            foreach ( var field in groupMemberAttributeFields )
                            {
                                var attribute = AttributeCache.Read( field.AttributeId.Value );
                                var literalControl = FindControl( this.ID + "_rl" + field.AttributeId.Value.ToString() ) as RockLiteral;
                                if ( attribute != null && literalControl != null )
                                {
                                    literalControl.Text = attribute.FieldType.Field.FormatValueAsHtml( null, value.GroupMember.GetAttributeValue( attribute.Key ), attribute.QualifierValues );
                                }
                            }
                        }
                    }

                    // Set the values of the person attribute controls
                    var registrantAttributeFields = Forms
                        .SelectMany( f => f.Fields
                        .Where( a =>
                            a.FieldSource == RegistrationFieldSource.RegistrationAttribute &&
                            a.AttributeId.HasValue ) );
                    {
                        if ( registrantAttributeFields.Any() )
                        {
                            if ( value.Attributes == null )
                            {
                                value.LoadAttributes();
                            }

                            foreach ( var field in registrantAttributeFields )
                            {
                                var attribute = AttributeCache.Read( field.AttributeId.Value );
                                var control = FindControl( string.Format( "attribute_field_{0}", attribute.Id ) );
                                if ( attribute != null && control != null )
                                {
                                    attribute.FieldType.Field.SetEditValue( control, attribute.QualifierValues, value.GroupMember.GetAttributeValue( attribute.Key ) );
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sets the forms.
        /// </summary>
        /// <param name="template">The template.</param>
        public void SetForms( RegistrationTemplate template )
        {
            var forms = new List<RegistrantEditorForm>();
            
            if ( template != null && template.Forms != null && template.Forms.Any() )
            {
                foreach ( var form in template.Forms )
                {
                    forms.Add( new RegistrantEditorForm( form ) );
                }
            }

            this.Forms = forms;
        }

        /// <summary>
        /// Shows the edit mode.
        /// </summary>
        public void ShowEditMode()
        {
            EditMode = true;
        }

        /// <summary>
        /// Shows the view mode.
        /// </summary>
        public void ShowViewMode()
        {
            EditMode = false;
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the SelectPerson event of the _ppRegistrant control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void _ppRegistrant_SelectPerson( object sender, EventArgs e )
        {
            if ( SelectPersonClick != null )
            {
                SelectPersonClick( this, e );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbEditRegistrant control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEditRegistrant_Click( object sender, EventArgs e )
        {
            if ( EditRegistrantClick != null )
            {
                EditRegistrantClick( this, e );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbDeleteRegistrant control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbDeleteRegistrant_Click( object sender, EventArgs e )
        {
            if ( DeleteRegistrantClick != null )
            {
                DeleteRegistrantClick( this, e );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbSaveRegistrant control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSaveRegistrant_Click( object sender, EventArgs e )
        {
            if ( SaveRegistrantClick != null )
            {
                SaveRegistrantClick( this, e );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbCancelRegistrant control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCancelRegistrant_Click( object sender, EventArgs e )
        {
            if ( CancelRegistrantClick != null )
            {
                CancelRegistrantClick( this, e );
            }
        }

        /// <summary>
        /// Occurs when [select person click].
        /// </summary>
        public event EventHandler SelectPersonClick;

        /// <summary>
        /// Occurs when [edit ristrant click].
        /// </summary>
        public event EventHandler EditRegistrantClick;

        /// <summary>
        /// Occurs when [delete registrant click].
        /// </summary>
        public event EventHandler DeleteRegistrantClick;

        /// <summary>
        /// Occurs when [save registrant click].
        /// </summary>
        public event EventHandler SaveRegistrantClick;

        /// <summary>
        /// Occurs when [cancel registrant click].
        /// </summary>
        public event EventHandler CancelRegistrantClick;

        #endregion

    }

    #region Helper Classes

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class RegistrantEditorForm
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the fields.
        /// </summary>
        /// <value>
        /// The fields.
        /// </value>
        public List<RegistrantEditorFormField> Fields { get; set; }

        public List<Control> Controls { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrantEditorForm"/> class.
        /// </summary>
        public RegistrantEditorForm()
        {
            Fields = new List<RegistrantEditorFormField>();
            Controls = new List<Control>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrantEditorForm"/> class.
        /// </summary>
        /// <param name="form">The form.</param>
        public RegistrantEditorForm( RegistrationTemplateForm form ) : this()
        {
            Name = form.Name;
            Order = form.Order;
            form.Fields.ToList()
                .ForEach( f => Fields.Add( new RegistrantEditorFormField( f ) ) );
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class RegistrantEditorFormField
    {
        /// <summary>
        /// Gets or sets the field source.
        /// </summary>
        /// <value>
        /// The field source.
        /// </value>
        public RegistrationFieldSource FieldSource { get; set; }

        /// <summary>
        /// Gets or sets the type of the person field.
        /// </summary>
        /// <value>
        /// The type of the person field.
        /// </value>
        public RegistrationPersonFieldType PersonFieldType { get; set; }

        /// <summary>
        /// Gets or sets the attribute identifier.
        /// </summary>
        /// <value>
        /// The attribute identifier.
        /// </value>
        public int? AttributeId { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is required.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is required; otherwise, <c>false</c>.
        /// </value>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrantEditorFormField"/> class.
        /// </summary>
        public RegistrantEditorFormField()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrantEditorFormField"/> class.
        /// </summary>
        /// <param name="field">The field.</param>
        public RegistrantEditorFormField( RegistrationTemplateFormField field ) : this()
        {
            FieldSource = field.FieldSource;
            PersonFieldType = field.PersonFieldType;
            AttributeId = field.AttributeId;
            Order = field.Order;
            IsRequired = field.IsRequired;
        }
    }

    #endregion

}