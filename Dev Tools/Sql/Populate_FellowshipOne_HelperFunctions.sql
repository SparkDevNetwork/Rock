create function [dbo].[FormatName](@NameString varchar(100), @NameFormat varchar(20))
returns varchar(100) as
begin
--blindman, 11/04: http://www.sqlteam.com/forums/topic.asp?TOPIC_ID=56499
--blindman, 7/06: modified to account for suffixes trailing last name.  Added to honorific/suffix list.
--blindman, 6/17/08:	Added "CPA" suffix and checks for non-alpha characters and "EXT" string.
--blindman, 6/18/2008:	Added support for poorly hyphenated names and suffixes preceded by commas.
--blindman, 11/11/2009:	Added LPN and RN to suffix list.
--blindman, 11/13/2009:	Added code to handle compound first names, for cases where there is a middle name.
--						Added code to ignore period in format string when outputting full FirstName or MiddleName
--						that is not a simple initial.
--blindman, 11/20/2009:	Added single quotes to allowed characters for names such as O'Neil
--						Added workaround to include dashes in allowed characters for hyphenated names.
--						Added support for "Von Der ....". "Vanden ...", "Di ...", and "Des ..." compound surnames.
--						Added support for secondary last names and nicknames in parenthesis.
--						Added support for accent character "`" in names.
--						Added conversion for numeric suffixes (2nd, 3rd, 4th).
--blindman, 11/24/2009:	Added more special case handling.
--blindman, 11/25/2009:	Commented out section for compound first names.

--FormatName decodes a NameString into its component parts and returns it in a requested format.
--@NameString is the raw value to be parsed.
--@NameFormat is a string that defines the output format.  Each letter in the string represents
--a component of the name in the order that it is to be returned.
--	[H] = Full honorific
--	[h] = Abbreviated honorific
--	[F] = First name
--	[f] = First initial
--	[M] = Middle name
--	[m] = Middle initial
--	[L] = Last name
--	[l] = Last initial
--	[S] = Full suffix
--	[s] = Abbreviated suffix
--	[.] = Period
--	[,] = Comma
--	[ ] = Space
--Sample Syntax	: select dbo.FormatName('President Barack Hussein Obama Senior', 'h. F m. L s.')
--Returns	: 'Pres. Barack H. Obama Sr.'

--Test variables
--declare	@NameString varchar(50)
--declare	@NameFormat varchar(20)
--set	@NameFormat = 'F'
--set	@NameString = ' IDA B CHANDLER JOHNSON'

Declare	@Honorific varchar(20)
Declare @FirstName varchar(20)
Declare @MiddleName varchar(30)
Declare @LastName varchar(30)
Declare @Suffix varchar(20)
Declare	@TempString varchar(100)
Declare	@TempString2 varchar(100)
Declare	@IgnorePeriod char(1)

--Prepare the string

--Make sure each period is followed by a space character.
set	@NameString = rtrim(ltrim(replace(@NameString, '.', '. ')))

--Replace numeric suffixes
set	@NameString = replace(@NameString, '2nd', 'II')
set	@NameString = replace(@NameString, '3rd', 'III')
set	@NameString = replace(@NameString, '4th', 'IV')

--Remove disallowed characters
declare	@PatternString varchar(50)
set	@NameString = replace(@NameString, '-', '¬') --Replace dashes we want to save, as patindex does not allow escaping characters.
set	@PatternString = '%[^a-z ¬()`,'''']%~'  --'''' includes single quote in permitted character list.
while	patindex(@PatternString, @NameString) > 0 set @NameString = stuff(@NameString, patindex(@PatternString, @NameString), 1, ' ')
set	@NameString = replace(@NameString, '¬', '-') --Put the dashes back

--Remove telephone ext
set	@NameString = ltrim(rtrim(replace(' ' + @NameString + ' ', ' EXT ', ' ')))

--Make sure there is at least one space after commas
set @NameString = replace(@NameString, ',', ', ')

--Eliminate double-spaces.
while  charindex('  ', @NameString) > 0 set @NameString = replace(@NameString, '  ', ' ')

--Eliminate periods
while  charindex('.', @NameString) > 0 set @NameString = replace(@NameString, '.', '')

--Remove spaces around hyphenated names
set	@NameString = replace(replace(@NameString, '- ', '-'), ' -', '-')

--Join Irish surnames
set	@NameString = Replace(@NameString, 'O'' ', 'O''')

