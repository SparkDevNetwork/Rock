using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CommandLine;
using CommandLine.Text;

namespace CheckVariableNaming
{
    /// <summary>
    /// Generates a report listing each control variable prefix naming violation.
    /// https://github.com/SparkDevNetwork/Rock-ChMS/wiki/Naming-Conventions#standard-control-variable-naming
    /// </summary>
    class Program
    {
        // Define a class to receive parsed values
        class Options
        {
            [Option( 'd', "outputDictionary", DefaultValue = false, HelpText = "Set to true to output a dictionary for use with future versions of this program." )]
            public bool outputDictionary { get; set; }

            [Option( 'r', "reportEnabled", DefaultValue = false, HelpText = "Set to true to output a report of each control type, prefix used, and how many times it's used." )]
            public bool reportEnabled { get; set; }

            [Option( 'f', "violationsByFile", DefaultValue = true, HelpText = "Set this to true to report violations by file name instead of by control type." )]
            public bool violationsByFile { get; set; }

            [Option( 'p', "pathToRockWeb", DefaultValue = @"", HelpText = "The path to your local rock project's RockWeb folder." )]
            public string PathToRockWeb { get; set; }

            [ParserState]
            public IParserState LastParserState { get; set; }

            [HelpOption]
            public string GetUsage()
            {
                return HelpText.AutoBuild( this, ( HelpText current ) => HelpText.DefaultParsingErrorsHandler( this, current ) );
            }
        }

        static int Main( string[] args )
        {
            var options = new Options();
            var parser = new CommandLine.Parser( with => with.HelpWriter = Console.Error );

            if ( parser.ParseArgumentsStrict( args, options, () => Environment.Exit( -2 ) ) )
            {
                if ( string.IsNullOrEmpty( options.PathToRockWeb ) )
                {
                    string removeString = "Dev Tools\\Applications";
                    string currentDirectory = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    int index = currentDirectory.IndexOf( removeString );
                    string rockDirectory = ( index < 0 )
                        ? currentDirectory
                        : currentDirectory.Substring( 0, index );

                    options.PathToRockWeb = Path.Combine( rockDirectory, "RockWeb" );

                }

                if ( !Directory.Exists( options.PathToRockWeb ) )
                {
                    Console.WriteLine( "Error: unable to find directory: " + options.PathToRockWeb );
                    return -1;
                }

                Run( options );
            }

            return 0;
        }

        private static void Run( Options options )
        {
            Dictionary<string, string> validPrefixes = initValidControlPrefixDict();
            Regex regex = new Regex( "(?<=<(asp|Rock):).*?(?=>)" );
            Regex ucRegex = new Regex( "(\\S+) .*?id=\"(.*?)\"", RegexOptions.IgnoreCase );
            
            int violations = 0;
            violations += AnalyzeVaribleNames( regex, ucRegex, validPrefixes, options );

            Console.WriteLine( "Number of Violations: {0}\n\nPress any key to continue.", violations );
            Console.ReadLine();
        }

