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
using System.Text.RegularExpressions;

using Rock.Attribute;
using Rock.Lava;

using UAParser;

namespace Rock.Net;

/// <summary>
/// Rock-owned details parsed from a user-agent string.
/// </summary>
// Candidate future additions, deferred to v2 (no audited call site needs
// them today; callers that want this kind of check go through ClientType):
//   IsUnknown -- distinguishes "Other-everywhere" parses from real ones
//   IsBot     -- crawler/spider check (likely ClientType == "Crawler")
//   IsMobile  -- likely ClientType == "Mobile"
//   IsTablet  -- likely ClientType == "Tablet"
[LavaType]
public sealed class UserAgentInfo
{
    #region ClientType Regexes

    private static readonly Regex _regexMobile1 = new( @"(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows ce|xda|xiino", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled );
    private static readonly Regex _regexMobile2 = new( @"1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\-(n|u)|c55\/|capi|ccwa|cdm\-|cell|chtm|cldc|cmd\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\-s|devi|dica|dmob|do(c|p)o|ds(12|\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\-|_)|g1 u|g560|gene|gf\-5|g\-mo|go(\.w|od)|gr(ad|un)|haie|hcit|hd\-(m|p|t)|hei\-|hi(pt|ta)|hp( i|ip)|hs\-c|ht(c(\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\-(20|go|ma)|i230|iac( |\-|\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc\-|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|\-[a-w])|libw|lynx|m1\-w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|m\-cr|me(rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\-2|po(ck|rt|se)|prox|psio|pt\-g|qa\-a|qc(07|12|21|32|60|\-[2-7]|i\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\-|oo|p\-)|sdk\/|se(c(\-|0|1)|47|mc|nd|ri)|sgh\-|shar|sie(\-|m)|sk\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\-|v\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\-|tdg\-|tel(i|m)|tim\-|t\-mo|to(pl|sh)|ts(70|m\-|m3|m5)|tx\-9|up(\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|yas\-|your|zeto|zte\-", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled );
    private static readonly Regex _regexTablet = new( @"android|ipad|playbook|silk", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled );

    // Crawler detection lives in CrawlerUserAgents, which is backed by the
    // crawler-user-agents dataset. The legacy keyword expression that used to
    // live here is retained there as a fallback.

    #endregion

    #region Properties

    /// <summary>
    /// The raw user-agent string this object was parsed from.
    /// </summary>
    public string UserAgent { get; }

    /// <summary>
    /// The OS family, e.g. "Windows", "iOS", or "Mac OS X". Returns
    /// "Other" when no user-agent string was supplied.
    /// </summary>
    public string OSFamily { get; }

    /// <summary>
    /// The OS version. This will never be <c>null</c>, though the version
    /// components may all be <c>null</c>.
    /// </summary>
    public UserAgentVersion OSVersion { get; }

    /// <summary>
    /// The browser family, e.g. "Chrome" or "Firefox". Returns "Other"
    /// when no user-agent string was supplied.
    /// </summary>
    public string BrowserFamily { get; }

    /// <summary>
    /// The browser version. This will never be <c>null</c>, though the version
    /// components may all be <c>null</c>.
    /// </summary>
    public UserAgentVersion BrowserVersion { get; }

    /// <summary>
    /// The device family, e.g. "iPhone" or "Other". Returns "Other"
    /// when no user-agent string was supplied.
    /// </summary>
    public string DeviceFamily { get; }

    /// <summary>
    /// The device brand, e.g. "Apple" or "Samsung". Returns an empty
    /// string when no user-agent string was supplied.
    /// </summary>
    public string DeviceBrand { get; }

    /// <summary>
    /// The device model. Returns an empty string when no user-agent
    /// string was supplied.
    /// </summary>
    public string DeviceModel { get; }

    /// <summary>
    /// The client type as one of "Mobile", "Tablet", "Crawler",
    /// "Outlook", "Desktop", or "None".
    /// </summary>
    /// <remarks>
    /// This should not be made public until the implementation is updated
    /// to use a better pattern (enum, IsMobile/IsTablet/etc.).
    /// </remarks>
    [RockInternal( "20.0", true )]
    public string ClientType { get; }

