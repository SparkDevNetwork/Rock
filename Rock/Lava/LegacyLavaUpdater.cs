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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Rock.Data;
using Rock.Model;

namespace Rock.Lava
{
    /// <summary>
    /// Finds and updates legacy lava
    /// </summary>
    public class LegacyLavaUpdater
    {
        /// <summary>
        /// The SQL Update scripts that need to be run to update Legacy Lava code.
        /// </summary>
        /// <value>
        /// The SQL update scripts.
        /// </value>
        public List<string> SQLUpdateScripts
        {
            get
            {
                return _sqlUpdateScripts;
            }
        }

        private List<string> _sqlUpdateScripts = new List<string>();
        private IQueryable<EntityAttribute> entityAttributes = GetEntitiyAttributes();

        /// <summary>
        /// Finds the legacy lava.
        /// </summary>
        public void FindLegacyLava()
        {
            try
            {
                CheckHtmlContent();
                CheckAttributeValue();
                CheckAttribute();

                // Made Obsolete in 1.7, and will be removed in 1.10
                // CheckCommunicationTemplate();

                CheckSystemEmail();
                CheckWorkflowActionFormAttribute();
                CheckWorkflowActionForm();
                CheckRegistrationTemplateFormField();
                CheckReportField();
                OutputToText();
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, null );
                throw;
            }
        }

