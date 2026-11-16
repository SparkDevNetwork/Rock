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

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Encodes the data item and dataset values that the Chart Lava shortcode writes into its script block.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 346, "17.8" )]
    public class EscapeLavaChartShortcodeData : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            /*
                11/13/26 - JMH

                The Chart shortcode markup is stored in the database, so each fragment that writes
                a label, tooltip, color, click URL or value into the script block is replaced in place.
                A fragment an administrator has edited no longer matches and is left alone,
                and a fragment that was already replaced is gone, so running this again changes nothing.
                The escape sequences are built with NCHAR(92) and held in captured Lava variables
                because a quoted Lava string can turn a backslash sequence back into the plain character.
                Click URLs are limited to http, https and paths that start with a single slash.
                Values stay unquoted numbers, so they keep only the characters a number can contain.

                Reason: Prevent stored XSS through data bound into the Chart shortcode.
            */
            Sql( EscapeChartMarkupSql );
        }

        /// <summary>
        /// The SQL that encodes the stored Chart shortcode markup, safe to run more than once.
        /// </summary>
        internal const string EscapeChartMarkupSql = @"
DECLARE @ShortcodeGuid uniqueidentifier = '43819A34-4819-4507-8FEA-2E406B5474EA';
DECLARE @Backslash nchar(1) = NCHAR(92);

DECLARE @Enc nvarchar(max) = N' | ToJSON | Replace:''<'',jsLt | Replace:''>'',jsGt | Replace:''&'',jsAmp';

DECLARE @Num nvarchar(max) = N' | Default:'''' | RegExReplace:''[^0-9.eE+-]'','''' | Default:''null''';

DECLARE @Helpers nvarchar(max) = N'{% capture jsLt %}' + @Backslash + N'u003c{% endcapture %}{% capture jsGt %}' + @Backslash + N'u003e{% endcapture %}{% capture jsAmp %}' + @Backslash + N'u0026{% endcapture %}{% capture clickBadStart %}/' + @Backslash + N'{% endcapture %}
';

DECLARE @ListTemplate nvarchar(max) = N'{% assign $list$ = '''' -%}
      {% for chartItem in dataitems -%}
          {% assign chartItemJson = chartItem.$prop$ | Default:''''$enc$ -%}
          {% if forloop.first -%}{% assign $list$ = chartItemJson -%}{% else -%}{% assign $list$ = $list$ | Append:'','' | Append:chartItemJson -%}{% endif -%}
      {% endfor -%}';
SET @ListTemplate = REPLACE( @ListTemplate, N'$enc$', @Enc );

DECLARE @BracketTemplate nvarchar(max) = N'
      {% assign $list$ = $list$ | Prepend:''['' | Append:'']'' -%}';

