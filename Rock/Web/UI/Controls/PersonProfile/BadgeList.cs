//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.PersonProfile;

namespace Rock.Web.UI.Controls
{ 
    /// <summary>
    /// abstract class for controls used to render a Person Profile Badge
    /// </summary>
    public class PersonProfileBadgeList : CompositeControl
    {
        private List<PersonProfileBadge> badges = new List<PersonProfileBadge>();

        /// <summary>
        /// Gets or sets the component guids.
        /// </summary>
        /// <value>
        /// The component guids.
        /// </value>
        public string ComponentGuids
        {
            get 
            { 
                return ViewState["ComponentGuids"] as string; 
            }
            set 
            { 
                ViewState["ComponentGuids"] = value;
                RecreateChildControls();
            }
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            badges.Clear();

            if ( !string.IsNullOrWhiteSpace( ComponentGuids ) )
            {
                var guids = new List<Guid>();
                ComponentGuids.SplitDelimitedValues().ToList().ForEach( g => guids.Add( Guid.Parse( g ) ) );

                foreach ( var component in BadgeContainer.Instance.Components )
                {
                    BadgeComponent badge = component.Value.Value;
                    if ( guids.Contains( badge.TypeGuid ) )
                    {
                        var badgeControl = new PersonProfileBadge();
                        badgeControl.BadgeEntityTypeName = badge.EntityType.Name;
                        badges.Add( badgeControl );
                    }
                }
            }

            base.CreateChildControls();
            Controls.Clear();

            badges.ForEach( b => Controls.Add( b ) );
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            foreach ( var badgeControl in badges )
            {
                badgeControl.RenderControl( writer );
                writer.Write( " " );
            }
        }

    }
}