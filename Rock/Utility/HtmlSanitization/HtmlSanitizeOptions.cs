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

using Rock.Attribute;

namespace Rock.Utility.HtmlSanitization
{
    /// <summary>
    /// The whole configuration for one call to <see cref="IHtmlAllowListSanitizer.Sanitize(string, HtmlSanitizeOptions)"/>.
    /// Every rule is a set of allowed values: anything not allowed is removed.
    /// Start from a read-only preset (<see cref="Simple"/> or <see cref="Rich"/>),
    /// edit a copy made with <see cref="HtmlSanitizeOptions(HtmlSanitizeOptions)"/>,
    /// then call <see cref="MakeReadOnly"/> and keep the result in a
    /// <c>static readonly</c> field.
    /// </summary>
    /// <remarks>
    /// The fixed rules (elements that never pass, <c>on*</c> attributes,
    /// blocked URL schemes, never-allowed CSS properties) live in the sanitizer.
    /// The edit methods throw <see cref="ArgumentException"/> when asked to
    /// loosen one of them.
    /// </remarks>
    [RockInternal( "17.12" )]
    public sealed class HtmlSanitizeOptions
    {
        #region Fields

        private const string DataAttributePrefix = "data-";

        private readonly HashSet<string> _allowedTags;

        private readonly HashSet<string> _everyTagAttributes;

        private readonly Dictionary<string, HashSet<string>> _tagAttributes;

        private bool _areDataAttributesAllowed;

        private readonly HashSet<string> _urlSchemes;

        private readonly HashSet<string> _cssProperties;

        private readonly Dictionary<string, HtmlTagRemovalMode> _removalModes;

        private HtmlTagRemovalMode _defaultRemovalMode;

        private volatile bool _isReadOnly;

        #endregion

        #region Properties

        /// <summary>
        /// The read-only preset for small snippets: inline formatting, links,
        /// paragraphs, line breaks and <c>div</c>. No tables, images, <c>id</c>,
        /// <c>data-*</c> or positional CSS.
        /// </summary>
        public static HtmlSanitizeOptions Simple { get; } = CreateSimple().MakeReadOnly();

        /// <summary>
        /// The read-only preset for larger staff-authored templates: everything
        /// in <see cref="Simple"/> plus headings, lists, HTML5 section elements,
        /// tables, images, form controls, SVG icons, <c>id</c>, <c>data-*</c> and
        /// positional, table and flex CSS.
        /// </summary>
        public static HtmlSanitizeOptions Rich { get; } = CreateRich().MakeReadOnly();

        /// <summary>
        /// Gets a value indicating whether these options can no longer be edited.
        /// </summary>
        /// <value><c>true</c> if every edit method throws <see cref="InvalidOperationException"/>.</value>
        public bool IsReadOnly => _isReadOnly;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates editable options that allow no elements, so only text is kept.
        /// </summary>
        public HtmlSanitizeOptions()
        {
            _allowedTags = new HashSet<string>( StringComparer.OrdinalIgnoreCase );
            _everyTagAttributes = new HashSet<string>( StringComparer.OrdinalIgnoreCase );
            _tagAttributes = new Dictionary<string, HashSet<string>>( StringComparer.OrdinalIgnoreCase );
            _urlSchemes = new HashSet<string>( StringComparer.OrdinalIgnoreCase );
            _cssProperties = new HashSet<string>( StringComparer.OrdinalIgnoreCase );
            _removalModes = new Dictionary<string, HtmlTagRemovalMode>( StringComparer.OrdinalIgnoreCase );
            _defaultRemovalMode = HtmlTagRemovalMode.Drop;
        }

