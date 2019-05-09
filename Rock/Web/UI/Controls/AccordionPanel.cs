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
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Web.UI.WebControls.CompositeControl" />
    [ToolboxData( "<{0}:AccordionPanel runat=server></{0}:AccordionPanel" )]
    [ParseChildren( true )]
    [PersistChildren( false )]
    public class AccordionPanel : CompositeControl
    {
        #region Properties

        /// <summary>
        /// Gets or sets the panel title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The title for the panel." )
        ]
        public string Title
        {
            get
            {
                return ( string ) ViewState["Title"] ?? string.Empty;
            }
            set
            {
                ViewState["Title"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the panel title icon.
        /// </summary>
        /// <value>
        /// The title icon.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The icon for the panel." )
        ]
        public string TitleIcon
        {
            get
            {
                return ( string ) ViewState["TitleIcon"] ?? string.Empty;
            }
            set
            {
                ViewState["TitleIcon"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the header CSS class.
        /// </summary>
        /// <value>
        /// The header CSS class.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The additional CSS class to be applied to the panel-heading node." )
        ]
        public string HeaderCssClass
        {
            get
            {
                return ( string ) ViewState["HeaderCssClass"] ?? string.Empty;
            }
            set
            {
                ViewState["HeaderCssClass"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the body CSS class.
        /// </summary>
        /// <value>
        /// The body CSS class.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The additional CSS class to be applied to the panel-body node." )
        ]
        public string BodyCssClass
        {
            get
            {
                return ( string ) ViewState["BodyCssClass"] ?? string.Empty;
            }
            set
            {
                ViewState["BodyCssClass"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the footer CSS class.
        /// </summary>
        /// <value>
        /// The footer CSS class.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The additional CSS class to be applied to the panel-footer node.." )
        ]
        public string FooterCssClass
        {
            get
            {
                return ( string ) ViewState["FooterCssClass"] ?? string.Empty;
            }
            set
            {
                ViewState["FooterCssClass"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="AccordionPanel"/> is collapsed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if collapsed; otherwise, <c>false</c>.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The collapsed state of the panel." )
        ]
        public bool Collapsed
        {
            get
            {
                EnsureChildControls();

                return _hfState.Value.AsBoolean();
            }
            set
            {
                EnsureChildControls();

                _hfState.Value = value.ToString().ToLower();
            }
        }

        #endregion

        #region ITemplates

        /// <summary>
        /// Gets or sets the additional buttons to be displayed next to the toggle button.
        /// </summary>
        /// <value>
        /// The additional buttons to be displayed next to the toggle button.
        /// </value>
        [PersistenceMode( PersistenceMode.InnerProperty )]
        [TemplateInstance( TemplateInstance.Single )]
        public ITemplate AdditionalButtons { get; set; }

        /// <summary>
        /// Gets or sets the controls that will comprise the body of the panel.
        /// </summary>
        /// <value>
        /// The controls that will comprise the body of the panel.
        /// </value>
        [PersistenceMode( PersistenceMode.InnerProperty )]
        [TemplateInstance( TemplateInstance.Single )]
        public ITemplate Body { get; set; }

        /// <summary>
        /// Gets or sets the controls that will comprise the footer of the panel.
        /// </summary>
        /// <value>
        /// The controls that will comprise the footer of the panel.
        /// </value>
        [PersistenceMode( PersistenceMode.InnerProperty )]
        [TemplateInstance( TemplateInstance.Single )]
        public ITemplate Footer { get; set; }

        #endregion

        #region Controls

        private HiddenField _hfState;
        private PlaceHolder _additionalButtonsPlaceholder;
        private PlaceHolder _bodyPlaceholder;
        private PlaceHolder _footerPlaceholder;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AccordionPanel"/> class.
        /// </summary>
        public AccordionPanel()
        {
            CssClass = "panel panel-widget";
        }

        #endregion

        #region Base Method Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            _bodyPlaceholder = new PlaceHolder();
            Body?.InstantiateIn( _bodyPlaceholder );
            Controls.Add( _bodyPlaceholder );

            if ( AdditionalButtons != null )
            {
                _additionalButtonsPlaceholder = new PlaceHolder();
                AdditionalButtons.InstantiateIn( _additionalButtonsPlaceholder );
                Controls.Add( _additionalButtonsPlaceholder );
            }

            if ( Footer != null )
            {
                _footerPlaceholder = new PlaceHolder();
                Footer.InstantiateIn( _footerPlaceholder );
                Controls.Add( _footerPlaceholder );
            }
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            _hfState = new HiddenField
            {
                ID = this.ID + "_hfState"
            };
            Controls.Add( _hfState );
        }

        /// <summary>
        /// Called just before rendering begins on the page.
        /// </summary>
        /// <param name="e">The EventArgs that describe this event.</param>
        protected override void OnPreRender( EventArgs e )
        {
            base.OnPreRender( e );

            RegisterStartupScript();
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            //
            // Render the panel.
            //
            writer.AddAttribute( HtmlTextWriterAttribute.Id, this.ClientID );
            writer.AddAttribute( HtmlTextWriterAttribute.Class, $"js-accordion-panel { CssClass }" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            {
                _hfState.RenderControl( writer );

                //
                // Render the panel heading.
                //
                writer.AddAttribute( HtmlTextWriterAttribute.Class, $"panel-heading clearfix clickable { HeaderCssClass }" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                {
                    //
                    // Render the panel title.
                    //
                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "panel-title pull-left" );
                    writer.RenderBeginTag( HtmlTextWriterTag.H3 );
                    {
                        if ( !string.IsNullOrWhiteSpace( TitleIcon ) )
                        {
                            writer.Write( $"<i class='{ TitleIcon }'></i> " );
                        }

                        writer.WriteEncodedText( Title );
                    }
                    writer.RenderEndTag();

                    //
                    // Render the panel header buttons.
                    //
                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "pull-right" );
                    writer.AddAttribute( "style", "margin: -4px 0px -4px 0px;" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Span );
                    {
                        _additionalButtonsPlaceholder?.RenderControl( writer );

                        writer.AddAttribute( HtmlTextWriterAttribute.Class, "btn btn-xs btn-link js-accordion-panel-chevron" );
                        writer.AddAttribute( HtmlTextWriterAttribute.Href, "#" );
                        writer.RenderBeginTag( HtmlTextWriterTag.A );
                        {
                            writer.Write( $"<i class='fa { ( Collapsed ? "fa-chevron-down" : "fa-chevron-up" ) }'></i>" );
                        }
                        writer.RenderEndTag();
                    }
                    writer.RenderEndTag();
                }
                writer.RenderEndTag();

                //
                // Render panel body.
                //
                writer.AddAttribute( HtmlTextWriterAttribute.Class, $"panel-collapse { ( Collapsed ? "collapse" : string.Empty ) }" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                {
                    writer.AddAttribute( HtmlTextWriterAttribute.Class, $"panel-body { BodyCssClass }" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Div );
                    {
                        _bodyPlaceholder.RenderControl( writer );
                    }
                    writer.RenderEndTag();
                }
                writer.RenderEndTag();

                //
                // Render the footer if we have one.
                //
                if ( _footerPlaceholder != null )
                {
                    writer.AddAttribute( HtmlTextWriterAttribute.Class, $"panel-footer { FooterCssClass }" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Div );
                    {
                        _footerPlaceholder.RenderControl( writer );
                    }
                }
            }
            writer.RenderEndTag();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Registers the startup script to initialize the control.
        /// </summary>
        protected void RegisterStartupScript()
        {
            //
            // Setup the script for expanding and collapsing the panel.
            //
            var script = string.Format( @"
(function () {{
    $('.js-accordion-panel > .panel-heading,.js-accordion-panel > .panel-heading .js-accordion-panel-chevron').click(function (e) {{
        e.stopImmediatePropagation();
        e.preventDefault();

        var $panel = $(this).closest('.js-accordion-panel');
        var $body = $panel.children('.panel-collapse');
        var $icon = $panel.find('> .panel-heading .js-accordion-panel-chevron > i');

        $body.slideToggle(400);
        $icon.toggleClass('fa-chevron-down').toggleClass('fa-chevron-up');
        $('#{0}').val($icon.hasClass('fa-chevron-down'));
    }});
}})();
",
                _hfState.ClientID );
            ScriptManager.RegisterStartupScript( this, GetType(), "AccordionPanelInit", script, true );
        }

        #endregion
    }
}