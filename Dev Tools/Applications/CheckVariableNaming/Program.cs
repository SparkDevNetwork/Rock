using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CheckVariableNaming
{
    /// <summary>
    /// Generates a report listing each control variable prefix naming violation.
    /// https://github.com/SparkDevNetwork/Rock-ChMS/wiki/Naming-Conventions#standard-control-variable-naming
    /// </summary>
    class Program
    {
        /// <summary>
        /// Mains the specified args.
        /// </summary>
        /// <param name="args">The args.</param>
        static void Main( string[] args )
        {
            Dictionary<string, string> validPrefixes = initValidControlPrefixDict();
            Regex regex = new Regex( "(?<=<asp:).*?(?=>)" );
            Regex ucRegex = new Regex( "(\\S+) .*?id=\"(.*?)\"", RegexOptions.IgnoreCase );

            //C:\Projects\Rock-ChMS\Dev Tools\Apps\EnsureCopyrightHeader\Program.cs
            string currentDirectory = Directory.GetCurrentDirectory();
            string rockDirectory = currentDirectory.Replace( "Dev Tools\\Applications\\CheckVariableNaming\\bin\\Debug", string.Empty );

            int violations = 0;
            violations += AnalyzeVaribleNames( regex, ucRegex, validPrefixes, rockDirectory + "RockWeb\\" );

            Console.WriteLine( "Number of Violations: {0}\n\nPress any key to continue.", violations );
            Console.ReadLine();
        }

        private static Dictionary<string, string> initValidControlPrefixDict()
        {
            Dictionary<string, string> d = new Dictionary<string, string>()
	        {
                { "Button", "btn" },
                { "CheckBox", "cb" },
                { "CheckBoxList", "cbl" },
                //{ "CompareValidator", "cv" },
                //{ "CustomValidator", "val" },
                { "DataPager", "dp" },
                { "DropDownList", "ddl" },
                { "FileUpload", "fu" },
                { "HiddenField", "hf" },
                { "HyperLink", "hl" },
                { "Image", "img" },
                { "ImageButton", "ib" },
                { "Label", "lbl" },
                { "LinkButton", "lb" },
                { "ListView", "lv" },
                { "Literal", "l" },
                { "ModalPopupExtender", "mpe" },
                { "Panel", "pnl" },
                { "PlaceHolder", "ph" },
                { "PostBackTrigger", "lb" },
                { "RadioButtonList", "rbl" },
                { "Repeater", "rpt" },
                { "Table", "tbl" },
                { "TextBox", "txt" },
                { "UpdatePanel", "up" },
                { "ValidationSummary", "val" }
                //{ "ValidationSummary", "vs" },
                //{ "Xml", "xml" }
            };

            return d;
        }

        /// <summary>
        /// Analyze variable names by type
        /// </summary>
        /// <param name="searchDirectory">The search directory.</param>
        private static int AnalyzeVaribleNames( Regex regex, Regex ucRegex, Dictionary<string, string> validPrefixes, string searchDirectory )
        {
            int numberFiles = 0;
            int numberControls = 0;
            int violation = 0;
            SortedDictionary<string, List<ControlInstance>> lookup = new SortedDictionary<string, List<ControlInstance>>();
            List<string> sourceFilenames = Directory.GetFiles( searchDirectory, "*.ascx", SearchOption.AllDirectories ).ToList();

            foreach ( string fileName in sourceFilenames )
            {
                numberFiles++;
                string[] fileContents = File.ReadAllLines( fileName );
                string origFileContents = File.ReadAllText( fileName );

                //string x = @"<h1><asp:Literal id=""lTitle"" runat=""server"" /><div class=""checkin-sub-title""><asp:Literal ID=""lSubTitle"" runat=""server""></asp:Literal></div></h1>";
                foreach ( Match match in regex.Matches( origFileContents ) )
                {
                    string ucString = match.Value;
                    Match m = ucRegex.Match( ucString );
                    if ( m.Success )
                    {
                        numberControls++;
                        string controlType = m.Groups[1].Value;
                        string variable = m.Groups[2].Value;
                        string prefix = ( SplitCase( variable ).Split( ' ' ) )[0];
                        ControlInstance ci = new ControlInstance() { Type = controlType, FileName = fileName, VariableName = variable, Prefix = prefix };

                        // Check if this controlType's prefix is valid
                        if ( validPrefixes.ContainsKey( controlType ) )
                        {
                            if ( validPrefixes[controlType] != prefix )
                            {
                                violation++;
                                Console.WriteLine( string.Format( "Violation: {0}\t({1})\t{2}", controlType, variable, fileName ) );
                            }
                        }

                        if ( lookup.ContainsKey( controlType ) )
                        {
                            lookup[controlType].Add( ci );
                        }
                        else
                        {
                            lookup[controlType] = new List<ControlInstance> { ci };
                        }
                    }
                }
            }

            //BuildReport( lookup );
            Console.WriteLine( "\nNumber of Files Checked: {0}", numberFiles );
            Console.WriteLine( "Number of Controls Checked: {0}", numberControls );
            return violation;
        }

        private static void BuildReport( SortedDictionary<string, List<ControlInstance>> lookup )
        {
            Console.WriteLine( "Report" );

            foreach ( string controlType in lookup.Keys )
            {
                //Console.WriteLine( string.Format( " - {0}", controlType ) );
                Dictionary<string, int> prefixTable = new Dictionary<string, int>();
                string example = "";
                foreach ( ControlInstance ci in lookup[controlType] )
                {
                    if ( prefixTable.ContainsKey( ci.Prefix ) )
                    {
                        prefixTable[ci.Prefix]++;
                    }
                    else
                    {
                        prefixTable[ci.Prefix] = 1;
                    }

                    // Set the example if we have more than 5 of them.
                    if ( prefixTable[ci.Prefix] > 5 )
                    {
                        example = ci.VariableName;
                    }
                }

                foreach ( string prefix in prefixTable.Keys )
                {
                    //Console.WriteLine( string.Format( "    {0,-20} | {1,-10} | {2, -20} | {3, -10}", controlType, prefix, example, prefixTable[prefix] ) );
                    //Console.WriteLine( string.Format( "   -- {0,-10} {1,15}", prefix, prefixTable[prefix] ) );
                    Console.WriteLine( string.Format( "{{ \"{0}\", \"{1}\" }},", controlType, prefix, example, prefixTable[prefix] ) );
                }
            }
        }

        public static string SplitCase( string str )
        {
            if ( str == null )
                return null;

            return Regex.Replace( Regex.Replace( str, @"(\P{Ll})(\P{Ll}\p{Ll})", "$1 $2" ), @"(\p{Ll})(\P{Ll})", "$1 $2" );
        }

        class ControlInstance
        {
            public ControlInstance()
            {
            }
            public string Type { get; set; }
            public string VariableName { get; set; }
            public string Prefix { get; set; }
            public string FileName { get; set; }
        }
    }
}
