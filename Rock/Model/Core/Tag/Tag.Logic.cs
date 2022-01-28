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

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using Rock.Lava;
using Rock.Security;

namespace Rock.Model
{
    public partial class Tag
    {
        /// <summary>
        /// The Regular Expression used to determine a valid tag name. This regex will return true if the string does not contain angle brackets, percent, or ampersand.
        /// </summary>
        public const string VALIDATOR_REGEX_BLACKLIST = @"^((?!<)(?!>)(?!%)(?!&).)*$";

        #region Properties

        /// <summary>
        /// Gets the parent security authority of this Tag. Where security is inherited from.
        /// </summary>
        /// <value>
        /// The parent authority.
        /// </value>
        public override Security.ISecured ParentAuthority
        {
            get
            {
                if ( this.CategoryId.HasValue )
                {
                    return Rock.Web.Cache.CategoryCache.Get( this.CategoryId.Value ) ?? base.ParentAuthority;
                }

                return this.Category != null ? this.Category : base.ParentAuthority;
            }
        }

        /// <summary>
        /// A dictionary of actions that this class supports and the description of each.
        /// </summary>
        public override Dictionary<string, string> SupportedActions
        {
            get
            {
                if ( _supportedActions == null )
                {
                    _supportedActions = new Dictionary<string, string>();
                    _supportedActions.Add( Authorization.VIEW, "The roles and/or users that have access to view." );
                    _supportedActions.Add( Authorization.TAG, "The roles and/or users that have access to tag items." );
                    _supportedActions.Add( Authorization.EDIT, "The roles and/or users that have access to edit." );
                    _supportedActions.Add( Authorization.ADMINISTRATE, "The roles and/or users that have access to administrate." );
                }

                return _supportedActions;
            }
        }

        private Dictionary<string, string> _supportedActions;

        /// <summary>
        /// Returns a hexidecimal value for the BackgroundColor value.
        /// </summary>
        /// <returns>System.String.</returns>
        [NotMapped]
        [LavaVisible]
        public string BackgroundColorHex
        {
            get
            {
                if ( string.IsNullOrEmpty( this.BackgroundColor ) )
                {
                    return null;
                }

                // Already HEX
                if ( this.BackgroundColor.StartsWith( "#" ) )
                {
                    return this.BackgroundColor;
                }

                // rgb(...) or rgba(...) string
                if ( this.BackgroundColor.ToLowerInvariant().StartsWith( "rgb" ) )
                {
                    var startParen = this.BackgroundColor.IndexOf( "(" ) + 1;
                    var bgColor = this.BackgroundColor.SubstringSafe(
                        startParen, this.BackgroundColor.LastIndexOf( ")" ) - startParen );
                    if ( !string.IsNullOrEmpty( bgColor ) )
                    {
                        var colorArray = bgColor.Split( ',' );
                        switch ( colorArray.Length )
                        {
                            case 3:
                                {
                                    // R,G,B from our color picker
                                    var color = Color.FromArgb( colorArray[0].AsInteger(), colorArray[1].AsInteger(), colorArray[2].AsInteger() );
                                    return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
                                }
                            case 4:
                                {
                                    // R,G,B,A from our color picker
                                    var color = Color.FromArgb( ( int ) ( 255 * colorArray[3].AsDouble() ), colorArray[0].AsInteger(), colorArray[1].AsInteger(), colorArray[2].AsInteger() );
                                    return $"#{color.R:X2}{color.G:X2}{color.B:X2}{color.A:X2}";
                                }
                        }
                    }
                }

                // If all else fails just return the original string value.
                return this.BackgroundColor;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Determines whether specified tag name is allowed.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <returns>
        ///   <c>true</c> if [is valid tag name] [the specified tag name]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsValidTagName( string tagName )
        {
            return new System.Text.RegularExpressions.Regex( VALIDATOR_REGEX_BLACKLIST ).IsMatch( tagName );
        }

        /// <summary>
        /// Determines whether the specified action is authorized. If the tag is personal and
        /// owned by the person, it returns true, but if it's personal and NOT owned by the person
        /// it returns false -- otherwise (not a personal tag) it returns what the chain of authority
        ///  determines, but note: the parent authority is the category (if the tag has a category).
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <returns>True if the person is authorized; false otherwise.</returns>
        public override bool IsAuthorized( string action, Person person )
        {
            if ( this.OwnerPersonAlias != null && person != null && this.OwnerPersonAlias.PersonId == person.Id )
            {
                // always allow people to do anything with their own tags
                return true;
            }
            else if ( this.OwnerPersonAlias != null && person != null && this.OwnerPersonAlias.PersonId != person.Id )
            {
                // always prevent people from doing anything with someone else's tags
                return false;
            }
            else
            {
                return base.IsAuthorized( action, person );
            }
        }

        #endregion Methods
    }
}