        /// <summary>
        /// Analyze variable names by type and report any prefix violations found.
        /// </summary>
        /// <param name="searchDirectory">The search directory.</param>
        private static int AnalyzeVaribleNames( Regex regex, Regex ucRegex, Dictionary<string, string> validPrefixes, Options options )
        {
            int numberFiles = 0;
            int numberControls = 0;
            int violations = 0;
            SortedDictionary<string, List<ControlInstance>> lookup = new SortedDictionary<string, List<ControlInstance>>();
            List<string> sourceFilenames = Directory.GetFiles( options.PathToRockWeb, "*.ascx", SearchOption.AllDirectories ).ToList();

            int idx = options.PathToRockWeb.IndexOf( @"\RockWeb" );
            
            foreach ( string fileName in sourceFilenames )
            {
                numberFiles++;
                string partialFileName = fileName.Substring( idx + 9 );
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
                        ControlInstance ci = new ControlInstance() { Type = controlType, FileName = partialFileName, VariableName = variable, Prefix = prefix };

                        // Check if this controlType's prefix is valid
                        if ( ! ( options.reportEnabled || options.violationsByFile ) &&  validPrefixes.ContainsKey( controlType ) )
                        {
                            if ( validPrefixes[controlType] != prefix )
                            {
                                violations++;
                                Console.WriteLine( string.Format( "Violation: {0}\t({1})\t{2}", controlType, variable, partialFileName ) );
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

            if ( options.violationsByFile )
            {
                violations = ViolationsByFileName( lookup, validPrefixes );
            }
            else if ( options.reportEnabled )
            {
                BuildReport( lookup, options );
            }

            Console.WriteLine( "\nNumber of Files Checked: {0}", numberFiles );
            Console.WriteLine( "Number of Controls Checked: {0}", numberControls );
            return violations;
        }

        /// <summary>
        /// Builds the report that shows which prefix is being used for each control type for each file.
        /// </summary>
        /// <param name="lookup"></param>
        /// <param name="options"></param>
        private static int ViolationsByFileName( SortedDictionary<string, List<ControlInstance>> lookup, Dictionary<string, string> validPrefixes )
        {
            int violations = 0;
            foreach ( string controlType in lookup.Keys )
            {
                Dictionary<string, List<ControlInstance>> fileTable = new Dictionary<string, List<ControlInstance>>();
                
                foreach ( ControlInstance ci in lookup[controlType] )
                {
                    // If this control instance does not have a valid prefix then add it to the fileTable.
                    if ( validPrefixes.ContainsKey( controlType ) && validPrefixes[controlType] != ci.Prefix )
                    {
                        if ( fileTable.ContainsKey( ci.FileName ) )
                        {
                            fileTable[ci.FileName].Add( ci );
                        }
                        else
                        {
                            fileTable[ci.FileName] = new List<ControlInstance> { ci };
                        }
                    }
                }

                // Now rip through all the files in the fileTable and spew out each one's list of invalid control instance.
                foreach ( string fileName in fileTable.Keys )
                {
                    foreach ( ControlInstance ci in fileTable[fileName] )
                    {
                        violations++;
                        Console.WriteLine( string.Format( "Violation: {0}\t({1})\t{2} != {3}", fileName, ci.Type, ci.VariableName, ProperVariable( validPrefixes, ci ) ) );
                    }
                }
            }
            return violations;
        }

        /// <summary>
        /// Returns a proper variable name for the given control instance
        /// </summary>
        /// <param name="validPrefixes"></param>
        /// <param name="ci"></param>
        /// <returns></returns>
        private static string ProperVariable( Dictionary<string, string> validPrefixes, ControlInstance ci )
        {
            string varRoot = ci.VariableName.Substring( ci.Prefix.Length );
            return validPrefixes[ci.Type] + varRoot;
        }

        /// <summary>
        /// Builds the report that shows which prefix is being used for each control type for each file.
        /// </summary>
        /// <param name="lookup"></param>
        /// <param name="options"></param>
        private static void BuildReport( SortedDictionary<string, List<ControlInstance>> lookup, Options options )
        {
            Console.WriteLine( "Report" );

            if ( ! options.outputDictionary )
            {
                Console.WriteLine( "    {0,-25} | {1,-12} | {2, -25} | {3, -10}", "Control Type", "Prefix", "Example", "# Times In Use");
                Console.WriteLine( "    {0,-25} | {1,-12} | {2, -25} | {3, -10}", new String( '=', 25 ), new String( '=', 12 ), new String( '=', 25 ), new String( '=', 10 ) );
            }

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
                    if ( options.outputDictionary )
                    {
                        Console.WriteLine( string.Format( "{{ \"{0}\", \"{1}\" }},", controlType, prefix, example, prefixTable[prefix] ) );
                    }
                    else
                    {
                        Console.WriteLine( string.Format( "    {0,-25} | {1,-12} | {2,-25} | {3,-10}", controlType, prefix, example, prefixTable[prefix] ) );
                    }
                }
            }
        }

        /// <summary>
        /// Function to split the camel/pascal case variable names into space delimited parts.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string SplitCase( string str )
        {
            if ( str == null )
                return null;

            return Regex.Replace( Regex.Replace( str, @"(\P{Ll})(\P{Ll}\p{Ll})", "$1 $2" ), @"(\p{Ll})(\P{Ll})", "$1 $2" );
        }

        private static Dictionary<string, string> initValidControlPrefixDict()
        {
            Dictionary<string, string> d = new Dictionary<string, string>()
	        {
                { "AccountPicker", "acctp" },
                { "AttributeEditor", "edt" },
                { "Badge", "badge" },
                { "BinaryFilePicker", "bfp" },
                { "BinaryFileTypePicker", "bftp" },
                { "BirthdayPicker", "bdayp" },
                { "BootstrapButton", "bbtn" },
                { "Button", "btn" },
                { "ButtonDropDownList", "bddl" },
                { "CampusesPicker", "mcamp" },
                { "CampusPicker", "camp" },
                { "CategoryPicker", "catp" },
                { "CheckBox", "cb" },
                { "CheckBoxList", "cbl" },
                { "CodeEditor", "ce" },
                { "CompareValidator", "coval" },
                { "ComponentPicker", "comp" },
                { "ConfirmPageUnload", "conpu" },
                { "CustomValidator", "cval" },
                { "DataDropDownList", "ddl" },
                { "DataPager", "dpgr" },
                { "DataTextBox", "dtb" },
                { "DatePicker", "dp" },
                { "DateRangePicker", "drp" },
                { "DateTimePicker", "dtp" },
                { "DropDownList", "ddl" },
                { "EntityTypePicker", "etp" },
                { "FieldTypeList", "ftl" },
                { "FileUpload", "fup" },
                { "FileUploader", "fupr" },
                { "GeoPicker", "geop" },
                { "Grid", "g" },
                { "GridFilter", "gf" },
                { "GroupPicker", "gp" },
                { "GroupRolePicker", "grp" },
                { "GroupTypePicker", "gtp" },
                { "HelpBlock", "hb" },
                { "HiddenField", "hf" },
                { "HighlightLabel", "hlbl" },
                { "HtmlEditor", "html" },
                { "HyperLink", "hl" },
                { "Image", "img" },
                { "ImageButton", "imb" },
                { "ImageUploader", "imgup" },
                { "Label", "lbl" },
                { "LinkButton", "lb" },
                { "ListView", "lv" },
                { "Literal", "l" },
                { "LocationItemPicker", "locip" },
                { "LocationPicker", "locp" },
                { "MergeFieldPicker", "mfp" },
                { "ModalAlert", "ma" },
                { "ModalDialog", "md" },
                { "ModalPopupExtender", "mpe" },
                { "MonthDayPicker", "mdp" },
                { "MonthYearPicker", "myp" },
                { "NewFamilyMembers", "nfm" },
                { "NoteEditor", "note" },
                { "NotificationBox", "nb" },
                { "NumberBox", "numb" },
                { "NumberRangeEditor", "nre" },
                { "PagePicker", "pagep" },
                { "Panel", "pnl" },
                { "PanelWidget", "pnlw" },
                { "PersonPicker", "pp" },
                { "PersonPicker2", "pp2" },
                { "PersonProfileBadgeList", "badgel" },
                { "PlaceHolder", "ph" },
                { "PostBackTrigger", "trgr" },
                { "RadioButtonList", "rbl" },
                { "Repeater", "rpt" },
                { "RockBulletedList", "blst" },
                { "RockCheckBox", "cb" },
                { "RockCheckBoxList", "cbl" },
                { "RockControlWrapper", "wrap" },
                { "RockDropDownList", "ddl" },
                { "RockLiteral", "l" },
                { "RockRadioButtonList", "rbl" },
                { "RockTextBox", "tb" },
                { "ScheduleBuilder", "schedb" },
                { "SecurityButton", "sbtn" },
                { "StateDropDownList", "statep" },
                { "Table", "tbl" },
                { "TagList", "tagl" },
                { "TermDescription", "termd" },
                { "TextBox", "tb" },
                { "TimePicker", "timep" },
                { "Toggle", "tgl" },
                { "UpdatePanel", "upnl" },
                { "ValidationSummary", "val" },
                { "Xml", "xml" }
            };

            return d;
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
