using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    [RockObsolete( "1.11" )]
    [Obsolete( "Use BadgeListControl instead." )]

    /// <summary>
    /// class for controls used to render a Person Profile Badge
    /// </summary>
    public class PersonProfileBadgeList : CompositeControl
    {
        private List<PersonProfileBadge> _badges = new List<PersonProfileBadge>();

        /// <summary>
        /// Gets or sets the component guids.
        /// </summary>
        /// <value>
        /// The component guids.
        /// </value>
        public List<BadgeCache> PersonBadges
        {
            get
            {
                return _personBadges;
            }
            set
            {
                _personBadges = value;
                RecreateChildControls();
            }
        }
        private List<BadgeCache> _personBadges = new List<BadgeCache>();

        /// <summary>
        /// Restores view-state information from a previous request that was saved with the <see cref="M:System.Web.UI.WebControls.WebControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An object that represents the control state to restore.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            var json = ViewState["PersonBadges"] as string;
            if ( !string.IsNullOrWhiteSpace( json ) )
            {
                PersonBadges = JsonConvert.DeserializeObject( json, typeof( List<BadgeCache> ) ) as List<BadgeCache>;
            }
        }

        /// <summary>
        /// Saves any state that was modified after the <see cref="M:System.Web.UI.WebControls.Style.TrackViewState" /> method was invoked.
        /// </summary>
        /// <returns>
        /// An object that contains the current view state of the control; otherwise, if there is no view state associated with the control, null.
        /// </returns>
        protected override object SaveViewState()
        {
            if ( PersonBadges != null )
            {
                ViewState["PersonBadges"] = PersonBadges.ToJson();
            }

            return base.SaveViewState();
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Controls.Clear();

            _badges.Clear();
            if ( PersonBadges != null )
            {
                var currentPerson = ( ( RockPage ) Page ).CurrentPerson;
                foreach ( var personBadge in PersonBadges.OrderBy( b => b.Order ) )
                {
                    if ( personBadge.IsAuthorized( Authorization.VIEW, currentPerson ) )
                    {
                        var badgeControl = new PersonProfileBadge();
                        badgeControl.PersonBadge = personBadge;
                        _badges.Add( badgeControl );
                        Controls.Add( badgeControl );
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
            foreach ( var badgeControl in _badges )
            {
                badgeControl.RenderControl( writer );
                writer.Write( " " );
            }
        }
    }
}
