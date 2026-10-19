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
    /// Adds the Escape filter to the group and person merge fields in the saved
    /// info window templates of the Group Map, Group Finder and Group Type Map
    /// blocks, adds the Escape filter to the signer, person and document names in
    /// the Electronic Signature Receipt system communication.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 330, "17.8" )]
    public class EscapeMapInfoWindowAndSignatureReceiptTemplates : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            /*
                10/19/2026 - MSE

                Two stored templates still printed user supplied text as raw HTML after
                the code fixes, because changing a code default does not reach existing
                installs:

                1. The info window templates of the Group Map, Group Finder and Group Type
                   Map blocks print group, person and location values that anonymous or
                   low privileged people can set. Each block also keeps a saved copy of
                   the template, so the saved values are updated too.
                2. The Electronic Signature Receipt email prints the typed signer name,
                   the document name and person names. The anonymous signer of a public
                   workflow controls the typed name.

                Only the exact original tokens are changed, so a template that already
                uses the Escape filter is left alone and running this again changes
                nothing. Customized templates that still contain an exact token are
                updated too; the Escape filter is safe in HTML text and attributes.

                Reason: Prevent stored XSS through map info windows and signature receipts.
            */
            EscapeGroupMapInfoWindowTemplatesUp();
            EscapeElectronicSignatureReceiptUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }

        /// <summary>
        /// Adds the Escape filter to the merge fields that the code fix changed in the
        /// default info window templates of the Group Map, Group Finder and Group Type
        /// Map blocks. Both the saved block setting values and the attribute default
        /// values are updated. Attributes are found by block type and key because the
        /// attribute guids are not the same on every install. Each token is matched
        /// with and without spaces inside the braces, using a binary collation so the
        /// match is exact (case and spacing) and an already escaped token never matches.
        /// </summary>
        private void EscapeGroupMapInfoWindowTemplatesUp()
        {
            Sql( @"
DECLARE @Tokens TABLE (
    [Id] int IDENTITY(1,1) NOT NULL,
    [BlockTypeGuid] uniqueidentifier NOT NULL,
    [AttributeKey] nvarchar(1000) NOT NULL,
    [OldText] nvarchar(200) NOT NULL,
    [NewText] nvarchar(200) NOT NULL )

/* Group Map: InfoWindowContents */
INSERT INTO @Tokens ( [BlockTypeGuid], [AttributeKey], [OldText], [NewText] ) VALUES
      ( '967F0D2B-DB76-486A-B034-D22B9D9240D3', N'InfoWindowContents', N'{{ GroupName }}', N'{{ GroupName | Escape }}' )
    , ( '967F0D2B-DB76-486A-B034-D22B9D9240D3', N'InfoWindowContents', N'{{GroupName}}', N'{{GroupName | Escape}}' )
    , ( '967F0D2B-DB76-486A-B034-D22B9D9240D3', N'InfoWindowContents', N'{{ GroupMember.NickName }}', N'{{ GroupMember.NickName | Escape }}' )
    , ( '967F0D2B-DB76-486A-B034-D22B9D9240D3', N'InfoWindowContents', N'{{GroupMember.NickName}}', N'{{GroupMember.NickName | Escape}}' )
    , ( '967F0D2B-DB76-486A-B034-D22B9D9240D3', N'InfoWindowContents', N'{{ GroupMember.LastName }}', N'{{ GroupMember.LastName | Escape }}' )
    , ( '967F0D2B-DB76-486A-B034-D22B9D9240D3', N'InfoWindowContents', N'{{GroupMember.LastName}}', N'{{GroupMember.LastName | Escape}}' )
    , ( '967F0D2B-DB76-486A-B034-D22B9D9240D3', N'InfoWindowContents', N'{{ GroupMember.Email }}', N'{{ GroupMember.Email | Escape }}' )
    , ( '967F0D2B-DB76-486A-B034-D22B9D9240D3', N'InfoWindowContents', N'{{GroupMember.Email}}', N'{{GroupMember.Email | Escape}}' )

/* Group Finder: MapInfo */
INSERT INTO @Tokens ( [BlockTypeGuid], [AttributeKey], [OldText], [NewText] ) VALUES
      ( '9F8F2D68-DEEA-4686-810F-AB32923F855E', N'MapInfo', N'{{ Group.Name }}', N'{{ Group.Name | Escape }}' )
    , ( '9F8F2D68-DEEA-4686-810F-AB32923F855E', N'MapInfo', N'{{Group.Name}}', N'{{Group.Name | Escape}}' )

/* Group Type Map: InfoWindowContents */
INSERT INTO @Tokens ( [BlockTypeGuid], [AttributeKey], [OldText], [NewText] ) VALUES
      ( '2CCAFC0B-8B48-4B64-B210-0EDAF9FFC4EF', N'InfoWindowContents', N'{{GroupName}}', N'{{GroupName | Escape}}' )
    , ( '2CCAFC0B-8B48-4B64-B210-0EDAF9FFC4EF', N'InfoWindowContents', N'{{ GroupName }}', N'{{ GroupName | Escape }}' )
    , ( '2CCAFC0B-8B48-4B64-B210-0EDAF9FFC4EF', N'InfoWindowContents', N'{{GroupLocation.Street1}}', N'{{GroupLocation.Street1 | Escape}}' )
    , ( '2CCAFC0B-8B48-4B64-B210-0EDAF9FFC4EF', N'InfoWindowContents', N'{{ GroupLocation.Street1 }}', N'{{ GroupLocation.Street1 | Escape }}' )
    , ( '2CCAFC0B-8B48-4B64-B210-0EDAF9FFC4EF', N'InfoWindowContents', N'{{GroupLocation.City}}', N'{{GroupLocation.City | Escape}}' )
    , ( '2CCAFC0B-8B48-4B64-B210-0EDAF9FFC4EF', N'InfoWindowContents', N'{{ GroupLocation.City }}', N'{{ GroupLocation.City | Escape }}' )
    , ( '2CCAFC0B-8B48-4B64-B210-0EDAF9FFC4EF', N'InfoWindowContents', N'{{GroupLocation.State}}', N'{{GroupLocation.State | Escape}}' )
    , ( '2CCAFC0B-8B48-4B64-B210-0EDAF9FFC4EF', N'InfoWindowContents', N'{{ GroupLocation.State }}', N'{{ GroupLocation.State | Escape }}' )
    , ( '2CCAFC0B-8B48-4B64-B210-0EDAF9FFC4EF', N'InfoWindowContents', N'{{GroupLocation.PostalCode}}', N'{{GroupLocation.PostalCode | Escape}}' )
    , ( '2CCAFC0B-8B48-4B64-B210-0EDAF9FFC4EF', N'InfoWindowContents', N'{{ GroupLocation.PostalCode }}', N'{{ GroupLocation.PostalCode | Escape }}' )
    , ( '2CCAFC0B-8B48-4B64-B210-0EDAF9FFC4EF', N'InfoWindowContents', N'{{GroupMember.NickName}}', N'{{GroupMember.NickName | Escape}}' )
    , ( '2CCAFC0B-8B48-4B64-B210-0EDAF9FFC4EF', N'InfoWindowContents', N'{{ GroupMember.NickName }}', N'{{ GroupMember.NickName | Escape }}' )
    , ( '2CCAFC0B-8B48-4B64-B210-0EDAF9FFC4EF', N'InfoWindowContents', N'{{GroupMember.LastName}}', N'{{GroupMember.LastName | Escape}}' )
    , ( '2CCAFC0B-8B48-4B64-B210-0EDAF9FFC4EF', N'InfoWindowContents', N'{{ GroupMember.LastName }}', N'{{ GroupMember.LastName | Escape }}' )
    , ( '2CCAFC0B-8B48-4B64-B210-0EDAF9FFC4EF', N'InfoWindowContents', N'{{GroupMember.Email}}', N'{{GroupMember.Email | Escape}}' )
    , ( '2CCAFC0B-8B48-4B64-B210-0EDAF9FFC4EF', N'InfoWindowContents', N'{{ GroupMember.Email }}', N'{{ GroupMember.Email | Escape }}' )

DECLARE @TokenId int = 1
DECLARE @MaxTokenId int = ( SELECT MAX( [Id] ) FROM @Tokens )
DECLARE @AttributeId int
DECLARE @OldText nvarchar(200)
DECLARE @NewText nvarchar(200)

WHILE @TokenId <= @MaxTokenId
BEGIN
    SELECT @AttributeId = A.[Id]
         , @OldText = T.[OldText]
         , @NewText = T.[NewText]
      FROM @Tokens T
     INNER JOIN [BlockType] BT
        ON BT.[Guid] = T.[BlockTypeGuid]
     INNER JOIN [Attribute] A
        ON A.[EntityTypeQualifierColumn] = 'BlockTypeId'
       AND A.[EntityTypeQualifierValue] = CAST( BT.[Id] AS nvarchar(50) )
       AND A.[Key] = T.[AttributeKey]
     WHERE T.[Id] = @TokenId

    IF @@ROWCOUNT = 1
    BEGIN
        UPDATE [Attribute]
           SET [DefaultValue] = REPLACE( [DefaultValue] COLLATE Latin1_General_BIN2, @OldText, @NewText )
             , [IsDefaultPersistedValueDirty] = 1
         WHERE [Id] = @AttributeId
           AND CHARINDEX( @OldText, [DefaultValue] COLLATE Latin1_General_BIN2 ) > 0

        UPDATE [AttributeValue]
           SET [Value] = REPLACE( [Value] COLLATE Latin1_General_BIN2, @OldText, @NewText )
             , [IsPersistedValueDirty] = 1
         WHERE [AttributeId] = @AttributeId
           AND CHARINDEX( @OldText, [Value] COLLATE Latin1_General_BIN2 ) > 0
    END

    SET @TokenId = @TokenId + 1
END" );
        }

        /// <summary>
        /// Adds the Escape filter to the signed name, the document name and the person
        /// names in the Electronic Signature Receipt system communication body. The
        /// stock body uses non-breaking spaces inside most of these tokens, so both
        /// the non-breaking space and the normal space forms are matched. A binary
        /// collation keeps the match exact, so an already escaped token never matches.
        /// The Default filter stays before Escape so the fallback text is unchanged.
        /// </summary>
        private void EscapeElectronicSignatureReceiptUp()
        {
            Sql( @"
DECLARE @Nbsp nchar(1) = NCHAR(160)
DECLARE @Tokens TABLE (
    [Id] int IDENTITY(1,1) NOT NULL,
    [OldText] nvarchar(200) NOT NULL,
    [NewText] nvarchar(200) NOT NULL )

INSERT INTO @Tokens ( [OldText], [NewText] ) VALUES
      ( N'{{' + @Nbsp + N'SignatureDocument.SignedName' + @Nbsp + N'}}', N'{{ SignatureDocument.SignedName | Escape }}' )
    , ( N'{{ SignatureDocument.SignedName }}', N'{{ SignatureDocument.SignedName | Escape }}' )
    , ( N'{{' + @Nbsp + N'SignatureDocument.Name' + @Nbsp + N'}}', N'{{ SignatureDocument.Name | Escape }}' )
    , ( N'{{ SignatureDocument.Name }}', N'{{ SignatureDocument.Name | Escape }}' )
    , ( N'{{' + @Nbsp + N'SignatureDocument.SignedByPersonAlias.Person.FullName' + @Nbsp + N'|' + @Nbsp + N'Default:''Hello''' + @Nbsp + N'}}', N'{{ SignatureDocument.SignedByPersonAlias.Person.FullName | Default:''Hello'' | Escape }}' )
    , ( N'{{ SignatureDocument.SignedByPersonAlias.Person.FullName | Default:''Hello'' }}', N'{{ SignatureDocument.SignedByPersonAlias.Person.FullName | Default:''Hello'' | Escape }}' )
    , ( N'{{' + @Nbsp + N'SignatureDocument.SignedByPersonAlias.Person.FullName' + @Nbsp + N'}}', N'{{ SignatureDocument.SignedByPersonAlias.Person.FullName | Escape }}' )
    , ( N'{{ SignatureDocument.SignedByPersonAlias.Person.FullName }}', N'{{ SignatureDocument.SignedByPersonAlias.Person.FullName | Escape }}' )
    , ( N'{{' + @Nbsp + N'SignatureDocument.AppliesToPersonAlias.Person.FullName' + @Nbsp + N'|' + @Nbsp + N'Default:''-''' + @Nbsp + N'}}', N'{{ SignatureDocument.AppliesToPersonAlias.Person.FullName | Default:''-'' | Escape }}' )
    , ( N'{{ SignatureDocument.AppliesToPersonAlias.Person.FullName | Default:''-'' }}', N'{{ SignatureDocument.AppliesToPersonAlias.Person.FullName | Default:''-'' | Escape }}' )

DECLARE @TokenId int = 1
DECLARE @MaxTokenId int = ( SELECT MAX( [Id] ) FROM @Tokens )
DECLARE @OldText nvarchar(200)
DECLARE @NewText nvarchar(200)

WHILE @TokenId <= @MaxTokenId
BEGIN
    SELECT @OldText = [OldText]
         , @NewText = [NewText]
      FROM @Tokens
     WHERE [Id] = @TokenId

    UPDATE [SystemCommunication]
       SET [Body] = REPLACE( [Body] COLLATE Latin1_General_BIN2, @OldText, @NewText )
     WHERE [Guid] = '224A0E80-069B-463C-8187-E13682F8A550'
       AND CHARINDEX( @OldText, [Body] COLLATE Latin1_General_BIN2 ) > 0

    SET @TokenId = @TokenId + 1
END" );
        }
    }
}
