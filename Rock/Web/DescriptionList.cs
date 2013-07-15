//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;

namespace Rock.Web
{
    /// <summary>
    /// Helps generate Bootstrap Description List HTML (<dl>...</dl>)
    /// </summary>
    public class DescriptionList
    {
        /// <summary>
        /// The term description list
        /// </summary>
        private Dictionary<string, string> termDescriptionList = new Dictionary<string, string>();

        /// <summary>
        /// Adds the specified term.
        /// </summary>
        /// <param name="term">The term.</param>
        /// <param name="description">The description.</param>
        /// <returns></returns>
        public DescriptionList Add( string term, string description )
        {
            termDescriptionList.Add( term, description );
            return this;
        }

        /// <summary>
        /// Adds the specified term.
        /// </summary>
        /// <param name="term">The term.</param>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        public DescriptionList Add( string term, Rock.Model.Person person )
        {
            if ( person != null )
            {
                termDescriptionList.Add( term, person.FullName );
            }
            else
            {
                termDescriptionList.Add( term, null );
            }
            
            return this;
        }

        /// <summary>
        /// Adds the specified term.
        /// </summary>
        /// <param name="term">The term.</param>
        /// <param name="dateTime">The date time.</param>
        /// <param name="format">The format.</param>
        /// <returns></returns>
        public DescriptionList Add( string term, DateTime? dateTime, string format = "g" )
        {
            if ( dateTime != null )
            {
                termDescriptionList.Add( term, dateTime.Value.ToString(format) );
            }
            else
            {
                termDescriptionList.Add( term, null );
            }

            return this;
        }

        /// <summary>
        /// Adds the specified term.
        /// </summary>
        /// <param name="term">The term.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public DescriptionList Add( string term, int? value )
        {
            if ( value != null )
            {
                termDescriptionList.Add( term, value.ToString() );
            }
            else
            {
                termDescriptionList.Add( term, null );
            }

            return this;
        }

        /// <summary>
        /// Starts the second column.
        /// </summary>
        /// <returns></returns>
        public DescriptionList StartSecondColumn()
        {
            termDescriptionList.Add( ColumnBreak, string.Empty );
            return this;
        }

        /// <summary>
        /// Gets the HTML.
        /// </summary>
        /// <value>
        /// The HTML.
        /// </value>
        public string Html
        {
            get
            {
                string descriptionFormat = "<dt>{0}</dt><dd>{1}</dd>";

                string result = @"<div class='span6'><dl>";

                foreach ( var pair in termDescriptionList )
                {
                    string displayValue = pair.Value;
                    if ( string.IsNullOrWhiteSpace( displayValue ) )
                    {
                        displayValue = Rock.Constants.None.TextHtml;
                    }


                    if ( pair.Key == ColumnBreak )
                    {
                        result += @"</dl></div><div class='span6'><dl>";
                    }
                    else
                    {
                        result += string.Format( descriptionFormat, pair.Key, displayValue );
                    }
                }

                result += @"</dl></div>";

                return result;
            }
        }

        /// <summary>
        /// The column break
        /// </summary>
        private const string ColumnBreak = "<<ColumnBreak>>";
    }
}