DECLARE @ClickUrls nvarchar(max) = N'{% assign itemclickurls = '''' -%}
      {% for chartItem in dataitems -%}
          {% assign clickUrl = chartItem.itemclickurl | Default:'''' | Trim -%}
          {% assign clickLower = clickUrl | Downcase -%}
          {% assign clickStart1 = clickLower | Slice:0,1 -%}
          {% assign clickStart2 = clickLower | Slice:0,2 -%}
          {% assign clickStart7 = clickLower | Slice:0,7 -%}
          {% assign clickStart8 = clickLower | Slice:0,8 -%}
          {% assign chartItemJson = ''""""'' -%}
          {% if clickStart7 == ''http://'' or clickStart8 == ''https://'' -%}
              {% assign chartItemJson = clickUrl$enc$ -%}
          {% elseif clickStart1 == ''/'' and clickStart2 != ''//'' and clickStart2 != clickBadStart -%}
              {% assign chartItemJson = clickUrl$enc$ -%}
          {% endif -%}
          {% if forloop.first -%}{% assign itemclickurls = chartItemJson -%}{% else -%}{% assign itemclickurls = itemclickurls | Append:'','' | Append:chartItemJson -%}{% endif -%}
      {% endfor -%}
      {% assign itemclickurls = itemclickurls | Prepend:''['' | Append:'']'' -%}';
SET @ClickUrls = REPLACE( @ClickUrls, N'$enc$', @Enc );

DECLARE @Replacements TABLE ( [Seq] int NOT NULL, [Old] nvarchar(max) NOT NULL, [New] nvarchar(max) NOT NULL );

INSERT INTO @Replacements ( [Seq], [Old], [New] )
VALUES
    ( 1, N'{% assign fillColors = dataitems | Map:''fillcolor'' | Join:''"", ""'' | Prepend:''[""'' | Append:''""]'' %}'
       , REPLACE( REPLACE( @ListTemplate + @BracketTemplate, N'$list$', N'fillColors' ), N'$prop$', N'fillcolor' ) ),
    ( 2, N'{% assign borderColors = dataitems | Map:''bordercolor'' | Join:''"", ""'' | Prepend:''[""'' | Append:''""]'' %}'
       , REPLACE( REPLACE( @ListTemplate + @BracketTemplate, N'$list$', N'borderColors' ), N'$prop$', N'bordercolor' ) ),
    ( 3, N'{% assign tooltips = dataitems | Map:''tooltip'' | Join:''"", ""'' | Prepend:''""'' | Append:''""'' %}'
       , REPLACE( REPLACE( @ListTemplate, N'$list$', N'tooltips' ), N'$prop$', N'tooltip' ) ),
    ( 4, N'{% assign labels = dataitems | Map:''label'' | Join:''"", ""'' | Prepend:''""'' | Append:''""'' -%}'
       , REPLACE( REPLACE( @ListTemplate, N'$list$', N'labels' ), N'$prop$', N'label' ) ),
    ( 5, N'{% assign itemclickurls = dataitems | Select:''itemclickurl'' | Join:''"", ""'' | Prepend:''[""'' | Append:''""]'' %}'
       , @ClickUrls ),
    ( 6, N'''{{dataitems[0].label}}'''
       , N'{{ dataitems[0].label | Default:''''' + @Enc + N' }}' ),
    ( 7, N'""{{dataitems[0].fillcolor | Default:''#000000''}}""'
       , N'{{ dataitems[0].fillcolor | Default:''#000000''' + @Enc + N' }}' ),
    ( 8, N'''{{ datasetLabel }}'''
       , N'{{ datasetLabel | Default:''''' + @Enc + N' }}' ),
    ( 9, N'''{{ datasetFillColor }}'''
       , N'{{ datasetFillColor | Default:''''' + @Enc + N' }}' ),
    ( 10, N'''{{ datasetBorderColor }}'''
        , N'{{ datasetBorderColor | Default:''''' + @Enc + N' }}' ),
    ( 11, N'''{{ datasetPointColor }}'''
        , N'{{ datasetPointColor | Default:''''' + @Enc + N' }}' ),
    ( 12, N'''{{ datasetPointBorderColor }}'''
        , N'{{ datasetPointBorderColor | Default:''''' + @Enc + N' }}' ),
    ( 13, N'''{{ datasetPointHoverColor }}'''
        , N'{{ datasetPointHoverColor | Default:''''' + @Enc + N' }}' ),
    ( 14, N'''{{ datasetPointHoverBorderColor }}'''
        , N'{{ datasetPointHoverBorderColor | Default:''''' + @Enc + N' }}' ),
    ( 15, N'labelString: ''{{ label }}'''
        , N'labelString: {{ label | Default:''''' + @Enc + N' }}' ),
    ( 16, N'data: {{ dataitems | Map:''value'' | Join:'','' | Prepend:''['' | Append:'']'' }}'
        , N'data: [{% for chartItem in dataitems %}{% unless forloop.first %},{% endunless %}{{ chartItem.value' + @Num + N' }}{% endfor %}]' ),
    ( 17, N'value: {{dataitems[0].value}}'
        , N'value: {{ dataitems[0].value' + @Num + N' }}' );

DECLARE @Seq int = 1;
DECLARE @Count int = ( SELECT COUNT(*) FROM @Replacements );
DECLARE @Old nvarchar(max);
DECLARE @New nvarchar(max);

WHILE @Seq <= @Count
BEGIN
    SELECT @Old = [Old], @New = [New] FROM @Replacements WHERE [Seq] = @Seq;

    UPDATE [LavaShortcode]
       SET [Markup] = REPLACE( [Markup] COLLATE Latin1_General_BIN2, @Old, @New )
     WHERE [Guid] = @ShortcodeGuid
       AND CHARINDEX( @Old, [Markup] COLLATE Latin1_General_BIN2 ) > 0;

    SET @Seq = @Seq + 1;
END

UPDATE [LavaShortcode]
   SET [Markup] = @Helpers + [Markup]
 WHERE [Guid] = @ShortcodeGuid
   AND CHARINDEX( N'jsLt', [Markup] COLLATE Latin1_General_BIN2 ) > 0
   AND CHARINDEX( N'{% capture jsLt %}', [Markup] COLLATE Latin1_General_BIN2 ) = 0;";

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }
    }
}
