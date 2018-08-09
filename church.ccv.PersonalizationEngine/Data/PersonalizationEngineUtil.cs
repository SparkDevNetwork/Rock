using church.ccv.PersonalizationEngine.Data;
using church.ccv.PersonalizationEngine.Model;
using Rock.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static church.ccv.PersonalizationEngine.Model.Campaign;

namespace church.ccv.PersonalizationEngine.Data
{
    public static class PersonalizationEngineUtil
    {
        #region PERSONA
        public static bool PersonaFits( Persona persona, int personId )
        {
            // determine if the given personId fits the given persona

            // execute the sql defining that persona
            using ( RockContext rockContext = new RockContext( ) )
            {
                var fitsPersona = rockContext.Database.SqlQuery<int>
                (
                    persona.RockSQL,
                    new SqlParameter( "@PersonId", personId ) { SqlDbType = SqlDbType.Int, IsNullable = false }
                ).SingleOrDefault( );

                // if the resulting value returned is non zero, then the person matched the persona definition
                return fitsPersona == 1 ? true : false;
            }
        }

        public static Persona GetPersona( int id )
        {
            // return a particular persona

            using ( RockContext rockContext = new RockContext( ) )
            {
                PersonalizationEngineService<Persona> peService = new PersonalizationEngineService<Persona>( rockContext );

                Persona persona = peService.Get( id );

                return persona;
            }
        }
        
        public static List<Persona> GetPersonas( int personId )
        {
            // return all personas that the personId fits

            using ( RockContext rockContext = new RockContext( ) )
            {
                PersonalizationEngineService<Persona> peService = new PersonalizationEngineService<Persona>( rockContext );
                var personaQuery = peService.Queryable( ).AsNoTracking( );
             
                // start with an empty list of personas
                List<Persona> personaList = new List<Persona>( );
                foreach( var persona in personaQuery )
                {
                    // for each persona, execute the sql defining that persona
                    if ( PersonaFits( persona, personId ) )
                    {
                        // if it returned true, then the persona fit, so add it to the list
                        personaList.Add( persona );
                    }
                }

                return personaList;
            }
        }
        #endregion

        #region CAMPAIGN
        public static Campaign GetCampaign( int id )
        {
            // get a particular campaign

            using ( RockContext rockContext = new RockContext( ) )
            { 
                PersonalizationEngineService<Campaign> peService = new PersonalizationEngineService<Campaign>( rockContext );

                var campaign = peService.Get( id );
                return campaign;
            }
        }

        public static List<Campaign> GetCampaigns( CampaignType[] typeList, DateTime? startDate = null, DateTime? endDate = null )
        {
            // get all campaigns of the provided types, that fall within the requested date range

            //note: the dates are treated as inclusive, meaning the campaign must have a startDate BEFORE the provided startDate
            // and an end date that lands AFTER the provided endDate

            using ( RockContext rockContext = new RockContext( ) )
            {
                PersonalizationEngineService<Campaign> peService = new PersonalizationEngineService<Campaign>( rockContext );

                // the types column on the campaign is a comma seperated value of all Types the campaign should display on.
                
                // We'll take the provided typeList and convert it into a string array of the types.
                // Then we'll use linq to take each element in the typeListArray, and see if any of those elements are contained
                // in the Campaign's Type CSV. If they are, then it's a match!
                
                // first convert the Enum type array into a string array
                string[] typesAsString = new string[typeList.Length];
                for( int i = 0; i < typeList.Length; i++ )
                {
                    typesAsString[ i ] = typeList[i].ToString( ).ToLower( );
                }
                
                var campaigns = peService.Queryable( ).AsNoTracking( )
                                         .Where( c => c.StartDate <= startDate.Value || startDate.HasValue == false )
                                         .Where( c => c.EndDate >= endDate.Value || endDate.HasValue == false )
                                         // for each Campaign, see if any element of typesAsString is contained in c.Type (the CSV)
                                         .Where( c => typesAsString.Any( t => c.Type.Contains( t ) ) )
                                         .ToList( ); //take those
                return campaigns;
            }
        }
        #endregion

        #region Linkages
        public static List<Persona> GetPersonasForCampaign( int campaignId )
        {
            // get all the personas tied to the given campaign

            using ( RockContext rockContext = new RockContext( ) )
            {
                // get queries to the linkage and persona table
                var peLinkageQry = new PersonalizationEngineService<Linkage>( rockContext ).Queryable( ).AsNoTracking( );
                var pePersonaQry = new PersonalizationEngineService<Persona>( rockContext ).Queryable( ).AsNoTracking( );

                // join the persona table to the linkage table, and take all rows where the campaign id matches the provided campaign
                var personas = peLinkageQry.Join( pePersonaQry, l => l.PersonaId, p => p.Id, ( l, p ) => new { Linkage = l, Persona = p } )
                                           .Where( lp => lp.Linkage.CampaignId == campaignId )
                                           .Select( a => a.Persona )
                                           .ToList( );

                return personas;
            }
        }

        public static List<Campaign> GetCampaignsForPersona( int personaId )
        {
            // get all the campaigns tied to the given persona

            using ( RockContext rockContext = new RockContext( ) )
            {
                // get queries to the linkage and persona table
                var peLinkageQry = new PersonalizationEngineService<Linkage>( rockContext ).Queryable( ).AsNoTracking( );
                var peCampaignQry = new PersonalizationEngineService<Campaign>( rockContext ).Queryable( ).AsNoTracking( );

                // join the persona table to the linkage table, and take all rows where the campaign id matches the provided campaign
                var campaigns = peLinkageQry.Join( peCampaignQry, l => l.CampaignId, c => c.Id, ( l, c ) => new { Linkage = l, Campaign = c } )
                                            .Where( lp => lp.Linkage.PersonaId == personaId )
                                            .Select( a => a.Campaign )
                                            .ToList( );

                return campaigns;
            }
        }
        #endregion

        #region General
        public static Campaign GetRelevantCampaign( CampaignType[] campaignTypeList, int personId )
        {
            //given a person id, simply get a relevant campaign of the given type. If there are more than one, it'll take the first found.

            // get all the campaigns that match the center card
            var campaignList = GetCampaigns( campaignTypeList, DateTime.Now, DateTime.Now );

            // now go thru their personas, and take the first campaign with a persona that fits
            Campaign relevantCampaign = null;
            foreach( Campaign campaign in campaignList )
            {
                var personas = GetPersonasForCampaign( campaign.Id );
                foreach( var persona in personas )
                {
                    // as soon as we find a matching persona, take this campaign and stop searching
                    if( PersonaFits( persona, personId ) )
                    {
                        relevantCampaign = campaign;
                        break;
                    }
                }

                // if we found a relevant campaign going thru its personas, stop
                if ( relevantCampaign != null )
                {
                    break;
                }
            }

            return relevantCampaign;
        }
        #endregion
    }
}
