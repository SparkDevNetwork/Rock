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
        private List<PersonProfileBadge> _badges = new List<PersonProfileBadge>();

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
            _badges.Clear();

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
                        _badges.Add( badgeControl );
                    }
                }
            }

            base.CreateChildControls();
            Controls.Clear();

            _badges.ForEach( b => Controls.Add( b ) );
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