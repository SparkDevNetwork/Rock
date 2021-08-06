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
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Field;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Workflow Action Form Row Editor
    /// </summary>
    public class WorkflowFormAttributeRow : CompositeControl
    {
        private HiddenField _hfOrder;
        private CheckBox _cbVisible;
        private CheckBox _cbEditable;
        private CheckBox _cbRequired;
        private CheckBox _cbHideLabel;
        private CheckBox _cbPreHtml;
        private CheckBox _cbPostHtml;
        private RockTextBox _tbPreHtml;
        private RockTextBox _tbPostHtml;
        private LinkButton _lbFilter;

        /// <summary>
        /// Gets or sets the attribute unique identifier.
        /// </summary>
        /// <value>
        /// The attribute unique identifier.
        /// </value>
        public Guid AttributeGuid
        {
            get { return ViewState["AttributeGuid"] as Guid? ?? Guid.Empty; }
            set { ViewState["AttributeGuid"] = value; }
        }

        /// <summary>
        /// Gets or sets the name of the attribute.
        /// </summary>
        /// <value>
        /// The name of the attribute.
        /// </value>
        public string AttributeName
        {
            get { return ViewState["AttributeName"] as string ?? string.Empty; }
            set { ViewState["AttributeName"] = value; }
        }

        /// <summary>
        /// Gets or sets the attribute used by the row.
        /// </summary>
        /// <value>
        /// The attribute.
        /// </value>
        public Model.Attribute Attribute
        {
            get { return ViewState["Attribute"] as Model.Attribute; }
            set { ViewState["Attribute"] = value; }
        }

        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <value>
        /// The unique identifier.
        /// </value>
        public Guid Guid
        {
            get { return ViewState["Guid"] as Guid? ?? Guid.Empty; }
            set { ViewState["Guid"] = value; }
        }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        public int Order
        {
            get
            {
                EnsureChildControls();
                return _hfOrder.Value.AsInteger();
            }
            set
            {
                EnsureChildControls();
                _hfOrder.Value = value.ToString();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [is visible].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is visible]; otherwise, <c>false</c>.
        /// </value>
        public bool IsVisible
        {
            get
            {
                EnsureChildControls();
                return _cbVisible.Checked;
            }
            set
            {
                EnsureChildControls();
                _cbVisible.Checked = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [is editable].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is editable]; otherwise, <c>false</c>.
        /// </value>
        public bool IsEditable
        {
            get
            {
                EnsureChildControls();
                return _cbEditable.Checked;
            }
            set
            {
                EnsureChildControls();
                _cbEditable.Checked = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [is required].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is required]; otherwise, <c>false</c>.
        /// </value>
        public bool IsRequired
        {
            get
            {
                EnsureChildControls();
                return _cbRequired.Checked;
            }
            set
            {
                EnsureChildControls();
                _cbRequired.Checked = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [hide label].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [hide label]; otherwise, <c>false</c>.
        /// </value>
        public bool HideLabel
        {
            get
            {
                EnsureChildControls();
                return _cbHideLabel.Checked;
            }
            set
            {
                EnsureChildControls();
                _cbHideLabel.Checked = value;
            }
        }

        /// <summary>
        /// Gets or sets the pre HTML.
        /// </summary>
        /// <value>
        /// The pre HTML.
        /// </value>
        public string PreHtml
        {
            get
            {
                EnsureChildControls();
                return _tbPreHtml.Text;
            }
            set
            {
                EnsureChildControls();
                _tbPreHtml.Text = value;
                _cbPreHtml.Checked = !string.IsNullOrWhiteSpace( value );
            }
        }

        /// <summary>
        /// Gets or sets the post HTML.
        /// </summary>
        /// <value>
        /// The post HTML.
        /// </value>
        public string PostHtml
        {
            get
            {
                EnsureChildControls();
                return _tbPostHtml.Text;
            }
            set
            {
                EnsureChildControls();
                _tbPostHtml.Text = value;
                _cbPostHtml.Checked = !string.IsNullOrWhiteSpace( value );
            }
        }

        /// <summary>
        /// Gets or sets the visibility rules.
        /// </summary>
        /// <value>
        /// The visibility rules.
        /// </value>
        public FieldVisibilityRules VisibilityRules
        {
            get
            {
                EnsureChildControls();
                return ViewState["VisibilityRules"] as FieldVisibilityRules;
            }
            set
            {
                EnsureChildControls();
                ViewState["VisibilityRules"] = value;
                if ( value.RuleList.Count > 0 )
                {
                    _lbFilter.CssClass = "btn btn-warning btn-xs btn-square";
                }
                else
                {
                    _lbFilter.CssClass = "btn btn-default btn-xs btn-square";
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            string script = @"
    $('input.js-form-attribute-show-pre-html').on('change', function(){
        $(this).closest('td').find('div.js-form-attribute-pre-html').slideToggle();
    });
    $('input.js-form-attribute-show-post-html').on('change', function(){
        $(this).closest('td').find('div.js-form-attribute-post-html').slideToggle();
    });
";
            ScriptManager.RegisterStartupScript( this, this.GetType(), "wf-attribute-row-html", script, true );
        }
        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            Controls.Clear();

            _hfOrder = new HiddenField();
            _hfOrder.ID = this.ID + "_hfOrder";
            Controls.Add( _hfOrder );

            _cbVisible = new CheckBox();
            _cbVisible.ID = this.ID + "_cbVisible";
            Controls.Add( _cbVisible );

            _cbEditable = new CheckBox();
            _cbEditable.ID = this.ID + "_cbEditable";
            Controls.Add( _cbEditable );

            _cbRequired = new CheckBox();
            _cbRequired.ID = this.ID + "_cbRequired";
            Controls.Add( _cbRequired );

            _cbHideLabel = new CheckBox();
            _cbHideLabel.ID = this.ID + "_cbHideLabel";
            Controls.Add( _cbHideLabel );

            _cbPreHtml = new CheckBox();
            _cbPreHtml.ID = this.ID + "_cbPreHtml";
            _cbPreHtml.AddCssClass( "js-form-attribute-show-pre-html" );
            Controls.Add( _cbPreHtml );

            _cbPostHtml = new CheckBox();
            _cbPostHtml.ID = this.ID + "_cbPostHtml";
            _cbPostHtml.AddCssClass( "js-form-attribute-show-post-html" );
            Controls.Add( _cbPostHtml );

            _tbPreHtml = new RockTextBox();
            _tbPreHtml.ID = this.ID + "_tbPreHtml";
            _tbPreHtml.TextMode = TextBoxMode.MultiLine;
            _tbPreHtml.Rows = 3;
            _tbPreHtml.ValidateRequestMode = System.Web.UI.ValidateRequestMode.Disabled;
            Controls.Add( _tbPreHtml );

            _tbPostHtml = new RockTextBox();
            _tbPostHtml.ID = this.ID + "_tbPostHtml";
            _tbPostHtml.TextMode = TextBoxMode.MultiLine;
            _tbPostHtml.Rows = 3;
            _tbPostHtml.ValidateRequestMode = System.Web.UI.ValidateRequestMode.Disabled;
            Controls.Add( _tbPostHtml );

            _lbFilter = new LinkButton();
            _lbFilter.ID = $"{ID}{nameof( _lbFilter )}";
            _lbFilter.Text = "<i class=\"fa fa-filter\"></i>";
            _lbFilter.CssClass = "btn btn-default btn-xs btn-square";
            _lbFilter.Click += _lbFilter_Click;
            Controls.Add( _lbFilter );

        }

        /// <summary>
        /// Event args for when the filter button is clicked.
        /// </summary>
        /// <seealso cref="System.EventArgs" />
        public class FilterEventArgs : EventArgs
        {
            /// <summary>
            /// Gets or sets the workflow form attribute row.
            /// </summary>
            /// <value>
            /// The workflow form attribute row.
            /// </value>
            public WorkflowFormAttributeRow WorkflowFormAttributeRow { get; set; }
        }

        /// <summary>
        /// The filter click event.
        /// </summary>
        public EventHandler<FilterEventArgs> FilterClick;

        /// <summary>
        /// Handles the Click event of the _lbFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void _lbFilter_Click( object sender, EventArgs e )
        {
            if ( FilterClick != null)
            {
                FilterClick( sender, new FilterEventArgs { WorkflowFormAttributeRow = this } );
            }
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if (this.Visible)
            {
                writer.AddAttribute( "data-key", Guid.ToString() );
                writer.RenderBeginTag( HtmlTextWriterTag.Tr );

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "grid-columncommand" );
                writer.AddAttribute( HtmlTextWriterAttribute.Align, "center" );
                writer.RenderBeginTag( HtmlTextWriterTag.Td );
                _hfOrder.RenderControl( writer );
                writer.AddAttribute( HtmlTextWriterAttribute.Href, "#" );
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "minimal workflow-formfield-reorder" );
                writer.RenderBeginTag( HtmlTextWriterTag.A );
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "fa fa-bars" );
                writer.RenderBeginTag( HtmlTextWriterTag.I );
                writer.RenderEndTag();      // I
                writer.RenderEndTag();      // A
                writer.RenderEndTag();      // Td

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "px-0 py-3" );
                writer.RenderBeginTag( HtmlTextWriterTag.Td );

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "row mb-2 js-form-attribute-pre-html" );
                writer.AddAttribute( HtmlTextWriterAttribute.Style, "display:" + ( _cbPreHtml.Checked ? "block" : "none" ) );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-xs-12" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _tbPreHtml.RenderControl( writer );
                writer.RenderEndTag();
                writer.RenderEndTag();

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "row no-gutters d-flex align-items-center" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-xs-3" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                writer.Write( AttributeName );
                writer.RenderEndTag();

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-xs-9" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "d-flex" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "flex-eq grid-select-field" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _cbVisible.RenderControl( writer );
                writer.RenderEndTag();

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "flex-eq grid-select-field" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _cbEditable.RenderControl( writer );
                writer.RenderEndTag();

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "flex-eq grid-select-field" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _cbRequired.RenderControl( writer );
                writer.RenderEndTag();

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "flex-eq grid-select-field" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _cbHideLabel.RenderControl( writer );
                writer.RenderEndTag();

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "flex-eq grid-select-field" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _cbPreHtml.RenderControl( writer );
                writer.RenderEndTag();

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "flex-eq grid-select-field" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _cbPostHtml.RenderControl( writer );
                writer.RenderEndTag();

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "d-flex flex-eq justify-content-center grid-select-field" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _lbFilter.RenderControl( writer );
                writer.RenderEndTag();

                writer.RenderEndTag();      // row

                writer.RenderEndTag();      // col-xs-9

                writer.RenderEndTag();      // row

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "row mt-2 js-form-attribute-post-html" );
                writer.AddAttribute( HtmlTextWriterAttribute.Style, "display:" + ( _cbPostHtml.Checked ? "block" : "none" ) );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-xs-12" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _tbPostHtml.RenderControl( writer );
                writer.RenderEndTag();
                writer.RenderEndTag();

                writer.RenderEndTag();      // Td

                writer.RenderEndTag();      // Tr
            }
        }
    }
}
