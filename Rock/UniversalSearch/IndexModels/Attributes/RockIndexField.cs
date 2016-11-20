using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.UniversalSearch.IndexModels.Attributes
{
    /// <summary>
    /// Attribute for passing index information 
    /// </summary>
    public class RockIndexField: System.Attribute
    {
        /// <summary>
        /// Gets or sets the index.
        /// </summary>
        /// <value>
        /// The index.
        /// </value>
        public IndexType Index {
            get
            {
                return _index;
            }
            set
            {
                _index = value;
            }
        }
        private IndexType _index = IndexType.Analyzed;

        /// <summary>
        /// Gets or sets the boost.
        /// </summary>
        /// <value>
        /// The boost.
        /// </value>
        public double Boost {
            get
            {
                return _boost;
            }
            set
            {
                _boost = value;
            }
        }
        private double _boost = 1;

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public IndexFieldType Type {
            get
            {
                return _type;
            }
            set
            {
                _type = value;
            }
        }
        private IndexFieldType _type = IndexFieldType.String;
    }

    /// <summary>
    /// Type of Index
    /// </summary>
    public enum IndexType {

        /// <summary>
        /// Analyzed
        /// </summary>
        Analyzed = 0,

        /// <summary>
        /// Not Analyzed
        /// </summary>
        NotAnalyzed = 1,

        /// <summary>
        /// Not Indexed
        /// </summary>
        NotIndexed = 2
    }

    /// <summary>
    /// Type of Index Field
    /// </summary>
    public enum IndexFieldType
    {
        /// <summary>
        /// String Field
        /// </summary>
        String,

        /// <summary>
        /// Number Field
        /// </summary>
        Number,

        /// <summary>
        /// Boolean Field
        /// </summary>
        Boolean,

        /// <summary>
        /// Date Field
        /// </summary>
        Date
    }
}
