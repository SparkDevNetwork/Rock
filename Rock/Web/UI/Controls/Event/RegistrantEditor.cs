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
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Workflow;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Control used by WorkflowTypeDetail block to edit a workflow action type
    /// </summary>
    [ToolboxData( "<{0}:RegistrantEditor runat=server></{0}:RegistrantEditor>" )]
    public class RegistrantEditor : PanelWidget, IHasValidationGroup
    {
        private HighlightLabel _hlCost;

        private HiddenField _hfRegistrantGuid;

        private PersonPicker _ppRegistrant;

        private RockLiteral _lCost;
        private CurrencyBox _curCost;

        private PlaceHolder _phRegistrantAttributes;

        private LinkButton _lbEditRegistrant;
        private LinkButton _lbDeleteRegistrant;
        private LinkButton _lbSaveRegistrant;
        private LinkButton _lbCancelRegistrant;

        private bool EditMode
        {
            get { return ViewState["EditMode"] as bool? ?? false; }
            set { ViewState["EditMode"] = value; }
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

        /// <summary>
        /// Gets the type of the workflow action.
        /// </summary>
        /// <returns></returns>
        public RegistrationRegistrant GetRegistrant()
        {
            EnsureChildControls();

            RegistrationRegistrant result = new RegistrationRegistrant();
            result.Guid = new Guid( _hfRegistrantGuid.Value );
            result.PersonAliasId = _ppRegistrant.PersonAliasId;;
            result.Cost = _curCost.Text.AsDecimal();

            result.LoadAttributes();

            Rock.Attribute.Helper.GetEditValues( _phRegistrantAttributes, result );

            return result;
        }

        /// <summary>
        /// Sets the type of the workflow action.
        /// </summary>
        /// <param name="value">The value.</param>
        public void SetRegistrant(RegistrationRegistrant value )
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
                _phRegistrantAttributes.Controls.Clear();
                Rock.Attribute.Helper.AddEditControls( value, _phRegistrantAttributes, true, ValidationGroup );
            }
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

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            _hfRegistrantGuid = new HiddenField();
            Controls.Add( _hfRegistrantGuid );
            _hfRegistrantGuid.ID = this.ID + "_hfRegistrantGuid";

            _hlCost = new HighlightLabel();
            Controls.Add( _hlCost );
            _hlCost.ID = this.ID + "_hlCost";
            _hlCost.LabelType = LabelType.Success;

            _ppRegistrant = new PersonPicker();
            Controls.Add( _ppRegistrant );
            _ppRegistrant.ID = this.ID + "_ppRegistrant";
            _ppRegistrant.Label = "Person";
            _ppRegistrant.Required = true;
            _ppRegistrant.SelectPerson += _ppRegistrant_SelectPerson;

            _lCost = new RockLiteral();
            Controls.Add( _lCost );
            _lCost.ID = this.ID + "_lCost";
            _lCost.Label = "Cost";

            _curCost = new CurrencyBox();
            Controls.Add( _curCost );
            _curCost.ID = this.ID + "_curCost";
            _curCost.Label = "Cost";

            _phRegistrantAttributes = new PlaceHolder();
            Controls.Add( _phRegistrantAttributes );
            _phRegistrantAttributes.ID = this.ID + "_phActionAttributes";

            _lbEditRegistrant = new LinkButton();
            Controls.Add( _lbEditRegistrant );
            _lbEditRegistrant.CausesValidation = false;
            _lbEditRegistrant.ID = this.ID + "_lbEditRegistrant";
            _lbEditRegistrant.Text = "Edit";
            _lbEditRegistrant.CssClass = "btn btn-primary js-action-edit";
            _lbEditRegistrant.Click += lbEditRegistrant_Click;

            _lbDeleteRegistrant = new LinkButton();
            Controls.Add( _lbDeleteRegistrant );
            _lbDeleteRegistrant.CausesValidation = false;
            _lbDeleteRegistrant.ID = this.ID + "_lbDeleteRegistrant";
            _lbDeleteRegistrant.Text = "Delete";
            _lbDeleteRegistrant.CssClass = "btn btn-link js-action-delete";
            _lbDeleteRegistrant.Click += lbDeleteRegistrant_Click;

            _lbSaveRegistrant = new LinkButton();
            Controls.Add( _lbSaveRegistrant );
            _lbSaveRegistrant.CausesValidation = true;
            _lbSaveRegistrant.ID = this.ID + "_lbSaveRegistrant";
            _lbSaveRegistrant.Text = "Save";
            _lbSaveRegistrant.CssClass = "btn btn-primary js-action-save";
            _lbSaveRegistrant.Click += lbSaveRegistrant_Click;

            _lbCancelRegistrant = new LinkButton();
            Controls.Add( _lbCancelRegistrant );
            _lbCancelRegistrant.CausesValidation = false;
            _lbCancelRegistrant.ID = this.ID + "_lbCancelRegistrant";
            _lbCancelRegistrant.Text = "Cancel";
            _lbCancelRegistrant.CssClass = "btn btn-link js-action-cancel";
            _lbCancelRegistrant.Click += lbCancelRegistrant_Click;
        }

        /// <summary>
        /// Writes the <see cref="T:System.Web.UI.WebControls.CompositeControl" /> content to the specified <see cref="T:System.Web.UI.HtmlTextWriter" /> object, for display on the client.
        /// </summary>
        /// <param name="writer">An <see cref="T:System.Web.UI.HtmlTextWriter" /> that represents the output stream to render HTML content on the client.</param>
        protected override void RenderChildControls( HtmlTextWriter writer )
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

        /// <summary>
        /// Renders the labels.
        /// </summary>
        /// <param name="writer">The writer.</param>
        protected override void RenderLabels( HtmlTextWriter writer )
        {
            _hlCost.RenderControl( writer );
        }

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

    }
}