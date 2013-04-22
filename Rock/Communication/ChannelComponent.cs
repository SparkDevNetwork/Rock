//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;

using Rock;
using Rock.Model;

using Rock.Attribute;
using Rock.Data;
using Rock.Extension;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Communication
{
    /// <summary>
    /// Base class for components communication channels (i.e. email, sms, twitter, etc) 
    /// </summary>
    public abstract class ChannelComponent : Component
    {
        public abstract string ControlPath { get; }

        public EntityTypeCache EntityType
        {
            get
            {
                return EntityTypeCache.Read( this.GetType() );
            }
        }

        public ChannelComponent()
        {
            this.LoadAttributes();
        }
    }

}