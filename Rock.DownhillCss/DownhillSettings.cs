using System;
using System.Collections.Generic;
using System.Text;

namespace Rock.DownhillCss
{
    public class DownhillSettings
    {
        /// <summary>
        /// Gets or sets the platform.
        /// </summary>
        /// <value>
        /// The platform.
        /// </value>
        public DownhillPlatform Platform { get; set; } = DownhillPlatform.Web;

        /// <summary>
        /// Gets or sets the spacing base.
        /// </summary>
        /// <value>
        /// The spacing base.
        /// </value>
        public decimal SpacingBase
        {
            get
            {
                if ( _spacingBase != 0 )
                {
                    return _spacingBase;
                }

                if ( this.Platform == DownhillPlatform.Mobile )
                {
                    _spacingBase = 10;
                }
                else
                {
                    _spacingBase = 1;
                }

                return _spacingBase;
            }
            set
            {
                _spacingBase = value;
            }
        }
        private decimal _spacingBase = 0;

        /// <summary>
        /// Gets the spacing units.
        /// </summary>
        /// <value>
        /// The spacing units.
        /// </value>
        public string SpacingUnits
        {
            get
            {
                if ( _spacingUnits != null )
                {
                    return _spacingUnits;
                }

                if ( this.Platform == DownhillPlatform.Mobile )
                {
                    _spacingUnits = string.Empty;
                }
                else
                {
                    _spacingUnits = "rem";
                }

                return _spacingUnits;
            }
        }
        private string _spacingUnits;

        /// <summary>
        /// Gets the border units.
        /// </summary>
        /// <value>
        /// The border units.
        /// </value>
        public string BorderUnits
        {
            get
            {
                if ( _borderUnits != null )
                {
                    return _borderUnits;
                }

                if ( this.Platform == DownhillPlatform.Mobile )
                {
                    _borderUnits = string.Empty;
                }
                else
                {
                    _borderUnits = "px";
                }

                return _borderUnits;
            }
        }
        private string _borderUnits;

        /// <summary>
        /// Gets the border units.
        /// </summary>
        /// <value>
        /// The border units.
        /// </value>
        public string FontUnits
        {
            get
            {
                if ( _fontUnits != null )
                {
                    return _fontUnits;
                }

                if ( this.Platform == DownhillPlatform.Mobile )
                {
                    _fontUnits = string.Empty;
                }
                else
                {
                    _fontUnits = "rem";
                }

                return _fontUnits;
            }
        }
        private string _fontUnits;

        /// <summary>
        /// Gets or sets the default size of the font.
        /// </summary>
        /// <value>
        /// The default size of the font.
        /// </value>
        public decimal FontSizeDefault
        {
            get
            {
                if ( _fontSizeDefault != 0 )
                {
                    return _fontSizeDefault;
                }

                if ( this.Platform == DownhillPlatform.Mobile )
                {
                    _fontSizeDefault = 14;
                }
                else
                {
                    _fontSizeDefault = 1; // rem
                }

                return _fontSizeDefault;
            }
            set
            {
                _fontSizeDefault = value;
            }
        }
        private decimal _fontSizeDefault;

        /// <summary>
        /// Gets or sets the application colors.
        /// </summary>
        /// <value>
        /// The application colors.
        /// </value>
        public ApplicationColors ApplicationColors { get; set; } = new ApplicationColors();

        /// <summary>
        /// Gets or sets the base radius.
        /// </summary>
        /// <value>
        /// The base radius.
        /// </value>
        public decimal RadiusBase { get; set; } = 0;

        /// <summary>
        /// Gets or sets the color of the text.
        /// </summary>
        /// <value>
        /// The color of the text.
        /// </value>
        public string TextColor { get; set; } = "#676767";

        /// <summary>
        /// Gets or sets the color of the heading.
        /// </summary>
        /// <value>
        /// The color of the heading.
        /// </value>
        public string HeadingColor { get; set; } = "#333333";

        /// <summary>
        /// Gets or sets the color of the background.
        /// </summary>
        /// <value>
        /// The color of the background.
        /// </value>
        public string BackgroundColor { get; set; } = "#ffffff";
    }

    public enum DownhillPlatform
    {
        Mobile = 0,
        Web = 1
    }
}
