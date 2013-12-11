//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class PersonPicker2 : CompositeControl
    {
        private HiddenField _hfPersonId;

        /// <summary>
        /// Gets or sets the person id.
        /// </summary>
        /// <value>
        /// The person id.
        /// </value>
        public int? PersonId
        {
            get
            {
                EnsureChildControls();
                if ( string.IsNullOrWhiteSpace( _hfPersonId.Value ) )
                {
                    _hfPersonId.Value = Rock.Constants.None.IdValue;
                }

                if ( _hfPersonId.Value.Equals( Rock.Constants.None.IdValue ) )
                {
                    return null;
                }
                else
                {
                    return _hfPersonId.Value.AsInteger( false );
                }
            }

            set
            {
                EnsureChildControls();
                _hfPersonId.Value = value.ToString();
            }
        }

        /// <summary>
        /// Gets or sets the selected value.
        /// </summary>
        /// <value>
        /// The selected value.
        /// </value>
        public int? SelectedValue
        {
            get
            {
                return PersonId;
            }

            set
            {
                PersonId = value;
            }
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="person">The person.</param>
        public void SetValue( Rock.Model.Person person )
        {
            if ( person != null )
            {
                PersonId = person.Id;
            }
            else
            {
                PersonId = Rock.Constants.None.Id;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            RegisterJavaScript();
        }

        /// <summary>
        /// Registers the java script.
        /// </summary>
        protected virtual void RegisterJavaScript()
        {
            string restUrl = this.ResolveUrl( "~/api/People/Search/" );
            const string scriptFormat = "Rock.controls.personPicker2.initialize({{ controlId: '{0}', restUrl: '{1}' }});";
            string script = string.Format( scriptFormat, this.ID, restUrl );
            ScriptManager.RegisterStartupScript( this, this.GetType(), "person_picker2-" + this.ID, script, true );
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            Controls.Clear();

            _hfPersonId = new HiddenField();
            _hfPersonId.ClientIDMode = System.Web.UI.ClientIDMode.Static;
            _hfPersonId.ID = string.Format( "hfPersonId_{0}", this.ID );
            Controls.Add( _hfPersonId );

        }

        /// <summary>
        /// Renders the <see cref="T:System.Web.UI.WebControls.TextBox" /> control to the specified <see cref="T:System.Web.UI.HtmlTextWriter" /> object.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> that receives the rendered output.</param>
        protected override void Render( HtmlTextWriter writer )
        {
            _hfPersonId.RenderControl( writer );

            if ( this.Enabled )
            {
                string personName = string.Empty;

                int? personId = _hfPersonId.ValueAsInt();
                if ( personId.HasValue )
                {
                    personName = new Rock.Model.PersonService().Queryable()
                        .Where( p => p.Id == personId.Value )
                        .Select( p => p.FullName )
                        .FirstOrDefault() ?? string.Empty;
                }

                writer.Write( string.Format(@"
    <div class='control-group'>
        <div class='control-label'>Search</div>
        <div class='controls'>
            <input id='personPicker_{0}' type='text' class='picker-search input-medium' value='{1}'/>
        </div>
    </div>
    <div id='person-search-results' class='scroll-container scroll-container-vertical'>
        <div class='scrollbar'>
            <div class='track'>
                <div class='thumb'>
                    <div class='end'></div>
                </div>
            </div>
        </div>
        <div class='viewport'>
            <div class='overview'>
                <ul class='picker-select' id='personPickerItems_{0}'>
                </ul>
            </div>
        </div>
    </div>		
", this.ID, personName ) );

            }
        }
    }
}