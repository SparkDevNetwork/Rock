//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Collections.Generic;
using System.Web.UI.WebControls;

using Rock.Model;

namespace Rock.Web.UI.Controls.Communication
{
    /// <summary>
    /// abstract class for controls used to render a communication channel
    /// </summary>
    public abstract class ChannelControl : CompositeControl
    {
        /// <summary>
        /// Gets or sets the channel data.
        /// </summary>
        /// <value>
        /// The channel data.
        /// </value>
        public abstract Dictionary<string, string> ChannelData { get; set; }

        /// <summary>
        /// Gets or sets any additional merge fields.
        /// </summary>
        public List<string> AdditionalMergeFields
        {
            get
            {
                var mergeFields = ViewState["AdditionalMergeFields"] as List<string>;
                if ( mergeFields == null )
                {
                    mergeFields = new List<string>();
                    ViewState["AdditionalMergeFields"] = mergeFields;
                }
                return mergeFields;
            }

            set { ViewState["AdditionalMergeFields"] = value; }
        }

        /// <summary>
        /// On new communicaiton, initializes controls from sender values
        /// </summary>
        /// <param name="sender">The sender.</param>
        public abstract void InitializeFromSender( Person sender );

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