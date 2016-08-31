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
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// A composite control that renders a label, textbox, and datavalidation control for a specific field of a data model
    /// </summary>
    [ToolboxData( "<{0}:DataTextBox runat=server></{0}:DataTextBox>" )]
    public class DataTextBox : RockTextBox
    {
        private Validation.DataAnnotationValidator dataValidator;

        /// <summary>
        /// Gets or sets the name of the assembly qualified name of the entity that is being validated
        /// </summary>
        /// <value>
        /// The name of the assembly qualified type name.
        /// </value>
        [
        Bindable( true ),
        Category( "Behavior" ),
        DefaultValue( "" ),
        Description( "The model to validate." )
        ]
        public string SourceTypeName
        {
            get
            {
                EnsureChildControls();
                return dataValidator.SourceTypeName;
            }
            set
            {
                EnsureChildControls();
                dataValidator.SourceTypeName = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="DataTextBox" /> is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if required; otherwise, <c>false</c>.
        /// </value>
        public override bool Required
        {
            get
            {
                return ( base.Required || ( dataValidator != null && dataValidator.IsRequired ) );
            }

            set
            {
                base.Required = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the entity property.
        /// </summary>
        /// <value>
        /// The name of the entity property.
        /// </value>
        [
        Bindable( true ),
        Category( "Behavior" ),
        DefaultValue( "" ),
        Description( "The model property that is annotated." )
        ]
        public string PropertyName
        {
            get
            {
                EnsureChildControls();
                return dataValidator.PropertyName;
            }
            set
            {
                EnsureChildControls();
                dataValidator.PropertyName = value;
                if ( ( this.Label == string.Empty ) && ( LabelTextFromPropertyName ) )
                {
                    this.Label = value.SplitCase();
                }
            }
        }

        /// <summary>
        /// Gets or sets the group of controls for which the <see cref="T:System.Web.UI.WebControls.TextBox" /> control causes validation when it posts back to the server.
        /// </summary>
        /// <returns>The group of controls for which the <see cref="T:System.Web.UI.WebControls.TextBox" /> control causes validation when it posts back to the server. The default value is an empty string ("").</returns>
        public override string ValidationGroup
        {
            get
            {
                return base.ValidationGroup;
            }
            set
            {
                base.ValidationGroup = value;
                EnsureChildControls();
                dataValidator.ValidationGroup = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [get label from property name].
        /// Default = True
        /// </summary>
        /// <value>
        /// <c>true</c> if [get label from property name]; otherwise, <c>false</c>.
        /// </value>
        [
        Bindable( false ),
        Category( "Appearance" ),
        DefaultValue( true ),
        Description( "If the Label is not set, get the Label from PropertyName" )
        ]
        public bool LabelTextFromPropertyName
        {
            get
            {
                if ( ViewState["LabelTextFromPropertyName"] != null )
                {
                    return (bool)ViewState["LabelTextFromPropertyName"];
                }
                else
                {
                    return true;
                }
            }
            set
            {
                ViewState["LabelTextFromPropertyName"] = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public override bool IsValid
        {
            get
            {
                //dataValidator.Validate();
                return base.IsValid && dataValidator.IsValid;
            }
        }

        /// <summary>
        /// Shows the error message.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        public override void ShowErrorMessage( string errorMessage )
        {
            dataValidator.ErrorMessage = errorMessage;
            dataValidator.IsValid = false;
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            dataValidator = new Validation.DataAnnotationValidator();
            dataValidator.ID = this.ID + "_dav";
            dataValidator.ControlToValidate = this.ID;
            dataValidator.Display = ValidatorDisplay.None;
            dataValidator.CssClass = "validation-error help-inline";
            Controls.Add( dataValidator );
        }

        /// <summary>
        /// Renders any data validators.
        /// </summary>
        /// <param name="writer">The writer.</param>
        protected override void RenderDataValidator( HtmlTextWriter writer )
        {
            dataValidator.ValidationGroup = this.ValidationGroup;
            dataValidator.RenderControl( writer );
        }
    }
}