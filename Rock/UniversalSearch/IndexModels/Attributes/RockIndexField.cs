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

    public enum IndexType {
        Analyzed = 0,
        NotAnalyzed = 1,
        NotIndexed = 2
    }

    public enum IndexFieldType { String, Number, Boolean, Date }
}
