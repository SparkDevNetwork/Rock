using System.ComponentModel;

namespace Rock.Enums.AI
{
    /// <summary>
    /// The type of text enhancement to be performed by AI automation (if any).
    /// </summary>
    public enum TextEnhancement
    {
        /// <summary>
        /// Do not make any changes in regards to text enhancement.
        /// </summary>
        NoChanges = 0,

        /// <summary>
        /// Fix minor formatting and spelling mistakes.
        /// </summary>
        [Description("Minor Formatting and Spelling")]
        MinorFormattingAndSpelling = 1,

        /// <summary>
        /// Improve the readabililty of the text.
        /// </summary>
        EnhanceReadability = 2
    }
}
