using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Attribute;
using Rock.Security;


namespace RockWeb.Plugins.org_newpointe.HTMLContext
{
    /// <summary>
    /// Adds an editable HTML fragment to the page.
    /// </summary>
    [DisplayName( "HTML Context Display" )]
    [Category( "NewPointe -> CMS" )]
    [Description( "Adds a context-sensitive HTML fragment to the page." )]

    [CustomDropdownListField( "HTML Context", "The Context that contains the HTML to display.", @"
DECLARE @leftPart varchar(max) = 'ContextName='
DECLARE @rightPart varchar(max) = '&'

SELECT [Text] AS [Value], [Text]
FROM (
        SELECT CASE WHEN RightIndex = 0 THEN EntityValue ELSE LEFT(EntityValue, RightIndex - 1) END AS [Text]
        FROM (
                SELECT Id, EntityValue, CHARINDEX(@rightPart, EntityValue) AS RightIndex
                FROM (
                        SELECT Id, CASE WHEN LeftIndex = 0 THEN EntityValue ELSE RIGHT(EntityValue, LEN(EntityValue) - (LeftIndex + LEN(@leftPart) - 1)) END AS EntityValue
                        FROM (
                                SELECT Id, EntityValue, CHARINDEX(@leftPart, EntityValue) AS LeftIndex
                                FROM [HtmlContent]
                                WHERE EntityValue LIKE '%ContextName=%'
                            ) hc
                    ) hc
            ) hc
        ) hc
", false, "", "", 0, "ContextName" )]
    [TextField( "Context Parameter", "Query string parameter to use for 'personalizing' content based on unique values.", false, "", "", 1, "ContextParameter" )]
    [IntegerField( "Cache Duration", "Number of seconds to cache the content.", false, 3600, "", 2 )]
    [BooleanField( "Enable Debug", "Show lava merge fields.", false, "", 3 )]
    public partial class HTMLContextDisplay : Rock.Web.UI.RockBlock
    {

        protected override void OnLoad( EventArgs e )
        {

            var entityValue = EntityValue();

            if (!String.IsNullOrWhiteSpace( entityValue ) )
            {
                string html = string.Empty;

                string cachedContent = HtmlContentService.GetCachedContent( this.BlockId, entityValue );

                // if content not cached load it from DB
                if ( cachedContent == null )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        var htmlContentService = new HtmlContentService( rockContext );
                        HtmlContent content = htmlContentService.GetActiveContent( this.BlockId, entityValue );

                        if ( content != null )
                        {
                            bool enableDebug = GetAttributeValue( "EnableDebug" ).AsBoolean();

                            if ( content.Content.HasMergeFields() || enableDebug )
                            {
                                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                                mergeFields.Add( "CurrentPage", Rock.Lava.LavaHelper.GetPagePropertiesMergeObject( this.RockPage ) );
                                if ( CurrentPerson != null )
                                {
                                    // TODO: When support for "Person" is not supported anymore (should use "CurrentPerson" instead), remove this line
                                    mergeFields.AddOrIgnore( "Person", CurrentPerson );
                                }


                                mergeFields.Add( "RockVersion", Rock.VersionInfo.VersionInfo.GetRockProductVersionNumber() );
                                mergeFields.Add( "CurrentPersonCanEdit", IsUserAuthorized( Authorization.EDIT ) );
                                mergeFields.Add( "CurrentPersonCanAdministrate", IsUserAuthorized( Authorization.ADMINISTRATE ) );

                                html = content.Content.ResolveMergeFields( mergeFields );

                                // show merge fields if enable debug true
                                if ( enableDebug && IsUserAuthorized( Authorization.EDIT ) )
                                {
                                    // TODO: When support for "Person" is not supported anymore (should use "CurrentPerson" instead), remove this line
                                    mergeFields.Remove( "Person" );
                                    html += mergeFields.lavaDebugInfo();
                                }
                            }
                            else
                            {
                                html = content.Content;
                            }
                        }
                        else
                        {
                            html = string.Empty;
                        }
                    }

                    // Resolve any dynamic url references
                    string appRoot = ResolveRockUrl( "~/" );
                    string themeRoot = ResolveRockUrl( "~~/" );
                    html = html.Replace( "~~/", themeRoot ).Replace( "~/", appRoot );

                    // cache content
                    int cacheDuration = GetAttributeValue( "CacheDuration" ).AsInteger();
                    if ( cacheDuration > 0 )
                    {
                        HtmlContentService.AddCachedContent( this.BlockId, entityValue, html, cacheDuration );
                    }
                }
                else
                {
                    html = cachedContent;
                }

                // add content to the content window
                htmlContent.Text = html;
            }
        }

        /// <summary>
        /// Entities the value.
        /// </summary>
        /// <returns></returns>
        private string EntityValue()
        {
            string entityValue = string.Empty;

            string contextParameter = GetAttributeValue( "ContextParameter" );
            if ( !string.IsNullOrEmpty( contextParameter ) )
            {
                entityValue = string.Format( "{0}={1}", contextParameter, PageParameter( contextParameter ) ?? string.Empty );
            }

            string contextName = GetAttributeValue( "ContextName" );
            if ( !string.IsNullOrEmpty( contextName ) )
            {
                entityValue += "&ContextName=" + contextName;
            }

            return entityValue;
        }
    }
}