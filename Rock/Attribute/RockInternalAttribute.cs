using System;

namespace Rock.Attribute
{
    /// <summary>
    /// Marks an API as internal to Rock. These APIs are not subject to the same
    /// compatibility standards as public APIs. It may be changed or removed
    /// without notice in any release. You should not use such APIs directly in
    /// any plug-ins. Doing so can result in application failures when updating
    /// to a new Rock release.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Enum
        | AttributeTargets.Class
        | AttributeTargets.Struct
        | AttributeTargets.Interface
        | AttributeTargets.Event
        | AttributeTargets.Field
        | AttributeTargets.Method
        | AttributeTargets.Delegate
        | AttributeTargets.Property
        | AttributeTargets.Constructor )]
    public sealed class RockInternalAttribute : System.Attribute
    {
    }
}
