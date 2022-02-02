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
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// A <see cref="System.Web.UI.WebControls.TextBox"/> control with an associated label.
    /// </summary>
    [ToolboxData("<{0}:FirstNameTextBox runat=server></{0}:FirstNameTextBox>")]
    public class FirstNameTextBox : RockTextBox
    {
        private CustomValidator _customValidator;
        private readonly List<string> _notAllowedStrings = new List<string> { "&", " & ", " and ", "-and-", "_and_", " plus ", "+" };
        private readonly string _defaultDelimter = "^";

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
                EnsureChildControls();
                return base.IsValid && _customValidator.IsValid;
            }
        }

        /// <summary>
        /// Gets or sets representing characters or strings, delimited by <see cref="Delimiter"/> that are not valid or allowed
        /// </summary>
        public string NotAllowed
        {
            get { return ViewState["NotAllowed"] as string; }
            set { ViewState["NotAllowed"] = value; }
        }

        /// <summary>
        /// Gets or sets the character that delimits the strings or characters that are not allowed
        /// </summary>
        public string Delimiter
        {
            get { return ViewState["Delimiter"] as string ?? _defaultDelimter; }
            set { ViewState["Delimiter"] = value; }
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            _customValidator = new CustomValidator();
            _customValidator.ID = this.ID + nameof( _customValidator );
            _customValidator.ControlToValidate = this.ID;
            _customValidator.Display = ValidatorDisplay.Dynamic;
            _customValidator.CssClass = "validation-error help-inline";
            _customValidator.ServerValidate += ServerValidation;

            Controls.Add( _customValidator );
        }

        private void ServerValidation(object source, ServerValidateEventArgs args)
        {
            var value = args.Value;
            if ( !string.IsNullOrWhiteSpace( NotAllowed ) )
            {
                _notAllowedStrings.AddRange( NotAllowed.Split( new string[] { Delimiter }, StringSplitOptions.RemoveEmptyEntries ) );
            }

            var invalidStrings = _notAllowedStrings.FindAll( notAllowedString => value.IndexOf( notAllowedString, StringComparison.InvariantCultureIgnoreCase ) >= 0 );
            if ( invalidStrings.Count > 0 )
            {
                _customValidator.ErrorMessage = string.Format("FirstName cannot contain {0}", Humanizer.CollectionHumanizeExtensions.Humanize( invalidStrings.Select(m => m.Trim()).Distinct() ) );
            }
            args.IsValid = invalidStrings.Count == 0;
        }
    }
}
