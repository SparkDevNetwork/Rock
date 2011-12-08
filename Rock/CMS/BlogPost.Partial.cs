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
