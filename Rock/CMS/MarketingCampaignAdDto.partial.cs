//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Collections.Generic;
using Rock.Attribute;
using Rock.Data;

namespace Rock.Cms
{
    public partial class MarketingCampaignAdDto : IDto, IHasAttributes
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name
        {
            get
            {
                if ( _name == null )
                {
                    var nameEntity = MarketingCampaignAdType.Read( this.MarketingCampaignAdTypeId );
                    if ( nameEntity != null )
                    {
                        _name = nameEntity.Name;
                    }
                }
                return _name ?? string.Empty;
            }
        }
        private string _name;


        /// <summary>
        /// Dictionary of categorized attributes.  Key is the category name, and Value is list of attributes in the category
        /// </summary>
        /// <value>
        /// The attribute categories.
        /// </value>
        /// <exception cref="System.NotImplementedException"></exception>
        public SortedDictionary<string, List<string>> AttributeCategories { get; set; }

        /// <summary>
        /// List of attributes associated with the object.  This property will not include the attribute values.
        /// The <see cref="AttributeValues" /> property should be used to get attribute values.  Dictionary key
        /// is the attribute key, and value is the cached attribute
        /// </summary>
        /// <value>
        /// The attributes.
        /// </value>
        /// <exception cref="System.NotImplementedException"></exception>
        public Dictionary<string, Web.Cache.AttributeCache> Attributes { get; set; }

        /// <summary>
        /// Dictionary of all attributes and their value.  Key is the attribute key, and value is the values
        /// associated with the attribute and object instance
        /// </summary>
        /// <value>
        /// The attribute values.
        /// </value>
        /// <exception cref="System.NotImplementedException"></exception>
        public Dictionary<string, List<Core.AttributeValueDto>> AttributeValues { get; set; }
    }
}