--Remove commas before suffixes
set	@NameString = replace(@NameString, ', CLU', ' CLU')
set	@NameString = replace(@NameString, ', CNP', ' CNP') --Certified Notary Public
set	@NameString = replace(@NameString, ', ESQ', ' ESQ')
set	@NameString = replace(@NameString, ', Jr', ' Jr')
set	@NameString = replace(@NameString, ', LPN', ' LPN')
set	@NameString = replace(@NameString, ', RN', ' RN')
set	@NameString = replace(@NameString, ', Sr', ' Sr')
set	@NameString = replace(@NameString, ', II', ' II')
set	@NameString = replace(@NameString, ', III', ' III')
set	@NameString = replace(@NameString, ', IV', ' IV')

--Temporarily join multi-word firstnames
set	@NameString = ltrim(replace(' ' + @NameString, ' Ann Marie ', ' Ann~Marie '))
set	@NameString = ltrim(replace(' ' + @NameString, ' Anna Marie ', ' Anna~Marie '))
set	@NameString = ltrim(replace(' ' + @NameString, ' Barbara Jo ', ' Barbara~Jo '))
set	@NameString = ltrim(replace(' ' + @NameString, ' Betty Lou ', ' Betty~Lou '))
set	@NameString = ltrim(replace(' ' + @NameString, ' Billie Jo ', ' Billie~Jo '))
set	@NameString = ltrim(replace(' ' + @NameString, ' Bobbi Jo ', ' Bobbi~Jo '))
set	@NameString = ltrim(replace(' ' + @NameString, ' Dee Dee ', ' Dee~Dee '))
set	@NameString = ltrim(replace(' ' + @NameString, ' Fannie Mae ', ' Fannie~Mae '))
set	@NameString = ltrim(replace(' ' + @NameString, ' Lisa Marie ', ' Lisa~Marie '))
set	@NameString = ltrim(replace(' ' + @NameString, ' Mary Beth ', ' Mary~Beth '))
set	@NameString = ltrim(replace(' ' + @NameString, ' Mary Ellen ', ' Mary~Ellen '))
set	@NameString = ltrim(replace(' ' + @NameString, ' Mary Jane ', ' Mary~Jane '))
set	@NameString = ltrim(replace(' ' + @NameString, ' Mary Jo ', ' Mary~Jo '))
set	@NameString = ltrim(replace(' ' + @NameString, ' Mary Lou ', ' Mary~Lou '))
set	@NameString = ltrim(replace(' ' + @NameString, ' Rose Mary ', ' Rose~Mary '))
set	@NameString = ltrim(replace(' ' + @NameString, ' Rose Marie ', ' Rose~Marie '))
set	@NameString = ltrim(replace(' ' + @NameString, ' Sugar Rae ', ' Sugar~Rae '))
--For compound names ending in Ann, also include Anne variation.
set	@NameString = ltrim(replace(' ' + @NameString, ' Beth Ann', ' Beth~Ann'))
set	@NameString = ltrim(replace(' ' + @NameString, ' Dee Ann', ' Dee~Ann'))
set	@NameString = ltrim(replace(' ' + @NameString, ' Dorothy Ann', ' Dorothy~Ann'))
set	@NameString = ltrim(replace(' ' + @NameString, ' Ellen Ann', ' Ellen~Ann'))
set	@NameString = ltrim(replace(' ' + @NameString, ' Jo Ann', ' Jo~Ann'))
set	@NameString = ltrim(replace(' ' + @NameString, ' Lea Ann', ' Lea~Ann'))
set	@NameString = ltrim(replace(' ' + @NameString, ' Lee Ann', ' Lee~Ann'))
set	@NameString = ltrim(replace(' ' + @NameString, ' Leigh Ann', ' Leigh~Ann'))
set	@NameString = ltrim(replace(' ' + @NameString, ' Lu Ann', ' Lu~Ann'))
set	@NameString = ltrim(replace(' ' + @NameString, ' Mary Ann', ' Mary~Ann'))
set	@NameString = ltrim(replace(' ' + @NameString, ' Rae Ann', ' Rae~Ann'))
set	@NameString = ltrim(replace(' ' + @NameString, ' Ruth Ann', ' Ruth~Ann'))

