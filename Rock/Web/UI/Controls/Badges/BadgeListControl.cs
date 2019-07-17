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
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Newtonsoft.Json;

using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// class for controls used to render a Person Profile Badge
    /// </summary>
    public class BadgeListControl : CompositeControl
    {
        private List<BadgeControl> _badges = new List<BadgeControl>();

        /// <summary>
        /// Gets or sets the component guids.
        /// </summary>
        /// <value>
        /// The component guids.
        /// </value>
        public List<BadgeTypeCache> BadgeTypes 
        { 
            get 
            {
                return _badgeTypes;
            }
            set
            {
                _badgeTypes = value;
                RecreateChildControls();
            }
        }
        private List<BadgeTypeCache> _badgeTypes = new List<BadgeTypeCache>();

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
                BadgeTypes = JsonConvert.DeserializeObject( json, typeof( List<BadgeTypeCache> ) ) as List<BadgeTypeCache>;
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
            if ( BadgeTypes != null )
            {
                ViewState["PersonBadges"] = BadgeTypes.ToJson();
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
            if ( BadgeTypes != null )
            {
                var currentPerson =  ((RockPage)Page).CurrentPerson;
                foreach ( var badgeType in BadgeTypes.OrderBy( b => b.Order ) )
                {
                    if ( badgeType.IsAuthorized( Authorization.VIEW, currentPerson ) )
                    {
                        var badgeControl = new BadgeControl();
                        badgeControl.BadgeTypeCache = badgeType;
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