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
using System.Linq;
using System.Text;
using System.Web;
using DocumentFormat.OpenXml.Drawing.Charts;
using Rock.Data;
using Rock.Security;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class Person
    {
        /// <summary>
        /// Gets an anchor tag to send person a communication
        /// </summary>
        /// <param name="rockUrlRoot">The rock URL root.</param>
        /// <param name="communicationPageReference">The communication page reference.</param>
        /// <param name="cssClass">The CSS class.</param>
        /// <param name="preText">The pre text.</param>
        /// <param name="postText">The post text.</param>
        /// <param name="styles">The styles.</param>
        /// <returns></returns>
        /// <value>
        /// The email tag.
        /// </value>
        public string GetEmailTag( string rockUrlRoot, Rock.Web.PageReference communicationPageReference, string cssClass = "", string preText = "", string postText = "", string styles = "" )
        {
            if ( !string.IsNullOrWhiteSpace( Email ) )
            {
                if ( IsEmailActive )
                {
                    rockUrlRoot.EnsureTrailingBackslash();

                    // get email link preference (new communication/mailto)
                    var globalAttributes = GlobalAttributesCache.Get();
                    string emailLinkPreference = globalAttributes.GetValue( "PreferredEmailLinkType" );

                    string emailLink = string.Empty;

                    // create link
                    if ( string.IsNullOrWhiteSpace( emailLinkPreference ) || emailLinkPreference == "1" )
                    {
                        if ( communicationPageReference != null )
                        {
                            communicationPageReference.QueryString = new System.Collections.Specialized.NameValueCollection( communicationPageReference.QueryString ?? new System.Collections.Specialized.NameValueCollection() );
                            communicationPageReference.QueryString["person"] = this.Id.ToString();
                            emailLink = new Rock.Web.PageReference( communicationPageReference.PageId, communicationPageReference.RouteId, communicationPageReference.Parameters, communicationPageReference.QueryString ).BuildUrl();
                        }
                        else
                        {
                            emailLink = string.Format( "{0}Communication?person={1}", rockUrlRoot, Id );
                        }
                    }
                    else
                    {
                        emailLink = string.Format( "mailto:{0}", Email );
                    }

                    switch ( EmailPreference )
                    {
                        case EmailPreference.EmailAllowed:
                            {
                                return string.Format(
                                    "<a class='{0}' style='{1}' href='{2}'>{3} {4} {5}</a>",
                                    cssClass,
                                    styles,
                                    emailLink,
                                    preText,
                                    Email,
                                    postText );
                            }

                        case EmailPreference.NoMassEmails:
                            {
                                return string.Format(
                                    "<span class='js-email-status email-status no-mass-email' data-toggle='tooltip' data-placement='top' title='Email Preference is set to \"No Mass Emails\"'><a class='{0}' href='{1}'>{2} {3} {4} <i class='fa fa-exchange'></i></a> </span>",
                                    cssClass,
                                    emailLink,
                                    preText,
                                    Email,
                                    postText );
                            }

                        case EmailPreference.DoNotEmail:
                            {
                                return string.Format(
                                    "<span class='{0} js-email-status email-status do-not-email' data-toggle='tooltip' data-placement='top' title='Email Preference is set to \"Do Not Email\"'>{1} {2} {3} <i class='fa fa-ban'></i></span>",
                                    cssClass,
                                    preText,
                                    Email,
                                    postText );
                            }
                    }
                }
                else
                {
                    return string.Format(
                        "<span class='js-email-status not-active email-status' data-toggle='tooltip' data-placement='top' title='Email is not active. {0}'>{1} <i class='fa fa-exclamation-triangle'></i></span>",
                        HttpUtility.HtmlEncode( EmailNote ),
                        Email );
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the person photo URL.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="photoId">The photo identifier.</param>
        /// <param name="age">The age.</param>
        /// <param name="gender">The gender.</param>
        /// <param name="recordTypeValueGuid">The record type value unique identifier.</param>
        /// <param name="ageClassification">The age classification.</param>
        /// <param name="maxWidth">The maximum width.</param>
        /// <param name="maxHeight">The maximum height.</param>
        /// <returns></returns>
        [RockObsolete( "1.15" )]
        [System.Obsolete( "Use alternative method for GetPersonPhotoUrl.", true )]
        public static string GetPersonPhotoUrl( int? personId, int? photoId, int? age, Gender gender, Guid? recordTypeValueGuid, AgeClassification? ageClassification, int? maxWidth = null, int? maxHeight = null )
        {
            
            int? recordTypeId = null;

            // Convert the record type value guid to an id
            if ( recordTypeValueGuid.HasValue )
            {
                recordTypeId = DefinedValueCache.Get( recordTypeValueGuid.Value ).Id;
            }

            // Convert maxsizes to size by selecting the greater of the values of maxwidth and maxheight
            int? size = maxWidth;

            if ( maxHeight.HasValue )
            {
                if ( size.HasValue && size.Value < maxHeight.Value )
                {
                    size = maxHeight.Value;
                }
            }

            return GetPersonPhotoUrl( string.Empty, photoId, age, gender, recordTypeId, ageClassification, size );
        }

        /// <summary>
        /// Gets the person photo URL.
        /// </summary>
        /// <param name="initials"></param>
        /// <param name="photoId"></param>
        /// <param name="age"></param>
        /// <param name="gender"></param>
        /// <param name="recordTypeValueId"></param>
        /// <param name="ageClassification"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static string GetPersonPhotoUrl( string initials, int? photoId, int? age, Gender gender, int? recordTypeValueId, AgeClassification? ageClassification, int? size = null )
        {
            var virtualPath = string.Empty;

            // If there are no initials provided we'll change the style of the avatar to be an icon
            var stylingOverride = string.Empty;

            SecuritySettingsService securitySettingsService = new SecuritySettingsService();

            // Determine if we need to provide a size
            var sizeParamter = string.Empty;

            if ( size.HasValue )
            {
                sizeParamter = $"&Size={size}";
            }

            if ( initials.IsNullOrWhiteSpace() )
            {
                stylingOverride = "&Style=icon";
            }

            var disablePredictableIds = securitySettingsService.SecuritySettings.DisablePredictableIds;

            if ( disablePredictableIds && photoId.HasValue )
            {
                var photoIdHash = IdHasher.Instance.GetHash( photoId.Value );
                virtualPath = $"~/GetAvatar.ashx?fileIdKey={photoIdHash}&AgeClassification={ageClassification}&Gender={gender}&RecordTypeId={recordTypeValueId}&Text={initials}{stylingOverride}{sizeParamter}";
            }
            else
            {
                virtualPath = $"~/GetAvatar.ashx?PhotoId={photoId}&AgeClassification={ageClassification}&Gender={gender}&RecordTypeId={recordTypeValueId}&Text={initials}{stylingOverride}{sizeParamter}";
            }

            if ( System.Web.HttpContext.Current == null )
            {
                return virtualPath;
            }
            else
            {
                return VirtualPathUtility.ToAbsolute( virtualPath );
            }
        }

        /// <summary>
        /// Gets the person image tag.
        /// </summary>
        /// <param name="person">The person to get the image for.</param>
        /// <param name="maxWidth">The maximum width (in px).</param>
        /// <param name="maxHeight">The maximum height (in px).</param>
        /// <param name="altText">The alt text to use on the image.</param>
        /// <param name="className">The css class name to apply to the image.</param>
        /// <returns></returns>
        public static string GetPersonPhotoImageTag( Person person, int? maxWidth = null, int? maxHeight = null, string altText = "", string className = "" )
        {
            Guid? recordTypeValueGuid = null;
            if ( person?.RecordStatusValueId != null )
            {
                recordTypeValueGuid = DefinedValueCache.Get( person.RecordTypeValueId.Value )?.Guid;
            }

            var personPhotoImageTagArgs = new GetPersonPhotoImageTagArgs
            {
                PhotoId = person?.PhotoId,
                Age = person?.Age,
                Gender = person?.Gender ?? Gender.Unknown,
                RecordTypeValueGuid = recordTypeValueGuid,
                AgeClassification = person.AgeClassification,
                MaxWidth = maxWidth,
                MaxHeight = maxHeight,
                AltText = altText,
                ClassName = className
            };

            return GetPersonPhotoImageTag( person?.Id, personPhotoImageTagArgs );
        }

        /// <summary>
        /// Gets the person image tag.
        /// </summary>
        /// <param name="personAlias">The person alias.</param>
        /// <param name="maxWidth">The maximum width (in px).</param>
        /// <param name="maxHeight">The maximum height (in px).</param>
        /// <param name="altText">The alt text to use on the image.</param>
        /// <param name="className">The css class name to apply to the image.</param>
        /// <returns></returns>
        public static string GetPersonPhotoImageTag( PersonAlias personAlias, int? maxWidth = null, int? maxHeight = null, string altText = "", string className = "" )
        {
            Person person = personAlias != null ? personAlias.Person : null;
            return GetPersonPhotoImageTag( person, maxWidth, maxHeight, altText, className );
        }

        /// <summary>
        /// Gets the person image tag.
        /// NOTE: You might want to use <seealso cref="GetPersonPhotoImageTag(int?, GetPersonPhotoImageTagArgs) "/> instead 
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="photoId">The photo identifier.</param>
        /// <param name="age">The age.</param>
        /// <param name="gender">The gender.</param>
        /// <param name="recordTypeValueGuid">The record type value unique identifier.</param>
        /// <param name="maxWidth">The maximum width (in px).</param>
        /// <param name="maxHeight">The maximum height (in px).</param>
        /// <param name="altText">The alt text to use on the image.</param>
        /// <param name="className">The css class name to apply to the image.</param>
        /// <returns></returns>
        public static string GetPersonPhotoImageTag( int? personId, int? photoId, int? age, Gender gender, Guid? recordTypeValueGuid, int? maxWidth = null, int? maxHeight = null, string altText = "", string className = "" )
        {
            var personPhotoImageTagArgs = new GetPersonPhotoImageTagArgs
            {
                PhotoId = photoId,
                Age = age,
                Gender = gender,
                RecordTypeValueGuid = recordTypeValueGuid,
                MaxWidth = maxWidth,
                MaxHeight = maxHeight,
                AltText = altText,
                ClassName = className
            };

            return GetPersonPhotoImageTag( personId, personPhotoImageTagArgs );
        }

        /// <summary>
        /// Gets the person photo image tag.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="personPhotoImageTagArgs">The person photo image tag arguments.</param>
        /// <returns></returns>
        public static string GetPersonPhotoImageTag( int? personId, GetPersonPhotoImageTagArgs personPhotoImageTagArgs )
        {
            var photoUrl = new StringBuilder();

            string baseUrl = Rock.Configuration.RockApp.Current.HostingSettings.VirtualRootPath;
            string altText = personPhotoImageTagArgs.AltText;
            string className = personPhotoImageTagArgs.ClassName;
            int? photoId = personPhotoImageTagArgs.PhotoId;
            int? maxWidth = personPhotoImageTagArgs.MaxWidth;
            int? maxHeight = personPhotoImageTagArgs.MaxHeight;
            Guid? recordTypeValueGuid = personPhotoImageTagArgs.RecordTypeValueGuid;
            int? age = personPhotoImageTagArgs.Age;
            Gender gender = personPhotoImageTagArgs.Gender;

            string styleString = string.Empty;

            string altString = string.IsNullOrWhiteSpace( altText ) ? string.Empty :
                string.Format( " alt='{0}'", altText );

            string classString = string.IsNullOrWhiteSpace( className ) ? string.Empty :
                string.Format( " class='{0}'", className );

            if ( photoId.HasValue )
            {
                photoUrl.Append( FileUrlHelper.GetImageUrl( photoId.Value ) );
                if ( maxWidth.HasValue )
                {
                    photoUrl.AppendFormat( "&maxwidth={0}", maxWidth.Value );
                }

                if ( maxHeight.HasValue )
                {
                    photoUrl.AppendFormat( "&maxheight={0}", maxHeight.Value );
                }
            }
            else
            {
                if ( recordTypeValueGuid.HasValue && recordTypeValueGuid.Value == SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid() )
                {
                    photoUrl.Append( baseUrl + "Assets/Images/business-no-photo.svg?" );
                }
                else if ( age.HasValue && age.Value < 18 )
                {
                    // it's a child
                    photoUrl.Append( baseUrl + GetPhotoPath( gender, false ) );
                }
                else
                {
                    var ageClassification = personPhotoImageTagArgs.AgeClassification;
                    if ( personId.HasValue && !ageClassification.HasValue )
                    {
                        using ( var rockContext = new RockContext() )
                        {
                            ageClassification = new PersonService( rockContext ).GetSelect( personId.Value, s => s.AgeClassification );
                        }
                    }

                    if ( ageClassification.HasValue && ageClassification == AgeClassification.Child )
                    {
                        // it's a child
                        photoUrl.Append( baseUrl + GetPhotoPath( gender, false ) );
                    }
                    else
                    {
                        // it's an adult
                        photoUrl.Append( baseUrl + GetPhotoPath( gender, true ) );
                    }
                }
            }

            return string.Format( "<img src='{0}'{1}{2}{3}/>", photoUrl.ToString(), styleString, altString, classString );
        }
    }
}
