using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DotLiquid.Exceptions;
using DotLiquid.Util;

namespace DotLiquid
{
	/// <summary>
	/// Holds variables. Variables are only loaded "just in time"
	/// and are not evaluated as part of the render stage
	///
	/// {{ monkey }}
	/// {{ user.name }}
	///
	/// Variables can be combined with filters:
	///
	/// {{ user | link }}
	/// </summary>
	public class Variable : IRenderable
	{
		public static readonly string FilterParser = string.Format(R.Q(@"(?:{0}|(?:\s*(?!(?:{0}))(?:{1}|\S+)\s*)+)"), Liquid.FilterSeparator, Liquid.QuotedFragment);

		public List<Filter> Filters { get; set; }
		public string Name { get; set; }

		private readonly string _markup;

		public Variable(string markup)
		{
			_markup = markup;

			Name = null;
			Filters = new List<Filter>();

			Match match = Regex.Match(markup, string.Format(R.Q(@"\s*({0})(.*)"), Liquid.QuotedAssignFragment));
			if (match.Success)
			{
				Name = match.Groups[1].Value;
				Match filterMatch = Regex.Match(match.Groups[2].Value, string.Format(R.Q(@"{0}\s*(.*)"), Liquid.FilterSeparator));
				if (filterMatch.Success)
				{
					foreach (string f in R.Scan(filterMatch.Value, FilterParser))
					{
						Match filterNameMatch = Regex.Match(f, R.Q(@"\s*(\w+)"));
						if (filterNameMatch.Success)
						{
							string filterName = filterNameMatch.Groups[1].Value;
							List<string> filterArgs = R.Scan(f, string.Format(R.Q(@"(?:{0}|{1})\s*({2})"), Liquid.FilterArgumentSeparator, Liquid.ArgumentSeparator, Liquid.QuotedFragment));
							Filters.Add(new Filter(filterName, filterArgs.ToArray()));
						}
					}
				}
			}
		}

		public void Render(Context context, TextWriter result)
		{
            // Do not apply transforms here as it will be done below.
			object output = RenderInternal(context, false);

			if (output is ILiquidizable)
				output = null;

			if (output != null)
			{
                // Apply any required transforms to the context value prior to passing it to the filter pipeline.
                output = ApplyValueTypeTransforms( context, output );

				string outputString;
				if (output is IEnumerable)
#if NET35
					outputString = string.Join(string.Empty, ((IEnumerable)output).Cast<object>().Select(o => o.ToString()).ToArray());
#else
					outputString = string.Join(string.Empty, ((IEnumerable)output).Cast<object>());
#endif
                else if (output is bool)
                    outputString = output.ToString().ToLower();
                else if (output is DateTimeOffset dto)
                    // Change the default output for a DateTimeOffset to omit the timezone offset.
                    outputString = dto.ToString( "G" );
                else
                    outputString = output.ToString();
				result.Write(outputString);
			}
		}

		private object RenderInternal(Context context, bool applyTransforms = true)
		{
			if (Name == null)
				return null;

			object output = context[Name];

            if ( applyTransforms )
            {
                // Apply any required transforms to the context value prior to passing it to the filter pipeline.
                output = ApplyValueTypeTransforms( context, output );
            }

            Filters.ToList().ForEach(filter =>
			{
				List<object> filterArgs = filter.Arguments.Select(a => context[a]).ToList();
				try
				{
					filterArgs.Insert(0, output);
					output = context.Invoke(filter.Name, filterArgs);
				}
				catch (FilterNotFoundException ex)
				{
					throw new FilterNotFoundException(string.Format(Liquid.ResourceManager.GetString("VariableFilterNotFoundException"), filter.Name, _markup.Trim()), ex);
				}
			});

            if (output is IValueTypeConvertible)
                output = ((IValueTypeConvertible) output).ConvertToValueType();

			return output;
		}

        /// <summary>
        /// Apply custom transformations to the specified value.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private object ApplyValueTypeTransforms( Context context, object value )
        {
            if ( value == null )
            {
                return null;
            }

            // Check if this context has a ValueTypeTranformer defined for the value.
            var transformer = context.GetValueTypeTransformer( value.GetType() );

            if ( transformer == null )
            {
                // If the context doesn't define a ValueTypeTranformer for the type, check for a global transformer.
                transformer = Template.GetValueTypeTransformer( value.GetType() );
            }

            if ( transformer != null )
            {
                return transformer( value );
            }

            return value;
        }

        /// <summary>
        /// Primarily intended for testing.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        internal object Render(Context context)
		{
			return RenderInternal(context);
		}

		public class Filter
		{
			public Filter(string name, string[] arguments)
			{
				Name = name;
				Arguments = arguments;
			}

			public string Name { get; set; }
			public string[] Arguments { get; set; }
		}
	}
}