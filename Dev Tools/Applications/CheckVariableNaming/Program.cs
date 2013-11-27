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
            bool outputDictionary = false;
            bool reportOnly = false;

            Dictionary<string, string> validPrefixes = initValidControlPrefixDict();
            Regex regex = new Regex( "(?<=<(asp|Rock):).*?(?=>)" );
            Regex ucRegex = new Regex( "(\\S+) .*?id=\"(.*?)\"", RegexOptions.IgnoreCase );

            //Something like C:\Projects\Rock-ChMS\Dev Tools\Applications\CheckVariableNaming\
            string currentDirectory = Directory.GetCurrentDirectory();
            string rockDirectory = currentDirectory.Replace( "Dev Tools\\Applications\\CheckVariableNaming\\bin\\Debug", string.Empty );

            int violations = 0;
            violations += AnalyzeVaribleNames( regex, ucRegex, validPrefixes, rockDirectory + "RockWeb\\", reportOnly, outputDictionary );

            Console.WriteLine( "Number of Violations: {0}\n\nPress any key to continue.", violations );
            Console.ReadLine();
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

        /// <summary>
        /// Analyze variable names by type
        /// </summary>
        /// <param name="searchDirectory">The search directory.</param>
        private static int AnalyzeVaribleNames( Regex regex, Regex ucRegex, Dictionary<string, string> validPrefixes, string searchDirectory, bool reportOnly, bool outputDictionary )
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
                        if ( ! reportOnly && validPrefixes.ContainsKey( controlType ) )
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

            if ( reportOnly )
            {
                BuildReport( lookup, outputDictionary );
            }
            Console.WriteLine( "\nNumber of Files Checked: {0}", numberFiles );
            Console.WriteLine( "Number of Controls Checked: {0}", numberControls );
            return violation;
        }

        private static void BuildReport( SortedDictionary<string, List<ControlInstance>> lookup, bool outputDictionary )
        {
            Console.WriteLine( "Report" );

            if ( !outputDictionary )
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
                    //Console.WriteLine( string.Format( "    {0,-25} | {1,-12} | {2, -25} | {3, -10}", controlType, prefix, example, prefixTable[prefix] ) );
                    //Console.WriteLine( string.Format( "   -- {0,-10} {1,12}", prefix, prefixTable[prefix] ) );

                    if ( outputDictionary )
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
