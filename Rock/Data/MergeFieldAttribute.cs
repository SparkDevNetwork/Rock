//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;

namespace Rock.Data
{
    /// <summary>
    /// Custom attribute used to decorate model properties that should be available as merge field when previewing a sample list of entities (ex DataView)
    /// </summary>
    [AttributeUsage(AttributeTargets.Property )]
    public class MergeFieldAttribute : System.Attribute
    {
    }
}