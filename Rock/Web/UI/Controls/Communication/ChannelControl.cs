//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Collections.Generic;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls.Communication
{
    /// <summary>
    /// abstract class for controls used to render a communication channel
    /// </summary>
    public abstract class ChannelControl : CompositeControl
    {
        public abstract Dictionary<string, string> ChannelData { get; set; }

        /// <summary>
        /// Gets the data value.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        protected string GetDataValue( Dictionary<string, string> data, string key )
        {
            if ( data != null && data.ContainsKey( key ) )
            {
                return data[key];
            }
            else
            {
                return string.Empty;
            }
        }

    }
}