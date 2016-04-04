﻿// <copyright>
// Copyright by the Spark Development Network
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
using System.Linq;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Collections.Generic;
using Rock.Extension;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Security;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class RemoteAuthsPicker : RockCheckBoxList
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An object that contains event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            foreach ( var serviceEntry in AuthenticationContainer.Instance.Components )
            {
                var component = serviceEntry.Value.Value;

                if ( component.IsActive && component.RequiresRemoteAuthentication )
                {
                    var entityType = EntityTypeCache.Read( component.GetType() );
                    if ( entityType != null )
                    {
                        this.Items.Add( new ListItem( entityType.FriendlyName, entityType.Guid.ToString() ) );
                    }
                }
            }
        }

        /// <summary>
        /// Selects the values.
        /// </summary>
        /// <value>
        /// The selected values.
        /// </value>
        public new List<Guid> SelectedValues
        {
            get
            {
                return this.Items.OfType<ListItem>().Where( l => l.Selected ).Select( a => Guid.Parse( a.Value ) ).ToList();
            }
            set
            {
                foreach ( ListItem componentItem in this.Items )
                {
                    componentItem.Selected = value.Exists( a => a.Equals( Guid.Parse( componentItem.Value ) ) );
                }
            }
        }
    }
}