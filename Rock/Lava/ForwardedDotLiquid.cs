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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace DotLiquid
{
    #region Interfaces

    [Obsolete( "DotLiquid is not supported and will be fully removed in the future." )]
    [Rock.RockObsolete( "18.0" )]
    public interface IIndexable
    {
        object this[object key] { get; }
        bool ContainsKey( object key );
    }

    [Obsolete( "DotLiquid is not supported and will be fully removed in the future." )]
    [Rock.RockObsolete( "18.0" )]
    public interface ILiquidizable
    {
        object ToLiquid();
    }

    [Obsolete( "DotLiquid is not supported and will be fully removed in the future." )]
    [Rock.RockObsolete( "18.0" )]
    public interface IContextAware
    {
        Context Context { set; }
    }

    [Obsolete( "DotLiquid is not supported and will be fully removed in the future." )]
    [Rock.RockObsolete( "18.0" )]
    public interface IValueTypeConvertible
    {
        object ConvertToValueType();
    }

    #endregion

    #region Attributes

    [Obsolete( "DotLiquid is not supported and will be fully removed in the future." )]
    [Rock.RockObsolete( "18.0" )]
    [AttributeUsage( AttributeTargets.Class )]
    public class LiquidTypeAttribute : Attribute
    {
        public string[] AllowedMembers { get; private set; }

        public LiquidTypeAttribute( params string[] allowedMembers )
        {
            AllowedMembers = allowedMembers;
        }
    }

    #endregion

    #region Mocked Classes

    // These classes aren't actual mocks, but they are basically "throw on everything"
    // placeholders as they shouldn't ever be actually used by a plugin. We just need
    // the structure to be valid.

    [Obsolete( "DotLiquid is not supported and will be fully removed in the future." )]
    [Rock.RockObsolete( "18.0" )]
    public class Hash : IDictionary<string, object>, IDictionary
    {
        #region Static construction methods

        public static Hash FromAnonymousObject( object anonymousObject )
        {
            throw new NotSupportedException();
        }

        public static Hash FromDictionary( IDictionary<string, object> dictionary )
        {
            throw new NotSupportedException();
        }

        #endregion

        #region Constructors

        public Hash( object defaultValue )
            : this()
        {
            throw new NotSupportedException();
        }

        public Hash( Func<Hash, string, object> lambda )
            : this()
        {
            throw new NotSupportedException();
        }

        public Hash()
        {
            throw new NotSupportedException();
        }

        #endregion

        public void Merge( IDictionary<string, object> otherValues )
        {
            throw new NotSupportedException();
        }

        public T Get<T>( string key )
        {
            throw new NotSupportedException();
        }

        #region IDictionary<string, object>

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            throw new NotSupportedException();
        }

        public void Remove( object key )
        {
            throw new NotSupportedException();
        }

        object IDictionary.this[object key]
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotSupportedException();
        }

        public void Add( KeyValuePair<string, object> item )
        {
            throw new NotSupportedException();
        }

        public bool Contains( object key )
        {
            throw new NotSupportedException();
        }

        public void Add( object key, object value )
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            throw new NotSupportedException();
        }

        public bool Contains( KeyValuePair<string, object> item )
        {
            throw new NotSupportedException();
        }

        public void CopyTo( KeyValuePair<string, object>[] array, int arrayIndex )
        {
            throw new NotSupportedException();
        }

        public bool Remove( KeyValuePair<string, object> item )
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IDictionary

        public void CopyTo( Array array, int index )
        {
            throw new NotSupportedException();
        }

        public int Count
        {
            get => throw new NotSupportedException();
        }

        public object SyncRoot
        {
            get => throw new NotSupportedException();
        }

        public bool IsSynchronized
        {
            get => throw new NotSupportedException();
        }

        ICollection IDictionary.Values
        {
            get => throw new NotSupportedException();
        }

        public bool IsReadOnly
        {
            get => throw new NotSupportedException();
        }

        public bool IsFixedSize
        {
            get => throw new NotSupportedException();
        }

        public bool ContainsKey( string key )
        {
            throw new NotSupportedException();
        }

        public void Add( string key, object value )
        {
            throw new NotSupportedException();
        }

        public bool Remove( string key )
        {
            throw new NotSupportedException();
        }

        public bool TryGetValue( string key, out object value )
        {
            throw new NotSupportedException();
        }

        public object this[string key]
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public ICollection<string> Keys
        {
            get => throw new NotSupportedException();
        }

        ICollection IDictionary.Keys
        {
            get => throw new NotSupportedException();
        }

        public ICollection<object> Values
        {
            get => throw new NotSupportedException();
        }

        #endregion
    }

    [Obsolete( "DotLiquid is not supported and will be fully removed in the future." )]
    [Rock.RockObsolete( "18.0" )]
    public class Strainer
    {
        public static void RegisterFilter( string filterName, Func<Context, List<object>, object> filterFunction )
        {
            throw new NotSupportedException();
        }

        public static void GlobalFilter( Type filter )
        {
            throw new NotSupportedException();
        }

        public static Strainer Create( Context context )
        {
            throw new NotSupportedException();
        }

        public IEnumerable<MethodInfo> Methods
        {
            get => throw new NotSupportedException();
        }

        public Strainer( Context context )
        {
            throw new NotSupportedException();
        }

        public void Extend( string filterName, Func<Context, List<object>, object> filterFunction )
        {
            throw new NotSupportedException();
        }

        public void Extend( Type type )
        {
            throw new NotSupportedException();
        }

        public bool RespondTo( string method )
        {
            throw new NotSupportedException();
        }

        public object Invoke( string method, List<object> args )
        {
            throw new NotSupportedException();
        }
    }

    [Obsolete( "DotLiquid is not supported and will be fully removed in the future." )]
    [Rock.RockObsolete( "18.0" )]
    public class Context
    {
        public List<Hash> Environments => throw new NotSupportedException();
        public List<Hash> Scopes => throw new NotSupportedException();
        public Hash Registers => throw new NotSupportedException();
        public List<Exception> Errors => throw new NotSupportedException();
        public Dictionary<Type, Func<object, object>> ValueTypeTransformers;

        public Func<object, object> GetValueTypeTransformer( Type type )
        {
            throw new NotSupportedException();
        }

        public Context( List<Hash> environments, Hash outerScope, Hash registers, bool rethrowErrors )
        {
            throw new NotSupportedException();
        }

        public Context()
            : this( new List<Hash>(), new Hash(), new Hash(), false )
        {
            throw new NotSupportedException();
        }

        public Strainer Strainer
        {
            get => throw new NotSupportedException();
        }

        public void AddFilters( IEnumerable<Type> filters )
        {
            throw new NotSupportedException();
        }

        public void AddFilters( params Type[] filters )
        {
            throw new NotSupportedException();
        }

        public string HandleError( Exception ex )
        {
            throw new NotSupportedException();
        }

        public object Invoke( string method, List<object> args )
        {
            throw new NotSupportedException();
        }

        public void Push( Hash newScope )
        {
            throw new NotSupportedException();
        }

        public void Merge( Hash newScopes )
        {
            throw new NotSupportedException();
        }

        public Hash Pop()
        {
            throw new NotSupportedException();
        }

        public void Stack( Hash newScope, Action callback )
        {
            throw new NotSupportedException();
        }

        public void Stack( Action callback )
        {
            throw new NotSupportedException();
        }

        public void ClearInstanceAssigns()
        {
            throw new NotSupportedException();
        }

        public object this[string key]
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public bool HasKey( string key )
        {
            throw new NotSupportedException();
        }
    }

    [Obsolete( "DotLiquid is not supported and will be fully removed in the future." )]
    [Rock.RockObsolete( "18.0" )]
    public class Document : Block
    {
        public override void Initialize( string tagName, string markup, List<string> tokens )
        {
        }

        protected override string BlockDelimiter
        {
            get { return string.Empty; }
        }

        protected override void AssertMissingDelimitation()
        {
        }

        public override void Render( Context context, TextWriter result )
        {
        }
    }

    [Obsolete( "DotLiquid is not supported and will be fully removed in the future." )]
    [Rock.RockObsolete( "18.0" )]
    public class RenderParameters
    {
        public Context Context { get; set; }

        public Hash LocalVariables { get; set; }
        public IEnumerable<Type> Filters { get; set; }
        public Hash Registers { get; set; }
        public Dictionary<Type, Func<object, object>> ValueTypeTransformers;

        public bool RethrowErrors { get; set; }

        public static RenderParameters FromContext( Context context )
        {
            throw new NotSupportedException();
        }
    }

    #endregion

    #region Placeholder Classes

    // These classes might actually be called or inherited by plugins, so we need them
    // to be somewhat useful. They don't need to actaully work, they just need to be
    // valid for the runtime. Therefore, some parts don't throw exceptions since they
    // might be called during Rock initialization by plugins and we don't want to blow
    // up that early.

    [Obsolete( "DotLiquid is not supported and will be fully removed in the future." )]
    [Rock.RockObsolete( "18.0" )]
    public abstract class DropBase : ILiquidizable, IIndexable, IContextAware
    {
        public Context Context { get; set; }

        public object this[object method]
        {
            get => null;
        }

        public virtual object BeforeMethod( string method )
        {
            return null;
        }

        public object InvokeDrop( object name )
        {
            return null;
        }

        public virtual bool ContainsKey( object name )
        {
            return false;
        }

        public virtual object ToLiquid()
        {
            return this;
        }
    }

    [Obsolete( "DotLiquid is not supported and will be fully removed in the future." )]
    [Rock.RockObsolete( "18.0" )]
    public abstract class Drop : DropBase
    {
    }

    [Obsolete( "DotLiquid is not supported and will be fully removed in the future." )]
    [Rock.RockObsolete( "18.0" )]
    public class DropProxy : DropBase, IValueTypeConvertible
    {
        public DropProxy( object obj, string[] allowedMembers )
        {
        }

        public DropProxy( object obj, string[] allowedMembers, Func<object, object> value )
        {
        }

        public virtual object ConvertToValueType()
        {
            return null;
        }
    }

    [Obsolete( "DotLiquid is not supported and will be fully removed in the future." )]
    [Rock.RockObsolete( "18.0" )]
    public class Tag
    {
        public List<object> NodeList { get; protected set; }
        protected string TagName { get; private set; }
        protected string Markup { get; private set; }

        protected internal Tag()
        {
        }

        internal virtual void AssertTagRulesViolation( List<object> rootNodeList )
        {
        }

        public virtual void Initialize( string tagName, string markup, List<string> tokens )
        {
            TagName = tagName;
            Markup = markup;
            Parse( tokens );
        }

        protected virtual void Parse( List<string> tokens )
        {
        }

        public string Name
        {
            get => GetType().Name.ToLower();
        }

        public virtual void Render( Context context, TextWriter result )
        {
        }
    }

    [Obsolete( "DotLiquid is not supported and will be fully removed in the future." )]
    [Rock.RockObsolete( "18.0" )]
    public class Block : Tag
    {
        protected override void Parse( List<string> tokens )
        {
        }

        public virtual void EndTag()
        {
        }

        public virtual void UnknownTag( string tag, string markup, List<string> tokens )
        {
        }

        protected virtual string BlockDelimiter
        {
            get => string.Format( "end{0}", TagName );
        }

        public Variable CreateVariable( string token )
        {
            throw new NotSupportedException();
        }

        public override void Render( Context context, TextWriter result )
        {
        }

        protected virtual void AssertMissingDelimitation()
        {
        }

        protected void RenderAll( List<object> list, Context context, TextWriter result )
        {
        }
    }

    [Obsolete( "DotLiquid is not supported and will be fully removed in the future." )]
    [Rock.RockObsolete( "18.0" )]
    public class Variable
    {
        public static readonly string FilterParser = "";

        public List<Filter> Filters { get; set; }
        public string Name { get; set; }

        public Variable( string markup )
        {
        }

        public void Render( Context context, TextWriter result )
        {
        }
        public class Filter
        {
            public Filter( string name, string[] arguments )
            {
                Name = name;
                Arguments = arguments;
            }

            public string Name { get; set; }
            public string[] Arguments { get; set; }
        }
    }

    [Obsolete( "DotLiquid is not supported and will be fully removed in the future." )]
    [Rock.RockObsolete( "18.0" )]
    public class Template
    {
        public static DotLiquid.NamingConventions.INamingConvention NamingConvention;
        public static DotLiquid.FileSystems.IFileSystem FileSystem { get; set; }
        public static bool DefaultIsThreadSafe { get; set; }
        public static Dictionary<string, Type> Tags { get; set; }

        static Template()
        {
        }

        public static void RegisterTag<T>( string name )
            where T : Tag, new()
        {
        }

        public static void RegisterTag( Type tagType, string name )
        {
        }

        public static Type GetTagType( string name )
        {
            return null;
        }

        public static void RegisterShortcode<T>( string name )
            where T : Tag, new()
        {
        }

        public static void UnregisterShortcode( string name )
        {
        }

        public static Type GetShortcodeType( string name )
        {
            return null;
        }

        public static void RegisterFilter( Type filter )
        {
        }

        public static void RegisterSafeType( Type type, string[] allowedMembers )
        {
        }

        public static void RegisterSafeType( Type type, string[] allowedMembers, Func<object, object> func )
        {
        }

        public static void RegisterSafeType( Type type, Func<object, object> func )
        {
        }

        public static void RegisterValueTypeTransformer( Type type, Func<object, object> func )
        {
        }

        public static Func<object, object> GetValueTypeTransformer( Type type )
        {
            return null;
        }

        public static Func<object, object> GetSafeTypeTransformer( Type type )
        {
            return null;
        }

        public static Template Parse( string source )
        {
            return null;
        }

        public Document Root { get; set; }

        public Hash Registers
        {
            get => null;
        }

        public Hash Assigns
        {
            get => null;
        }

        public Hash InstanceAssigns
        {
            get => null;
        }

        public List<Exception> Errors
        {
            get => null;
        }

        public bool IsThreadSafe
        {
            get => false;
        }

        public void MakeThreadSafe()
        {
        }

        public string Render()
        {
            return null;
        }

        public string Render( Hash localVariables )
        {
            return null;
        }

        public string Render( RenderParameters parameters )
        {
            return null;
        }

        public void Render( TextWriter result, RenderParameters parameters )
        {
        }

        public void Render( TextWriter result, RenderParameters parameters, out List<Exception> errors )
        {
            errors = null;
        }

        public void Render( Stream stream, RenderParameters parameters )
        {
        }
    }

    #endregion
}

