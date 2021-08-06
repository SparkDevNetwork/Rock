using System;
using System.Runtime.Serialization;
using System.Text;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Rock.Rest
{
    /// <summary>
    /// Data used to request another page of results.
    /// </summary>
    /// <remarks>
    /// This has a hard dependency on Newtonsoft so that we can use JToken
    /// to translate the value back to it's proper type once we know what
    /// property it is for.
    /// </remarks>
    public class PaginationCursor
    {
        #region Properties

        /// <summary>
        /// Gets or sets the name of the property being ordered by.
        /// </summary>
        /// <value>
        /// The name of the property being ordered by.
        /// </value>
        [JsonProperty( PropertyName = "op", DefaultValueHandling = DefaultValueHandling.Ignore )]
        public string OrderByProperty { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether ordering in ascending order.
        /// </summary>
        /// <value>
        ///   <c>true</c> if order in ascending order; otherwise, <c>false</c>.
        /// </value>
        [JsonProperty( PropertyName = "oa", DefaultValueHandling = DefaultValueHandling.Ignore )]
        public bool OrderByAscending { get; set; }

        /// <summary>
        /// Gets or sets the last value seen in the order by property.
        /// </summary>
        /// <value>
        /// The last value seen in the order by property.
        /// </value>
        [JsonProperty( PropertyName = "lv", DefaultValueHandling = DefaultValueHandling.Ignore )]
        public JToken LastOrderValue { get; set; }

        /// <summary>
        /// Gets or sets the last identifier seen.
        /// </summary>
        /// <value>
        /// The last identifier seen.
        /// </value>
        [JsonProperty( PropertyName = "li", DefaultValueHandling = DefaultValueHandling.Ignore )]
        public long LastId { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Decodes the specified encoded cursor.
        /// </summary>
        /// <param name="encodedCursor">The encoded cursor.</param>
        /// <returns>The decoded <see cref="PaginationCursor"/>.</returns>
        public static PaginationCursor Decode( string encodedCursor )
        {
            var cursorJson = Encoding.UTF8.GetString( Convert.FromBase64String( encodedCursor ) );

            return cursorJson.FromJsonOrNull<PaginationCursor>();
        }

        /// <summary>
        /// Encodes the specified cursor.
        /// </summary>
        /// <param name="cursor">The cursor to be encoded.</param>
        /// <returns>A string that contains the encoded data.</returns>
        public static string Encode( PaginationCursor cursor )
        {
            return Convert.ToBase64String( Encoding.UTF8.GetBytes( cursor.ToJson() ) );
        }

        #endregion
    }
}
