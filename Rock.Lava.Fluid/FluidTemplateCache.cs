using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;

using Fluid;

namespace Rock.Lava.Fluid
{
    /// <summary>
    /// This is a copy implementation of Fluid's DefaultTemplateCache
    /// <see cref="ITemplateCache"/> which is used to resolve ~~ in subpaths.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <strong>This is an internal API</strong> that supports the Rock
    ///         infrastructure and not subject to the same compatibility standards
    ///         as public APIs. It may be changed or removed without notice in any
    ///         release and should therefore not be directly used in any plug-ins.
    ///     </para>
    /// </remarks>
#pragma warning disable FLUID001
    public class FluidTemplateCache : ITemplateCache
#pragma warning restore FLUID001
    {
        //private sealed class TemplateCacheEntry( string Subpath, DateTimeOffset LastModified, IFluidTemplate Template );

        /// <summary>
        /// Immutable cache entry for parsed templates.
        /// WHY: C# 7 has no primary constructors; use ctor + get-only props.
        /// </summary>
        public sealed class TemplateCacheEntry
        {
            public string Subpath { get; }
            public DateTimeOffset LastModified { get; }
            public IFluidTemplate Template { get; }

            public TemplateCacheEntry( string subpath, DateTimeOffset lastModified, IFluidTemplate template )
            {
                if ( subpath == null )
                    throw new ArgumentNullException( nameof( subpath ) );
                if ( template == null )
                    throw new ArgumentNullException( nameof( template ) );

                Subpath = subpath;
                LastModified = lastModified;
                Template = template;
            }
        }
        private readonly ConcurrentDictionary<string, TemplateCacheEntry> _cache;
        private ILavaFileSystem LavaFileSystem;

        public FluidTemplateCache()
        {
            // Use case-insensitive comparison only on Windows. Create a dedicated cache entry in other cases, even
            // on MacOS when the file system could be case-sensitive too.

            // C# 7.3: no target-typed `new`.
            _cache = new ConcurrentDictionary<string, TemplateCacheEntry>( RuntimeInformation.IsOSPlatform( OSPlatform.Windows ) ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal );
        }

        public FluidTemplateCache( ILavaFileSystem fileSystem ) : this()
        {
            LavaFileSystem = fileSystem;
        }

        public bool TryGetTemplate( string subpath, DateTimeOffset lastModified, out IFluidTemplate template )
        {
            template = default;
            if ( subpath.StartsWith( "~~" ) && LavaFileSystem != null )
            {
                subpath = LavaFileSystem.ResolveTemplatePath( subpath );
            }

            if ( _cache.TryGetValue( subpath, out var templateCacheEntry ) )
            {
                if ( templateCacheEntry.LastModified < lastModified )
                {
                    // The template has been modified, so we can remove it from the cache
                    _cache.TryRemove( subpath, out _ );

                    return false;
                }
                else
                {
                    template = templateCacheEntry.Template;
                    return true;
                }
            }

            return false;
        }

        public void SetTemplate( string subpath, DateTimeOffset lastModified, IFluidTemplate template )
        {
            if ( subpath.StartsWith( "~~" ) && LavaFileSystem != null )
            {
                subpath = LavaFileSystem.ResolveTemplatePath( subpath );
            }
            _cache[subpath] = new TemplateCacheEntry( subpath, lastModified, template );
        }
    }
}
