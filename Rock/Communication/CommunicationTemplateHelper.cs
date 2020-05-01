using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Hosting;
using System.Web.UI.WebControls;
using Humanizer;
using Rock.Web.UI.Controls;

namespace Rock.Communication
{
    /// <summary>
    /// 
    /// </summary>
    public static class CommunicationTemplateHelper
    {
        #region TemplateHtml Helpers

        /// <summary>
        /// Gets the template help. If this is for a Communication Entry template, set includeCommunicationWizardHelp to true
        /// </summary>
        /// <param name="includeCommunicationWizardHelp">if set to <c>true</c> [include communication wizard help].</param>
        /// <returns></returns>
        public static string GetTemplateHelp( bool includeCommunicationWizardHelp )
        {
            StringBuilder helpTextBuilder = new StringBuilder();

            if ( includeCommunicationWizardHelp )
            {
                helpTextBuilder.Append(
            @"
<p>An email template needs to be an html doc with some special divs to support the communication wizard.</p>
<br/>
<p>The template needs to have at least one div with a 'dropzone' class in the BODY</p>
<br/>
<pre>
&lt;div class=""dropzone""&gt;
&lt;/div&gt;
</pre>
<br/>

<p>A template also needs to have at least one div with a 'structure-dropzone' class in the BODY to support adding zones</p>
<br/>
<pre>
&lt;div class=""structure-dropzone""&gt;
    &lt;div class=""dropzone""&gt;
    &lt;/div&gt;
&lt;/div&gt;
</pre>
<br/>

<p>To have some starter text, include a 'component component-text' div within the 'dropzone' div</p>
<br/>
<pre>
&lt;div class=""structure-dropzone""&gt;
    &lt;div class=""dropzone""&gt;
        &lt;div class=""component component-text"" data-content=""&lt;h1&gt;Hello There!&lt;/h1&gt;"" data-state=""component""&gt;
            &lt;h1&gt;Hello There!&lt;/h1&gt;
        &lt;/div&gt;
    &lt;/div&gt;
&lt;/div&gt;
</pre>
<br/>

<p>To enable the PREHEADER text, a div with an id of 'preheader-text' needs to be the first div in the BODY</p>
<br/>
<pre>
&lt;!-- HIDDEN PREHEADER TEXT --&gt;
&lt;div id=""preheader-text"" style=""display: none; font-size: 1px; color: #fefefe; line-height: 1px; font-family: Helvetica, Arial, sans-serif; max-height: 0px; max-width: 0px; opacity: 0; overflow: hidden;""&gt;
    Entice the open with some amazing preheader text. Use a little mystery and get those subscribers to read through...
&lt;/div&gt;
</pre>
" );
            }

            helpTextBuilder.Append( @"
<p>To include a logo, an img div with an id of 'template-logo' can be placed anywhere in the template, which will then show the 'Logo' image uploader under the template editor which will be used to set the src of the template-logo</p>
<br/>
<pre>
&lt;!-- LOGO --&gt;
&lt;img id='template-logo' src='/Content/EmailTemplates/placeholder-logo.png' width='200' height='50' data-instructions='Provide a PNG with a transparent background or JPG with the background color of #ee7725.' /&gt;
</pre>

<br/>
" );
            return helpTextBuilder.ToString();
        }

        /// <summary>
        /// Gets the updated template HTML.
        /// </summary>
        /// <param name="templateHtml">The template HTML.</param>
        /// <param name="binaryFileId">The binary file identifier.</param>
        /// <param name="lavaFieldsTemplateDictionaryFromControls">The lava fields template dictionary from controls.</param>
        /// <param name="lavaFieldsDefaultDictionary">The lava fields default dictionary.</param>
        /// <returns></returns>
        public static string GetUpdatedTemplateHtml(
            string templateHtml,
            int? binaryFileId,
            Dictionary<string, string> lavaFieldsTemplateDictionaryFromControls,
            Dictionary<string, string> lavaFieldsDefaultDictionary )
        {
            var templateLogoHtmlMatch = new Regex( "<img[^>]+id=[',\"]template-logo[',\"].*src=[',\"]([^\">]+)[',\"].*>" ).Match( templateHtml );
            string updatedTemplateHtml = templateHtml;

            if ( templateLogoHtmlMatch.Groups.Count == 2 )
            {
                string originalTemplateLogoHtml = templateLogoHtmlMatch.Groups[0].Value;
                string originalTemplateLogoSrc = templateLogoHtmlMatch.Groups[1].Value;

                string newTemplateLogoSrc;

                // if a template-logo exists in the template, update the src attribute to whatever the uploaded logo is (or set it to the placeholder if it is not set)
                if ( binaryFileId != null && binaryFileId > 0 )
                {
                    string getImagePath;

                    if ( HostingEnvironment.IsHosted )
                    {
                        getImagePath = System.Web.VirtualPathUtility.ToAbsolute( "~/GetImage.ashx" );
                    }
                    else
                    {
                        getImagePath = "/GetImage.ashx";
                    }

                    newTemplateLogoSrc = $"{getImagePath}?Id={binaryFileId}";
                }
                else
                {
                    newTemplateLogoSrc = "/Content/EmailTemplates/placeholder-logo.png";
                }

                string newTemplateLogoHtml = originalTemplateLogoHtml.Replace( originalTemplateLogoSrc, newTemplateLogoSrc );

                templateHtml = templateHtml.Replace( originalTemplateLogoHtml, newTemplateLogoHtml );
            }

            var origHtml = templateHtml;

            /* We want to update the lava-fields node of template so that the lava-fields tag
             * is in sync with the lava merge fields in kvlMergeFields (which comes from CommunicationTemplate.LavaFields).
             * NOTE: We don't want to use a HTML Parser (like HtmlAgilityPack or AngleSharp),
             * because this is a mix of html and lava, and html parsers will end up corrupting the html+lava
             * So we'll..
             * 1) Use regex to find the lava-fields html and remove it from the code editor text
             * 2) make a new lava-fields tag with the values from kvlMergeFields (which comes from CommunicationTemplate.LavaFields)
             * 3) Put thge new lava-fields html back into the code editor text in the head in a noscript tag
             */

            // First, we'll take out the lava-fields tag (could be a div or a noscript, depending on which version of Rock last edited it)
            var lavaFieldsRegExLegacy = new System.Text.RegularExpressions.Regex( @"<div[\s\S]*lava-fields[\s\S]+?</div>?", RegexOptions.Multiline );
            var lavaFieldsHtmlLegacy = lavaFieldsRegExLegacy.Match( templateHtml ).Value;
            if ( lavaFieldsHtmlLegacy.IsNotNullOrWhiteSpace() )
            {
                templateHtml = templateHtml.Replace( lavaFieldsHtmlLegacy, string.Empty );
            }

            var lavaFieldsRegEx = new System.Text.RegularExpressions.Regex( @"<noscript[\s\S]*lava-fields[\s\S]+?</noscript>?", RegexOptions.Multiline );
            var lavaFieldsHtml = lavaFieldsRegEx.Match( templateHtml ).Value;
            if ( lavaFieldsHtml.IsNotNullOrWhiteSpace() )
            {
                templateHtml = templateHtml.Replace( lavaFieldsHtml, string.Empty );
            }

            var templateDocLavaFieldLines = lavaFieldsHtml.Split( new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries ).Select( a => a.Trim() ).Where( a => a.IsNotNullOrWhiteSpace() ).ToList();

            // add any new lava fields that were added to the KeyValueList editor
            foreach ( var keyValue in lavaFieldsDefaultDictionary )
            {
                string pattern = string.Format( @"{{%\s+assign\s+{0}.*\s+=\s", keyValue.Key );
                if ( !templateDocLavaFieldLines.Any( a => Regex.IsMatch( a, pattern ) ) )
                {
                    templateDocLavaFieldLines.Add( "{% assign " + keyValue.Key + " = '" + keyValue.Value + "' %}" );
                }
            }

            // remove any lava fields that are not in the KeyValueList editor
            foreach ( var templateDocLavaFieldLine in templateDocLavaFieldLines.ToList() )
            {
                var found = false;
                foreach ( var keyValue in lavaFieldsDefaultDictionary )
                {
                    var pattern = string.Format( @"{{%\s+assign\s+{0}.*\s+=\s", keyValue.Key );
                    if ( !Regex.IsMatch( templateDocLavaFieldLine, pattern ) )
                    {
                        continue;
                    }

                    found = true;
                    break;
                }

                // if not found, delete it
                if ( !found )
                {
                    templateDocLavaFieldLines.Remove( templateDocLavaFieldLine );
                }
            }

            // dictionary of keys and values from the lava fields in the 'lava-fields' div
            var lavaFieldsTemplateDictionary = new Dictionary<string, string>();
            foreach ( var templateDocLavaFieldLine in templateDocLavaFieldLines )
            {
                var match = Regex.Match( templateDocLavaFieldLine, @"{% assign (.*)\=(.*) %}" );
                if ( match.Groups.Count != 3 )
                {
                    continue;
                }

                var key = match.Groups[1].Value.Trim().RemoveSpaces();
                var value = match.Groups[2].Value.Trim().Trim( '\'' );

                // if there are values from Controls that different from the ones in the templateHtml, use the values from the control
                if ( lavaFieldsTemplateDictionaryFromControls.ContainsKey( key ) )
                {
                    var controlValue = lavaFieldsTemplateDictionaryFromControls[key];

                    if ( controlValue != value )
                    {
                        value = controlValue;
                    }
                }

                lavaFieldsTemplateDictionary.Add( key, value );
            }

            if ( !lavaFieldsTemplateDictionary.Any() )
            {
                return templateHtml;
            }

            // there are 3 cases of where the <noscript> tag should be
            // 1) There is a head tag (usually the case)
            // 2) There is not head tag, but there is a html tag
            // 3) There isn't a head or html tag
            var headTagRegex = new Regex( "<head(.*?)>" );
            var htmlTagRegex = new Regex( "<html(.*?)>" );

            var lavaAssignsHtmlBuilder = new StringBuilder();
            lavaAssignsHtmlBuilder.Append( "<noscript id=\"lava-fields\">\n" );
            lavaAssignsHtmlBuilder.Append( "  {% comment %}  Lava Fields: Code-Generated from Template Editor {% endcomment %}\n" );
            foreach ( var lavaFieldsTemplateItem in lavaFieldsTemplateDictionary )
            {
                lavaAssignsHtmlBuilder.Append( string.Format( "  {{% assign {0} = '{1}' %}}\n", lavaFieldsTemplateItem.Key, lavaFieldsTemplateItem.Value ) );
            }

            lavaAssignsHtmlBuilder.Append( "</noscript>" );
            var lavaAssignsHtml = lavaAssignsHtmlBuilder.ToString();

            if ( headTagRegex.Match( templateHtml ).Success )
            {
                templateHtml = headTagRegex.Replace( templateHtml, ( m ) => { return m.Value.TrimEnd() + lavaAssignsHtml; } );
            }
            else if ( htmlTagRegex.Match( templateHtml ).Success )
            {
                templateHtml = htmlTagRegex.Replace( templateHtml, ( m ) => { return m.Value.TrimEnd() + lavaAssignsHtml; } );
            }
            else
            {
                templateHtml = lavaAssignsHtml + "\n" + templateHtml.TrimStart();
            }

            return templateHtml;
        }

        /// <summary>
        /// Gets the lava fields template dictionary.
        /// </summary>
        /// <param name="templateHtml">The template HTML.</param>
        /// <returns></returns>
        public static Dictionary<string, string> GetLavaFieldsTemplateDictionaryFromTemplateHtml( string templateHtml )
        {
            var templateDoc = new HtmlAgilityPack.HtmlDocument();
            templateDoc.LoadHtml( templateHtml );

            // take care of the lava fields stuff
            var lavaFieldsNode = templateDoc.GetElementbyId( "lava-fields" );
            var lavaFieldsTemplateDictionary = new Dictionary<string, string>();

            if ( lavaFieldsNode != null )
            {
                var templateDocLavaFieldLines = lavaFieldsNode.InnerText.Split( new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries ).Select( a => a.Trim() ).Where( a => a.IsNotNullOrWhiteSpace() ).ToList();

                // dictionary of keys and values from the lava fields in the 'lava-fields' div
                foreach ( var templateDocLavaFieldLine in templateDocLavaFieldLines )
                {
                    var match = Regex.Match( templateDocLavaFieldLine, @"{% assign (.*)\=(.*) %}" );
                    if ( match.Groups.Count != 3 )
                    {
                        continue;
                    }

                    var key = match.Groups[1].Value.Trim().RemoveSpaces();
                    var value = match.Groups[2].Value.Trim().Trim( '\'' );
                    lavaFieldsTemplateDictionary.Add( key, value );
                }
            }

            return lavaFieldsTemplateDictionary;
        }

        /// <summary>
        /// Determines whether [has template logo] [the specified template HTML].
        /// </summary>
        /// <param name="templateHtml">The template HTML.</param>
        /// <returns>
        ///   <c>true</c> if [has template logo] [the specified template HTML]; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasTemplateLogo( string templateHtml )
        {
            var templateDoc = new HtmlAgilityPack.HtmlDocument();
            templateDoc.LoadHtml( templateHtml );

            // only show the template logo uploader if there is a div with id='template-logo'
            // then update the help-message on the loader based on the template-logo's data-instructions attribute and width and height
            // this gets called when the codeeditor is done initializing and when the cursor blurs out of the template code editor
            HtmlAgilityPack.HtmlNode templateLogoNode = templateDoc.GetElementbyId( "template-logo" );

            if ( templateLogoNode == null )
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the template logo help text.
        /// </summary>
        /// <param name="templateHtml">The template HTML.</param>
        /// <returns></returns>
        public static string GetTemplateLogoHelpText( string templateHtml )
        {
            var templateDoc = new HtmlAgilityPack.HtmlDocument();
            templateDoc.LoadHtml( templateHtml );

            HtmlAgilityPack.HtmlNode templateLogoNode = templateDoc.GetElementbyId( "template-logo" );
            if ( templateLogoNode == null )
            {
                return string.Empty;
            }

            string helpText = null;
            if ( templateLogoNode.Attributes.Contains( "data-instructions" ) )
            {
                helpText = templateLogoNode.Attributes["data-instructions"].Value;
            }

            if ( helpText.IsNullOrWhiteSpace() )
            {
                helpText = "The Logo that can be included in the contents of the message";
            }

            string helpWidth = null;
            string helpHeight = null;
            if ( templateLogoNode.Attributes.Contains( "width" ) )
            {
                helpWidth = templateLogoNode.Attributes["width"].Value;
            }

            if ( templateLogoNode.Attributes.Contains( "height" ) )
            {
                helpHeight = templateLogoNode.Attributes["height"].Value;
            }

            if ( helpWidth.IsNotNullOrWhiteSpace() && helpHeight.IsNotNullOrWhiteSpace() )
            {
                helpText += string.Format( " (Image size: {0}px x {1}px)", helpWidth, helpHeight );
            }

            return helpText;
        }

        #endregion TemplateHtml Helpers

        #region Controls

        /// <summary>
        /// Creates the dynamic lava value controls.
        /// </summary>
        public static void CreateDynamicLavaValueControls(
            Dictionary<string, string> lavaFieldsTemplateDictionary,
            Dictionary<string, string> lavaFieldsDefaultDictionary,
            PlaceHolder phLavaFieldsControls )
        {
            phLavaFieldsControls.Controls.Clear();

            foreach ( var keyValue in lavaFieldsTemplateDictionary )
            {
                var lavaValueControl = keyValue.Key.EndsWith( "Color" ) ? new ColorPicker() : new RockTextBox() { CssClass = "input-width-lg" };

                var rcwLavaValue = new RockControlWrapper();
                phLavaFieldsControls.Controls.Add( rcwLavaValue );

                rcwLavaValue.Label = keyValue.Key.SplitCase().Transform( To.TitleCase );
                rcwLavaValue.ID = "rcwLavaValue_" + keyValue.Key;

                lavaValueControl.ID = "lavaValue_" + keyValue.Key;
                lavaValueControl.AddCssClass( "pull-left" );
                lavaValueControl.Text = keyValue.Value;
                rcwLavaValue.Controls.Add( lavaValueControl );

                var btnRevertLavaValue = new Literal { ID = "btnRevertLavaValue_" + keyValue.Key };
                var defaultValue = lavaFieldsDefaultDictionary.GetValueOrNull( keyValue.Key );
                var visibility = keyValue.Value != defaultValue ? "visible" : "hidden";
                btnRevertLavaValue.Text = string.Format( "<div class='btn js-revertlavavalue' title='Revert to default' data-value-control='{0}' data-default='{1}' style='visibility:{2}'><i class='fa fa-times'></i></div>", lavaValueControl.ClientID, defaultValue, visibility );
                rcwLavaValue.Controls.Add( btnRevertLavaValue );
            }
        }

        /// <summary>
        /// Updates the lava fields template dictionary from controls.
        /// </summary>
        /// <param name="phLavaFieldsControls">The ph lava fields controls.</param>
        /// <param name="lavaFieldsTemplateDictionary">The lava fields template dictionary.</param>
        /// <returns></returns>
        public static Dictionary<string, string> UpdateLavaFieldsTemplateDictionaryFromControls( PlaceHolder phLavaFieldsControls, Dictionary<string, string> lavaFieldsTemplateDictionary )
        {
            foreach ( var item in lavaFieldsTemplateDictionary.ToList() )
            {
                // If this is a postback, there will be a control that holds the value
                var lavaValueControl = phLavaFieldsControls.FindControl( "lavaValue_" + item.Key ) as RockTextBox;
                if ( lavaValueControl != null && lavaValueControl.Text != item.Value )
                {
                    lavaFieldsTemplateDictionary[item.Key] = lavaValueControl.Text;
                }
            }

            return lavaFieldsTemplateDictionary;
        }

        #endregion Controls
    }
}
