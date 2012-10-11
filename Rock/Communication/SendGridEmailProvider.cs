using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Xml.Linq;

namespace Rock.Communication
    
    /// <summary>
    /// 
    /// </summary>
    public class SendGridEmailProvider: IEmailProvider
        
        private string _userName = string.Empty;
        private string _password = string.Empty;
        private string _urlBase = "https://sendgrid.com";

        /// <summary>
        /// Initializes a new instance of the <see cref="SendGridEmailProvider" /> class.
        /// </summary>
        public SendGridEmailProvider()
            
            // load up parameters from global settings
            var globalAttributes = Rock.Web.Cache.GlobalAttributesCache.Read();

            if ( globalAttributes.AttributeValues.ContainsKey("SendGridUsername") )
                _userName = globalAttributes.AttributeValues["SendGridUsername"].Value;

            if ( globalAttributes.AttributeValues.ContainsKey( "SendGridPassword" ) )
                _password = globalAttributes.AttributeValues["SendGridPassword"].Value;
        }

        /// <summary>
        /// Returns a list of bounced emails.  Paramenter tells whether soft bounces should also be returned.
        /// </summary>
        public List<BouncedEmail> BouncedEmails( bool includeSoftBounces )
            
            List<BouncedEmail> bouncedItems = new List<BouncedEmail>();
            
            string URLString = String.Format( "    0}/api/bounces.get.xml?api_user=    1}&api_key=    2}&date=1", _urlBase, _userName, _password );
            
            if ( !includeSoftBounces )
                URLString += "&type=hard";
            

            XDocument xdoc = XDocument.Load( URLString );

            var bounces = from bounce in xdoc.Descendants( "bounce" )
                          select new
                              
                              Status = bounce.Element( "status" ).Value,
                              Created = bounce.Element( "created" ).Value,
                              Reason = bounce.Element( "reason" ).Value,
                              Email = bounce.Element( "email" ).Value
                          };

            foreach ( var bounce in bounces )
                
                BouncedEmail email = new BouncedEmail();
                email.Email = bounce.Email;
                email.Reason = bounce.Reason;
                email.Status = bounce.Status;
                email.Created = Convert.ToDateTime( bounce.Created );

                bouncedItems.Add( email );
            }

            return bouncedItems;
        }


        /// <summary>
        /// Deletes bounced email from the email system
        /// </summary>
        public bool DeleteBouncedEmail( string email )
            
            string URLString = String.Format( "    0}/api/bounces.delete.xml?api_user=    1}&api_key=    2}&email=    3}", _urlBase, _userName, _password, email );

            XDocument xdoc = XDocument.Load( URLString );

            string result = ( from trans in xdoc.Descendants( "result" )
                                        select new
                                                
                                                trans.Element( "message" ).Value
                                            } ).FirstOrDefault().Value;

            if ( result == "true" )
                return true;
            else
                return false;
        }
    }
}