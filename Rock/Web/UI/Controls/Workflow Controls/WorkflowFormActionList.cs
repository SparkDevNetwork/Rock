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
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock.Data;
using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class WorkflowFormActionList : HiddenField
    {
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
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "form-action-list" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            base.RenderControl( writer );

            StringBuilder valueHtml = new StringBuilder();
            valueHtml.Append( @"<div class=""row"">" );
            valueHtml.Append( @"<div class=""col-sm-2""><input class=""form-action-key form-control js-form-action-input"" type=""text"" placeholder=""Action""></input></div>" );
            valueHtml.Append( @"<div class=""col-sm-3""><select class=""form-action-value form-control js-form-action-input""><option value=""""></option>" );
            foreach ( var activity in Activities )
            {
                valueHtml.AppendFormat( @"<option value=""{0}"">{1}</option>", activity.Key, activity.Value );
            }
            valueHtml.Append( @"</select></div>" );
            valueHtml.Append( @"<div class=""col-sm-6""><input class=""form-action-response form-control js-form-action-input"" type=""text"" placeholder=""Response""></input></div>" );
            valueHtml.Append( @"<div class=""col-sm-1""><a href=""#"" class=""btn btn-sm btn-danger form-action-remove""><i class=""fa fa-minus-circle""></i></a></div></div>" );

            var hfValueHtml = new HtmlInputHidden();
            hfValueHtml.AddCssClass( "js-value-html" );
            hfValueHtml.Value = valueHtml.ToString();
            hfValueHtml.RenderControl( writer );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "form-action-rows" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            writer.WriteLine();

            string[] nameValues = this.Value.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries );
            foreach ( string nameValue in nameValues )
            {
                string[] nameValueResponse = nameValue.Split( new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries );

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "row" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                // Write Name
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-sm-2" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "form-action-key form-control js-form-action-input" );
                writer.AddAttribute( HtmlTextWriterAttribute.Type, "text" );
                writer.AddAttribute( HtmlTextWriterAttribute.Value, nameValueResponse.Length >= 1 ? nameValueResponse[0] : string.Empty );
                writer.AddAttribute( "placeholder", "Action" );
                writer.RenderBeginTag( HtmlTextWriterTag.Input );
                writer.RenderEndTag();  // Input
                writer.RenderEndTag();  // Div

                // Write Value
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-sm-3" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                DropDownList ddl = new DropDownList();
                ddl.AddCssClass( "form-action-value form-control js-form-action-input" );
                ddl.DataTextField = "Value";
                ddl.DataValueField = "Key";
                ddl.DataSource = Activities;
                ddl.DataBind();
                ddl.Items.Insert( 0, new ListItem( string.Empty, string.Empty ) );
                if ( nameValueResponse.Length >= 2 )
                {
                    ddl.SelectedValue = nameValueResponse[1];
                }
                ddl.RenderControl( writer );
                writer.RenderEndTag();  // Div

                // Write Response
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-sm-6" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "form-action-response form-control js-form-action-input" );
                writer.AddAttribute( HtmlTextWriterAttribute.Type, "text" );
                writer.AddAttribute( HtmlTextWriterAttribute.Value, nameValueResponse.Length >= 3 ? nameValueResponse[2] : string.Empty );
                writer.AddAttribute( "placeholder", "Response" );
                writer.RenderBeginTag( HtmlTextWriterTag.Input );
                writer.RenderEndTag();  // Input
                writer.RenderEndTag();  // Div

                // Write Remove Button
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-sm-1" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                writer.AddAttribute( HtmlTextWriterAttribute.Href, "#" );
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "btn btn-sm btn-danger form-action-remove" );
                writer.RenderBeginTag( HtmlTextWriterTag.A );
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "fa fa-minus-circle" );
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
        $actionList.children('div.form-action-rows:first').children('div.row').each(function( index ) {
            newValue += $(this).find('.form-action-key:first').val() + '^' + 
                $(this).find('.form-action-value:first').val() + '^' + 
                $(this).find('.form-action-response:first').val() + '|'
        });
        $actionList.children('input:first').val(newValue);            
    }

    $('a.form-action-add').click(function (e) {
        e.preventDefault();
        var $actionList = $(this).closest('.form-action-list');
        $actionList.find('.form-action-rows:first').append($actionList.find('.js-value-html').val());
    });

    $(document).on('click', 'a.form-action-remove', function (e) {
        e.preventDefault();
        var $rows = $(this).closest('div.form-action-rows');
        $(this).closest('div.row').remove();
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