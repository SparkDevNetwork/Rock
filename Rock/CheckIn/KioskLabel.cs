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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.CheckIn
{
    /// <summary>
    /// Cached Check-in Label
    /// </summary>
    [DataContract]
    [Serializable]
    public class KioskLabel : ItemCache<KioskLabel>
    {
        /// <summary>
        /// Prevents a default instance of the <see cref="KioskLabel"/> class from being created.
        /// </summary>
        private KioskLabel()
        {
        }

        /// <summary>
        /// Gets or sets the GUID.
        /// </summary>
        /// <value>
        /// The GUID.
        /// </value>
        [DataMember]
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the type of the label.
        /// </summary>
        /// <value>
        /// The type of the label.
        /// </value>
        [DataMember]
        public KioskLabelType LabelType { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [DataMember]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        /// <value>
        /// The URL.
        /// </value>
        [DataMember]
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the content of the file.
        /// </summary>
        /// <value>
        /// The content of the file.
        /// </value>
        [DataMember]
        public string FileContent { get; set; }

        /// <summary>
        /// Gets the merge fields.
        /// </summary>
        /// <value>
        /// The merge fields.
        /// </value>
        public Dictionary<string, string> MergeFields
        {
            get
            {
                var mergeFields = new Dictionary<string, string>();
                if ( _mergeCodeDefinedValueIds?.Any() != true )
                {
                    // no merge fields
                    return mergeFields;
                }

                foreach ( var keyDefinedValueId in _mergeCodeDefinedValueIds )
                {
                    var definedValue = DefinedValueCache.Get( keyDefinedValueId.Value );
                    if ( definedValue != null )
                    {
                        string mergeField = definedValue.GetAttributeValue( "MergeField" );
                        if ( mergeField != null )
                        {
                            mergeFields.AddOrIgnore( keyDefinedValueId.Key, mergeField );
                        }
                    }
                }

                return mergeFields;
            }

            set
            {
                // not supported
            }
        }

        [DataMember]
        private Dictionary<string, int> _mergeCodeDefinedValueIds { get; set; } = null;

        #region Static Methods

        /// <summary>
        /// Reads the specified label by guid.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        /// <returns></returns>
        [RockObsolete( "1.8" )]
        [Obsolete( "Use Get( Guid guid ) instead.")]
        public static KioskLabel Read( Guid guid )
        {
            return Get( guid );
        }

        /// <summary>
        /// Gets the specified label by guid.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        /// <returns></returns>
        public static KioskLabel Get( Guid guid )
        {
            var now = RockDateTime.Now;
            var timespan = now.Date.AddDays( 1 ).Subtract( now );
            return GetOrAddExisting( guid.ToString(), () => Create( guid ), timespan );
        }

        /// <summary>
        /// Creates the KioskLabel
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        /// <returns></returns>
        private static KioskLabel Create( Guid guid )
        {
            using ( var rockContext = new RockContext() )
            {
                var file = new BinaryFileService( rockContext ).Get( guid );
                if ( file != null )
                {
                    var label = new KioskLabel();
                    label.Guid = file.Guid;
                    label.Url = string.Format( "{0}GetFile.ashx?id={1}", System.Web.VirtualPathUtility.ToAbsolute( "~" ), file.Id );
                    label._mergeCodeDefinedValueIds = new Dictionary<string, int>();
                    label.FileContent = file.ContentsToString();

                    file.LoadAttributes( rockContext );

                    label.LabelType = file.GetAttributeValue( "core_LabelType" ).ConvertToEnum<KioskLabelType>();

                    string attributeValue = file.GetAttributeValue( "MergeCodes" );
                    if ( !string.IsNullOrWhiteSpace( attributeValue ) )
                    {
                        string[] nameValues = attributeValue.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries );
                        foreach ( string nameValue in nameValues )
                        {
                            string[] nameAndValue = nameValue.Split( new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries );

                            if ( nameAndValue.Length == 2 )
                            {
                                string mergeCodeKey = nameAndValue[0];
                                int? definedValueId = nameAndValue[1].AsIntegerOrNull();
                                if ( definedValueId.HasValue )
                                {
                                    label._mergeCodeDefinedValueIds.AddOrIgnore( mergeCodeKey, definedValueId.Value );
                                }
                            }
                        }
                    }

                    return label;
                }
            }

            return null;
        }

        /// <summary>
        /// Flushes the specified guid.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        [RockObsolete( "1.8" )]
        [Obsolete( "Use Remove( Guid guid ) instead.")]
        public static void Flush( Guid guid )
        {
            Remove( guid );
        }

        /// <summary>
        /// Flushes the specified guid.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        public static void Remove( Guid guid)
        {
            Remove( guid.ToString() );
        }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public enum KioskLabelType
    {
        /// <summary>
        /// The family
        /// </summary>
        Family = 0,

        /// <summary>
        /// The person
        /// </summary>
        Person = 1,

        /// <summary>
        /// The location
        /// </summary>
        Location = 2,

        /// <summary>
        /// Print for each person being checked out
        /// </summary>
        Checkout = 3,
    }
}