        /// <summary>
        /// Creates an editable copy of <paramref name="source"/>, even when the
        /// source is read-only. The copy shares no state with its source.
        /// </summary>
        /// <param name="source">The options to copy.</param>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
        public HtmlSanitizeOptions( HtmlSanitizeOptions source )
        {
            if ( source == null )
            {
                throw new ArgumentNullException( nameof( source ) );
            }

            _allowedTags = new HashSet<string>( source._allowedTags, StringComparer.OrdinalIgnoreCase );
            _everyTagAttributes = new HashSet<string>( source._everyTagAttributes, StringComparer.OrdinalIgnoreCase );
            _tagAttributes = new Dictionary<string, HashSet<string>>( StringComparer.OrdinalIgnoreCase );
            foreach ( var tagAttributes in source._tagAttributes )
            {
                _tagAttributes.Add( tagAttributes.Key, new HashSet<string>( tagAttributes.Value, StringComparer.OrdinalIgnoreCase ) );
            }

            _areDataAttributesAllowed = source._areDataAttributesAllowed;
            _urlSchemes = new HashSet<string>( source._urlSchemes, StringComparer.OrdinalIgnoreCase );
            _cssProperties = new HashSet<string>( source._cssProperties, StringComparer.OrdinalIgnoreCase );
            _removalModes = new Dictionary<string, HtmlTagRemovalMode>( source._removalModes, StringComparer.OrdinalIgnoreCase );
            _defaultRemovalMode = source._defaultRemovalMode;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prevents any further edits. Calling it again is harmless.
        /// </summary>
        /// <returns>This instance.</returns>
        public HtmlSanitizeOptions MakeReadOnly()
        {
            _isReadOnly = true;

            return this;
        }

        /// <summary>
        /// Allows an element.
        /// </summary>
        /// <param name="tag">The tag name.</param>
        /// <returns>This instance.</returns>
        /// <exception cref="InvalidOperationException">The options are read-only.</exception>
        /// <exception cref="ArgumentException"><paramref name="tag"/> is blank or names an element that must never pass.</exception>
        public HtmlSanitizeOptions AllowTag( string tag )
        {
            ThrowIfReadOnly();
            ThrowIfBlank( tag, nameof( tag ) );
            ThrowIfNeverPass( tag, nameof( tag ) );

            _allowedTags.Add( tag );

            return this;
        }

        /// <summary>
        /// Stops allowing an element.
        /// </summary>
        /// <param name="tag">The tag name.</param>
        /// <returns>This instance.</returns>
        /// <exception cref="InvalidOperationException">The options are read-only.</exception>
        /// <exception cref="ArgumentException"><paramref name="tag"/> is blank.</exception>
        public HtmlSanitizeOptions DisallowTag( string tag )
        {
            ThrowIfReadOnly();
            ThrowIfBlank( tag, nameof( tag ) );

            _allowedTags.Remove( tag );

            return this;
        }

        /// <summary>
        /// Allows an attribute on one tag, or on every tag when <paramref name="tag"/> is blank.
        /// </summary>
        /// <param name="tag">The tag name, or a blank value for every tag.</param>
        /// <param name="attribute">The attribute name.</param>
        /// <returns>This instance.</returns>
        /// <exception cref="InvalidOperationException">The options are read-only.</exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="attribute"/> is blank, is an <c>on*</c> event handler,
        /// is <c>formaction</c> or <c>srcdoc</c>, or is a URL-list attribute (<c>archive</c>, <c>ping</c>, <c>srcset</c>).
        /// </exception>
        public HtmlSanitizeOptions AllowAttribute( string tag, string attribute )
        {
            ThrowIfReadOnly();
            ThrowIfBlank( attribute, nameof( attribute ) );

            if ( attribute.StartsWith( "on", StringComparison.OrdinalIgnoreCase ) )
            {
                throw new ArgumentException( $"Event handler attributes such as '{attribute}' can never be allowed.", nameof( attribute ) );
            }

            if ( HtmlAllowListTables.NeverAllowedAttributes.Contains( attribute ) )
            {
                throw new ArgumentException( $"The '{attribute}' attribute can never be allowed.", nameof( attribute ) );
            }

            if ( HtmlAllowListTables.UrlListAttributes.Contains( attribute ) )
            {
                throw new ArgumentException( $"The URL-list attribute '{attribute}' can not be allowed because its entries are not checked.", nameof( attribute ) );
            }

            if ( string.IsNullOrWhiteSpace( tag ) )
            {
                _everyTagAttributes.Add( attribute );
            }
            else
            {
                if ( !_tagAttributes.TryGetValue( tag, out var attributes ) )
                {
                    attributes = new HashSet<string>( StringComparer.OrdinalIgnoreCase );
                    _tagAttributes.Add( tag, attributes );
                }

                attributes.Add( attribute );
            }

            return this;
        }

        /// <summary>
        /// Stops allowing an attribute on one tag, or on every tag when
        /// <paramref name="tag"/> is blank. A blank tag also removes the
        /// attribute from every tag's own set.
        /// </summary>
        /// <param name="tag">The tag name, or a blank value for every tag.</param>
        /// <param name="attribute">The attribute name.</param>
        /// <returns>This instance.</returns>
        /// <exception cref="InvalidOperationException">The options are read-only.</exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="attribute"/> is blank, or <paramref name="tag"/> is not
        /// blank and the attribute is allowed on every tag, because one tag can
        /// not opt out of an every-tag attribute.
        /// </exception>
        public HtmlSanitizeOptions DisallowAttribute( string tag, string attribute )
        {
            ThrowIfReadOnly();
            ThrowIfBlank( attribute, nameof( attribute ) );

            if ( string.IsNullOrWhiteSpace( tag ) )
            {
                _everyTagAttributes.Remove( attribute );

                foreach ( var attributes in _tagAttributes.Values )
                {
                    attributes.Remove( attribute );
                }

                return this;
            }

            if ( _everyTagAttributes.Contains( attribute ) )
            {
                throw new ArgumentException( $"The attribute '{attribute}' is allowed on every tag, so it can not be disallowed for '{tag}' alone. Disallow it with a blank tag instead.", nameof( attribute ) );
            }

            if ( _tagAttributes.TryGetValue( tag, out var tagAttributes ) )
            {
                tagAttributes.Remove( attribute );
            }

            return this;
        }

        /// <summary>
        /// Allows <c>data-*</c> attributes on every allowed tag.
        /// </summary>
        /// <returns>This instance.</returns>
        /// <exception cref="InvalidOperationException">The options are read-only.</exception>
        public HtmlSanitizeOptions AllowDataAttributes()
        {
            ThrowIfReadOnly();

            _areDataAttributesAllowed = true;

            return this;
        }

        /// <summary>
        /// Stops allowing <c>data-*</c> attributes, except those allowed by name.
        /// </summary>
        /// <returns>This instance.</returns>
        /// <exception cref="InvalidOperationException">The options are read-only.</exception>
        public HtmlSanitizeOptions DisallowDataAttributes()
        {
            ThrowIfReadOnly();

            _areDataAttributesAllowed = false;

            return this;
        }

        /// <summary>
        /// Allows a URL scheme in URL attributes and CSS <c>url()</c> values.
        /// Relative URLs are always allowed.
        /// </summary>
        /// <param name="scheme">The scheme, without the colon.</param>
        /// <returns>This instance.</returns>
        /// <exception cref="InvalidOperationException">The options are read-only.</exception>
        /// <exception cref="ArgumentException"><paramref name="scheme"/> is blank or is a blocked scheme.</exception>
        public HtmlSanitizeOptions AllowUrlScheme( string scheme )
        {
            ThrowIfReadOnly();
            ThrowIfBlank( scheme, nameof( scheme ) );

            if ( HtmlAllowListTables.BlockedUrlSchemes.Contains( scheme ) )
            {
                throw new ArgumentException( $"The URL scheme '{scheme}' can never be allowed.", nameof( scheme ) );
            }

            _urlSchemes.Add( scheme );

            return this;
        }

        /// <summary>
        /// Stops allowing a URL scheme.
        /// </summary>
        /// <param name="scheme">The scheme, without the colon.</param>
        /// <returns>This instance.</returns>
        /// <exception cref="InvalidOperationException">The options are read-only.</exception>
        /// <exception cref="ArgumentException"><paramref name="scheme"/> is blank.</exception>
        public HtmlSanitizeOptions DisallowUrlScheme( string scheme )
        {
            ThrowIfReadOnly();
            ThrowIfBlank( scheme, nameof( scheme ) );

            _urlSchemes.Remove( scheme );

            return this;
        }

        /// <summary>
        /// Allows a CSS property in <c>style</c> attributes.
        /// </summary>
        /// <param name="property">The property name.</param>
        /// <returns>This instance.</returns>
        /// <exception cref="InvalidOperationException">The options are read-only.</exception>
        /// <exception cref="ArgumentException"><paramref name="property"/> is blank or is a never-allowed property.</exception>
        public HtmlSanitizeOptions AllowCssProperty( string property )
        {
            ThrowIfReadOnly();
            ThrowIfBlank( property, nameof( property ) );

            if ( HtmlAllowListTables.NeverAllowedCssProperties.Contains( property ) )
            {
                throw new ArgumentException( $"The CSS property '{property}' can never be allowed.", nameof( property ) );
            }

            _cssProperties.Add( property );

            return this;
        }

        /// <summary>
        /// Stops allowing a CSS property.
        /// </summary>
        /// <param name="property">The property name.</param>
        /// <returns>This instance.</returns>
        /// <exception cref="InvalidOperationException">The options are read-only.</exception>
        /// <exception cref="ArgumentException"><paramref name="property"/> is blank.</exception>
        public HtmlSanitizeOptions DisallowCssProperty( string property )
        {
            ThrowIfReadOnly();
            ThrowIfBlank( property, nameof( property ) );

            _cssProperties.Remove( property );

            return this;
        }

        /// <summary>
        /// Sets what happens to one element when it is not allowed.
        /// </summary>
        /// <param name="tag">The tag name.</param>
        /// <param name="mode">The removal mode.</param>
        /// <returns>This instance.</returns>
        /// <exception cref="InvalidOperationException">The options are read-only.</exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="tag"/> is blank, names an element that must never
        /// pass, names <c>svg</c> or <c>math</c> (always dropped when not
        /// allowed), or <paramref name="mode"/> is not a defined value.
        /// </exception>
        public HtmlSanitizeOptions SetRemovalMode( string tag, HtmlTagRemovalMode mode )
        {
            ThrowIfReadOnly();
            ThrowIfBlank( tag, nameof( tag ) );
            ThrowIfNeverPass( tag, nameof( tag ) );
            ThrowIfUndefined( mode );

            if ( HtmlAllowListTables.DropOnlyElements.Contains( tag ) )
            {
                throw new ArgumentException( $"The element '{tag}' is always removed with its content when it is not allowed, so its removal mode can not be set.", nameof( tag ) );
            }

            _removalModes[tag] = mode;

            return this;
        }

        /// <summary>
        /// Sets what happens to elements that are not allowed and have no mode
        /// of their own. The default is <see cref="HtmlTagRemovalMode.Drop"/>.
        /// </summary>
        /// <param name="mode">The removal mode.</param>
        /// <returns>This instance.</returns>
        /// <exception cref="InvalidOperationException">The options are read-only.</exception>
        /// <exception cref="ArgumentException"><paramref name="mode"/> is not a defined value.</exception>
        public HtmlSanitizeOptions SetDefaultRemovalMode( HtmlTagRemovalMode mode )
        {
            ThrowIfReadOnly();
            ThrowIfUndefined( mode );

            _defaultRemovalMode = mode;

            return this;
        }

        /// <summary>
        /// Determines whether an element is allowed.
        /// </summary>
        /// <param name="tag">The tag name.</param>
        /// <returns><c>true</c> if the element is allowed.</returns>
        internal bool IsTagAllowed( string tag )
        {
            return _allowedTags.Contains( tag );
        }

        /// <summary>
        /// Determines whether an attribute is allowed on a tag: it is allowed
        /// on every tag, on that tag, or it is a <c>data-*</c> attribute and
        /// data attributes are allowed.
        /// </summary>
        /// <param name="tag">The tag name.</param>
        /// <param name="attribute">The attribute name.</param>
        /// <returns><c>true</c> if the attribute is allowed on the tag.</returns>
        internal bool IsAttributeAllowed( string tag, string attribute )
        {
            if ( _everyTagAttributes.Contains( attribute ) )
            {
                return true;
            }

            if ( _tagAttributes.TryGetValue( tag, out var tagAttributes ) && tagAttributes.Contains( attribute ) )
            {
                return true;
            }

            return _areDataAttributesAllowed && attribute.StartsWith( DataAttributePrefix, StringComparison.OrdinalIgnoreCase );
        }

        /// <summary>
        /// Determines whether a URL scheme is allowed.
        /// </summary>
        /// <param name="scheme">The scheme, without the colon.</param>
        /// <returns><c>true</c> if the scheme is allowed.</returns>
        internal bool IsUrlSchemeAllowed( string scheme )
        {
            return _urlSchemes.Contains( scheme );
        }

        /// <summary>
        /// Determines whether a CSS property is allowed.
        /// </summary>
        /// <param name="property">The property name.</param>
        /// <returns><c>true</c> if the property is allowed.</returns>
        internal bool IsCssPropertyAllowed( string property )
        {
            return _cssProperties.Contains( property );
        }

        /// <summary>
        /// Gets what happens to an element that is not allowed.
        /// </summary>
        /// <param name="tag">The tag name.</param>
        /// <returns>The removal mode for the tag.</returns>
        internal HtmlTagRemovalMode GetRemovalMode( string tag )
        {
            return _removalModes.TryGetValue( tag, out var mode ) ? mode : _defaultRemovalMode;
        }

        /// <summary>
        /// Builds the <see cref="Simple"/> preset.
        /// </summary>
        /// <returns>The editable options.</returns>
        private static HtmlSanitizeOptions CreateSimple()
        {
            return new HtmlSanitizeOptions().AddGroups(
                HtmlAllowListTables.SimpleElements,
                HtmlAllowListTables.SimpleEveryTagAttributes,
                HtmlAllowListTables.SimpleTagAttributes,
                HtmlAllowListTables.UrlSchemes,
                HtmlAllowListTables.SimpleCssProperties );
        }

        /// <summary>
        /// Builds the <see cref="Rich"/> preset as a copy of <see cref="Simple"/>
        /// plus the Rich-only groups.
        /// </summary>
        /// <returns>The editable options.</returns>
        private static HtmlSanitizeOptions CreateRich()
        {
            return new HtmlSanitizeOptions( Simple )
                .AddGroups(
                    HtmlAllowListTables.RichOnlyElements,
                    HtmlAllowListTables.RichOnlyEveryTagAttributes,
                    HtmlAllowListTables.RichOnlyTagAttributes,
                    Array.Empty<string>(),
                    HtmlAllowListTables.RichOnlyCssProperties )
                .AllowDataAttributes();
        }

        /// <summary>
        /// Adds groups of preset data through the edit methods, so the presets
        /// pass the same fixed-rule checks as any caller's options.
        /// </summary>
        /// <param name="tags">The elements to allow.</param>
        /// <param name="everyTagAttributes">The attributes to allow on every tag.</param>
        /// <param name="tagAttributes">The attributes to allow per tag.</param>
        /// <param name="urlSchemes">The URL schemes to allow.</param>
        /// <param name="cssProperties">The CSS properties to allow.</param>
        /// <returns>This instance.</returns>
        private HtmlSanitizeOptions AddGroups(
            IEnumerable<string> tags,
            IEnumerable<string> everyTagAttributes,
            IReadOnlyDictionary<string, HashSet<string>> tagAttributes,
            IEnumerable<string> urlSchemes,
            IEnumerable<string> cssProperties )
        {
            foreach ( var tag in tags )
            {
                AllowTag( tag );
            }

            foreach ( var attribute in everyTagAttributes )
            {
                AllowAttribute( string.Empty, attribute );
            }

            foreach ( var tagAttribute in tagAttributes )
            {
                foreach ( var attribute in tagAttribute.Value )
                {
                    AllowAttribute( tagAttribute.Key, attribute );
                }
            }

            foreach ( var scheme in urlSchemes )
            {
                AllowUrlScheme( scheme );
            }

            foreach ( var property in cssProperties )
            {
                AllowCssProperty( property );
            }

            return this;
        }

        /// <summary>
        /// Throws when the options are read-only.
        /// </summary>
        /// <exception cref="InvalidOperationException">The options are read-only.</exception>
        private void ThrowIfReadOnly()
        {
            if ( _isReadOnly )
            {
                throw new InvalidOperationException( "These HTML sanitize options are read-only. Edit a copy made with the copy constructor instead." );
            }
        }

        /// <summary>
        /// Throws when a name argument is null or blank.
        /// </summary>
        /// <param name="value">The argument value.</param>
        /// <param name="parameterName">The parameter name.</param>
        /// <exception cref="ArgumentException"><paramref name="value"/> is null or blank.</exception>
        private static void ThrowIfBlank( string value, string parameterName )
        {
            if ( string.IsNullOrWhiteSpace( value ) )
            {
                throw new ArgumentException( "The value can not be null or blank.", parameterName );
            }
        }

        /// <summary>
        /// Throws when a tag names an element that must never pass.
        /// </summary>
        /// <param name="tag">The tag name.</param>
        /// <param name="parameterName">The parameter name.</param>
        /// <exception cref="ArgumentException"><paramref name="tag"/> names an element that must never pass.</exception>
        private static void ThrowIfNeverPass( string tag, string parameterName )
        {
            if ( HtmlAllowListTables.NeverPassElements.Contains( tag ) )
            {
                throw new ArgumentException( $"The element '{tag}' is always removed with its content and can not be configured.", parameterName );
            }
        }

        /// <summary>
        /// Throws when a removal mode is not a defined value.
        /// </summary>
        /// <param name="mode">The removal mode.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="mode"/> is not a defined value.</exception>
        private static void ThrowIfUndefined( HtmlTagRemovalMode mode )
        {
            if ( mode != HtmlTagRemovalMode.Drop && mode != HtmlTagRemovalMode.Unwrap )
            {
                throw new ArgumentOutOfRangeException( nameof( mode ) );
            }
        }

        #endregion
    }
}
