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
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class WorkflowFormActionList : CompositeControl
    {

        #region Controls

        private HiddenField _hfValue;
        private List<RockTextBox> _actionControls;
        private List<RockDropDownList> _buttonHtmlControls;
        private List<RockDropDownList> _activityControls;
        private List<RockTextBox> _responseControls;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowFormActionList"/> class.
        /// </summary>
        public WorkflowFormActionList()
        {
            _hfValue = new HiddenField();
        }

        /// <summary>
        /// Gets or sets the available activities
        /// </summary>
        /// <value>
        /// The activities.
        /// </value>
        public Dictionary<string, string> Activities
        {
            get { return ViewState["Activities"] as Dictionary<string, string>; }
            set { ViewState["Activities"] = value; }
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public string Value
        {
            get
            {
                return _hfValue.Value;
            }
            set
            {
                _hfValue.Value = value;
                RecreateChildControls();
            }
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            Controls.Clear();

            _hfValue.ID = this.ID + "_hfValue";
            Controls.Add( _hfValue );

            _actionControls = new List<RockTextBox>();
            _buttonHtmlControls = new List<RockDropDownList>();
            _activityControls = new List<RockDropDownList>();
            _responseControls = new List<RockTextBox>();

            string[] nameValues = this.Value.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries );
            for ( int i = 0; i < nameValues.Length; i++ )
            {
                string[] nameValueResponse = nameValues[i].Split( new char[] { '^' } );

                var tbAction = new RockTextBox();
                tbAction.ID = this.ID + "_tbAction" + i.ToString();
                Controls.Add( tbAction );
                tbAction.Placeholder = "Action";
                tbAction.AddCssClass( "form-action-key" );
                tbAction.AddCssClass( "form-control" );
                tbAction.AddCssClass( "js-form-action-input" );
                tbAction.Text = nameValueResponse.Length > 0 ? nameValueResponse[0] : string.Empty;
                _actionControls.Add( tbAction );

                var ddlButtonHtml = new RockDropDownList();
                ddlButtonHtml.ID = this.ID + "_ddlButtonHtml" + i.ToString();
                ddlButtonHtml.EnableViewState = false;
                Controls.Add( ddlButtonHtml );
                ddlButtonHtml.AddCssClass( "form-action-button" );
                ddlButtonHtml.AddCssClass( "form-control" );
                ddlButtonHtml.AddCssClass( "js-form-action-input" );
                var definedType = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.BUTTON_HTML.AsGuid() );
                foreach( var definedValue in definedType.DefinedValues )
                {
                    var li = new ListItem( definedValue.Value, definedValue.Guid.ToString() );
                    li.Selected = nameValueResponse.Length > 1 && li.Value.Equals( nameValueResponse[1], StringComparison.OrdinalIgnoreCase );
                    ddlButtonHtml.Items.Add( li );
                }
                _buttonHtmlControls.Add( ddlButtonHtml );

                var ddlActivity = new RockDropDownList();
                ddlActivity.ID = this.ID + "_ddlActivity" + i.ToString();
                ddlActivity.EnableViewState = false;
                Controls.Add( ddlActivity );
                ddlActivity.AddCssClass( "form-action-value" );
                ddlActivity.AddCssClass( "form-control" );
                ddlActivity.AddCssClass( "js-form-action-input" );
                ddlActivity.DataTextField = "Value";
                ddlActivity.DataValueField = "Key";
                ddlActivity.DataSource = Activities;
                ddlActivity.DataBind();
                ddlActivity.Items.Insert( 0, new ListItem( string.Empty, string.Empty ) );
                foreach(ListItem li in ddlActivity.Items)
                {
                    li.Selected = nameValueResponse.Length > 2 && li.Value.Equals( nameValueResponse[2], StringComparison.OrdinalIgnoreCase );
                }
                _activityControls.Add( ddlActivity );

                var tbResponse = new RockTextBox();
                tbResponse.ID = this.ID + "_tbResponse" + i.ToString();
                Controls.Add( tbResponse );
                tbResponse.Placeholder = "Response Text";
                tbResponse.AddCssClass( "form-action-response" );
                tbResponse.AddCssClass( "form-control" );
                tbResponse.AddCssClass( "js-form-action-input" );
                tbResponse.Text = nameValueResponse.Length > 3 ? nameValueResponse[3] : string.Empty;
                _responseControls.Add( tbResponse );
            }

        }
        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "form-action-list form-group" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            _hfValue.RenderControl( writer );

            StringBuilder valueHtml = new StringBuilder();
            valueHtml.Append( @"<div class=""form-row"">" );
            valueHtml.Append( @"<div class=""col-sm-2""><input class=""form-action-key form-control js-form-action-input"" type=""text"" placeholder=""Action""></input></div>" );
            valueHtml.Append( @"<div class=""col-sm-2""><select class=""form-action-button form-control js-form-action-input"">" );
            var definedType = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.BUTTON_HTML.AsGuid() );
            foreach ( var definedValue in definedType.DefinedValues )
            {
                valueHtml.AppendFormat( @"<option value=""{0}"">{1}</option>", definedValue.Guid.ToString(), definedValue.Value );
            }
            valueHtml.Append( @"</select></div>" );
            valueHtml.Append( @"<div class=""col-sm-3""><select class=""form-action-value form-control js-form-action-input""><option value=""""></option>" );
            foreach ( var activity in Activities )
            {
                valueHtml.AppendFormat( @"<option value=""{0}"">{1}</option>", activity.Key, activity.Value );
            }
            valueHtml.Append( @"</select></div>" );
            valueHtml.Append( @"<div class=""col-sm-4""><input class=""form-action-response form-control js-form-action-input"" type=""text"" placeholder=""Response Text""></input></div>" );
            valueHtml.Append( @"<div class=""col-sm-1""><a href=""#"" class=""btn btn-sm btn-danger form-action-remove""><i class=""fa fa-times""></i></a></div></div>" );

            var hfValueHtml = new HtmlInputHidden();
            hfValueHtml.AddCssClass( "js-value-html" );
            hfValueHtml.Value = valueHtml.ToString();
            hfValueHtml.RenderControl( writer );

            // Write Header row
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            // Write Action
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-sm-2" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "control-label" );
            writer.RenderBeginTag( HtmlTextWriterTag.Label );
            writer.Write( "Command Label" );
            writer.RenderEndTag();
            writer.RenderEndTag();

            // Write Css
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-sm-2" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "control-label" );
            writer.RenderBeginTag( HtmlTextWriterTag.Label );
            writer.Write( "Button Type" );
            writer.RenderEndTag();
            writer.RenderEndTag();

            // Write Activity Value
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-sm-3" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "control-label" );
            writer.RenderBeginTag( HtmlTextWriterTag.Label );
            writer.Write( "Activate Activity" );
            writer.RenderEndTag();
            writer.RenderEndTag();

            // Write Response
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-sm-4" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "control-label" );
            writer.RenderBeginTag( HtmlTextWriterTag.Label );
            writer.Write( "Response Text" );
            writer.RenderEndTag();
            writer.RenderEndTag();

            // Write Remove Button
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-sm-1" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            writer.RenderEndTag();  // Div

            writer.RenderEndTag();  // Div.row

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "form-action-rows" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            writer.WriteLine();

            for (int i = 0; i < _actionControls.Count; i++)
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "form-row" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                // Write Action
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-sm-2" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _actionControls[i].RenderControl( writer );
                writer.RenderEndTag();  

                // Write Button Type
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-sm-2" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _buttonHtmlControls[i].RenderControl( writer );
                writer.RenderEndTag();  

                // Write Activity Value
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-sm-3" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _activityControls[i].RenderControl( writer );
                writer.RenderEndTag();  

                // Write Response
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-sm-4" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _responseControls[i].RenderControl( writer );
                writer.RenderEndTag();  

                // Write Remove Button
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-sm-1" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                writer.AddAttribute( HtmlTextWriterAttribute.Href, "#" );
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "btn btn-sm btn-danger form-action-remove" );
                writer.RenderBeginTag( HtmlTextWriterTag.A );
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "fa fa-times" );
                writer.RenderBeginTag( HtmlTextWriterTag.I );
                writer.RenderEndTag();  // I
                writer.RenderEndTag();  // A
                writer.RenderEndTag();  // Div

                writer.RenderEndTag();  // Div.row
            }

            writer.RenderEndTag();      // Div.form-action-rows

            // Add Actions
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "actions" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "btn btn-action btn-xs form-action-add" );
            writer.AddAttribute( HtmlTextWriterAttribute.Href, "#" );
            writer.RenderBeginTag( HtmlTextWriterTag.A );
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "fa fa-plus-circle" );
            writer.RenderBeginTag( HtmlTextWriterTag.I );
            writer.RenderEndTag();      // I
            writer.RenderEndTag();      // A
            writer.RenderEndTag();      // Div.actions

            writer.RenderEndTag();      // Div.form-action-list

            RegisterClientScript();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        private void RegisterClientScript()
        {
            string script = @"
    function updateFormActions( e ) {
        var $actionList = e.closest('div.form-action-list');
        var newValue = '';
        $actionList.find('div.form-action-rows:first').children('div.form-row').each(function( index ) {
            newValue += 
                $(this).find('.form-action-key:first').val() + '^' + 
                $(this).find('.form-action-button:first').val() + '^' + 
                $(this).find('.form-action-value:first').val() + '^' + 
                $(this).find('.form-action-response:first').val() + '|'
        });
        $actionList.children('input:first').val(newValue);            
    }

    $('a.form-action-add').click(function (e) {
        e.preventDefault();
        var $actionList = $(this).closest('.form-action-list');
        $actionList.find('div.form-action-rows:first').append($actionList.find('.js-value-html').val());
    });

    $(document).on('click', 'a.form-action-remove', function (e) {
        e.preventDefault();
        var $rows = $(this).closest('div.form-action-rows');
        $(this).closest('div.form-row').remove();
        updateFormActions($rows);            
    });

    $(document).on('focusout', '.js-form-action-input', function (e) {
        updateFormActions($(this));            
    });
";

            ScriptManager.RegisterStartupScript( this, this.GetType(), "form-action-list", script, true );
        }

    }
}