--Temporarily join multi-word surnames
set	@NameString = ltrim(replace(' ' + @NameString, ' Dos ', ' Dos~'))
set	@NameString = ltrim(replace(' ' + @NameString, ' St ', ' St.~'))
set	@NameString = ltrim(replace(' ' + @NameString, ' St. ', ' St.~'))
set	@NameString = ltrim(replace(' ' + @NameString, ' Da ', ' Da~'))
set	@NameString = ltrim(replace(' ' + @NameString, ' Di ', ' Di~'))
set	@NameString = ltrim(replace(' ' + @NameString, ' Del ', ' Del~'))
set	@NameString = ltrim(replace(' ' + @NameString, ' Des ', ' Des~'))
set	@NameString = ltrim(replace(' ' + @NameString, ' Vanden ', ' Vanden~'))
set	@NameString = ltrim(replace(' ' + @NameString, ' Van De ', ' Van~De~'))
set	@NameString = ltrim(replace(' ' + @NameString, ' Van Den ', ' Van~Den~'))
set	@NameString = ltrim(replace(' ' + @NameString, ' Vander ', ' Vander~'))
set	@NameString = ltrim(replace(' ' + @NameString, ' Van ', ' Van~'))
set	@NameString = ltrim(replace(' ' + @NameString, ' Ver ', ' Ver~'))
set	@NameString = ltrim(replace(' ' + @NameString, ' Van Der ', ' Van~Der~'))
set	@NameString = ltrim(replace(' ' + @NameString, ' Von Der ', ' Von~Der~'))
set	@NameString = ltrim(replace(' ' + @NameString, ' Von ', ' Von~'))
set	@NameString = ltrim(replace(' ' + @NameString, ' Mc ', ' Mc~'))
set	@NameString = ltrim(replace(' ' + @NameString, ' Mac ', ' Mac~'))
set	@NameString = ltrim(replace(' ' + @NameString, ' La ', ' La~')) --Must be checked before "De", to handle "De La [Surname]"s.
set	@NameString = ltrim(replace(' ' + @NameString, ' De ', ' De~'))

--Temporarily join 2nd lastname and nicknames defined within parethesis
set	@NameString = ltrim(replace(' ' + @NameString, ' (', '~('))

--If the lastname is listed first, strip it off.
set	@TempString = rtrim(left(@NameString, charindex(' ', @NameString)))
--Below logic now handled by joining multi-word surnames above.
--if	@TempString in ('VAN', 'VON', 'MC', 'Mac', 'DE') set @TempString = rtrim(left(@NameString, charindex(' ', @NameString, len(@TempString)+2)))

--Search for suffixes trailing the LastName
set	@TempString2 = ltrim(right(@NameString, len(@NameString) - len(@TempString)))
set	@TempString2 = rtrim(left(@TempString2, charindex(' ', @TempString2)))

if	right(@TempString2, 1) = ','
	begin
		set @Suffix = left(@TempString2, len(@TempString2)-1)
		set @LastName = left(@TempString, len(@TempString))
	end
if	right(@TempString, 1) = ',' set @LastName = left(@TempString, len(@TempString)-1)
if	len(@LastName) > 0 set	@NameString = ltrim(right(@NameString, len(@NameString) - len(@TempString)))
if	len(@Suffix) > 0 set	@NameString = ltrim(right(@NameString, len(@NameString) - len(@TempString2)))

--Get rid of any remaining commas
while  charindex(',', @NameString) > 0 set @NameString = replace(@NameString, ',', '')
--Get Honorific and strip it out of the string
set	@TempString = rtrim(left(@NameString, charindex(' ', @NameString + ' ')))
if	@TempString in (
		 'Admiral', 'Adm',
		'Captain', 'Cpt', 'Capt',
		'Commander', 'Cmd',
		'Corporal', 'Cpl',
		'Doctor', 'Dr',
		'Father', 'Fr',
		'General', 'Gen',
		'Governor', 'Gov',
		'Honorable', 'Hon',
		'Lieutenant', 'Lt',
		'Madam', 'Mdm',
		'Madame', 'Mme',
		'Mademoiselle', 'Mlle',
		'Major', 'Maj',
		'Miss', 'Ms',
		'Mr',
		'Mrs',
		'President', 'Pres',
		'Private', 'Pvt',
		'Professor', 'Prof',
		'Rabbi',
		'Reverend', 'Rev',
		'Senior', 'Sr',
		'Seniora', 'Sra',
		'Seniorita', 'Srta',
		'Sergeant', 'Sgt',
		'Sir',
		'Sister') set @Honorific = @TempString
if	len(@Honorific) > 0 set	@NameString = ltrim(right(@NameString, len(@NameString) - len(@TempString)))
--Get Suffix and strip it out of the string
if @Suffix is null
	begin
		set	@TempString = ltrim(right(@NameString, charindex(' ', Reverse(@NameString) + ' ')))
