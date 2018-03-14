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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Rock.Data;
using Rock.Model;

namespace Rock.Lava
{
    /// <summary>
    /// Finds and updatges legacy lava
    /// </summary>
    public class LegacyLavaUpdater
    {
        /// <summary>
        /// Match Match EntityType.AttributeKey unless the key has "_u"
        /// </summary>
        private string DotNotationPattern = "{{(.*?){0}.{1}(.*?)}}";
        
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
        private IQueryable<EntityAttribute> EntityAttributes = GetEntitiyAttributes();

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
                CheckAttribute();
                CheckCommunicationTemplate();
                CheckSystemEmail();
                CheckWorkflowActionFormAttribute();
                CheckWorkflowActionForm();
                CheckRegistrationTemplateFormField();
                CheckReportField();
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine( e.Message );
            }
        }

        /// <summary>
        /// Finds the attributes that are of type Entity..
        /// </summary>
        /// <returns></returns>
        private static IQueryable<EntityAttribute> GetEntitiyAttributes()
        {
            var rockContext = new RockContext();

            EntityTypeService entityTypeService = new EntityTypeService( rockContext );
            var entityTypes = entityTypeService.Queryable().Where( e => e.IsEntity == true );

            AttributeService attributeService = new AttributeService( rockContext );
            var attributes = attributeService.Queryable();

            return entityTypes.Join( attributes,
                t => t.Id, 
                a => a.EntityTypeId, 
                ( type, attribute ) => new EntityAttribute
                {
                    EntityTypeLegacyLava = type,
                    AttributeLegacyLava = attribute
                } );
        }

        /// <summary>
        /// Updates all occurrances of GlobalAttribute legacy lava for the string
        /// </summary>
        /// <param name="lavaText">The lava text.</param>
        /// <returns></returns>
        private string ReplaceGlobal( string lavaText, ref bool isUpdated )
        {
            if ( lavaText.IsNullOrWhiteSpace() )
            {
                return lavaText;
            }

            int startIndex = lavaText.IndexOf( "GlobalAttribute" );
            while ( lavaText.Contains( "GlobalAttribute." ) )
            {
                isUpdated = true;
                int stopIndex = lavaText.IndexOf( " ", startIndex ) - startIndex;
                string legacyNotation = lavaText.Substring( startIndex, stopIndex ).Trim();
                string globalAttribute = legacyNotation.Split( '.' )[1];
                lavaText = lavaText.Replace( legacyNotation, $"''Global'' | Attribute:''{globalAttribute}''" );
                startIndex = lavaText.IndexOf( "GlobalAttribute" );
            }

            return lavaText;
        }

        private string ReplaceUnformatted( string lavaText, ref bool isUpdated )
        {
            if ( lavaText.IsNullOrWhiteSpace() )
            {
                return lavaText;
            }

            if ( lavaText.Contains( "_unformatted" ) )
            {
                isUpdated = true;
                lavaText = lavaText.Replace( "_unformatted", ", ''RawValue''" );
            }

            return lavaText;
        }

        private string ReplaceUrl( string lavaText, ref bool isUpdated )
        {
            if ( lavaText.IsNullOrWhiteSpace() )
            {
                return lavaText;
            }

            if ( lavaText.Contains( "_url" ) )
            {
                isUpdated = true;
                lavaText = lavaText.Replace( "_url", ", ''Url''" );
            }

            return lavaText;
        }

        private string ReplaceDotNotation( string lavaText, ref bool isUpdated )
        {
            if ( lavaText.IsNullOrWhiteSpace() )
            {
                return lavaText;
            }

            foreach ( EntityAttribute entityAttribute in EntityAttributes )
            {
                string friendlyName = entityAttribute.EntityTypeLegacyLava.FriendlyName;
                string attributeKey = entityAttribute.AttributeLegacyLava.Key;
                Regex regex = new Regex( string.Format( DotNotationPattern, friendlyName, attributeKey ), RegexOptions.IgnoreCase );

                if ( regex.IsMatch( lavaText ) )
                {
                    isUpdated = true;
                    string legacyNotation = $"{friendlyName}.{attributeKey}";
                    string newLava = $"{friendlyName} | Attribute: ''{attributeKey}''";
                    lavaText = lavaText.Replace( legacyNotation, newLava );
                }
            }

            return lavaText;
        }
        /// <summary>
        /// Checks HtmlContent model for legacy lava and output SQL to correct it
        /// Fields evaluated: Content
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="attribute">The attribute.</param>
        private void CheckHtmlContent()
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
                    string sql = $"UPDATE HtmlContent SET Content = '{htmlContent.Content}' WHERE Id = {htmlContent.Id};";
                    _sqlUpdateScripts.Add( sql );
                }
            }
        }

        private void CheckAttributeValue()
        {
            //Value 
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
                    string sql = $"UPDATE AttributeValue SET Value = '{attributeValue.Value}' WHERE Id = {attributeValue.Id};";
                    _sqlUpdateScripts.Add( sql );
                }
            }
        }

        private void CheckAttribute()
        {
            //DefaultValue
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
                    string sql = $"UPDATE Attribute SET DefaultValue = '{attribute.DefaultValue}' WHERE ID = {attribute.Id};";
                    _sqlUpdateScripts.Add( sql );
                }
            }
        }


        private void CheckCommunicationTemplate()
        {
            //MediumDataJson [Subject] 
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
                    string sql = $"UPDATE CommunicationTemplate SET MediumDataJson = '{communicationTemplate.MediumDataJson}', [Subject] = '{communicationTemplate.Subject}' WHERE ID = {communicationTemplate.Id};";
                    _sqlUpdateScripts.Add( sql );
                }
            }
        }


        private void CheckSystemEmail()
        {
            //Title [From] [To] [Cc] [Bcc] [Subject] [Body] 
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
                    string sql = $@"UPDATE [HtmlContent]
                        SET [Title] = '{systemEmail.Title}'
                        , [From] = '{systemEmail.From}'
                        , [To] = '{systemEmail.To}'
                        , [Cc] = '{systemEmail.Cc}'
                        , [Bcc] = '{systemEmail.Bcc}'
                        , [Subject] = '{systemEmail.Subject}'
                        , [Body] = '{systemEmail.Body}'
                        WHERE Id = {systemEmail.Id};";

                    _sqlUpdateScripts.Add( sql );
                }
            }
        }


        private void CheckWorkflowActionFormAttribute()
        {
            //PreHtml PostHtml 

        }


        private void CheckWorkflowActionForm()
        {
            //Header Footer 
        }


        private void CheckRegistrationTemplateFormField()
        {
            //PreText PostText 
        }

        private void CheckReportField()
        {
            //Selection 
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

                foreach ( EntityAttribute entityAttribute in EntityAttributes )
                {
                    string friendlyName = entityAttribute.EntityTypeLegacyLava.FriendlyName;
                    string attributeKey = entityAttribute.AttributeLegacyLava.Key;
                    Regex regex = new Regex( string.Format( DotNotationPattern, friendlyName, attributeKey ), RegexOptions.IgnoreCase );

                    if ( regex.IsMatch( reportField.Selection ) )
                    {
                        isUpdated = true;
                        string legacyNotation = $"{friendlyName}.{attributeKey}";
                        string newLava = $"{friendlyName} | Attribute: ''{attributeKey}''";
                        reportField.Selection = reportField.Selection.Replace( legacyNotation, newLava );
                    }
                }

                if ( isUpdated )
                {
                    string sql = $"UPDATE ReportField SET Selection = '{reportField.Selection}' WHERE ID = {reportField.Id};";
                    _sqlUpdateScripts.Add( sql );
                }
            }
        }

    }

    public class EntityAttribute
    {
        public EntityType EntityTypeLegacyLava { get; set; }
        public Rock.Model.Attribute AttributeLegacyLava { get; set; }

    }
}
