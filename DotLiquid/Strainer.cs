using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotLiquid.Exceptions;

namespace DotLiquid
{
	/// <summary>
	/// Strainer is the parent class for the filters system.
	/// New filters are mixed into the strainer class which is then instanciated for each liquid template render run.
	/// 
	/// One of the strainer's responsibilities is to keep malicious method calls out
	/// </summary>
    /// <remarks>
    /// This version of the Strainer has been modified to allow specific filter functions to be added, in addition to filters contained in declaring types.
    /// This supports boxing and unboxing of filter parameters to replace DotLiquid types with Lava types.
    /// </remarks>
	public class Strainer
	{
        private static readonly Dictionary<string, Type> Filters = new Dictionary<string, Type>();

        private static readonly Dictionary<string, Func<Context, List<object>, object>> _filterFunctions = new Dictionary<string, Func<Context, List<object>, object>>();

        // Register a single filter method.
        public static void RegisterFilter( string filterName, Func<Context, List<object>, object> filterFunction )
        {
            _filterFunctions[filterName] = filterFunction;
        }

		public static void GlobalFilter(Type filter)
		{
			Filters[filter.AssemblyQualifiedName] = filter;
		}

		public static Strainer Create(Context context)
		{
			Strainer strainer = new Strainer(context);
			foreach (var keyValue in Filters)
				strainer.Extend(keyValue.Value);
            foreach ( var keyValue in _filterFunctions )
                strainer.Extend( keyValue.Key, keyValue.Value );
            return strainer;
		}

		private readonly Context _context;
		private readonly Dictionary<string, IList<MethodInfo>> _methods = new Dictionary<string, IList<MethodInfo>>();
        private readonly Dictionary<string, IList<Func<Context, List<object>, object>>> _functions = new Dictionary<string, IList<Func<Context, List<object>, object>>>();

        public IEnumerable<MethodInfo> Methods
		{
			get { return _methods.Values.SelectMany(m => m); }
		}

		public Strainer(Context context)
		{
			_context = context;
		}

        /// <summary>
        /// Add a specific filter function.
        /// </summary>
        /// <param name="type"></param>
        public void Extend( string filterName, Func<Context, List<object>, object> filterFunction )
        {
            var name = Template.NamingConvention.GetMemberName( filterName );

            if ( !_functions.ContainsKey( name ) )
            {
                _functions[name] = new List<Func<Context, List<object>, object>>();
            }

            _functions[name].Add( filterFunction );
        }

        /// <summary>
        /// In this C# implementation, we can't use mixins. So we grab all the static
        /// methods from the specified type and use them instead.
        /// </summary>
        /// <param name="type"></param>
        public void Extend(Type type)
		{
			// From what I can tell, calls to Extend should replace existing filters. So be it.
			var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);
			var methodNames = type.GetMethods(BindingFlags.Public | BindingFlags.Static).Select(m => Template.NamingConvention.GetMemberName(m.Name));

			foreach (var methodName in methodNames)
				_methods.Remove(methodName);

			foreach (MethodInfo methodInfo in methods)
			{
				var name = Template.NamingConvention.GetMemberName(methodInfo.Name);
				if (!_methods.ContainsKey(name))
					_methods[name] = new List<MethodInfo>();

				_methods[name].Add(methodInfo);
			} // foreach
		}

		public bool RespondTo(string method)
		{
			return _methods.ContainsKey(method) || _functions.ContainsKey(method);
		}

		public object Invoke(string method, List<object> args)
		{
            // If a specific function is registered for the filter name, prefer it.
            if ( _functions.ContainsKey( method ) )
            {
                var filterFunction = _functions[method].FirstOrDefault();

                try
                {
                    return filterFunction( _context, args );
                }
                catch ( TargetInvocationException ex )
                {
                    throw ex.InnerException;
                }
            }

            ParameterInfo[] parameterInfos = null;

            // First, try to find a method with the same number of arguments.
            var methodInfo = _methods[method].FirstOrDefault(m => m.GetParameters().Length == args.Count);
            if ( methodInfo != null )
            {
                // If this method's first parameter is a context, ignore this method for now
                parameterInfos = methodInfo.GetParameters();
                if ( parameterInfos.Length > 0 && parameterInfos[0].ParameterType == typeof(Context) )
                {
                    methodInfo = null;
                }
            }

            // Second, try to find a method with one extra parameter where first parameter is a context
            if ( methodInfo == null )
            {
                methodInfo = _methods[method].FirstOrDefault( m => m.GetParameters().Length == args.Count + 1 );
                if ( methodInfo != null )
                {
                    // If this method's first parameter is NOT a context, ignore this method for now
                    parameterInfos = methodInfo.GetParameters();
                    if ( parameterInfos.Length <= 0 || parameterInfos[0].ParameterType != typeof( Context ) )
                    {
                        methodInfo = null;
                    }
                }
            }

            // If we failed to do so, try one with max numbers of arguments, hoping
            // that those not explicitly specified will be taken care of
            // by default values
            if ( methodInfo == null )
            {
                methodInfo = GetBestMatchedMethod( method, args.Count );
                parameterInfos = methodInfo.GetParameters();
            }

            // If first parameter is Context, send in actual context.
            if ( parameterInfos.Length > 0 && parameterInfos[0].ParameterType == typeof( Context ) )
            {
                args.Insert( 0, _context );
            }

            // Add in any default parameters - .NET won't do this for us.
            if ( parameterInfos.Length > args.Count )
            {
                for ( int i = args.Count; i < parameterInfos.Length; ++i )
                {
                    if ( ( parameterInfos[i].Attributes & ParameterAttributes.HasDefault ) != ParameterAttributes.HasDefault )
                        throw new SyntaxException( Liquid.ResourceManager.GetString( "StrainerFilterHasNoValueException" ), method, parameterInfos[i].Name );
                    args.Add( parameterInfos[i].DefaultValue );
                }
            }

			try
			{
				return methodInfo.Invoke(null, args.ToArray());
			}
			catch (TargetInvocationException ex)
			{
				throw ex.InnerException;
			}
		}

        /// <summary>
        /// Get the filter method that best matches the supplied argument list.
        /// </summary>
        /// <param name="method"></param>
        /// <param name="suppliedArgsCount"></param>
        /// <returns></returns>
        private MethodInfo GetBestMatchedMethod( string method, int suppliedArgsCount )
        {
            var candidateMethodInfoList = _methods[method].OrderByDescending( m => m.GetParameters().Length );

            foreach ( var methodInfo in candidateMethodInfoList )
            {
                bool isMatch = true;

                var parameterInfos = methodInfo.GetParameters();

                if ( parameterInfos.Length > suppliedArgsCount )
                {
                    for ( int i = suppliedArgsCount; i < parameterInfos.Length; ++i )
                    {
                        if ( ( parameterInfos[i].Attributes & ParameterAttributes.HasDefault ) != ParameterAttributes.HasDefault )
                        {
                            // No match
                            isMatch = false;
                            continue;
                        }
                    }

                    if ( isMatch )
                    {
                        return methodInfo;
                    }
                }

            }

            return candidateMethodInfoList.FirstOrDefault();
        }
    }
}