--		if	@TempString in (
		while @TempString in (
				'Attorney', 'Att', 'Atty',
				'BA',
				'BS',
				'CPA',
				'CNP', --Certified Notary Public
				'DDS',
				'DVM',
				'Esquire', 'Esq',
				'II',
				'III',
				'IV',
				'Junior', 'Jr',
				'LPN',
				'MBA',
				'MD',
				'OD',
				'PHD',
				'RN',
				'Senior', 'Sr',
				'ASA', 'SRA', 'CLU' --Realestate Certifictaions
				)
			begin
				set @Suffix = @TempString + coalesce(' ' + @Suffix, '')
				set @NameString = rtrim(left(@NameString, len(@NameString) - len(@TempString)))
				set	@TempString = ltrim(right(@NameString, charindex(' ', Reverse(@NameString) + ' ')))
			end
-- 		if	len(@Suffix) > 0 set @NameString = rtrim(left(@NameString, len(@NameString) - len(@TempString)))
	end

if @LastName is null
begin
	--Get LastName and strip it out of the string
	set	@LastName = ltrim(right(@NameString, charindex(' ', Reverse(@NameString) + ' ')))
	set	@NameString = rtrim(left(@NameString, len(@NameString) - len(@LastName)))
end

--Get FirstName and strip it out of the string
set	@FirstName = rtrim(left(@NameString, charindex(' ', @NameString + ' ')))
set	@NameString = ltrim(right(@NameString, len(@NameString) - len(@FirstName)))

--If the remaining string is compound, include the first portion in the FirstName
--if charindex(' ', @NameString) > 1
--	begin
--		set @FirstName = @FirstName + ' ' + left(@NameString, charindex(' ', @NameString))
--		set @NameString = right(@NameString, len(@NameString) - charindex(' ', @NameString))
--	end

--Anything remaining is MiddleName
set	@MiddleName = @NameString

--Create the output string
set	@TempString = ''
while len(@NameFormat) > 0
begin
	if @IgnorePeriod = 'F' or left(@NameFormat, 1) <> '.'
	begin
	set @IgnorePeriod = 'F'
	set @TempString = @TempString +
		case ascii(left(@NameFormat, 1))
			when '32' then case right(@TempString, 1) --Space
				when ' ' then ''
				else ' '
				end
			when '44' then case right(@TempString, 1) --Comma
				when ' ' then ''
				else ','
				end
			when '46' then case right(@TempString, 1) --Period
				when ' ' then ''
				else '.'
				end
			when '70' then isnull(@FirstName, '') --F
			when '72' then case @Honorific --H
				when 'Adm' then 'Admiral'
				when 'Capt' then 'Captain'
				when 'Cmd' then 'Commander'
				when 'Cpl' then 'Corporal'
				when 'Cpt' then 'Captain'
				when 'Dr' then 'Doctor'
				when 'Fr' then 'Father'
				when 'Gen' then 'General'
				when 'Gov' then 'Governor'
				when 'Hon' then 'Honorable'
				when 'Lt' then 'Lieutenant'
				when 'Maj' then 'Major'
				when 'Mdm' then 'Madam'
				when 'Mlle' then 'Mademoiselle'
				when 'Mme' then 'Madame'
				when 'Ms' then 'Miss'
				when 'Pres' then 'President'
				when 'Prof' then 'Professor'
				when 'Pvt' then 'Private'
				when 'Sr' then 'Senior'
				when 'Sra' then 'Seniora'
				when 'Srta' then 'Seniorita'
				when 'Rev' then 'Reverend'
				when 'Sgt' then 'Sergeant'
				else isnull(@Honorific, '')
				end
			when '76' then isnull(@LastName, '') --L
			when '77' then isnull(@MiddleName, '') --M
			when '83' then case @Suffix --S
				when 'Att' then 'Attorney'
				when 'Atty' then 'Attorney'
				when 'Esq' then 'Esquire'
				when 'Jr' then 'Junior'
				when 'Sr' then 'Senior'
				else isnull(@Suffix, '')
				end
			when '102' then isnull(left(@FirstName, 1), '') --f
			when '104' then case @Honorific --h
				when 'Admiral' then 'Adm'
				when 'Captain' then 'Capt'
				when 'Commander' then 'Cmd'
				when 'Corporal' then 'Cpl'
				when 'Doctor' then 'Dr'
				when 'Father' then 'Fr'
				when 'General' then 'Gen'
				when 'Governor' then 'Gov'
				when 'Honorable' then 'Hon'
				when 'Lieutenant' then 'Lt'
				when 'Madam' then 'Mdm'
				when 'Madame' then 'Mme'
				when 'Mademoiselle' then 'Mlle'
				when 'Major' then 'Maj'
				when 'Miss' then 'Ms'
				when 'President' then 'Pres'
				when 'Private' then 'Pvt'
				when 'Professor' then 'Prof'
				when 'Reverend' then 'Rev'
				when 'Senior' then 'Sr'
				when 'Seniora' then 'Sra'
				when 'Seniorita' then 'Srta'
				when 'Sergeant' then 'Sgt'
				else isnull(@Honorific, '')
				end
			when '108' then isnull(left(@LastName, 1), '') --l
			when '109' then isnull(left(@MiddleName, 1), '') --m
			when '115' then case @Suffix --s
				when 'Attorney' then 'Atty'
				when 'Esquire' then 'Esq'
				when 'Junior' then 'Jr'
				when 'Senior' then 'Sr'
				else isnull(@Suffix, '')
				end
			else ''
		end
		--The following honorifics and suffixes have no further abbreviations, and so should not be followed by a period:
		if ((ascii(left(@NameFormat, 1)) = 72 and @Honorific in ('Rabbi', 'Sister'))
			or (ascii(left(@NameFormat, 1)) = 115 and @Suffix in ('ASA', 'BA', 'BS', 'CLU', 'CNP', 'DDS', 'DVM', 'II', 'III', 'IV', 'V', 'MBA', 'MD', 'PHD', 'RN', 'LPN', 'SRA')))
			set @IgnorePeriod = 'T'
		--If the FirstName or MiddleName is not an initial, then do not follow with a period.
		if	ascii(left(@NameFormat, 1)) = '70' and len(@FirstName) > 1 set @IgnorePeriod = 'T'
		if	ascii(left(@NameFormat, 1)) = '77' and len(@MiddleName) > 1 set @IgnorePeriod = 'T'
	end
	set @NameFormat = right(@NameFormat, len(@NameFormat) - 1)
end
--select	replace(@TempString, '~', ' ')
Return	replace(@TempString, '~', ' ')
end
GO

/* ==================================================================
 ====================================================================
	     Cleans the case of weirdly capitalized strings
====================================================================
================================================================== */

CREATE FUNCTION [dbo].[CleanCase] ( @Name varchar(200) )
RETURNS varchar(200)
AS  
BEGIN 
	-- first trim the name
	SET @Name = LTRIM(RTRIM(@Name))

	-- declare variable to hold the reset name
	DECLARE @Reset varchar(200)
	SET @Reset = ''
 
	If @Name <> ''
	BEGIN
	-- declare and assign variables that will be used to
	-- loop through each character in the name 
	DECLARE @CharCount int, @LoopCount int
	SET @LoopCount = 1
	SET @CharCount = Len(@Name)
  
	-- should the next character we append be upper case?
	-- first character is always upper case.
	DECLARE @MakeUpper bit 
	SET @MakeUpper = 1

	WHILE @LoopCount <= @CharCount
	BEGIN
	DECLARE @Character char
	SET @Character = Substring(@Name, @LoopCount, 1)
   
	-- append this character to the value we will return
	IF @MakeUpper  = 1 
	SET @Reset  = @Reset + UPPER(@Character)
	ELSE
	SET @Reset = @Reset + LOWER(@Character)
   
	-- work out if the next character should be upper case
	SELECT @MakeUpper = CASE 
		WHEN @Character = '-' THEN  1
		WHEN @Character = '.' THEN  1
		WHEN @Character = ' ' THEN  1
		WHEN @Character = '''' THEN 1
		ELSE 0
	END
   
	-- increment the loop counter
	SET @LoopCount = @LoopCount + 1
	END
	END 
	ELSE
	BEGIN
	SET @Reset = ''
	END

	RETURN @Reset
END
GO

/* ==================================================================
 ====================================================================
	     Strips the string of the specified pattern
====================================================================
================================================================== */
CREATE FUNCTION [dbo].[Strip](@STRING VARCHAR(8000), @PATTERN VARCHAR(100))
RETURNS VARCHAR(8000)
AS
BEGIN
WHILE PATINDEX(@PATTERN, @STRING) > 0
	SELECT @STRING = STUFF(@STRING, PatIndex(@PATTERN, @STRING), 1, '')
RETURN (@STRING)
END