        /// <summary>
        /// Finds the legacy lava in files.
        /// </summary>
        public void FindLegacyLavaInFiles()
        {
            try
            {
                bool isUpdated = false;
                string basePath = Path.GetFullPath(Path.Combine( System.AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\" ));

                var files = System.IO.Directory.EnumerateFiles( basePath, "*.lava", System.IO.SearchOption.AllDirectories );
                foreach ( string filePath in files )
                {
                    isUpdated = false;
                    string lavaFileText = System.IO.File.ReadAllText( filePath );
                    
                    lavaFileText = ReplaceUnformatted( lavaFileText, ref isUpdated );
                    lavaFileText = ReplaceUrl( lavaFileText, ref isUpdated );
                    lavaFileText = ReplaceGlobal( lavaFileText, ref isUpdated );
                    lavaFileText = ReplaceDotNotation( lavaFileText, ref isUpdated );

                    if (isUpdated)
                    {
                        string s = filePath.Replace( ":", string.Empty ).Replace( "\\", "-" );
                        OutputToText( $"{s}", lavaFileText );
                        SQLUpdateScripts.Add($"File: {filePath}");
                    }
                }

                if ( isUpdated )
                {
                    OutputToText();
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, null );
                throw;
            }
        }

        /// <summary>
        /// Outputs to text.
        /// </summary>
        public void OutputToText()
        {
            System.IO.File.WriteAllLines( $"C:\\temp\\LegacyLavaUpdater_UpdateSQL_{DateTime.Now.ToString("yyyyMMdd-HHmmss")}.txt", SQLUpdateScripts );
        }

        /// <summary>
        /// Outputs to text.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="fileContnets">The file contents.</param>
        public void OutputToText(string fileName, string fileContnets)
        {
            System.IO.File.WriteAllText( $"C:\\temp\\LegacyLavaUpdater_{fileName}_{DateTime.Now.ToString( "yyyyMMdd-HHmmss" )}.txt", fileContnets );
        }

        /// <summary>
        /// Finds the attributes that are of type Entity..
        /// </summary>
        /// <returns></returns>
        public static IQueryable<EntityAttribute> GetEntitiyAttributes()
        {
            var rockContext = new RockContext();

            EntityTypeService entityTypeService = new EntityTypeService( rockContext );
            var entityTypes = entityTypeService.Queryable().Where( e => e.IsEntity == true );

            AttributeService attributeService = new AttributeService( rockContext );
            var attributes = attributeService.Queryable();

            return entityTypes
                .Where( t => t.FriendlyName != null && t.FriendlyName.Trim() != string.Empty )
                .Join(
                    attributes,
                    t => t.Id, 
                    a => a.EntityTypeId, 
                    ( type, attribute ) => new EntityAttribute
                    {
                        EntityTypeLegacyLava = type,
                        AttributeLegacyLava = attribute
                    } );
        }

        /// <summary>
        /// Updates all occurrences of GlobalAttribute legacy lava for the string
        /// </summary>
        /// <param name="lavaText">The lava text.</param>
        /// <param name="isUpdated">if set to <c>true</c> [is updated].</param>
        /// <returns></returns>
        public string ReplaceGlobal( string lavaText, ref bool isUpdated )
        {
            if ( lavaText.IsNullOrWhiteSpace() )
            {
                return lavaText;
            }

            try
            {
                int startIndex = lavaText.IndexOf( "GlobalAttribute" );
                while ( lavaText.Contains( "GlobalAttribute." ) )
                {
                    isUpdated = true;
                    int stopIndex = lavaText.IndexOf( "}}", startIndex ) - startIndex;
                    string legacyNotation = lavaText.Substring( startIndex, stopIndex ).Trim();
                    string globalAttribute = legacyNotation.Split( '.' )[1];
                    lavaText = lavaText.Replace( legacyNotation, $"'Global' | Attribute:'{globalAttribute}'" );
                    startIndex = lavaText.IndexOf( "GlobalAttribute" );
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, null );
                throw;
            }
            
            return lavaText;
        }

        /// <summary>
        /// Finds and replaces all instances of {{ *_unformatted* }} with {{ *, 'RawValue' }}
        /// </summary>
        /// <param name="lavaText">The lava text.</param>
        /// <param name="isUpdated">if set to <c>true</c> [is updated].</param>
        /// <returns></returns>
        public string ReplaceUnformatted( string lavaText, ref bool isUpdated )
        {
            if ( lavaText.IsNullOrWhiteSpace() || !lavaText.Contains( "{{" ) )
            {
                return lavaText;
            }

            try
            {
                Regex regex = new Regex( @"{{{{(.*?)_unformatted(.*?)}}}}" );
                MatchCollection matches = regex.Matches( lavaText );
                if ( matches.Count > 0 )
                {
                    isUpdated = true;
                    foreach ( Match match in matches )
                    {
                        string oldLava = match.Value;
                        string newLava = oldLava.Replace( "_unformatted", ",'RawValue'" );
                        lavaText = lavaText.Replace( oldLava, newLava );
                    }
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, null );
                throw;
            }

            return lavaText;
        }

        /// <summary>
        /// Finds and replaces all instances of {{ *_url* }} with {{ *, 'Url' }}
        /// </summary>
        /// <param name="lavaText">The lava text.</param>
        /// <param name="isUpdated">if set to <c>true</c> [is updated].</param>
        /// <returns></returns>
        public string ReplaceUrl( string lavaText, ref bool isUpdated )
        {
            if ( lavaText.IsNullOrWhiteSpace() || !lavaText.Contains( "{{" ) )
            {
                return lavaText;
            }
            
            try
            {
                Regex regex = new Regex( @"{{{{(.*?)_url(.*?)}}}}" );
                MatchCollection matches = regex.Matches( lavaText );
                if ( matches.Count > 0 )
                {
                    isUpdated = true;
                    foreach ( Match match in matches )
                    {
                        string oldLava = match.Value;
                        string newLava = oldLava.Replace( "_url", ",'Url'" );
                        lavaText = lavaText.Replace( oldLava, newLava );
                    }
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, null );
                throw;
            }

            return lavaText;
        }

        /// <summary>
        /// Replaces the dot notation with attribute notation for attributes
        /// </summary>
        /// <param name="lavaText">The lava text.</param>
        /// <param name="isUpdated">if set to <c>true</c> [is updated].</param>
        /// <returns></returns>
        public string ReplaceDotNotation( string lavaText, ref bool isUpdated )
        {
            if ( lavaText.IsNullOrWhiteSpace() )
            {
                return lavaText;
            }

            foreach ( EntityAttribute entityAttribute in entityAttributes )
            {
                if ( entityAttribute.EntityTypeLegacyLava.FriendlyName.IsNullOrWhiteSpace() )
                {
                    continue;
                }

                string friendlyName = entityAttribute.EntityTypeLegacyLava.FriendlyName.Replace( " ", string.Empty );
                string attributeKey = entityAttribute.AttributeLegacyLava.Key;
                Regex regex = new Regex( $"{{{{(.*?){friendlyName}.{attributeKey}([}}, ])(.*?)}}}}", RegexOptions.IgnoreCase );

                if ( regex.IsMatch( lavaText ) )
                {
                    isUpdated = true;
                    string legacyNotation = $"{friendlyName}.{attributeKey}";
                    string newLava = $"{friendlyName} | Attribute:'{attributeKey}'";
                    lavaText = lavaText.Replace( legacyNotation, newLava );
                }
            }

            return lavaText;
        }

        /// <summary>
        /// Checks HtmlContent model for legacy lava and outputs SQL to correct it
        /// Fields evaluated: Content
        /// </summary>
        public void CheckHtmlContent()
        {
            RockContext rockContext = new RockContext();
            HtmlContentService htmlContentService = new HtmlContentService( rockContext );

            foreach ( HtmlContent htmlContent in htmlContentService.Queryable().ToList() )
            {
                // don't change if modified
                if ( htmlContent.ModifiedDateTime != null )
                {
                    continue;
                }

                bool isUpdated = false;

                htmlContent.Content = ReplaceUnformatted( htmlContent.Content, ref isUpdated );
                htmlContent.Content = ReplaceUrl( htmlContent.Content, ref isUpdated );
                htmlContent.Content = ReplaceGlobal( htmlContent.Content, ref isUpdated );
                htmlContent.Content = ReplaceDotNotation( htmlContent.Content, ref isUpdated );

                if ( isUpdated )
                {
                    string sql = $"UPDATE [HtmlContent] SET [Content] = '{htmlContent.Content.Replace( "'", "''" )}' WHERE [Guid] = '{htmlContent.Guid}';";
                    _sqlUpdateScripts.Add( sql );
                }
            }
        }

        /// <summary>
        /// Checks AttributeValue model for legacy lava and outputs SQL to correct it.
        /// Fields evaluated: Value
        /// </summary>
        public void CheckAttributeValue()
        {
            RockContext rockContext = new RockContext();
            AttributeValueService attributeValueService = new AttributeValueService( rockContext );

            foreach ( AttributeValue attributeValue in attributeValueService.Queryable().ToList() )
            {
                // don't change if modified
                if ( attributeValue.ModifiedDateTime != null )
                {
                    continue;
                }

                bool isUpdated = false;

                attributeValue.Value = ReplaceUnformatted( attributeValue.Value, ref isUpdated );
                attributeValue.Value = ReplaceUrl( attributeValue.Value, ref isUpdated );
                attributeValue.Value = ReplaceGlobal( attributeValue.Value, ref isUpdated );
                attributeValue.Value = ReplaceDotNotation( attributeValue.Value, ref isUpdated );

                if ( isUpdated )
                {
                    string sql = $"UPDATE [AttributeValue] SET [Value] = '{attributeValue.Value.Replace( "'", "''" )}' WHERE [Guid] = '{attributeValue.Guid}';";
                    _sqlUpdateScripts.Add( sql );
                }
            }
        }

        /// <summary>
        /// Checks Attribute model for legacy lava and outputs SQL to correct it.
        /// Fields evaluated: DefaultValue
        /// </summary>
        public void CheckAttribute()
        {
            RockContext rockContext = new RockContext();
            AttributeService attributeService = new AttributeService( rockContext );

            foreach ( Model.Attribute attribute in attributeService.Queryable().ToList() )
            {
                // don't change if modified
                if ( attribute.ModifiedDateTime != null )
                {
                    continue;
                }

                bool isUpdated = false;

                attribute.DefaultValue = ReplaceUnformatted( attribute.DefaultValue, ref isUpdated );
                attribute.DefaultValue = ReplaceUrl( attribute.DefaultValue, ref isUpdated );
                attribute.DefaultValue = ReplaceGlobal( attribute.DefaultValue, ref isUpdated );
                attribute.DefaultValue = ReplaceDotNotation( attribute.DefaultValue, ref isUpdated );

                if ( isUpdated )
                {
                    string sql = $"UPDATE [Attribute] SET [DefaultValue] = '{attribute.DefaultValue.Replace( "'", "''" )}' WHERE [Guid] = '{attribute.Guid}';";
                    _sqlUpdateScripts.Add( sql );
                }
            }
        }

        /// <summary>
        /// Checks CommunicationTemplate model for legacy lava and outputs SQL to correct it.
        /// Fields evaluated: MediumDataJson Subject
        /// </summary>
        [RockObsolete( "1.7" )]
        [Obsolete( "The Communication.MediumDataJson and CommunicationTemplate.MediumDataJson fields will be removed in Rock 1.10" )]
        public void CheckCommunicationTemplate()
        {
            #pragma warning disable 0618
            RockContext rockContext = new RockContext();
            CommunicationTemplateService communicationTemplateService = new CommunicationTemplateService( rockContext );

            foreach ( CommunicationTemplate communicationTemplate in communicationTemplateService.Queryable().ToList() )
            {
                // don't change if modified
                if ( communicationTemplate.ModifiedDateTime != null )
                {
                    continue;
                }

                bool isUpdated = false;
                
                communicationTemplate.MediumDataJson = ReplaceUnformatted( communicationTemplate.MediumDataJson, ref isUpdated );
                communicationTemplate.MediumDataJson = ReplaceUrl( communicationTemplate.MediumDataJson, ref isUpdated );
                communicationTemplate.MediumDataJson = ReplaceGlobal( communicationTemplate.MediumDataJson, ref isUpdated );
                communicationTemplate.MediumDataJson = ReplaceDotNotation( communicationTemplate.MediumDataJson, ref isUpdated );

                communicationTemplate.Subject = ReplaceUnformatted( communicationTemplate.Subject, ref isUpdated );
                communicationTemplate.Subject = ReplaceUrl( communicationTemplate.Subject, ref isUpdated );
                communicationTemplate.Subject = ReplaceGlobal( communicationTemplate.Subject, ref isUpdated );
                communicationTemplate.Subject = ReplaceDotNotation( communicationTemplate.Subject, ref isUpdated );

                if ( isUpdated )
                {
                    string sql = $"UPDATE [CommunicationTemplate] SET [MediumDataJson] = '{communicationTemplate.MediumDataJson.Replace( "'", "''" )}', [Subject] = '{communicationTemplate.Subject.Replace( "'", "''" )}' WHERE [Guid] = '{communicationTemplate.Guid}';";
                    _sqlUpdateScripts.Add( sql );
                }
            }

            #pragma warning restore 0618
        }

        /// <summary>
        /// Checks SystemEmail model for legacy lava and outputs SQL to correct it.
        /// Fields evaluated: Title From To Cc Bcc Subject Body
        /// </summary>
        public void CheckSystemEmail()
        {
            try
            {
                RockContext rockContext = new RockContext();
                SystemEmailService systemEmailService = new SystemEmailService( rockContext );

                foreach ( SystemEmail systemEmail in systemEmailService.Queryable().ToList() )
                {
                    // don't change if modified
                    if ( systemEmail.ModifiedDateTime != null )
                    {
                        continue;
                    }

                    bool isUpdated = false;

                    systemEmail.Title = ReplaceUnformatted( systemEmail.Title, ref isUpdated );
                    systemEmail.Title = ReplaceUrl( systemEmail.Title, ref isUpdated );
                    systemEmail.Title = ReplaceGlobal( systemEmail.Title, ref isUpdated );
                    systemEmail.Title = ReplaceDotNotation( systemEmail.Title, ref isUpdated );

                    systemEmail.From = ReplaceUnformatted( systemEmail.From, ref isUpdated );
                    systemEmail.From = ReplaceUrl( systemEmail.From, ref isUpdated );
                    systemEmail.From = ReplaceGlobal( systemEmail.From, ref isUpdated );
                    systemEmail.From = ReplaceDotNotation( systemEmail.From, ref isUpdated );

                    systemEmail.To = ReplaceUnformatted( systemEmail.To, ref isUpdated );
                    systemEmail.To = ReplaceUrl( systemEmail.To, ref isUpdated );
                    systemEmail.To = ReplaceGlobal( systemEmail.To, ref isUpdated );
                    systemEmail.To = ReplaceDotNotation( systemEmail.To, ref isUpdated );

                    systemEmail.Cc = ReplaceUnformatted( systemEmail.Cc, ref isUpdated );
                    systemEmail.Cc = ReplaceUrl( systemEmail.Cc, ref isUpdated );
                    systemEmail.Cc = ReplaceGlobal( systemEmail.Cc, ref isUpdated );
                    systemEmail.Cc = ReplaceDotNotation( systemEmail.Cc, ref isUpdated );

                    systemEmail.Bcc = ReplaceUnformatted( systemEmail.Bcc, ref isUpdated );
                    systemEmail.Bcc = ReplaceUrl( systemEmail.Bcc, ref isUpdated );
                    systemEmail.Bcc = ReplaceGlobal( systemEmail.Bcc, ref isUpdated );
                    systemEmail.Bcc = ReplaceDotNotation( systemEmail.Bcc, ref isUpdated );

                    systemEmail.Subject = ReplaceUnformatted( systemEmail.Subject, ref isUpdated );
                    systemEmail.Subject = ReplaceUrl( systemEmail.Subject, ref isUpdated );
                    systemEmail.Subject = ReplaceGlobal( systemEmail.Subject, ref isUpdated );
                    systemEmail.Subject = ReplaceDotNotation( systemEmail.Subject, ref isUpdated );

                    systemEmail.Body = ReplaceUnformatted( systemEmail.Body, ref isUpdated );
                    systemEmail.Body = ReplaceUrl( systemEmail.Body, ref isUpdated );
                    systemEmail.Body = ReplaceGlobal( systemEmail.Body, ref isUpdated );
                    systemEmail.Body = ReplaceDotNotation( systemEmail.Body, ref isUpdated );

                    if ( isUpdated )
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine( "UPDATE[SystemEmail]" );
                        if ( systemEmail.Title != null )
                        {
                            sb.AppendLine( $"SET [Title] = '{systemEmail.Title.Replace( "'", "''" )}', " );
                        }

                        if ( systemEmail.From != null )
                        {
                            sb.AppendLine( $"[From] = '{systemEmail.From.Replace( "'", "''" )}', " );
                        }

                        if ( systemEmail.To != null)
                        {
                            sb.AppendLine( $"[To] = '{systemEmail.To.Replace( "'", "''" )}', " );
                        }

                        if ( systemEmail.Cc != null)
                        {
                            sb.AppendLine( $"[Cc] = '{systemEmail.Cc.Replace( "'", "''" )}', " );
                        }

                        if ( systemEmail.Bcc != null )
                        {
                            sb.AppendLine( $"[Bcc] = '{systemEmail.Bcc.Replace( "'", "''" )}', " );
                        }

                        if ( systemEmail.Subject != null )
                        {
                            sb.AppendLine( $"[Subject] = '{systemEmail.Subject.Replace( "'", "''" )}' , " );
                        }

                        if ( systemEmail.Body != null )
                        {
                            sb.AppendLine( $"[Body] = '{systemEmail.Body.Replace( "'", "''" )}' " );
                        }

                        sb.AppendLine( $"WHERE [Guid] = '{systemEmail.Guid}';" );

                        _sqlUpdateScripts.Add( sb.ToString() );
                    }
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, null );
                throw;
            }
        }

        /// <summary>
        /// Checks WorkflowActionFormAttribute model for legacy lava and outputs SQL to correct it.
        /// Fields evaluated: PreHtml PostHtml
        /// </summary>
        public void CheckWorkflowActionFormAttribute()
        {
            RockContext rockContext = new RockContext();
            WorkflowActionFormAttributeService workflowActionFormAttributeService = new WorkflowActionFormAttributeService( rockContext );

            foreach ( WorkflowActionFormAttribute workflowActionFormAttribute in workflowActionFormAttributeService.Queryable().ToList() )
            {
                // don't change if modified
                if ( workflowActionFormAttribute.ModifiedDateTime != null )
                {
                    continue;
                }

                bool isUpdated = false;

                workflowActionFormAttribute.PreHtml = ReplaceUnformatted( workflowActionFormAttribute.PreHtml, ref isUpdated );
                workflowActionFormAttribute.PreHtml = ReplaceUrl( workflowActionFormAttribute.PreHtml, ref isUpdated );
                workflowActionFormAttribute.PreHtml = ReplaceGlobal( workflowActionFormAttribute.PreHtml, ref isUpdated );
                workflowActionFormAttribute.PreHtml = ReplaceDotNotation( workflowActionFormAttribute.PreHtml, ref isUpdated );

                workflowActionFormAttribute.PostHtml = ReplaceUnformatted( workflowActionFormAttribute.PostHtml, ref isUpdated );
                workflowActionFormAttribute.PostHtml = ReplaceUrl( workflowActionFormAttribute.PostHtml, ref isUpdated );
                workflowActionFormAttribute.PostHtml = ReplaceGlobal( workflowActionFormAttribute.PostHtml, ref isUpdated );
                workflowActionFormAttribute.PostHtml = ReplaceDotNotation( workflowActionFormAttribute.PostHtml, ref isUpdated );

                if ( isUpdated )
                {
                    string sql = $"UPDATE [WorkflowActionFormAttribute] SET [PreHtml] = '{workflowActionFormAttribute.PreHtml.Replace( "'", "''" )}', [PostHtml] = '{workflowActionFormAttribute.PostHtml.Replace( "'", "''" )}' WHERE [Guid] = '{workflowActionFormAttribute.Guid}';";
                    _sqlUpdateScripts.Add( sql );
                }
            }
        }

        /// <summary>
        /// Checks WorkflowActionForm model for legacy lava and outputs SQL to correct it.
        /// Fields evaluated: Header Footer
        /// </summary>
        public void CheckWorkflowActionForm()
        {
            RockContext rockContext = new RockContext();
            WorkflowActionFormService workflowActionFormService = new WorkflowActionFormService( rockContext );

            foreach ( WorkflowActionForm workflowActionForm in workflowActionFormService.Queryable().ToList() )
            {
                // don't change if modified
                if ( workflowActionForm.ModifiedDateTime != null )
                {
                    continue;
                }

                bool isUpdated = false;

                workflowActionForm.Header = ReplaceUnformatted( workflowActionForm.Header, ref isUpdated );
                workflowActionForm.Header = ReplaceUrl( workflowActionForm.Header, ref isUpdated );
                workflowActionForm.Header = ReplaceGlobal( workflowActionForm.Header, ref isUpdated );
                workflowActionForm.Header = ReplaceDotNotation( workflowActionForm.Header, ref isUpdated );

                workflowActionForm.Footer = ReplaceUnformatted( workflowActionForm.Footer, ref isUpdated );
                workflowActionForm.Footer = ReplaceUrl( workflowActionForm.Footer, ref isUpdated );
                workflowActionForm.Footer = ReplaceGlobal( workflowActionForm.Footer, ref isUpdated );
                workflowActionForm.Footer = ReplaceDotNotation( workflowActionForm.Footer, ref isUpdated );

                if ( isUpdated )
                {
                    string sql = $"UPDATE [WorkflowActionForm] SET [Header] = '{workflowActionForm.Header.Replace( "'", "''" )}', [Footer] = '{workflowActionForm.Footer.Replace( "'", "''" )}' WHERE [Guid] = '{workflowActionForm.Guid}';";
                    _sqlUpdateScripts.Add( sql );
                }
            }
        }

        /// <summary>
        /// Checks RegistrationTemplateFormField model for legacy lava and outputs SQL to correct it.
        /// Fields evaluated: PreText PostText 
        /// </summary>
        public void CheckRegistrationTemplateFormField()
        {
            RockContext rockContext = new RockContext();
            RegistrationTemplateFormFieldService registrationTemplateFormFieldService = new RegistrationTemplateFormFieldService( rockContext );

            foreach ( RegistrationTemplateFormField registrationTemplateFormField in registrationTemplateFormFieldService.Queryable().ToList() )
            {
                // don't change if modified
                if ( registrationTemplateFormField.ModifiedDateTime != null )
                {
                    continue;
                }

                bool isUpdated = false;

                registrationTemplateFormField.PreText = ReplaceUnformatted( registrationTemplateFormField.PreText, ref isUpdated );
                registrationTemplateFormField.PreText = ReplaceUrl( registrationTemplateFormField.PreText, ref isUpdated );
                registrationTemplateFormField.PreText = ReplaceGlobal( registrationTemplateFormField.PreText, ref isUpdated );
                registrationTemplateFormField.PreText = ReplaceDotNotation( registrationTemplateFormField.PreText, ref isUpdated );

                registrationTemplateFormField.PostText = ReplaceUnformatted( registrationTemplateFormField.PostText, ref isUpdated );
                registrationTemplateFormField.PostText = ReplaceUrl( registrationTemplateFormField.PostText, ref isUpdated );
                registrationTemplateFormField.PostText = ReplaceGlobal( registrationTemplateFormField.PostText, ref isUpdated );
                registrationTemplateFormField.PostText = ReplaceDotNotation( registrationTemplateFormField.PostText, ref isUpdated );

                if ( isUpdated )
                {
                    string sql = $"UPDATE [RegistrationTemplateFormField] SET [PreText] = '{registrationTemplateFormField.PreText.Replace( "'", "''" )}', [PostText] = '{registrationTemplateFormField.PostText.Replace( "'", "''" )}' WHERE [Id] = {registrationTemplateFormField.Id};";
                    _sqlUpdateScripts.Add( sql );
                }
            }
        }

        /// <summary>
        /// Checks ReportField model for legacy lava and outputs SQL to correct it.
        /// Fields evaluated: Selection 
        /// </summary>
        public void CheckReportField()
        {
            RockContext rockContext = new RockContext();
            ReportFieldService reportFieldService = new ReportFieldService( rockContext );

            foreach ( ReportField reportField in reportFieldService.Queryable().ToList() )
            {
                // don't change if modified
                if ( reportField.ModifiedDateTime != null )
                {
                    continue;
                }

                bool isUpdated = false;

                reportField.Selection = ReplaceUnformatted( reportField.Selection, ref isUpdated );
                reportField.Selection = ReplaceUrl( reportField.Selection, ref isUpdated );
                reportField.Selection = ReplaceGlobal( reportField.Selection, ref isUpdated );
                reportField.Selection = ReplaceDotNotation( reportField.Selection, ref isUpdated );

                if ( isUpdated )
                {
                    string sql = $"UPDATE [ReportField] SET [Selection] = '{reportField.Selection.Replace( "'", "''" )}' WHERE [Id] = {reportField.Id};";
                    _sqlUpdateScripts.Add( sql );
                }
            }
        }
    }

    /// <summary>
    /// POCO to hold matched EntityAttributes and Attributes
    /// </summary>
    public class EntityAttribute
    {
        /// <summary>
        /// Gets or sets the entity type legacy lava.
        /// </summary>
        /// <value>
        /// The entity type legacy lava.
        /// </value>
        public EntityType EntityTypeLegacyLava { get; set; }

        /// <summary>
        /// Gets or sets the attribute legacy lava.
        /// </summary>
        /// <value>
        /// The attribute legacy lava.
        /// </value>
        public Rock.Model.Attribute AttributeLegacyLava { get; set; }
    }
}
