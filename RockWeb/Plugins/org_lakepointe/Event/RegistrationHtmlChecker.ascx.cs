using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AngleSharp.Text;
using DotLiquid.Util;
using Humanizer;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;



namespace RockWeb.Plugins.org_lakepointe.Event
{
    [DisplayName( "Registration HTML Checker" )]
    [Category( "LPC > Event" )]
    [Description( "Checks the HTML of registration templates for any issues." )]

    public partial class RegistrationHtmlChecker : RockBlock
    {
        private RockContext _context;

        #region Properties

        readonly List<string> stateStyling = new List<string> {
            "class=\"list-group-item list-group-item-success\" style=\"background-color: #eaf6ef;\"",
            "class=\"list-group-item list-group-item-warning\" style=\"background-color: #fffae5;\"",
            "class=\"list-group-item list-group-item-danger\" style=\"background-color: #fcf2f1;\""
        };

        readonly List<string> selfClosingTags = new List<string>
        {
            "area", "base", "br", "col", "embed", "hr", "img", "input", "link", "meta", "param", "source", "track", "wbr"
        };

        List<HtmlError> htmlErrors = new List<HtmlError>();

        #endregion
        #region Base Control Methods

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            _context = new RockContext();
        }

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                pnlMain.Visible = false;
                CheckTemplateHTML();
            }
        }

        #endregion  
        #region Events

        #endregion
        #region Methods

        private void CheckTemplateHTML()
        {
            RegistrationTemplate template = GetRegistrationTemplate();

            if ( template == null || template.Id <= 0 )
            {
                return;
            }

            pnlMain.Visible = true;

            int formIndex = 0;
            foreach ( var form in template.Forms.OrderBy( f => f.Order ) )
            {
                ValidateForm( form, formIndex );
                formIndex++;
            }

            DisplayResults( template );
        }

        private RegistrationTemplate GetRegistrationTemplate()
        {
            string pageParameter = PageParameter( "RegistrationTemplateId" );

            if ( pageParameter != "" )
            {
                int templateId = pageParameter.ToIntSafe();

                if ( templateId > 0 )
                {
                    RegistrationTemplateService templateService = new RegistrationTemplateService( _context );

                    RegistrationTemplate template = templateService.Get( templateId );

                    return template;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        // Runs all validators against the form
        private void ValidateForm( RegistrationTemplateForm form, int formIndex )
        {
            ValidateBasics( form, formIndex );
            ValidateTags( form, formIndex );
            ValidateAttributes( form, formIndex );
            ValidateBestPractices(form, formIndex);
        }

        // Validates that HTML in the form:
            // Has the correct closing tag for each opening tag
            // Does not have too many opening tags
            // Does not have too many closing tags
            // Is self-contained for filtered fields
            // Does not use <html>, <head>, or <body> tags
        private void ValidateBasics( RegistrationTemplateForm form, int formIndex )
        {
            // Check form without any filtered fields
            Dictionary<int, RegistrationTemplateFormField> filteredFields = new Dictionary<int, RegistrationTemplateFormField>();

            List<string> currentPosition = new List<string>();

            int fieldIndex = 0;
            foreach ( var field in form.Fields.OrderBy( f => f.Order ) )
            {
                if ( field.FieldVisibilityRules.RuleList.Count > 0 )
                {
                    // Field is filtered
                    filteredFields.Add( fieldIndex, field );
                }
                else
                {
                    // Field is not filtered and should be checked with the rest of the form

                    // Check Pre Text
                    // Regex finds any valid tag so that it can be checked against the rest of the HTML structure
                    var matches = Regex.Matches( field.PreText, "</?([a-z0-9])+", RegexOptions.IgnoreCase );

                    foreach ( Match match in matches )
                    {
                        if ( match.Success )
                        {
                            string value = match.Value.ToLower();

                            if ( value.Contains( "</" ) )
                            {
                                value = value.ReplaceFirst( "</", "" );

                                if ( selfClosingTags.Contains( value ) )
                                {
                                    htmlErrors.Add( new HtmlError(
                                            formIndex,
                                            fieldIndex,
                                            true,
                                            HttpUtility.HtmlEncode( $"<{value}> tags can't be closed. The </{value}> is redundant." ),
                                            HtmlError.HtmlState.Error ) );
                                }
                                else if ( currentPosition.Count > 0 )
                                {
                                    if ( currentPosition.Last() == value )
                                    {
                                        currentPosition.Pop();
                                    }
                                    else
                                    {
                                        htmlErrors.Add( new HtmlError(
                                            formIndex,
                                            fieldIndex,
                                            true,
                                            HttpUtility.HtmlEncode( $"The tag before </{value}> is not a <{value}> tag. Previous tag: <{currentPosition.Last()}>" ),
                                            HtmlError.HtmlState.Error ) );
                                        currentPosition.Pop();
                                    }
                                }
                                else
                                {
                                    htmlErrors.Add( new HtmlError(
                                        formIndex,
                                        fieldIndex,
                                        true,
                                        HttpUtility.HtmlEncode( $"There are no open tags for that </{value}> closing tag to close." ),
                                        HtmlError.HtmlState.Error ) );
                                }
                            }
                            else if ( value.Contains( "<" ) )
                            {
                                value = value.ReplaceFirst( "<", "" );

                                if ( value == "html" || value == "head" || value == "body" )
                                {
                                    htmlErrors.Add( new HtmlError(
                                        formIndex,
                                        fieldIndex,
                                        true,
                                        HttpUtility.HtmlEncode( $"There should only ever be one <{value}> tag per page. All Rock pages already have a <{value}> tag. Having multiple will likely cause rendering issues." ),
                                        HtmlError.HtmlState.Error ) );
                                }

                                if ( selfClosingTags.Contains( value ) == false )
                                {
                                    currentPosition.Add( value );
                                }
                            }
                        }
                    }

                    // Check Post Text
                    // Regex finds any valid tag so that it can be checked against the rest of the HTML structure
                    matches = Regex.Matches( field.PostText, "</?([a-z0-9])+", RegexOptions.IgnoreCase );

                    foreach ( Match match in matches )
                    {
                        if ( match.Success )
                        {
                            string value = match.Value.ToLower();

                            if ( value.Contains( "</" ) )
                            {
                                value = value.ReplaceFirst( "</", "" );

                                if ( selfClosingTags.Contains( value ) )
                                {
                                    htmlErrors.Add( new HtmlError(
                                            formIndex,
                                            fieldIndex,
                                            false,
                                            HttpUtility.HtmlEncode( $"<{value}> tags can't be closed. The </{value}> is redundant." ),
                                            HtmlError.HtmlState.Error ) );
                                }
                                else if ( currentPosition.Count > 0 )
                                {
                                    if ( currentPosition.Last() == value )
                                    {
                                        currentPosition.Pop();
                                    }
                                    else
                                    {
                                        htmlErrors.Add( new HtmlError(
                                            formIndex,
                                            fieldIndex,
                                            false,
                                            HttpUtility.HtmlEncode( $"The tag before </{value}> is not a <{value}> tag. Previous tag: <{currentPosition.Last()}>" ),
                                            HtmlError.HtmlState.Error ) );
                                    }
                                }
                                else
                                {
                                    htmlErrors.Add( new HtmlError(
                                        formIndex,
                                        fieldIndex,
                                        false,
                                        HttpUtility.HtmlEncode( $"There are no open tags for that </{value}> closing tag to close." ),
                                        HtmlError.HtmlState.Error ) );
                                }
                            }
                            else if ( value.Contains( "<" ) )
                            {
                                value = value.ReplaceFirst( "<", "" );

                                if ( value == "html" || value == "head" || value == "body" )
                                {
                                    htmlErrors.Add( new HtmlError(
                                        formIndex,
                                        fieldIndex,
                                        false,
                                        HttpUtility.HtmlEncode( $"There should only ever be one <{value}> tag per page. All Rock pages already have a <{value}> tag. Having multiple will likely cause rendering issues." ),
                                        HtmlError.HtmlState.Error ) );
                                }

                                if ( selfClosingTags.Contains( value ) == false )
                                {
                                    currentPosition.Add( value );
                                }
                            }
                        }
                    }
                }
                fieldIndex++;
            }
            // Check for unclosed tags
            foreach ( var parent in currentPosition )
            {
                htmlErrors.Add( new HtmlError(
                    formIndex,
                    null,
                    false,
                    HttpUtility.HtmlEncode( $"A <{parent}> tag was never closed." ),
                    HtmlError.HtmlState.Error ) );
            }

            // Check each filtered field by itself
            foreach ( var filteredField in filteredFields )
            {
                RegistrationTemplateFormField field = filteredField.Value;
                List<string> localCurrentPosition = new List<string>();

                // Check Pre Text
                // Regex finds any valid tag so that it can be checked against the rest of the HTML structure
                var matches = Regex.Matches( field.PreText, "</?([a-z0-9])+", RegexOptions.IgnoreCase );

                foreach ( Match match in matches )
                {
                    if ( match.Success )
                    {
                        string value = match.Value.ToLower();

                        if ( value.Contains( "</" ) )
                        {
                            value = value.ReplaceFirst( "</", "" );
                            if ( localCurrentPosition.Count > 0 )
                            {
                                if ( localCurrentPosition.Last() == value )
                                {
                                    localCurrentPosition.Pop();
                                }
                                else
                                {
                                    htmlErrors.Add( new HtmlError(
                                        formIndex,
                                        filteredField.Key,
                                        true,
                                        HttpUtility.HtmlEncode( $"The tag before </{value}> is not a <{value}> tag. Previous tag: <{localCurrentPosition.Last()}>" ),
                                        HtmlError.HtmlState.Error ) );
                                    localCurrentPosition.Pop();
                                }
                            }
                            else
                            {
                                htmlErrors.Add( new HtmlError(
                                    formIndex,
                                    filteredField.Key,
                                    true,
                                    HttpUtility.HtmlEncode( $"There are no open tags for that </{value}> closing tag to close. (filtered fields must be self-contained)" ),
                                    HtmlError.HtmlState.Error ) );
                            }
                        }
                        else if ( value.Contains( "<" ) )
                        {
                            value = value.ReplaceFirst( "<", "" );

                            if ( selfClosingTags.Contains( value ) == false )
                            {
                                localCurrentPosition.Add( value );
                            }
                        }
                    }
                }

                // Check Post Text
                // Regex finds any valid tag so that it can be checked against the rest of the HTML structure
                matches = Regex.Matches( field.PostText, "</?([a-z0-9])+", RegexOptions.IgnoreCase );

                foreach ( Match match in matches )
                {
                    if ( match.Success )
                    {
                        string value = match.Value.ToLower();

                        if ( value.Contains( "</" ) )
                        {
                            value = value.ReplaceFirst( "</", "" );
                            if ( localCurrentPosition.Count > 0 )
                            {
                                if ( localCurrentPosition.Last() == value )
                                {
                                    localCurrentPosition.Pop();
                                }
                                else
                                {
                                    htmlErrors.Add( new HtmlError(
                                        formIndex,
                                        filteredField.Key,
                                        false,
                                        HttpUtility.HtmlEncode( $"The tag before </{value}> is not a <{value}> tag. Previous tag: <{localCurrentPosition.Last()}>" ),
                                        HtmlError.HtmlState.Error ) );
                                }
                            }
                            else
                            {
                                htmlErrors.Add( new HtmlError(
                                    formIndex,
                                    filteredField.Key,
                                    false,
                                    HttpUtility.HtmlEncode( $"There are no open tags for that </{value}> closing tag to close. (filtered fields must be self-contained)" ),
                                    HtmlError.HtmlState.Error ) );
                            }
                        }
                        else if ( value.Contains( "<" ) )
                        {
                            value = value.ReplaceFirst( "<", "" );

                            if ( selfClosingTags.Contains( value ) == false )
                            {
                                localCurrentPosition.Add( value );
                            }
                        }
                    }
                }

                // Check for unclosed tags
                foreach ( var parent in localCurrentPosition )
                {
                    htmlErrors.Add( new HtmlError(
                        formIndex,
                        filteredField.Key,
                        false,
                        HttpUtility.HtmlEncode( $"A <{parent}> tag was never closed within the filtered attribute. (filtered fields must be self-contained)" ),
                        HtmlError.HtmlState.Error ) );
                }
            }
        }

        // Validates that HTML tags in the form:
            // Have a closing angle bracket
            // Don't have a space between the opening angle bracket and the tag name
        private void ValidateTags( RegistrationTemplateForm form, int formIndex )
        {
            int fieldIndex = 0;
            foreach ( var field in form.Fields.OrderBy( f => f.Order ) )
            {
                // Check Pre Text
                // Regex finds any tag that doesn't have a closing bracket before the next tag starts or the search area ends
                var preTextMatches = Regex.Matches( field.PreText, "</?[a-z0-9]*(?!([^<]*?>))", RegexOptions.IgnoreCase );

                foreach ( Match match in preTextMatches )
                {
                    if ( match.Success )
                    {
                        string value = match.Value.Replace( "<", "" ).Trim().Split(' ').FirstOrDefault().ToLower();

                        htmlErrors.Add( new HtmlError(
                            formIndex,
                            fieldIndex,
                            true,
                            HttpUtility.HtmlEncode( $"The <{value}> tag is missing a closing angle bracket (\">\")" ),
                            HtmlError.HtmlState.Error ) );
                    }
                }

                // Regex finds any tags with a non-alphanumeric character after the "<" or "</"
                preTextMatches = Regex.Matches( field.PreText, "</?[^a-z0-9/][^a-z0-9]*[a-z0-9]*", RegexOptions.IgnoreCase );

                foreach ( Match match in preTextMatches )
                {
                    if ( match.Success )
                    {
                        string value = Regex.Replace( match.Value, "[^a-z0-9]", "", RegexOptions.IgnoreCase );

                        htmlErrors.Add( new HtmlError(
                            formIndex,
                            fieldIndex,
                            true,
                            HttpUtility.HtmlEncode( $"The <{(match.Value.Contains("/") ? "/" : "")}{value}> tag has a space before the tag name" ),
                            HtmlError.HtmlState.Error ) );
                    }
                }

                // Check Post Text
                // Regex finds any tag that doesn't have a closing bracket before the next tag starts or the search area ends
                var postTextMatches = Regex.Matches( field.PostText, "</?[a-z0-9]*(?!([^<]*?>))", RegexOptions.IgnoreCase );

                foreach ( Match match in postTextMatches )
                {
                    if ( match.Success )
                    {
                        string value = match.Value.Replace( "<", "" ).Trim().Split( ' ' ).FirstOrDefault().ToLower();

                        htmlErrors.Add( new HtmlError(
                            formIndex,
                            fieldIndex,
                            false,
                            HttpUtility.HtmlEncode( $"The <{value}> tag is missing a closing angle bracket (\">\")" ),
                            HtmlError.HtmlState.Error ) );
                    }
                }

                // Regex finds any tags with a non-alphanumeric character after the "<" or "</"
                postTextMatches = Regex.Matches( field.PostText, "</?[^a-z0-9/][^a-z0-9]*[a-z0-9]*", RegexOptions.IgnoreCase );

                foreach ( Match match in postTextMatches )
                {
                    if ( match.Success )
                    {
                        string value = Regex.Replace( match.Value, "[^a-z0-9]", "", RegexOptions.IgnoreCase );

                        htmlErrors.Add( new HtmlError(
                            formIndex,
                            fieldIndex,
                            false,
                            HttpUtility.HtmlEncode( $"The <{( match.Value.Contains( "/" ) ? "/" : "" )}{value}> tag has a space before the tag name" ),
                            HtmlError.HtmlState.Error ) );
                    }
                }

                fieldIndex++;
            }
        }

        // Validates that HTML attributes in the form:
            // Leave a space after the end quotes before the next attribute
        private void ValidateAttributes( RegistrationTemplateForm form, int formIndex )
        {
            int fieldIndex = 0;
            foreach ( var field in form.Fields.OrderBy( f => f.Order ) )
            {
                // Check Pre Text
                // Regex finds any attribute that doesn't have a space after the end quotes before the next attribute
                var preTextMatches = Regex.Matches( field.PreText, @"(?<![""']\s)<[^>]*?[^=][""']\b", RegexOptions.IgnoreCase );

                foreach ( Match match in preTextMatches )
                {
                    if ( match.Success )
                    {
                        string value = match.Value.Replace( "<", "" ).Trim().Split( ' ' ).FirstOrDefault().ToLower();

                        htmlErrors.Add( new HtmlError(
                            formIndex,
                            fieldIndex,
                            true,
                            HttpUtility.HtmlEncode( $"The <{value}> tag does not leave a space between the end quote of an attribute and the start of another attribute." ),
                            HtmlError.HtmlState.Error ) );
                    }
                }

                // Check Post Text
                // Regex finds any attribute that doesn't have a space after the end quotes before the next attribute
                var postTextMatches = Regex.Matches( field.PostText, @"(?<![""']\s)<[^>]*?[^=][""']\b", RegexOptions.IgnoreCase );

                foreach ( Match match in postTextMatches )
                {
                    if ( match.Success )
                    {
                        string value = match.Value.Replace( "<", "" ).Trim().Split( ' ' ).FirstOrDefault().ToLower();

                        htmlErrors.Add( new HtmlError(
                            formIndex,
                            fieldIndex,
                            false,
                            HttpUtility.HtmlEncode( $"The <{value}> tag does not leave a space between the end quote of an attribute and the start of another attribute." ),
                            HtmlError.HtmlState.Error ) );
                    }
                }

                fieldIndex++;
            }
        }

        // Validates that HTML in the form:
            // Does not include an <h1>
            // Uses <em> instead of <i> where element is not an icon
            // Uses <strong> instead of <b>
            // Uses lowercase for tag names
        private void ValidateBestPractices( RegistrationTemplateForm form, int formIndex )
        {
            int fieldIndex = 0;
            foreach ( var field in form.Fields.OrderBy( f => f.Order ) )
            {
                // Check Pre Text
                if ( Regex.IsMatch( field.PreText.ToLower(), @"<h1[\s>]" ) )
                {
                    htmlErrors.Add( new HtmlError(
                        formIndex,
                        fieldIndex,
                        true,
                        HttpUtility.HtmlEncode( $"There should only ever be one <h1> per page. Since the page title is an <h1>, this heading should be replaced by an <h2>." ),
                        HtmlError.HtmlState.Warning ) );
                }
                if ( Regex.IsMatch( field.PreText.ToLower(), @"<b[\s>]" ) )
                {
                    htmlErrors.Add( new HtmlError(
                        formIndex,
                        fieldIndex,
                        true,
                        HttpUtility.HtmlEncode( $"Using <b> tags is bad practice because they represent styling and all styling should be in the CSS and not in the HTML. Use <strong> instead as it represents importance." ),
                        HtmlError.HtmlState.Warning ) );
                }
                // Finds any <i> tag that is being used as a FontAwesome icon, removes it, and then searches the remaining text for any remaining <i> tags
                if ( Regex.IsMatch(
                        Regex.Replace( field.PreText.ToLower(), @"<i[^>]+class=[""'][^""']*fa-", "" )
                        , @"<i[\s>]" ) )
                {
                    htmlErrors.Add( new HtmlError(
                        formIndex,
                        fieldIndex,
                        true,
                        HttpUtility.HtmlEncode( $"Using <i> tags is bad practice because they represent styling and all styling should be in the CSS and not in the HTML. Use <em> instead as it represents emphasis. An exception to this rule is when using <i> tags for FontAwesome icons." ),
                        HtmlError.HtmlState.Warning ) );
                }

                // Regex finds any <img> tag that doesn't have alt text
                if ( Regex.Matches( field.PreText.ToLower(), @"<img\s+(?!(?:[^>""']|""[^""]*""|'[^']*')*\salt\s*=)[^>]*>" ).Count > 0 )
                {
                    htmlErrors.Add( new HtmlError(
                        formIndex,
                        fieldIndex,
                        true,
                        HttpUtility.HtmlEncode( $"An <img> tag does not have alt text. Alt text is what people who can't see the image (due to slow internet connection or vision impairment) see/hear in place of the image. Alt text should describe the image. If the image is in a link, the alt text should describe where the link goes. If the image is only for decoration and does not provide additional information, an empty alt attribute should be used ( alt=\"\" )." ),
                        HtmlError.HtmlState.Warning ) );
                }

                // Regex finds any valid tag so that it can be checked to see if it uses any capital letters
                var preTextMatches = Regex.Matches( field.PreText, "</?([a-z0-9])+", RegexOptions.IgnoreCase );

                foreach ( Match match in preTextMatches )
                {
                    if ( match.Success )
                    {
                        string value = match.Value.ReplaceFirst( "<", "" );

                        if ( value != value.ToLower() )
                        {
                            htmlErrors.Add( new HtmlError(
                                formIndex,
                                fieldIndex,
                                true,
                                HttpUtility.HtmlEncode( $"The <{value}> tag has capital letters in the tag name. Best practice is to always use lowercase tag names." ),
                                HtmlError.HtmlState.Warning ) );
                        }
                    }
                }

                // Check Post Text
                if ( Regex.IsMatch( field.PostText.ToLower(), @"<h1[\s>]" ) )
                {
                    htmlErrors.Add( new HtmlError(
                        formIndex,
                        fieldIndex,
                        false,
                        HttpUtility.HtmlEncode( $"There should only ever be one <h1> per page. Since the page title is an <h1>, this heading should be replaced by an <h2>." ),
                        HtmlError.HtmlState.Warning ) );
                }
                if ( Regex.IsMatch( field.PostText.ToLower(), @"<b[\s>]" ) )
                {
                    htmlErrors.Add( new HtmlError(
                        formIndex,
                        fieldIndex,
                        false,
                        HttpUtility.HtmlEncode( $"Using <b> tags is bad practice because they represent styling and all styling should be in the CSS and not in the HTML. Use <strong> instead as it represents importance." ),
                        HtmlError.HtmlState.Warning ) );
                }
                // Finds any <i> tag that is being used as a FontAwesome icon, removes it, and then searches the remaining text for any remaining <i> tags
                if ( Regex.IsMatch(
                        Regex.Replace( field.PostText.ToLower(), @"<i[^>]+class=[""'][^""']*fa-", "" )
                        , @"<i[\s>]" ) )
                {
                    htmlErrors.Add( new HtmlError(
                        formIndex,
                        fieldIndex,
                        false,
                        HttpUtility.HtmlEncode( $"Using <i> tags is bad practice because they represent styling and all styling should be in the CSS and not in the HTML. Use <em> instead as it represents importance. An exception to this rule is when using <i> tags for FontAwesome icons." ),
                        HtmlError.HtmlState.Warning ) );
                }

                // Regex finds any <img> tag that doesn't have alt text
                if ( Regex.Matches( field.PostText.ToLower(), @"<img\s+(?!(?:[^>""']|""[^""]*""|'[^']*')*\salt\s*=)[^>]*>" ).Count > 0 )
                {
                    htmlErrors.Add( new HtmlError(
                        formIndex,
                        fieldIndex,
                        false,
                        HttpUtility.HtmlEncode( $"An <img> tag does not have alt text. Alt text is what people who can't see the image (due to slow internet connection or vision impairment) see/hear in place of the image. Alt text should describe the image. If the image is in a link, the alt text should describe where the link goes. If the image is only for decoration and does not provide additional information, an empty alt attribute should be used ( alt=\"\" )." ),
                        HtmlError.HtmlState.Warning ) );
                }

                // Regex finds any valid tag so that it can be checked to see if it uses any capital letters
                var postTextMatches = Regex.Matches( field.PostText, "</?([a-z0-9])+", RegexOptions.IgnoreCase );

                foreach ( Match match in postTextMatches )
                {
                    if ( match.Success )
                    {
                        string value = match.Value.ReplaceFirst( "<", "" );

                        if ( value != value.ToLower() )
                        {
                            htmlErrors.Add( new HtmlError(
                                formIndex,
                                fieldIndex,
                                false,
                                HttpUtility.HtmlEncode( $"The <{value}> tag has capital letters in the tag name. Best practice is to always use lowercase tag names." ),
                                HtmlError.HtmlState.Warning ) );
                        }
                    }
                }

                fieldIndex++;
            }
        }

        private void DisplayResults( RegistrationTemplate template )
        {
            if ( template == null || template.Id <= 0 ) { return; }

            int warningCount = htmlErrors.Where( e => e.State == HtmlError.HtmlState.Warning ).Count();
            int errorCount = htmlErrors.Where( e => e.State == HtmlError.HtmlState.Error ).Count();

            lContent.Text += $@"<div class=""block-output"">
<div>
    <i class=""fa fa-times"" style=""width: 20px; color: {(errorCount > 0 ? "#e55235" : "#bbb")};height: 20px; background-color: {( errorCount > 0 ? "#fcf2f1" : "#f5f5f5" )}; text-align: center; border-radius: 4px; border: 1px solid #ddd; line-height: 20px; margin-right: 4px;""></i><strong>{errorCount}</strong>
    <i class=""fa fa-minus"" style=""width: 20px; color: {( warningCount > 0 ? "#8a6d3b" : "#bbb" )};height: 20px; background-color: {( warningCount > 0 ? "#fffae5" : "#f5f5f5" )}; text-align: center; border-radius: 4px; border: 1px solid #ddd; line-height: 20px; margin-right: 4px; margin-left: 8px;""></i><strong>{warningCount}</strong>
</div>
<div style=""text-align: right;"">
<a href=""#RegistrationHtmlCheckerLegend"" data-toggle=""collapse"">Legend <i class=""fa fa-chevron-down"" style=""font-size: 0.75rem;""></i></a>
</div>
<div id=""RegistrationHtmlCheckerLegend"" class=""collapse"">
<ul style=""list-style: none; padding: 0 10px; margin: 0;"">
    <li><i class=""fa fa-check"" style=""width: 20px; color: #108043;height: 20px; background-color: #eaf6ef; text-align: center; border-radius: 4px; border: 1px solid #ddd; line-height: 20px; margin-right: 4px;""></i><strong>Good:</strong> these items have no errors or warnings. Good job!</li>
    <li><i class=""fa fa-minus"" style=""width: 20px; color: #8a6d3b;height: 20px; background-color: #fffae5; text-align: center; border-radius: 4px; border: 1px solid #ddd; line-height: 20px; margin-right: 4px;""></i><strong>Warning:</strong> these items don't have anything that will break Obsidian, but have some things that could be improved.</li>
    <li><i class=""fa fa-times"" style=""width: 20px; color: #e55235;height: 20px; background-color: #fcf2f1; text-align: center; border-radius: 4px; border: 1px solid #ddd; line-height: 20px; margin-right: 4px;""></i><strong>Error:</strong> these items have some errors that will will break Obsidian. They need to be addressed before this registration template will work with Obsidian.</li>
</ul>
</div>";

            int formIndex = 0;
            foreach ( var form in template.Forms.OrderBy( f => f.Order ) )
            {
                lContent.Text += $"<h2>{form.Name}</h2><hr>";

                int fieldIndex = 0;
                foreach ( var field in form.Fields.OrderBy( f => f.Order ) )
                {
                    lContent.Text += "<ul class=\"list-group\">";

                    // Display Pre Text Results
                    List<HtmlError> preTextErrors = new List<HtmlError>();
                    HtmlError.HtmlState preTextMaxState = HtmlError.HtmlState.Good;
                    foreach ( var error in htmlErrors )
                    {
                        if ( error.FormIndex == formIndex && error.FieldIndex == fieldIndex && error.IsPreText == true )
                        {
                            preTextErrors.Add( error );

                            if ( error.State > preTextMaxState )
                            {
                                preTextMaxState = error.State;
                            }
                        }
                    }

                    if ( preTextErrors.Count > 0 )
                    {
                        lContent.Text += $"<li {stateStyling[( int )preTextMaxState]}><div style=\"float: right;\"><i class=\"fa fa-{(preTextMaxState == HtmlError.HtmlState.Warning ? "minus" : "times")}\"></i></div><div style=\"font-family: Menlo,Monaco,Consolas,'Courier New',monospace; white-space: pre-wrap; color: black;\">{HttpUtility.HtmlEncode( field.PreText )}</div></li>";
                        foreach ( var error in preTextErrors )
                        {
                            lContent.Text += $"<li {stateStyling[( int )error.State]}><div style=\"margin-left: 2rem; font-style: italic; font-weight: 500;\">{error.Message}</div></li>";
                        }
                    }
                    else
                    {
                        lContent.Text += $"<li {stateStyling[( int )HtmlError.HtmlState.Good]}><div style=\"float: right;\"><i class=\"fa fa-check\"></i></div><div style=\"font-family: Menlo,Monaco,Consolas,'Courier New',monospace; white-space: pre-wrap; color: black;\">{HttpUtility.HtmlEncode( field.PreText )}</div></li>";
                    }

                    // Display Field Name

                    lContent.Text += $"<li class=\"list-group-item\">{ (field.FieldVisibilityRules.RuleList.Count() > 0 ? "<i class=\"fa fa-filter\" style=\"width: 26px; padding: 5px 0; background-color: #ffc870; border-radius: 4px; text-align: center; color: white;\"></i>&nbsp;" : "") }<strong>{ field.Attribute?.Name ?? field.PersonFieldType.ToString()?.Humanize(LetterCasing.Title) }</strong></li>";

                    // Display Post Text Results

                    List<HtmlError> postTextErrors = new List<HtmlError>();
                    HtmlError.HtmlState postTextMaxState = HtmlError.HtmlState.Good;
                    foreach ( var error in htmlErrors )
                    {
                        if ( error.FormIndex == formIndex && error.FieldIndex == fieldIndex && error.IsPreText == false )
                        {
                            postTextErrors.Add( error );

                            if ( error.State > postTextMaxState )
                            {
                                postTextMaxState = error.State;
                            }
                        }
                    }

                    if ( postTextErrors.Count > 0 )
                    {
                        lContent.Text += $"<li {stateStyling[( int )postTextMaxState]}><div style=\"float: right;\"><i class=\"fa fa-{( postTextMaxState == HtmlError.HtmlState.Warning ? "minus" : "times" )}\"></i></div><div style=\"font-family: Menlo,Monaco,Consolas,'Courier New',monospace; white-space: pre-wrap; color: black;\">{HttpUtility.HtmlEncode( field.PostText )}</div></li>";
                        foreach ( var error in postTextErrors )
                        {
                            lContent.Text += $"<li {stateStyling[( int )error.State]}><div style=\"margin-left: 2rem; font-style: italic; font-weight: 500;\">{error.Message}</div></li>";
                        }
                    }
                    else
                    {
                        lContent.Text += $"<li {stateStyling[( int )HtmlError.HtmlState.Good]}><div style=\"float: right;\"><i class=\"fa fa-check\"></i></div><div style=\"font-family: Menlo,Monaco,Consolas,'Courier New',monospace; white-space: pre-wrap; color: black;\">{HttpUtility.HtmlEncode( field.PostText )}</div></li>";
                    }

                    lContent.Text += "</ul>";

                    // Increment Field Index
                    fieldIndex++; 
                }

                // Display errors with a null FieldIndex
                foreach ( var error in htmlErrors )
                {
                    if ( error.FormIndex == formIndex && error.FieldIndex == null )
                    {
                        lContent.Text += $@"
                            <ul class=""list-group"">
                                <li {stateStyling[( int )error.State]}>
                                    <div style=""margin-left: 2rem; font-style: italic; font-weight: 500;"">{error.Message}</div>
                                </li>
                            </ul>";
                    }
                }

                formIndex++;
            }

            lContent.Text += "</div>";
        }

        #endregion

        private class HtmlError
        {
            public int FormIndex { get; set; }
            public int? FieldIndex { get; set; }
            public bool IsPreText { get; set; }
            public string Message { get; set; }
            public HtmlState State { get; set; }

            public enum HtmlState
            {
                Good = 0,
                Warning = 1,
                Error = 2
            }

            public HtmlError( int formIndex, int? fieldIndex, bool isPreText, string message, HtmlState state )
            {
                FormIndex = formIndex;
                FieldIndex = fieldIndex;
                IsPreText = isPreText;
                Message = message;
                State = state;
            }
        }
    }
}
