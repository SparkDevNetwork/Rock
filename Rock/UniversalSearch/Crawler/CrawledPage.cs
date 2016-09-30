using System;

namespace Rock.UniversalSearch.Crawler
{
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

        public int Size
        {
            get { return _size; }
        }

        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                _size = value.Length;
            }
        }

        public string Url
        {
            get { return _url; }
            set { _url = value; }
        }

        public int ViewstateSize
        {
            get { return _viewstateSize; }
            set { _viewstateSize = value; }
        }

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