    /// <summary>
    /// The underlying <see cref="UAParser.ClientInfo"/> that was used
    /// to construct this instance. Internal-only deprecation-window
    /// holdover that backs the obsolete
    /// <see cref="ClientInformation.Browser"/> property; do not use from
    /// new code. Removed when <see cref="ClientInformation.Browser"/> is
    /// removed.
    /// </summary>
    [Obsolete( "Internal-only deprecation-window holdover. Will be removed when ClientInformation.Browser is removed." )]
    internal ClientInfo OriginalClientInfo { get; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="UserAgentInfo"/> class
    /// by parsing the supplied <see cref="UAParser.ClientInfo"/>.
    /// </summary>
    /// <param name="userAgent">The raw user-agent string the client info was parsed from.</param>
    /// <param name="clientInfo">The parser result.</param>
    internal UserAgentInfo( string userAgent, ClientInfo clientInfo )
    {
        UserAgent = userAgent ?? string.Empty;

        if ( clientInfo == null )
        {
            OSFamily = "Other";
            BrowserFamily = "Other";
            DeviceFamily = "Other";
            DeviceBrand = string.Empty;
            DeviceModel = string.Empty;
            ClientType = "None";
            return;
        }

#pragma warning disable CS0618
        OriginalClientInfo = clientInfo;
#pragma warning restore CS0618

        OSFamily = clientInfo.OS?.Family ?? string.Empty;
        OSVersion = new UserAgentVersion( clientInfo.OS?.Major, clientInfo.OS?.Minor, clientInfo.OS?.Patch, clientInfo.OS?.PatchMinor );

        BrowserFamily = clientInfo.UA?.Family ?? string.Empty;
        BrowserVersion = new UserAgentVersion( clientInfo.UA?.Major, clientInfo.UA?.Minor, clientInfo.UA?.Patch, null );

        DeviceFamily = clientInfo.Device?.Family ?? string.Empty;
        DeviceBrand = clientInfo.Device?.Brand ?? string.Empty;
        DeviceModel = clientInfo.Device?.Model ?? string.Empty;

        ClientType = DetermineClientType( UserAgent );
    }

    #endregion

    #region Methods

    /// <summary>
    /// Returns "{OSFamily} {OSVersion}" with the trailing space removed
    /// when the version is empty. Matches the format previously produced
    /// by UAParser's <c>OS.ToString()</c> that is persisted to the
    /// database.
    /// </summary>
    /// <returns>The display-formatted OS family and version string.</returns>
    public string GetOSFamilyVersion()
    {
        return CombineFamilyAndVersion( OSFamily, OSVersion );
    }

    /// <summary>
    /// Returns "{BrowserFamily} {BrowserVersion}" with the trailing space
    /// removed when the version is empty. Matches the format previously
    /// produced by UAParser's <c>UserAgent.ToString()</c> that is
    /// persisted to the database.
    /// </summary>
    /// <returns>The display-formatted browser family and version string.</returns>
    public string GetBrowserFamilyVersion()
    {
        return CombineFamilyAndVersion( BrowserFamily, BrowserVersion );
    }

    /// <summary>
    /// Returns the space-separated "{OS} {Device} {Browser}" display
    /// string. Used by Lava <c>{{ '' | Client:'BROWSER' }}</c> rendering
    /// and by the HtmlContentDetail <c>CurrentBrowser</c> wrapper. Matches
    /// the byte-for-byte format previously produced by
    /// <c>UAParser.ClientInfo.ToString()</c>.
    /// </summary>
    /// <returns>The combined family/version display string.</returns>
    public override string ToString()
    {
        var os = CombineFamilyAndVersion( OSFamily, OSVersion );
        var device = DeviceFamily;
        var browser = CombineFamilyAndVersion( BrowserFamily, BrowserVersion );

        return $"{os} {device} {browser}";
    }

    /// <summary>
    /// Combines the family and version information into a single string.
    /// This is designed to match the format previously returned by UAParser.
    /// This format is used by various persisted database values and should
    /// not be changed without careful consideration of the impact on
    /// analytics information.
    /// </summary>
    /// <param name="family">The family name (e.g., "Windows", "Chrome").</param>
    /// <param name="version">The version information.</param>
    /// <returns>The combined family/version string.</returns>
    private static string CombineFamilyAndVersion( string family, UserAgentVersion version )
    {
        var versionString = version?.ToString() ?? string.Empty;

        if ( family.IsNullOrWhiteSpace() )
        {
            return versionString;
        }

        if ( versionString.Length == 0 )
        {
            return family;
        }

        return family + " " + versionString;
    }

    /// <summary>
    /// Computes the <see cref="ClientType"/> value for the supplied raw
    /// user-agent string without requiring DI. Used by the obsolete
    /// <see cref="Rock.Model.InteractionDeviceType.GetClientType(string)"/>
    /// shim so it works in unit tests where the DI container has not been
    /// initialized.
    /// </summary>
    /// <param name="userAgent">The raw user-agent string.</param>
    /// <returns>The detected client type.</returns>
    internal static string DetermineClientType( string userAgent )
    {
        if ( userAgent.IsNullOrWhiteSpace() )
        {
            return "None";
        }

        /*
            8/24/2026 - CLAUDE

            The crawler test must run before the mobile and tablet tests.
            Googlebot Smartphone's user agent contains both "Android" and
            "Mobile Safari", so when mobile was evaluated first it returned
            "Mobile" and the crawler test was never reached. The same applied to
            any crawler whose user agent contained "android", because the tablet
            expression is a bare alternation of four device words.

            Reason: The largest crawler on the internet was never being
            classified as one.
        */
        if ( CrawlerUserAgents.IsCrawler( userAgent ) )
        {
            return "Crawler";
        }

        // The mobile detection regexes are sourced from
        // http://detectmobilebrowsers.com/ and should be revisited
        // periodically.
        if ( _regexMobile1.IsMatch( userAgent ) || ( userAgent.Length >= 4 && _regexMobile2.IsMatch( userAgent.Substring( 0, 4 ) ) ) )
        {
            return "Mobile";
        }

        if ( _regexTablet.IsMatch( userAgent ) )
        {
            return "Tablet";
        }

        // Outlook calendar feeds identify as "Microsoft Office/...". The
        // legacy regex was case-sensitive and missed real-world UAs;
        // OrdinalIgnoreCase fixes that.
        if ( userAgent.IndexOf( "microsoft office", StringComparison.OrdinalIgnoreCase ) >= 0 )
        {
            return "Outlook";
        }

        return "Desktop";
    }

    #endregion
}
