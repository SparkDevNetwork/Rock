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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Imaging;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using Humanizer;
using Humanizer.Localisation;
using Ical.Net;
using ImageResizer;
using Rock;
using Rock.Attribute;
using Rock.Cms.StructuredContent;
using Rock.Data;
using Rock.Enums.Core;
using Rock.Logging;
using Rock.Model;
using Rock.Security;
using Rock.Utilities;
using Rock.Utility;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using UAParser;

namespace Rock.Lava
{
    /// <summary>
    /// Defines filter methods available for use with the Lava library.
    /// </summary>
    /// <remarks>
    /// This class is marked for internal use because it should only be used in the context of resolving a Lava template.
    /// Filters should only be defined in this class if they are specific to the Rock web application, as these definitions
    /// override any implementation of the same name defined in the TemplateFilters class.
    /// Filters that are confirmed as suitable for use with both the Rock Web and Rock Mobile applications should be
    /// implemented in the TemplateFilters class.
    /// </remarks>
    internal static partial class LavaFilters
    {
        /// <summary>
        /// Sets the person preference.
        /// </summary>
        /// <param name="context">The Lava context.</param>
        /// <param name="input">The filter input.</param>
        /// <param name="settingKey">The setting key.</param>
        /// <param name="settingValue">The setting value.</param>
        public static void SetUserPreference( ILavaRenderContext context, object input, string settingKey, string settingValue )
        {
            Person person = null;

            var rockContext = LavaHelper.GetRockContextFromLavaContext( context );

            if ( input is int )
            {
                // If the input is an integer, assume it's a PersonId and retrieve the associated Person.
                person = new PersonService( rockContext ).Get( ( int ) input );
            }
            else if ( input is Person )
            {
                // If the input is a Person object, reference it directly.
                person = ( Person ) input;
            }

            if ( person != null )
            {
                var preferences = PersonPreferenceCache.GetPersonPreferenceCollection( person );

                preferences.SetValue( settingKey, settingValue );
                preferences.Save();

                rockContext.QueryCount++; // The method above won't allow passing in a RockContext so we'll just increment a query manually.
                if ( rockContext.QueryMetricDetailLevel == QueryMetricDetailLevel.Full )
                {
                    rockContext.QueryMetricDetails.Add( new QueryMetricDetail
                    {
                        Sql = "Query metrics are not available for SetUserPreference."
                    } );
                }
            }
        }

        /// <summary>
        /// Gets a user preference for the specified person.
        /// </summary>
        /// <param name="context">The Lava context.</param>
        /// <param name="input">The filter input, either a PersonId or a Person object.</param>
        /// <param name="settingKey">The setting key name.</param>
        /// <returns>The value of the user preference.</returns>
        public static string GetUserPreference( ILavaRenderContext context, object input, string settingKey )
        {
            Person person = null;

            var rockContext = LavaHelper.GetRockContextFromLavaContext( context );

            if ( input is int )
            {
                // If the input is an integer, assume it's a PersonId and retrieve the associated Person.
                person = new PersonService( rockContext ).Get( ( int ) input );
            }
            else if ( input is Person )
            {
                person = ( Person ) input;
            }

            if ( person != null )
            {
                var preferences = PersonPreferenceCache.GetPersonPreferenceCollection( person );

                rockContext.QueryCount++; // The method above won't allow passing in a RockContext so we'll just increment a query manually.
                if ( rockContext.QueryMetricDetailLevel == QueryMetricDetailLevel.Full )
                {
                    rockContext.QueryMetricDetails.Add( new QueryMetricDetail
                    {
                        Sql = "Query metrics are not available for GetUserPreference."
                    } );
                }

                return preferences.GetValue( settingKey );
            }

            return string.Empty;
        }

        /// <summary>
        /// Deletes the user preference.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="settingKey">The setting key.</param>
        public static void DeleteUserPreference( ILavaRenderContext context, object input, string settingKey )
        {
            Person person = null;

            var rockContext = LavaHelper.GetRockContextFromLavaContext( context );

            if ( input is int )
            {
                person = new PersonService( rockContext ).Get( ( int ) input );
            }
            else if ( input is Person )
            {
                person = ( Person ) input;
            }

            if ( person != null )
            {
                var preferences = PersonPreferenceCache.GetPersonPreferenceCollection( person );

                preferences.SetValue( settingKey, string.Empty );
                preferences.Save();

                rockContext.QueryCount++; // The method above won't allow passing in a RockContext so we'll just increment a query manually.
                if ( rockContext.QueryMetricDetailLevel == QueryMetricDetailLevel.Full )
                {
                    rockContext.QueryMetricDetails.Add( new QueryMetricDetail
                    {
                        Sql = "Query metrics are not available for DeleteUserPreference."
                    } );
                }
            }
        }

        /// <summary>
        /// Persons the by identifier.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static Person PersonById( ILavaRenderContext context, object input )
        {
            if ( input == null )
            {
                return null;
            }

            int personId = -1;

            if ( !Int32.TryParse( input.ToString(), out personId ) )
            {
                return null;
            }

            return new PersonService( LavaHelper.GetRockContextFromLavaContext( context ) ).Get( personId );
        }

