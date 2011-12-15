//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

namespace Rock.CMS
{
    public partial class BlogPost
    {
        /// <summary>
        /// Blog post status
        /// </summary>
        public enum PostStatus {
            
            /// <summary>
            /// A published blog post 
            /// </summary>
            Published = 1,

            /// <summary>
            /// A blog post that has not yet been published
            /// </summary>
            Draft = 2
        };
    }
}
