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
namespace Rock.UniversalSearch.Crawler
{
    /// <summary>
    /// Crawled Page
    /// </summary>
    public class CrawledPage
    {
        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public CrawledPage() { }

        #endregion

        #region Private Instance Fields

        private int _size;
        private string _text;
        private string _url;
        private int _viewstateSize;
        private string _title;
        private bool? _allowsIndex;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the size.
        /// </summary>
        /// <value>
        /// The size.
        /// </value>
        public int Size
        {
            get { return _size; }
        }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>
        /// The text.
        /// </value>
        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                _size = value.Length;
            }
        }

        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        /// <value>
        /// The URL.
        /// </value>
        public string Url
        {
            get { return _url; }
            set { _url = value; }
        }

        /// <summary>
        /// Gets or sets the size of the viewstate.
        /// </summary>
        /// <value>
        /// The size of the viewstate.
        /// </value>
        public int ViewstateSize
        {
            get { return _viewstateSize; }
            set { _viewstateSize = value; }
        }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [allows index].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allows index]; otherwise, <c>false</c>.
        /// </value>
        public bool AllowsIndex
        {
            get
            {
                if (_allowsIndex == null )
                {
                    return true;
                }
                else
                {
                    return _allowsIndex.Value;
                }
            }
            set
            {
                _allowsIndex = value;
            }
        }

        #endregion
    }
}