        /// <summary>
        /// Persons the by unique identifier.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static Person PersonByGuid( ILavaRenderContext context, object input )
        {
            if ( input == null )
            {
                return null;
            }

            Guid? personGuid = input.ToString().AsGuidOrNull();

            if ( personGuid.HasValue )
            {
                return new PersonService( LavaHelper.GetRockContextFromLavaContext( context ) ).Get( personGuid.Value );
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Loads a person by their alias guid
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static Person PersonByAliasGuid( ILavaRenderContext context, object input )
        {
            if ( input == null )
            {
                return null;
            }

            Guid? personAliasGuid = input.ToString().AsGuidOrNull();

            if ( personAliasGuid.HasValue )
            {
                return new PersonAliasService( LavaHelper.GetRockContextFromLavaContext( context ) ).Get( personAliasGuid.Value ).Person;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Persons the by alias identifier.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static Person PersonByAliasId( ILavaRenderContext context, object input )
        {
            if ( input == null )
            {
                return null;
            }

            int personAliasId = -1;

            if ( !Int32.TryParse( input.ToString(), out personAliasId ) )
            {
                return null;
            }

            return new PersonAliasService( LavaHelper.GetRockContextFromLavaContext( context ) ).Get( personAliasId ).Person;
        }

        /// <summary>
        /// Returns a Person from the person alternate identifier.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static Person PersonByPersonAlternateId( ILavaRenderContext context, object input )
        {
            if ( input == null )
            {
                return null;
            }

            var alternateId = input.ToString();

            if ( alternateId.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var alternateIdSearchTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_SEARCH_KEYS_ALTERNATE_ID.AsGuid() ).Id;
            return new PersonSearchKeyService( LavaHelper.GetRockContextFromLavaContext( context ) ).Queryable().AsNoTracking()
                    .Where( k =>
                        k.SearchValue == alternateId
                        && k.SearchTypeValueId == alternateIdSearchTypeValueId )
                    .Select( k => k.PersonAlias.Person )
                    .FirstOrDefault();
        }

        /// <summary>
        /// Gets the person's alternate identifier.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static string GetPersonAlternateId( ILavaRenderContext context, object input )
        {
            Person person = null;

            var rockContext = LavaHelper.GetRockContextFromLavaContext( context );

            if ( input is int )
            {
                person = new PersonService( rockContext ).Get( ( int ) input );
            }
            else if ( input is Person )
            {
                person = ( Person ) input;
            }

            if ( person != null )
            {
                var alternateIdSearchTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_SEARCH_KEYS_ALTERNATE_ID.AsGuid() ).Id;
                return person.GetPersonSearchKeys( rockContext ).Where( k => k.SearchTypeValueId == alternateIdSearchTypeValueId )?.FirstOrDefault()?.SearchValue;
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the parents of the person
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static List<Person> Parents( ILavaRenderContext context, object input )
        {
            var person = GetPerson( input, context );
            if ( person != null )
            {
                var adultGuid = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid();
                var parents = new PersonService( LavaHelper.GetRockContextFromLavaContext( context ) )
                    .GetFamilyMembers( person.Id, includeSelf: true )
                    .Where( m => m.GroupRole.Guid == adultGuid )
                    .Select( a => a.Person )
                    .ToList();

                // If the list includes the target Person, they are a parent themselves.
                if ( !parents.Any( c => c.Id == person.Id ) )
                {
                    return parents.ToList();
                }
            }

            return new List<Person>();
        }

        /// <summary>
        /// Gets the children of the person
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static List<Person> Children( ILavaRenderContext context, object input )
        {
            var person = GetPerson( input, context );
            if ( person != null )
            {
                var childGuid = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid();

                var children = new PersonService( LavaHelper.GetRockContextFromLavaContext( context ) )
                    .GetFamilyMembers( person.Id, includeSelf: true )
                    .Where( m => m.GroupRole.Guid == childGuid )
                    .Select( a => a.Person )
                    .ToList();

                // If the list includes the target Person, they are a child themselves.
                if ( !children.Any( c => c.Id == person.Id ) )
                {
                    return children.ToList();
                }
            }

            return new List<Person>();
        }

        /// <summary>
        /// Gets an address for a person object
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="addressType">Type of the address.</param>
        /// <param name="qualifier">The qualifier.</param>
        /// <returns></returns>
        public static string Address( ILavaRenderContext context, object input, string addressType, string qualifier = "" )
        {
            Person person = GetPerson( input, context );

            if ( person != null )
            {
                var familyGroupTypeId = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY ).Id;

                // Get all GroupMember records tied to this Person and the Family GroupType. Note that a given Person can belong to multiple families.
                var groupMemberQuery = new GroupMemberService( LavaHelper.GetRockContextFromLavaContext( context ) )
                    .Queryable( "GroupLocations.Location" )
                    .AsNoTracking()
                    .Where( m => m.PersonId == person.Id &&
                                 m.Group.GroupTypeId == familyGroupTypeId );

                /*
                    8/5/2020 - JH
                    If this Person has a primary family defined, use that to determine the Group whose Location should be used.
                    Otherwise, simply use the previous logic leveraging GroupMember.GroupOrder.

                    Reason: Change Lava 'Address' filter to use person's primary family property.
                */
                if ( person.PrimaryFamilyId.HasValue )
                {
                    groupMemberQuery = groupMemberQuery
                        .Where( m => m.GroupId == person.PrimaryFamilyId.Value );
                }

                // Get all GroupLocations tied to the GroupMembers queried up to this point.
                var groupLocationQuery = groupMemberQuery
                    .OrderBy( m => m.GroupOrder ?? int.MaxValue )
                    .SelectMany( m => m.Group.GroupLocations );

                switch ( addressType )
                {
                    case "Mailing":
                        groupLocationQuery = groupLocationQuery
                            .Where( gl => gl.IsMailingLocation == true );
                        break;
                    case "MapLocation":
                        groupLocationQuery = groupLocationQuery
                            .Where( gl => gl.IsMappedLocation == true );
                        break;
                    default:
                        groupLocationQuery = groupLocationQuery
                            .Where( gl => gl.GroupLocationTypeValue.Value == addressType );
                        break;
                }

                // Select the first GroupLocation's Location.
                Location location = groupLocationQuery
                    .Select( gl => gl.Location )
                    .FirstOrDefault();

                if ( location != null )
                {
                    if ( qualifier == "" )
                    {
                        return location.GetFullStreetAddress();
                    }
                    else
                    {
                        var matches = Regex.Matches( qualifier, @"\[\[([^\]]+)\]\]" );
                        foreach ( var match in matches )
                        {
                            string propertyKey = match.ToString().Replace( "[", "" );
                            propertyKey = propertyKey.ToString().Replace( "]", "" );
                            propertyKey = propertyKey.ToString().Replace( " ", "" );

                            switch ( propertyKey )
                            {
                                case "Street1":
                                    qualifier = qualifier.Replace( match.ToString(), location.Street1 );
                                    break;
                                case "Street2":
                                    qualifier = qualifier.Replace( match.ToString(), location.Street2 );
                                    break;
                                case "City":
                                    qualifier = qualifier.Replace( match.ToString(), location.City );
                                    break;
                                case "County":
                                    qualifier = qualifier.Replace( match.ToString(), location.County );
                                    break;
                                case "State":
                                    qualifier = qualifier.Replace( match.ToString(), location.State );
                                    break;
                                case "PostalCode":
                                case "Zip":
                                    qualifier = qualifier.Replace( match.ToString(), location.PostalCode );
                                    break;
                                case "Country":
                                    qualifier = qualifier.Replace( match.ToString(), location.Country );
                                    break;
                                case "GeoPoint":
                                    if ( location.GeoPoint != null )
                                    {
                                        qualifier = qualifier.Replace( match.ToString(), string.Format( "{0},{1}", location.GeoPoint.Latitude.ToString(), location.GeoPoint.Longitude.ToString() ) );
                                    }
                                    else
                                    {
                                        qualifier = qualifier.Replace( match.ToString(), "" );
                                    }

                                    break;
                                case "Latitude":
                                    if ( location.GeoPoint != null )
                                    {
                                        qualifier = qualifier.Replace( match.ToString(), location.GeoPoint.Latitude.ToString() );
                                    }
                                    else
                                    {
                                        qualifier = qualifier.Replace( match.ToString(), "" );
                                    }

                                    break;
                                case "Longitude":
                                    if ( location.GeoPoint != null )
                                    {
                                        qualifier = qualifier.Replace( match.ToString(), location.GeoPoint.Longitude.ToString() );
                                    }
                                    else
                                    {
                                        qualifier = qualifier.Replace( match.ToString(), "" );
                                    }

                                    break;
                                case "FormattedAddress":
                                    qualifier = qualifier.Replace( match.ToString(), location.FormattedAddress );
                                    break;
                                case "FormattedHtmlAddress":
                                    qualifier = qualifier.Replace( match.ToString(), location.FormattedHtmlAddress );
                                    break;
                                case "Guid":
                                    qualifier = qualifier.Replace( match.ToString(), location.Guid.ToString() );
                                    break;
                                default:
                                    qualifier = qualifier.Replace( match.ToString(), "" );
                                    break;
                            }
                        }

                        return qualifier;
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the Spouse of the selected person
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static Person Spouse( ILavaRenderContext context, object input )
        {
            Person person = GetPerson( input, context );

            if ( person == null )
            {
                return null;
            }
            return person.GetSpouse( LavaHelper.GetRockContextFromLavaContext( context ) );
        }

        /// <summary>
        /// Gets the Head of Household of the selected person
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static Person HeadOfHousehold( ILavaRenderContext context, object input )
        {
            Person person = GetPerson( input, context );

            if ( person == null )
            {
                return null;
            }
            return person.GetHeadOfHousehold( LavaHelper.GetRockContextFromLavaContext( context ) );
        }

        /// <summary>
        /// Families the salutation.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="includeChildren">if set to <c>true</c> [include children].</param>
        /// <param name="includeInactive">if set to <c>true</c> [include inactive].</param>
        /// <param name="useFormalNames">if set to <c>true</c> [use formal names].</param>
        /// <param name="finalfinalSeparator">The finalfinal separator.</param>
        /// <param name="separator">The separator.</param>
        /// <returns></returns>
        public static string FamilySalutation( ILavaRenderContext context, object input, bool includeChildren = false, bool includeInactive = true, bool useFormalNames = false, string finalfinalSeparator = "&", string separator = "," )
        {
            Person person = GetPerson( input, context );

            if ( person == null )
            {
                return null;
            }

            var args = new Person.CalculateFamilySalutationArgs( includeChildren )
            {
                IncludeInactive = includeInactive,
                UseFormalNames = useFormalNames,
                FinalSeparator = finalfinalSeparator,
                Separator = separator,
                RockContext = LavaHelper.GetRockContextFromLavaContext( context )
            };

            return Person.CalculateFamilySalutation( person, args );
        }

        /// <summary>
        /// Gets an number for a person object
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="phoneType">Type of the phone number.</param>
        /// <param name="countryCode">Whether or not there should be a country code returned</param>
        /// <returns></returns>
        public static string PhoneNumber( ILavaRenderContext context, object input, string phoneType = "Home", bool countryCode = false )
        {
            Person person = GetPerson( input, context );


            string phoneNumber = null;

            if ( person != null )
            {
                var phoneNumberQuery = new PhoneNumberService( LavaHelper.GetRockContextFromLavaContext( context ) )
                            .Queryable()
                            .AsNoTracking()
                            .Where( p =>
                               p.PersonId == person.Id )
                            .Where( a => a.NumberTypeValue.Value == phoneType )
                            .FirstOrDefault();

                if ( phoneNumberQuery != null )
                {
                    if ( countryCode && !string.IsNullOrEmpty( phoneNumberQuery.CountryCode ) )
                    {
                        phoneNumber = phoneNumberQuery.NumberFormattedWithCountryCode;
                    }
                    else
                    {
                        phoneNumber = phoneNumberQuery.NumberFormatted;
                    }
                }
            }

            return phoneNumber;
        }

        /// <summary>
        /// Gets the profile photo for a person object in a string that zebra printers can use.
        /// If the person has no photo, a default silhouette photo (adult/child, male/female)
        /// photo is used.
        /// See http://www.rockrms.com/lava/person#ZebraPhoto for details.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input, which is the person.</param>
        /// <param name="size">The size.</param>
        /// <param name="brightness">The brightness adjustment (-1.0 to 1.0).</param>
        /// <param name="contrast">The contrast adjustment (-1.0 to 1.0).</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="rotationDegree">The degree of rotation to apply to the image (0, 90, 180, 270).</param>
        /// <returns>
        /// A ZPL field containing the photo data with a label of LOGO (^FS ~DYE:{fileName},P,P,{contentLength},,{zplImageData} ^FD").
        /// </returns>
        public static string ZebraPhoto( ILavaRenderContext context, object input, string size = "395", double brightness = 1.0, double contrast = 1.0, string fileName = "LOGO", int rotationDegree = 0 )
        {
            var person = GetPerson( input, context );
            try
            {
                if ( person != null )
                {
                    Stream initialPhotoStream;
                    if ( person.PhotoId.HasValue )
                    {
                        initialPhotoStream = new BinaryFileService( LavaHelper.GetRockContextFromLavaContext( context ) ).Get( person.PhotoId.Value ).ContentStream;
                    }
                    else
                    {
                        var photoUrl = new StringBuilder();
                        photoUrl.Append( HttpContext.Current.Server.MapPath( "~/" ) );

                        if ( person.Age.HasValue && person.Age.Value < 18 )
                        {
                            // it's a child
                            if ( person.Gender == Gender.Female )
                            {
                                photoUrl.Append( "Assets/FamilyManagerThemes/RockDefault/photo-child-female.png" );
                            }
                            else
                            {
                                photoUrl.Append( "Assets/FamilyManagerThemes/RockDefault/photo-child-male.png" );
                            }
                        }
                        else
                        {
                            // it's an adult
                            if ( person.Gender == Gender.Female )
                            {
                                photoUrl.Append( "Assets/FamilyManagerThemes/RockDefault/photo-adult-female.png" );
                            }
                            else
                            {
                                photoUrl.Append( "Assets/FamilyManagerThemes/RockDefault/photo-adult-male.png" );
                            }
                        }

                        initialPhotoStream = File.Open( photoUrl.ToString(), FileMode.Open );
                    }

                    Bitmap initialBitmap = new Bitmap( initialPhotoStream );

                    // Adjust the image if any of the parameters not default
                    if ( brightness != 1.0 || contrast != 1.0 )
                    {
                        initialBitmap = ImageAdjust( initialBitmap, ( float ) brightness, ( float ) contrast );
                    }

                    // Calculate rectangle to crop image into
                    int height = initialBitmap.Height;
                    int width = initialBitmap.Width;
                    Rectangle cropSection = new Rectangle( 0, 0, height, width );
                    if ( height < width )
                    {
                        cropSection = new Rectangle( ( width - height ) / 2, 0, ( width + height ) / 2, height ); // (width + height)/2 is a simplified version of the (width - height)/2 + height function
                    }
                    else if ( height > width )
                    {
                        cropSection = new Rectangle( 0, ( height - width ) / 2, width, ( height + width ) / 2 );
                    }

                    // Crop and resize image
                    int pixelSize = size.AsIntegerOrNull() ?? 395;
                    Bitmap resizedBitmap = new Bitmap( pixelSize, pixelSize );
                    using ( Graphics g = Graphics.FromImage( resizedBitmap ) )
                    {
                        g.DrawImage( initialBitmap, new Rectangle( 0, 0, resizedBitmap.Width, resizedBitmap.Height ), cropSection, GraphicsUnit.Pixel );
                    }

                    // Grayscale Image
                    var masks = new byte[] { 0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x01 };
                    var outputBitmap = new Bitmap( resizedBitmap.Width, resizedBitmap.Height, PixelFormat.Format1bppIndexed );
                    var data = new sbyte[resizedBitmap.Width, resizedBitmap.Height];
                    var inputData = resizedBitmap.LockBits( new Rectangle( 0, 0, resizedBitmap.Width, resizedBitmap.Height ), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb );
                    try
                    {
                        var scanLine = inputData.Scan0;
                        var line = new byte[inputData.Stride];
                        for ( var y = 0; y < inputData.Height; y++, scanLine += inputData.Stride )
                        {
                            Marshal.Copy( scanLine, line, 0, line.Length );
                            for ( var x = 0; x < resizedBitmap.Width; x++ )
                            {
                                // Change to greyscale
                                data[x, y] = ( sbyte ) ( 64 * ( ( ( line[x * 3 + 2] * 0.299 + line[x * 3 + 1] * 0.587 + line[x * 3 + 0] * 0.114 ) / 255 ) - 0.4 ) );
                            }
                        }
                    }
                    finally
                    {
                        resizedBitmap.UnlockBits( inputData );
                    }

                    //Dither Image
                    var outputData = outputBitmap.LockBits( new Rectangle( 0, 0, outputBitmap.Width, outputBitmap.Height ), ImageLockMode.WriteOnly, PixelFormat.Format1bppIndexed );
                    try
                    {
                        var scanLine = outputData.Scan0;
                        for ( var y = 0; y < outputData.Height; y++, scanLine += outputData.Stride )
                        {
                            var line = new byte[outputData.Stride];
                            for ( var x = 0; x < resizedBitmap.Width; x++ )
                            {
                                var j = data[x, y] > 0;
                                if ( j )
                                    line[x / 8] |= masks[x % 8];
                                var error = ( sbyte ) ( data[x, y] - ( j ? 32 : -32 ) );
                                if ( x < resizedBitmap.Width - 1 )
                                    data[x + 1, y] += ( sbyte ) ( 7 * error / 16 );
                                if ( y < resizedBitmap.Height - 1 )
                                {
                                    if ( x > 0 )
                                        data[x - 1, y + 1] += ( sbyte ) ( 3 * error / 16 );
                                    data[x, y + 1] += ( sbyte ) ( 5 * error / 16 );
                                    if ( x < resizedBitmap.Width - 1 )
                                        data[x + 1, y + 1] += ( sbyte ) ( 1 * error / 16 );
                                }
                            }

                            Marshal.Copy( line, 0, scanLine, outputData.Stride );
                        }
                    }
                    finally
                    {
                        outputBitmap.UnlockBits( outputData );
                    }

                    // Rotate image
                    switch ( rotationDegree )
                    {
                        case 90:
                            {
                                outputBitmap.RotateFlip( RotateFlipType.Rotate90FlipNone );
                                break;
                            }
                        case 180:
                            {
                                outputBitmap.RotateFlip( RotateFlipType.Rotate180FlipNone );
                                break;
                            }
                        case 270:
                        case -90:
                            {
                                outputBitmap.RotateFlip( RotateFlipType.Rotate270FlipNone );
                                break;
                            }
                    }

                    // Convert from x to .png
                    MemoryStream convertedStream = new MemoryStream();
                    outputBitmap.Save( convertedStream, System.Drawing.Imaging.ImageFormat.Png );
                    convertedStream.Seek( 0, SeekOrigin.Begin );

                    // Convert the .png stream into a ZPL-readable Hex format
                    var content = convertedStream.ReadBytesToEnd();
                    StringBuilder zplImageData = new StringBuilder();

                    foreach ( Byte b in content )
                    {
                        string hexRep = string.Format( "{0:X}", b );
                        if ( hexRep.Length == 1 )
                            hexRep = "0" + hexRep;
                        zplImageData.Append( hexRep );
                    }

                    convertedStream.Dispose();
                    initialPhotoStream.Dispose();

                    return string.Format( "^FS ~DYR:{0},P,P,{1},,{2} ^FD", fileName, content.Length, zplImageData.ToString() );
                }
            }
            catch
            {
                // intentionally blank
            }

            return string.Empty;
        }

        /// <summary>
        /// Adjust the brightness, contrast or gamma of the given image.
        /// </summary>
        /// <param name="originalImage">The original image.</param>
        /// <param name="brightness">The brightness multiplier (-1.99 to 1.99 fully white).</param>
        /// <param name="contrast">The contrast multiplier (2.0 would be twice the contrast).</param>
        /// <param name="gamma">The gamma multiplier (1.0 would no change in gamma).</param>
        /// <returns>A new adjusted image.</returns>
        private static Bitmap ImageAdjust( Bitmap originalImage, float brightness = 1.0f, float contrast = 1.0f, float gamma = 1.0f )
        {
            Bitmap adjustedImage = originalImage;

            float adjustedBrightness = brightness - 1.0f;
            // Matrix used to effect the image
            float[][] ptsArray = {
                new float[] { contrast, 0, 0, 0, 0 }, // scale red
                new float[] { 0, contrast, 0, 0, 0 }, // scale green
                new float[] { 0, 0, contrast, 0, 0 }, // scale blue
                new float[] { 0, 0, 0, 1.0f, 0 },     // no change to alpha
                new float[] { adjustedBrightness, adjustedBrightness, adjustedBrightness, 0, 1 }
            };

            var imageAttributes = new ImageAttributes();
            imageAttributes.ClearColorMatrix();
            imageAttributes.SetColorMatrix( new ColorMatrix( ptsArray ), ColorMatrixFlag.Default, ColorAdjustType.Bitmap );
            imageAttributes.SetGamma( gamma, ColorAdjustType.Bitmap );
            Graphics g = Graphics.FromImage( adjustedImage );
            g.DrawImage( originalImage, new Rectangle( 0, 0, adjustedImage.Width, adjustedImage.Height ),
                0, 0, originalImage.Width, originalImage.Height, GraphicsUnit.Pixel, imageAttributes );

            return adjustedImage;
        }

        /// <summary>
        /// Gets the groups of selected type that person is a member of
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <param name="memberStatus">The member status.</param>
        /// <param name="groupStatus">The group status.</param>
        /// <returns></returns>
        public static List<Rock.Model.GroupMember> Groups( ILavaRenderContext context, object input, string groupTypeId, string memberStatus = "Active", string groupStatus = "Active" )
        {
            var person = GetPerson( input, context );
            int? numericalGroupTypeId = groupTypeId.AsIntegerOrNull();

            if ( person != null && numericalGroupTypeId.HasValue )
            {
                var groupQuery = new GroupMemberService( LavaHelper.GetRockContextFromLavaContext( context ) )
                    .Queryable( "Group, GroupRole" )
                    .Where( m =>
                        m.PersonId == person.Id &&
                        m.Group.GroupTypeId == numericalGroupTypeId.Value &&
                        !m.Group.IsArchived );

                if ( groupStatus != "All" )
                {
                    groupQuery = groupQuery.Where( m => m.Group.IsActive );
                }

                if ( memberStatus != "All" )
                {
                    GroupMemberStatus queryStatus = GroupMemberStatus.Active;
                    queryStatus = ( GroupMemberStatus ) Enum.Parse( typeof( GroupMemberStatus ), memberStatus, true );

                    groupQuery = groupQuery.Where( m => m.GroupMemberStatus == queryStatus );
                }

                return groupQuery.ToList();
            }

            return new List<Model.GroupMember>();
        }

        /// <summary>
        /// Groups the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="status">The status.</param>
        /// <returns></returns>
        public static List<Rock.Model.GroupMember> Group( ILavaRenderContext context, object input, string groupId, string status = "Active" )
        {
            var person = GetPerson( input, context );
            int? numericalGroupId = groupId.AsIntegerOrNull();

            if ( string.IsNullOrWhiteSpace( status ) )
            {
                status = "All";
            }

            if ( person != null && numericalGroupId.HasValue )
            {
                var groupQuery = new GroupMemberService( LavaHelper.GetRockContextFromLavaContext( context ) )
                    .Queryable( "Group, GroupRole" )
                    .AsNoTracking()
                    .Where( m =>
                        m.PersonId == person.Id &&
                        m.Group.Id == numericalGroupId.Value &&
                        m.Group.IsActive && !m.Group.IsArchived );

                if ( status != "All" )
                {
                    GroupMemberStatus queryStatus = GroupMemberStatus.Active;
                    queryStatus = ( GroupMemberStatus ) Enum.Parse( typeof( GroupMemberStatus ), status, true );

                    groupQuery = groupQuery.Where( m => m.GroupMemberStatus == queryStatus );
                }

                return groupQuery.ToList();
            }

            return new List<Model.GroupMember>();
        }

        /// <summary>
        /// Gets the groups of selected type that person is a member of which they have attended at least once
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <returns></returns>
        public static List<Rock.Model.Group> GroupsAttended( ILavaRenderContext context, object input, string groupTypeId )
        {
            var person = GetPerson( input, context );
            int? numericalGroupTypeId = groupTypeId.AsIntegerOrNull();

            if ( person != null && numericalGroupTypeId.HasValue )
            {
                return new AttendanceService( LavaHelper.GetRockContextFromLavaContext( context ) ).Queryable()
                    .AsNoTracking()
                    .Where( a =>
                        a.Occurrence.Group != null &&
                        a.Occurrence.Group.GroupTypeId == numericalGroupTypeId &&
                        a.PersonAlias.PersonId == person.Id &&
                        a.DidAttend == true )
                    .Select( a => a.Occurrence.Group ).Distinct().ToList();
            }

            return new List<Model.Group>();
        }

        /// <summary>
        /// Gets the last attendance item for a given person in a group of type provided
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <returns></returns>
        public static Attendance LastAttendedGroupOfType( ILavaRenderContext context, object input, string groupTypeId )
        {
            var person = GetPerson( input, context );
            int? numericalGroupTypeId = groupTypeId.AsIntegerOrNull();

            if ( person == null && !numericalGroupTypeId.HasValue )
            {
                return new Attendance();
            }

            return new AttendanceService( LavaHelper.GetRockContextFromLavaContext( context ) ).Queryable()
                .AsNoTracking()
                .Where( a =>
                    a.Occurrence.Group != null &&
                    a.Occurrence.Group.GroupTypeId == numericalGroupTypeId &&
                    a.PersonAlias.PersonId == person.Id &&
                    a.DidAttend == true )
                .OrderByDescending( a => a.StartDateTime )
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets the groups of selected type that geofence the selected person
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <returns></returns>
        public static List<Rock.Model.Group> GeofencingGroups( ILavaRenderContext context, object input, string groupTypeId )
        {
            var person = GetPerson( input, context );
            int? numericalGroupTypeId = groupTypeId.AsIntegerOrNull();

            if ( person != null && numericalGroupTypeId.HasValue )
            {
                return new GroupService( LavaHelper.GetRockContextFromLavaContext( context ) )
                    .GetGeofencingGroups( person.Id, numericalGroupTypeId.Value )
                    .ToList();
            }

            return new List<Model.Group>();
        }

        /// <summary>
        /// Gets the groups of selected type that geofence the selected person
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <param name="groupTypeRoleId">The group type role identifier.</param>
        /// <returns></returns>
        public static List<Rock.Model.Person> GeofencingGroupMembers( ILavaRenderContext context, object input, string groupTypeId, string groupTypeRoleId )
        {
            var person = GetPerson( input, context );
            int? numericalGroupTypeId = groupTypeId.AsIntegerOrNull();
            int? numericalGroupTypeRoleId = groupTypeRoleId.AsIntegerOrNull();

            if ( person != null && numericalGroupTypeId.HasValue && numericalGroupTypeRoleId.HasValue )
            {
                return new GroupService( LavaHelper.GetRockContextFromLavaContext( context ) )
                    .GetGeofencingGroups( person.Id, numericalGroupTypeId.Value )
                    .SelectMany( g => g.Members.Where( m => m.GroupRole.Id == numericalGroupTypeRoleId ) )
                    .Select( m => m.Person )
                    .ToList();
            }

            return new List<Model.Person>();
        }

        /// <summary>
        /// Returnes the nearest group of a specific type.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <returns></returns>
        public static Rock.Model.Group NearestGroup( ILavaRenderContext context, object input, string groupTypeId )
        {
            var person = GetPerson( input, context );
            int? numericalGroupTypeId = groupTypeId.AsIntegerOrNull();

            if ( person != null && numericalGroupTypeId.HasValue )
            {
                return new GroupService( LavaHelper.GetRockContextFromLavaContext( context ) )
                    .GetNearestGroup( person.Id, numericalGroupTypeId.Value );
            }

            return null;
        }

        /// <summary>
        /// Returns the list of campuses closest to the target person.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">A Person entity object or identifier.</param>
        /// <param name="maximumCount">The maximum number of campuses to return.</param>
        /// <returns>A single Campus, or a list of Campuses in ascending order of distance from the target Person.</returns>
        public static object NearestCampus( ILavaRenderContext context, object input, object maximumCount = null )
        {
            var resultSetSize = InputParser.ConvertToIntegerOrDefault( maximumCount, 1, 0 ) ?? 0;
            if ( resultSetSize <= 0 )
            {
                return null;
            }

            // Return a singleton or collection according to the maximum count.
            object result = resultSetSize == 1 ? null : new List<Campus>();

            if ( resultSetSize < 1 )
            {
                return result;
            }

            var person = GetPerson( input, context );
            if ( person == null )
            {
                return result;
            }

            var rockContext = LavaHelper.GetRockContextFromLavaContext( context );

            // Get the Geopoint associated with the mapped location of the person's primary family.
            var personService = new PersonService( rockContext );

            var personGeopoint = personService.GetGeopoints( person.Id ).FirstOrDefault();
            if ( personGeopoint == null )
            {
                return result;
            }

            // Get the nearest campuses in order of distance.
            var campusService = new CampusService( rockContext );
            var campuses = campusService.Queryable()
                .Where( c =>
                    c.IsActive == true
                    && c.Location != null
                    && c.Location.GeoPoint != null )
                .OrderBy( c => c.Location.GeoPoint.Distance( personGeopoint ) )
                .Take( resultSetSize )
                .ToList();

            if ( resultSetSize == 1 )
            {
                return campuses.FirstOrDefault();
            }
            else
            {
                return campuses;
            }
        }

        /// <summary>
        /// Returns the Campus (or Campuses) that the Person belongs to
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="option">The option.</param>
        /// <returns></returns>
        public static object Campus( ILavaRenderContext context, object input, object option = null )
        {
            var person = GetPerson( input, context );
            if ( person == null )
            {
                return null;
            }

            bool getAll = false;
            if ( option != null && option.GetType() == typeof( string ) )
            {
                // if a string of "all" is specified for the option, return all of the campuses (if they are part of multiple families from different campuses)
                if ( string.Equals( ( string ) option, "all", StringComparison.OrdinalIgnoreCase ) )
                {
                    getAll = true;
                }
            }

            if ( getAll )
            {
                return person.GetFamilies().Select( a => a.Campus ).OrderBy( a => a.Name );
            }
            else
            {
                return person.GetCampus();
            }

        }

        /// <summary>
        /// Gets the person.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private static Person GetPerson( object input, ILavaRenderContext context )
        {
            if ( input != null )
            {
                if ( input is int )
                {
                    return new PersonService( LavaHelper.GetRockContextFromLavaContext( context ) ).Get( ( int ) input );
                }

                var person = input as Person;
                if ( person != null )
                {
                    return person;
                }

                var checkinPerson = input as CheckIn.CheckInPerson;
                if ( checkinPerson != null )
                {
                    return checkinPerson.Person;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the Notes of the entity
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="documentTemplateId">The Id number of the signed document type to query for.</param>
        /// <param name="trueValue">The value to be returned if the person has signed the document.</param>
        /// <param name="falseValue">The value to be returned if the person has not signed the document.</param>
        /// <returns></returns>
        public static object HasSignedDocument( ILavaRenderContext context, object input, object documentTemplateId, object trueValue = null, object falseValue = null )
        {
            int personId;
            int templateId;

            trueValue = trueValue ?? true;
            falseValue = falseValue ?? false;

            if ( input == null || documentTemplateId == null )
            {
                return falseValue;
            }

            templateId = documentTemplateId.ToString().AsInteger();

            if ( input is Person )
            {
                personId = ( input as Person ).Id;
            }
            else
            {
                personId = input.ToString().AsInteger();
            }

            bool found = new SignatureDocumentService( LavaHelper.GetRockContextFromLavaContext( context ) )
                .Queryable().AsNoTracking()
                .Where( d =>
                    d.SignatureDocumentTemplateId == templateId &&
                    d.Status == SignatureDocumentStatus.Signed &&
                    d.BinaryFileId.HasValue &&
                    d.AppliesToPersonAlias.PersonId == personId )
                .Any();

            return found ? trueValue : falseValue;
        }

        /// <summary>
        /// Creates a Person Action Identifier (rckid) for the specified Person (person can be specified by Person, Guid, or Id) for specific action.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        public static string PersonActionIdentifier( ILavaRenderContext context, object input, string action )
        {
            Person person = GetPerson( input, context ) ?? PersonById( context, input ) ?? PersonByGuid( context, input );

            if ( person != null )
            {
                return person.GetPersonActionIdentifier( action );
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Creates a Person Token (rckipid) for the specified Person (person can be specified by Person, Guid, or Id). Specify ExpireMinutes, UsageLimit and PageId to
        /// limit the usage of the token for the specified number of minutes, usage count, and specific pageid
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="expireMinutes">The expire minutes.</param>
        /// <param name="usageLimit">The usage limit.</param>
        /// <param name="pageId">The page identifier.</param>
        /// <returns></returns>
        public static string PersonTokenCreate( ILavaRenderContext context, object input, int? expireMinutes = null, int? usageLimit = null, int? pageId = null )
        {
            Person person = GetPerson( input, context ) ?? PersonById( context, input ) ?? PersonByGuid( context, input );

            if ( person != null )
            {
                DateTime? expireDateTime = null;
                if ( expireMinutes.HasValue )
                {
                    expireDateTime = RockDateTime.Now.AddMinutes( expireMinutes.Value );
                }

                if ( pageId.HasValue )
                {
                    var page = new PageService( LavaHelper.GetRockContextFromLavaContext( context ) ).Get( pageId.Value );
                    if ( page == null )
                    {
                        // invalid page specified, so don't return a token
                        return null;
                    }
                }

                return person.GetImpersonationToken( expireDateTime, usageLimit, pageId );
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Looks up a Person using an encrypted person token (rckipid) with an option to incrementUsage and to validate against a specific page
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="incrementUsage">if set to <c>true</c> [increment usage].</param>
        /// <param name="pageId">The page identifier.</param>
        /// <returns></returns>
        public static Person PersonTokenRead( ILavaRenderContext context, object input, bool incrementUsage = false, int? pageId = null )
        {
            string encryptedPersonToken = input as string;

            if ( !string.IsNullOrEmpty( encryptedPersonToken ) )
            {
                return new PersonService( LavaHelper.GetRockContextFromLavaContext( context ) ).GetByImpersonationToken( encryptedPersonToken, incrementUsage, pageId );
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets Steps associated with a specified person.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="stepProgram">The step program identifier, expressed as an Id or Guid.</param>
        /// <param name="stepStatus">The step status, expressed as an Id, Guid, or Name.</param>
        /// <param name="stepType">The step type identifier, expressed as an Id or Guid.</param>
        /// <returns></returns>
        public static List<Model.Step> Steps( ILavaRenderContext context, object input, string stepProgram = "All", string stepStatus = "All", string stepType = "All" )
        {
            var person = GetPerson( input, context );

            if ( person == null )
            {
                return new List<Step>();
            }

            var rockContext = LavaHelper.GetRockContextFromLavaContext( context );

            var stepsQuery = GetPersonSteps( rockContext, person, stepProgram, stepStatus, stepType );

            return stepsQuery.ToList();
        }

        /// <summary>
        /// Gets Steps associated with a specified person.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="person">The person.</param>
        /// <param name="stepProgram">The step program identifier, expressed as an Id or Guid.</param>
        /// <param name="stepStatus">The step status, expressed as an Id, Guid, or Name.</param>
        /// <param name="stepType">The step type identifier, expressed as an Id or Guid.</param>
        /// <returns></returns>
        internal static IQueryable<Model.Step> GetPersonSteps( RockContext rockContext, Person person, string stepProgram = null, string stepStatus = null, string stepType = null )
        {
            // Get Person from context.
            if ( person == null )
            {
                return null;
            }

            // Get base Steps query.
            var stepQuery = new StepService( rockContext )
                .Queryable( "Campus,StepStatus,StepType" )
                .Where( s => s.PersonAlias.PersonId == person.Id );

            // Filter by: Step Program.
            // The identifier can be either an Id or a Guid.
            stepProgram = stepProgram ?? string.Empty;
            stepProgram = stepProgram.Trim().ToLower();

            if ( !string.IsNullOrWhiteSpace( stepProgram )
                 && stepProgram != "all" )
            {
                var stepProgramId = stepProgram.AsIntegerOrNull();

                if ( stepProgramId.HasValue )
                {
                    stepQuery = stepQuery.Where( s => s.StepType.StepProgramId == stepProgramId.Value );
                }
                else
                {
                    var stepProgramGuid = stepProgram.AsGuidOrNull();

                    if ( stepProgramGuid.HasValue )
                    {
                        stepQuery = stepQuery.Where( s => s.StepType != null && s.StepType.StepProgram != null && s.StepType.StepProgram.Guid == stepProgramGuid.Value );
                    }
                }

                // Step Program Identifier is invalid.
            }

            // Filter by: Step Type.
            // The identifier can be either an Id or a Guid.
            stepType = stepType ?? string.Empty;
            stepType = stepType.Trim().ToLower();

            if ( !string.IsNullOrWhiteSpace( stepType )
                 && stepType != "all" )
            {
                var stepTypeId = stepType.AsIntegerOrNull();

                if ( stepTypeId.HasValue )
                {
                    stepQuery = stepQuery.Where( s => s.StepTypeId == stepTypeId.Value );
                }
                else
                {
                    var stepTypeGuid = stepType.AsGuidOrNull();

                    if ( stepTypeGuid.HasValue )
                    {
                        stepQuery = stepQuery.Where( s => s.StepType != null && s.StepType.Guid == stepTypeGuid.Value );
                    }
                }

                // Step Type Identifier is invalid.
            }

            // Filter by: Step Status
            stepStatus = stepStatus ?? string.Empty;
            stepStatus = stepStatus.Trim().ToLower();

            if ( !string.IsNullOrWhiteSpace( stepStatus )
                 && stepStatus != "all" )
            {
                var stepStatusId = stepStatus.AsIntegerOrNull();

                if ( stepStatusId.HasValue )
                {
                    stepQuery = stepQuery.Where( s => s.StepStatusId == stepStatusId.Value );
                }
                else
                {
                    var stepStatusGuid = stepStatus.AsGuidOrNull();

                    if ( stepStatusGuid.HasValue )
                    {
                        stepQuery = stepQuery.Where( s => s.StepStatus != null && s.StepStatus.Guid == stepStatusGuid.Value );
                    }
                    else
                    {
                        // Name
                        stepQuery = stepQuery.Where( s => s.StepStatus != null && s.StepStatus.Name == stepStatus );
                    }
                }
            }

            return stepQuery;
        }

        /// <summary>
        /// Determines whether [is in security role] [the specified context].
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="input">The input.</param>
        /// <param name="roleId">The role Id.</param>
        /// <returns>
        ///   <c>true</c> if [is in security role] [the specified context]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsInSecurityRole( ILavaRenderContext context, object input, object roleId )
        {
            var person = GetPerson( input, context );
            var role = RoleCache.Get( roleId.ToStringSafe().AsInteger() );

            if ( person == null || role == null )
            {
                return false;
            }

            if ( !role.IsSecurityTypeGroup )
            {
                ExceptionLogService.LogException( $"LavaFilter.IsInSecurityRole group with Id: {roleId} is not a SecurityRole" );
                return false;
            }

            return role.IsPersonInRole( person.Guid );
        }
    }
}