namespace DotLiquid.NamingConventions
{
    [Obsolete( "DotLiquid is not supported and will be fully removed in the future." )]
    [Rock.RockObsolete( "18.0" )]
    public interface INamingConvention
    {
        StringComparer StringComparer { get; }
        string GetMemberName( string name );
    }
}

namespace DotLiquid.FileSystems
{
    [Obsolete( "DotLiquid is not supported and will be fully removed in the future." )]
    [Rock.RockObsolete( "18.0" )]
    public interface IFileSystem
    {
        string ReadTemplateFile( Context context, string templateName );
    }
}

namespace DotLiquid.Util
{
    #region Deprecated Functional Classes

    // These are actually used by plugins so we need to keep the
    // functionality working.

    [Obsolete( "DotLiquid is not supported and will be fully removed in the future." )]
    [Rock.RockObsolete( "18.0" )]
    public static class ListExtensionMethods
    {
        public static T Shift<T>( List<T> list )
            where T : class
        {
            if ( list == null || list.Count == 0 )
                return null;

            T result = list[0];
            list.RemoveAt( 0 );

            return result;
        }

        public static T Pop<T>( List<T> list )
            where T : class
        {
            if ( list == null || list.Count == 0 )
                return null;

            T result = list[list.Count - 1];
            list.RemoveAt( list.Count - 1 );

            return result;
        }
    }

    #endregion
}

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
