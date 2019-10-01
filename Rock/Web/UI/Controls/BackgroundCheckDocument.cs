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

using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// A <see cref="T:System.Web.UI.Controls.BackgroundCheckDocument"/> control with an associated label.
    /// </summary>
    [ToolboxData( "<{0}:BackgroundCheckDocument runat=server></{0}:BackgroundCheckDocument>" )]
    public class BackgroundCheckDocument : CompositeControl
    {
        #region Utilities        
        /// <summary>
        /// BackgroundCheck Types
        /// </summary>
        private enum BackgroundCheckTypes
        {
            ProtectMyMinistry,
            Checkr,
            Unknown
        }

        /// <summary>
        /// Gets the type of the Background Check.
        /// </summary>
        /// <returns></returns>
        private BackgroundCheckTypes GetControlType()
        {
            EnsureChildControls();


            Guid? cpGuid = _componentPicker.SelectedValue.AsGuidOrNull();
            if ( cpGuid.HasValue )
            {
                if ( cpGuid.Value == Rock.SystemGuid.EntityType.PROTECT_MY_MINISTRY_PROVIDER.AsGuid() )
                {
                    return BackgroundCheckTypes.ProtectMyMinistry;
                }
                else if ( cpGuid.Value == Rock.SystemGuid.EntityType.CHECKR_PROVIDER.AsGuid() )
                {
                    return BackgroundCheckTypes.Checkr;
                }
            }

            return BackgroundCheckTypes.Unknown;
        }
        #endregion
        #region Controls
        /// <summary>
        /// BackgroundChecker service selector
        /// </summary>
        private ComponentPicker _componentPicker;

        /// <summary>
        /// The PMM FileUploader
        /// </summary>
        private FileUploader _fileUploader;

        /// <summary>
        /// The Checkr TextBox
        /// </summary>
        private RockTextBox _textBox;
        #endregion
        #region Properties
        /// <summary>
        /// Gets or sets the binary file type GUID.
        /// </summary>
        /// <value>
        /// The binary file type GUID.
        /// </value>
        public Guid? BinaryFileTypeGuid
        {
            get
            {
                EnsureChildControls();
                return _fileUploader.BinaryFileTypeGuid;

            }
            set
            {
                EnsureChildControls();
                _fileUploader.BinaryFileTypeGuid = value ?? Guid.Empty;
            }
        }

        /// <summary>
        /// Gets or sets the binary file id.
        /// </summary>
        /// <value>
        /// The binary file id.
        /// </value>
        public int? BinaryFileId
        {
            get
            {
                EnsureChildControls();
                return _fileUploader.BinaryFileId;
            }

            set
            {
                EnsureChildControls();
                var li = _componentPicker.Items.FindByValue( Rock.SystemGuid.EntityType.PROTECT_MY_MINISTRY_PROVIDER.ToUpper() );
                if ( li != null )
                {
                    li.Selected = true;
                    _componentPicker_SelectedIndexChanged( null, null );
                }

                _fileUploader.BinaryFileId = value;
            }
        }

        /// <summary>
        /// Gets or sets the text content of the <see cref="T:System.Web.UI.WebControls.TextBox" /> control.
        /// </summary>
        /// <returns>The text displayed in the <see cref="T:System.Web.UI.WebControls.TextBox" /> control. The default is an empty string ("").</returns>
        public string Text
        {
            get
            {
                EnsureChildControls();
                Guid? providerGuid = _componentPicker.SelectedValue.AsGuidOrNull();
                if ( _textBox.Text.IsNotNullOrWhiteSpace() && providerGuid.HasValue )
                {
                    return $"{EntityTypeCache.Get( providerGuid.Value ).Id},{_textBox.Text}";
                }

                return _textBox.Text;
            }

            set
            {
                EnsureChildControls();
                if ( value.IsNotNullOrWhiteSpace() )
                {
                    var valueSplit = value.Split( ',' );
                    if ( valueSplit != null && valueSplit.Length == 2 )
                    {
                        //EntityTypeCache.Get( typeof(Checkr) ).Id
                        //Type backgroundCheckComponentType = Type.GetType( EntityTypeCache.Get( entityTypeId ).AssemblyName )
                        string entityTypeId = valueSplit[0];
                        var li = _componentPicker.Items.FindByValue( EntityTypeCache.Get( entityTypeId.AsInteger() ).Guid.ToString().ToUpper() );
                        if ( li != null )
                        {
                            li.Selected = true;
                            _componentPicker_SelectedIndexChanged( null, null );
                            _textBox.Text = valueSplit[1];
                            return;
                        }
                    }
                }

                _textBox.Text = value;
            }
        }
        #endregion
        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            Controls.Clear();

            _componentPicker = new ComponentPicker()
            {
                ContainerType = "Rock.Security.BackgroundCheckContainer"
            };
            _componentPicker.ID = this.ID + "_componentPicker";
            _componentPicker.AutoPostBack = true;
            _componentPicker.SelectedIndexChanged += _componentPicker_SelectedIndexChanged;
            Controls.Add( _componentPicker );

            _fileUploader = new FileUploader();
            _fileUploader.ID = this.ID + "_fileUploader";
            _fileUploader.Visible = false;
            Controls.Add( _fileUploader );

            _textBox = new RockTextBox();
            _textBox.ID = this.ID + "_rockTextBox";
            _textBox.Label = "RecordKey";
            _textBox.Help = "Unique key for the background check report.";
            _textBox.Visible = false;
            Controls.Add( _textBox );
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the _ddlGroupTypeInheritFrom control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void _componentPicker_SelectedIndexChanged( object sender, EventArgs e )
        {
            EnsureChildControls();
            var type = GetControlType();
            if ( type == BackgroundCheckTypes.ProtectMyMinistry )
            {
                _textBox.Visible = false;
                _fileUploader.Visible = true;
                _textBox.Text = string.Empty;
            }
            else if ( type == BackgroundCheckTypes.Checkr )
            {
                _textBox.Visible = true;
                _fileUploader.Visible = false;
                _fileUploader.BinaryFileId = null;
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
                RenderBaseControl( writer );
            }
        }

        /// <summary>
        /// This is where you implement the simple aspects of rendering your control.  The rest
        /// will be handled by calling RenderControlHelper's RenderControl() method.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public void RenderBaseControl( HtmlTextWriter writer )
        {
            writer.AddAttribute( HtmlTextWriterAttribute.Style, this.Style.Value );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _componentPicker.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _fileUploader.RenderControl( writer );
            _textBox.TextMode = TextBoxMode.MultiLine;
            _textBox.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();
        }
    }
}