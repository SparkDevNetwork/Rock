using System;

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
