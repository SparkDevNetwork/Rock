using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace com.centralaz.Utility.MSSearch
{
    /// <summary>
    /// The MOSS search query request
    /// </summary>
    public class MSQueryRequest
    {
        /// <summary>
        /// The type of MOSS query: Keyword or MS-SQL
        /// </summary>
        public enum QueryType { Keyword, MsSql };

        private Guid _queryID = Guid.Empty;
        private string _queryText = string.Empty;
        private string _originatorContext = string.Empty;
        private int _startAt = 0;
        private int _count = 0;
        private List<MSQueryProperty> _properties = new List<MSQueryProperty>();
        private bool _enableStemming = true;
        private bool _trimDuplicates = true;
        private bool _includeSpecialTermResults = true;
        private bool _ignoreAllNoiseQuery = true;
        private bool _includeRelevantResults = true;
        private bool _implicitAndBehavior = true;
        private bool _includeHighConfidenceResults = true;

        private QueryType _queryType = QueryType.Keyword;
        private string _target = string.Empty;
        private string _language = "en-US";

        public override string ToString()
        {
            return ( BuildXmlQueryString() );
        }

        #region Public Properties and Methods

        /// <summary>
        /// The Query ID
        /// </summary>
        public Guid QueryID
        {
            get { return _queryID; }
            set { _queryID = value; }
        }

        /// <summary>
        /// The query text
        /// </summary>
        public string QueryText
        {
            get { return _queryText; }
            set { _queryText = value; }
        }

        /// <summary>
        /// The Originator Context that allows the client to pass data to the Query Web service. The Query Web service returns this data to the client in the response.
        /// </summary>
        public string OriginatorContext
        {
            get { return _originatorContext; }
            set { _originatorContext = value; }
        }

        /// <summary>
        /// Specifies which result is the initial result in the response
        /// </summary>
        public int StartAt
        {
            get { return _startAt; }
            set { _startAt = value; }
        }

        /// <summary>
        /// Contains number of results included in the response
        /// </summary>
        public int Count
        {
            get { return _count; }
            set { _count = value; }
        }

        /// <summary>
        /// The query properties to be returned
        /// </summary>
        public List<MSQueryProperty> Properties
        {
            get { return _properties; }
        }

        /// <summary>
        /// Specifies whether stemming is enabled
        /// </summary>
        public bool EnableStemming
        {
            get { return _enableStemming; }
            set { _enableStemming = value; }
        }

        /// <summary>
        /// Specifies whether to trim duplicates from the result set
        /// </summary>
        public bool TrimDuplicates
        {
            get { return _trimDuplicates; }
            set { _trimDuplicates = value; }
        }

        /// <summary>
        /// Specifies whether to include special term results
        /// </summary>
        public bool IncludeSpecialTermResults
        {
            get { return _includeSpecialTermResults; }
            set { _includeSpecialTermResults = value; }
        }

        /// <summary>
        /// Specifies whether to ignore all noise words
        /// </summary>
        public bool IgnoreAllNoiseQuery
        {
            get { return _ignoreAllNoiseQuery; }
            set { _ignoreAllNoiseQuery = value; }
        }

        /// <summary>
        /// Specifies whether to include the relevant results
        /// </summary>
        public bool IncludeRelevantResults
        {
            get { return _includeRelevantResults; }
            set { _includeRelevantResults = value; }
        }

        /// <summary>
        /// Specifies whether keywords are implicitly ANDed
        /// </summary>
        public bool ImplicitAndBehavior
        {
            get { return _implicitAndBehavior; }
            set { _implicitAndBehavior = value; }
        }

        /// <summary>
        /// Specifies whether to include high confidence results
        /// </summary>
        public bool IncludeHighConfidenceResults
        {
            get { return _includeHighConfidenceResults; }
            set { _includeHighConfidenceResults = value; }
        }

        /// <summary>
        /// Defines the type of the search query
        /// </summary>
        public QueryType MSQueryType
        {
            get { return _queryType; }
            set { _queryType = value; }
        }

        /// <summary>
        /// The URL of the search target
        /// </summary>
        public string Target
        {
            get { return _target; }
            set { _target = value; }
        }

        /// <summary>
        /// The language
        /// </summary>
        public string Language
        {
            get { return _language; }
            set { _language = value; }
        }

        /// <summary>
        /// Add a property
        /// </summary>
        /// <param name="name">Property name</param>
        /// <param name="sortDirection">The sort direction</param>
        /// <param name="sortOrder">The sort order</param>
        public void AddProperty( string name )
        {
            // Get a new query property
            MSQueryProperty newProperty = new MSQueryProperty();

            // Set its name and indicate it should be included in results
            newProperty.PropertyName = name;
            newProperty.IncludeInResults = MSQueryProperty.YesNo.Yes;

            // Add it to the list of properties
            _properties.Add( newProperty );
        }

        /// <summary>
        /// Add a property
        /// </summary>
        /// <param name="name">Property name</param>
        /// <param name="sortDirection">The sort direction</param>
        /// <param name="sortOrder">The sort order</param>
        public void AddProperty( string name, MSQueryProperty.YesNo includeInResults, MSQueryProperty.SortDirection sortDirection, int sortOrder )
        {
            // Get a new query property
            MSQueryProperty newProperty = new MSQueryProperty();

            // Set its name and indicate whether it should be included in results
            newProperty.PropertyName = name;
            newProperty.IncludeInResults = includeInResults;

            // Set the sort direction and order
            newProperty.Direction = sortDirection;
            newProperty.SortOrder = sortOrder;

            // Add it to the list of properties
            _properties.Add( newProperty );
        }

        #endregion

        private string BuildXmlQueryString()
        {
            // Create a new XML Document
            XmlDocument xmlQueryRequestDocument = new XmlDocument();

            // Create the packet element
            XmlElement queryPacketElement = xmlQueryRequestDocument.CreateElement( "QueryPacket", "urn:Microsoft.Search.Query" );

            // Create the query element
            XmlElement queryElement = xmlQueryRequestDocument.CreateElement( "Query" );

            // Add the query element to the query packet element
            queryPacketElement.AppendChild( queryElement );

            // Add the query packet element to the query request document
            xmlQueryRequestDocument.AppendChild( queryPacketElement );

            // Add the query id element
            AddQueryIdElement( queryElement, this.QueryID );

            // Add the context element
            AddContextElement( queryElement, this.MSQueryType, this.Language, this.QueryText, this.OriginatorContext );

            // Add the supported formats element (only used by the Query method and not the QueryEx method)
            AddSupportedFormatsElement( queryElement );

            // Add the range element
            AddRangeElement( queryElement, this.StartAt, this.Count );

            // Add the properties element
            if ( this.MSQueryType == QueryType.Keyword )
            {
                AddPropertiesElement( queryElement, this.Properties );
            }

            // Addd the this elements
            AddBooleans( queryElement, this );

            return ( xmlQueryRequestDocument.OuterXml );
        }

        /// <summary>
        /// Add the query id element to the query element
        /// </summary>
        /// <param name="queryElement">The query element</param>
        /// <param name="queryID">The query id</param>
        private void AddQueryIdElement( XmlElement queryElement, Guid queryID )
        {
            // Create the query id element
            XmlElement queryIdElement = queryElement.OwnerDocument.CreateElement( "QueryId" );

            // Add the query id value
            queryElement.InnerText = queryID.ToString();

            // Append the query id element
            queryElement.AppendChild( queryIdElement );
        }

        /// <summary>
        /// Add the supported formats element to the query element
        /// </summary>
        /// <param name="queryElement">The query element</param>
        private void AddSupportedFormatsElement( XmlElement queryElement )
        {
            // Create the supported formats element
            XmlElement supportedFormatsElement = queryElement.OwnerDocument.CreateElement( "SupportedFormats" );

            // Create the format element
            XmlElement formatElement = queryElement.OwnerDocument.CreateElement( "Format" );

            // Create the revision attribute
            XmlAttribute revisionAttribute = queryElement.OwnerDocument.CreateAttribute( "revision" );
            revisionAttribute.Value = "1";

            // Add the revisions attribute
            formatElement.Attributes.Append( revisionAttribute );

            // Specify the response document
            formatElement.InnerText = "urn:Microsoft.Search.Response.Document:Document";

            // Add the format element to the supported formats element
            supportedFormatsElement.AppendChild( formatElement );

            // Append the suppported formats element
            queryElement.AppendChild( supportedFormatsElement );
        }

        /// <summary>
        /// Add the context element to the query element
        /// </summary>
        /// <param name="queryElement">The query element</param>
        /// <param name="queryRequest">The search queryRequest</param>
        /// <param name="query">The query text</param>
        private void AddContextElement( XmlElement queryElement, QueryType queryType, string language, string queryText, string originatorContext )
        {
            // Create the context element
            XmlElement contextElement = queryElement.OwnerDocument.CreateElement( "Context" );

            // Create the query text and originator context elements
            XmlElement queryTextElement = queryElement.OwnerDocument.CreateElement( "QueryText" );
            XmlElement originatorContextElement = queryElement.OwnerDocument.CreateElement( "OriginatorContext" );

            // Create the language element
            XmlAttribute languageAttribute = queryElement.OwnerDocument.CreateAttribute( "language" );
            languageAttribute.Value = language;

            // Create the type element
            XmlAttribute typeAttribute = queryElement.OwnerDocument.CreateAttribute( "type" );
            typeAttribute.Value = ( queryType == QueryType.Keyword ? "STRING" : "MSSQLFT" );

            // Add the language and type attriobutes
            queryTextElement.Attributes.Append( languageAttribute );
            queryTextElement.Attributes.Append( typeAttribute );

            // Specify the query
            queryTextElement.InnerText = queryText;

            // Add the query text element to the context element
            contextElement.AppendChild( queryTextElement );

            // Include the originator context
            originatorContextElement.InnerText = originatorContext;

            // Add the originator context element to the context element
            contextElement.AppendChild( originatorContextElement );

            // Append the context element to the query text element
            queryElement.AppendChild( contextElement );
        }

        /// <summary>
        /// Add the range element to the query element
        /// </summary>
        /// <param name="queryElement">The query element</param>
        /// <param name="queryRequest">The search queryRequest</param>
        private void AddRangeElement( XmlElement queryElement, int startAt, int count )
        {
            // Create the range element
            XmlElement rangeElement = queryElement.OwnerDocument.CreateElement( "Range" );

            // Create the "start at" element
            XmlElement startAtTextElement = queryElement.OwnerDocument.CreateElement( "StartAt" );
            startAtTextElement.InnerText = startAt.ToString();

            // Create the count element
            XmlElement countElement = queryElement.OwnerDocument.CreateElement( "Count" );
            countElement.InnerText = count.ToString();

            // Add the start at and count elements
            rangeElement.AppendChild( startAtTextElement );
            rangeElement.AppendChild( countElement );

            // Add the range element to the query element
            queryElement.AppendChild( rangeElement );
        }

        /// <summary>
        /// Add the properties element to the query element
        /// </summary>
        /// <param name="queryElement">The query element</param>
        /// <param name="queryProperties">The query properties</param>
        private void AddPropertiesElement( XmlElement queryElement, IList<MSQueryProperty> queryProperties )
        {
            if ( queryProperties != null )
            {
                // Create the properties element and the sort by properties element
                XmlElement propertiesElement = queryElement.OwnerDocument.CreateElement( "Properties" );
                XmlElement sortByPropertiesElement = queryElement.OwnerDocument.CreateElement( "SortByProperties" );

                // Declare the various elements and attributes
                XmlElement propertyElement;
                XmlAttribute nameAttribute;
                XmlElement sortByPropertyElement;
                XmlAttribute sortByPropertyNameAttribute;
                XmlAttribute sortByPropertyDirectionAttribute;
                XmlAttribute sortByPropertyOrderAttribute;

                // Loop through each property name
                foreach ( MSQueryProperty queryProperty in queryProperties )
                {
                    // Create the property element
                    propertyElement = queryElement.OwnerDocument.CreateElement( "Property" );

                    // Create the attributes
                    nameAttribute = queryElement.OwnerDocument.CreateAttribute( "name" );

                    // Assign the name attribute
                    nameAttribute.Value = queryProperty.PropertyName;

                    // Add the name attribute to the property element
                    propertyElement.Attributes.Append( nameAttribute );

                    // Append the property element to the properties element
                    propertiesElement.AppendChild( propertyElement );

                    if ( queryProperty.Direction != null )
                    {
                        sortByPropertyElement = queryElement.OwnerDocument.CreateElement( "SortByProperty" );

                        sortByPropertyNameAttribute = queryElement.OwnerDocument.CreateAttribute( "name" );
                        sortByPropertyDirectionAttribute = queryElement.OwnerDocument.CreateAttribute( "direction" );
                        sortByPropertyOrderAttribute = queryElement.OwnerDocument.CreateAttribute( "order" );

                        sortByPropertyNameAttribute.Value = queryProperty.PropertyName;
                        sortByPropertyDirectionAttribute.Value = queryProperty.Direction.ToString();
                        //Enum.Format(typeof(SortDirection), queryProperty.Direction, "G");
                        sortByPropertyOrderAttribute.Value = queryProperty.SortOrder.ToString();

                        sortByPropertyElement.Attributes.Append( sortByPropertyNameAttribute );
                        sortByPropertyElement.Attributes.Append( sortByPropertyDirectionAttribute );
                        sortByPropertyElement.Attributes.Append( sortByPropertyOrderAttribute );

                        sortByPropertiesElement.AppendChild( sortByPropertyElement );
                    }
                }

                // Append the properties element
                if ( propertiesElement.ChildNodes.Count > 0 )
                {
                    queryElement.AppendChild( propertiesElement );
                }

                // Append the sort by properties element, if it has values in it
                if ( sortByPropertiesElement.ChildNodes.Count > 0 )
                {
                    queryElement.AppendChild( sortByPropertiesElement );
                }
            }

        }

        /// <summary>
        /// Add the booleans elements of the queryRequest to the query element
        /// </summary>
        /// <param name="queryElement">The query element</param>
        /// <param name="queryRequest">The query queryRequest</param>
        private void AddBooleans( XmlElement queryElement, MSQueryRequest queryRequest )
        {
            AddNewElement( queryElement, "EnableStemming", queryRequest.EnableStemming );
            AddNewElement( queryElement, "IgnoreAllNoiseQuery", queryRequest.IgnoreAllNoiseQuery );
            AddNewElement( queryElement, "ImplicitAndBehavior", queryRequest.ImplicitAndBehavior );
            AddNewElement( queryElement, "IncludeRelevantResults", queryRequest.IncludeRelevantResults );
            AddNewElement( queryElement, "TrimDuplicates", queryRequest.TrimDuplicates );
            AddNewElement( queryElement, "IncludeSpecialTermsResults", queryRequest.IncludeSpecialTermResults );
            AddNewElement( queryElement, "IncludeHighConfidenceResults", queryRequest.IncludeHighConfidenceResults );
        }

        /// <summary>
        /// Add a new element to an element
        /// </summary>
        /// <param name="xmlElement">The element</param>
        /// <param name="name">The name of the new element</param>
        /// <param name="value">The value of the bew element</param>
        private void AddNewElement( XmlElement xmlElement, string name, bool value )
        {
            XmlElement newElement = xmlElement.OwnerDocument.CreateElement( name );
            newElement.InnerText = value.ToString().ToLower();
            xmlElement.AppendChild( newElement );
        }
    }
}
