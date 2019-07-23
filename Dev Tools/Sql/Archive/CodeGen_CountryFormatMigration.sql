set nocount on
declare
@crlf varchar(2) = char(13) + char(10)

IF OBJECT_ID('tempdb..#codeTable') IS NOT NULL
    DROP TABLE #codeTable

create table #codeTable (
    Id int identity(1,1) not null,
    CodeText nvarchar(max),
    CONSTRAINT [pk_codeTable] PRIMARY KEY CLUSTERED  ( [Id]) );
   
DECLARE @Order int = 0
DECLARE @CountryCode varchar(2)
DECLARE @Country varchar(100)
DECLARE @CityLabel varchar(100)
DECLARE @StateLabel varchar(100)
DECLARE @ZipLabel varchar(100)
DECLARE @AddressFormat varchar(400)

DECLARE country_cursor CURSOR FORWARD_ONLY READ_ONLY
FOR
	SELECT 
		'AF' AS [CountryCode]
		 , 'Afghanistan' AS [Country]
		 , NULL AS [CityLabel]
		 , NULL AS [StateLabel]
		 , NULL AS [PostalCodeLabel]
		 , NULL AS [AddressFormat] UNION ALL
	SELECT 'AX', 'Aland Islands', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'AL', 'Albania', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'DZ', 'Algeria', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'AS', 'American Samoa', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'AD', 'Andorra', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'AO', 'Angola', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'AI', 'Anguilla', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'AQ', 'Antarctica', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'AG', 'Antigua and Barbuda', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'AR', 'Argentina', 'Complements', 'Municipality', 'Postal Code', '{{ Street1 }}' + @crlf + '{{ Street2 }} {{ City }}' + @crlf + '{{ Zip }} {{ State }}' + @crlf + '{{ Country }}' UNION ALL
	SELECT 'AM', 'Armenia', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'AW', 'Aruba', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'AU', 'Australia', 'Locality', 'State', 'Postcode', NULL UNION ALL
	SELECT 'AT', 'Austria', 'Town', '', 'Postal Code', '{{ Street1 }}' + @crlf + '{{ Street2 }}' + @crlf + '{{ Zip }} {{ City }}' + @crlf + '{{ Country }}' UNION ALL
	SELECT 'AZ', 'Azerbaijan', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'BS', 'Bahamas', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'BH', 'Bahrain', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'BD', 'Bangladesh', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'BB', 'Barbados', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'BY', 'Belarus', 'Village', 'Region', 'Postal Code', '{{ Street1 }}' + @crlf + '{{ Street2 }}' + @crlf + '{{ City }}' + @crlf + '{{ Zip }}' + @crlf + '{{ State }}' + @crlf + '{{ Country }}' UNION ALL
	SELECT 'BE', 'Belgium', 'Spatial', 'Town', 'Postal Code', '{{ Street1 }}' + @crlf + '{{ Street2 }}' + @crlf + '{{ City }}' + @crlf + '{{ Zip }} {{ State }}' + @crlf + '{{ Country }}' UNION ALL
	SELECT 'BZ', 'Belize', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'BJ', 'Benin', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'BM', 'Bermuda', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'BT', 'Bhutan', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'BO', 'Bolivia', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'BQ', 'Bonaire, Saint Eustatius and Saba ', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'BA', 'Bosnia and Herzegovina', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'BW', 'Botswana', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'BV', 'Bouvet Island', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'BR', 'Brazil', 'Neighbourhood', 'Municipality, State', 'Postal Code', '{{ Street1 }}' + @crlf + '{{ Street2 }}' + @crlf + '{{ City }}' + @crlf + '{{ State }}' + @crlf + '{{ Zip }}' + @crlf + '{{ Country }}' UNION ALL
	SELECT 'IO', 'British Indian Ocean Territory', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'VG', 'British Virgin Islands', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'BN', 'Brunei', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'BG', 'Bulgaria', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'BF', 'Burkina Faso', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'BI', 'Burundi', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'KH', 'Cambodia', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'CM', 'Cameroon', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'CA', 'Canada', 'City', 'Province/Territory', 'Postal Code', NULL UNION ALL
	SELECT 'CV', 'Cape Verde', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'KY', 'Cayman Islands', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'CF', 'Central African Republic', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'TD', 'Chad', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'CL', 'Chile', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'CN', 'China', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'CX', 'Christmas Island', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'CC', 'Cocos Islands', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'CO', 'Colombia', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'KM', 'Comoros', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'CK', 'Cook Islands', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'CR', 'Costa Rica', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'HR', 'Croatia', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'CU', 'Cuba', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'CW', 'Curacao', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'CY', 'Cyprus', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'CZ', 'Czech Republic', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'CD', 'Democratic Republic of the Congo', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'DK', 'Denmark', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'DJ', 'Djibouti', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'DM', 'Dominica', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'DO', 'Dominican Republic', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'TL', 'East Timor', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'EC', 'Ecuador', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'EG', 'Egypt', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'SV', 'El Salvador', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'GQ', 'Equatorial Guinea', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'ER', 'Eritrea', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'EE', 'Estonia', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'ET', 'Ethiopia', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'FK', 'Falkland Islands', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'FO', 'Faroe Islands', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'FJ', 'Fiji', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'FI', 'Finland', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'FR', 'France', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'GF', 'French Guiana', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'PF', 'French Polynesia', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'TF', 'French Southern Territories', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'GA', 'Gabon', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'GM', 'Gambia', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'GE', 'Georgia', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'DE', 'Germany', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'GH', 'Ghana', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'GI', 'Gibraltar', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'GR', 'Greece', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'GL', 'Greenland', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'GD', 'Grenada', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'GP', 'Guadeloupe', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'GU', 'Guam', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'GT', 'Guatemala', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'GG', 'Guernsey', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'GN', 'Guinea', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'GW', 'Guinea-Bissau', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'GY', 'Guyana', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'HT', 'Haiti', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'HM', 'Heard Island and McDonald Islands', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'HN', 'Honduras', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'HK', 'Hong Kong', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'HU', 'Hungary', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'IS', 'Iceland', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'IN', 'India', 'Locality', 'State', 'City - Postal Code', '{{ Street1 }}' + @crlf + '{{ Street2 }}' + @crlf + '{{ City }}' + @crlf + '{{ Zip }}' + @crlf + '{{ State }}' + @crlf + '{{ Country }}' UNION ALL
	SELECT 'ID', 'Indonesia', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'IR', 'Iran', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'IQ', 'Iraq', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'IE', 'Ireland', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'IM', 'Isle of Man', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'IL', 'Israel', 'Town', NULL, 'Postal Code', NULL UNION ALL
	SELECT 'IT', 'Italy', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'CI', 'Ivory Coast', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'JM', 'Jamaica', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'JP', 'Japan', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'JE', 'Jersey', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'JO', 'Jordan', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'KZ', 'Kazakhstan', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'KE', 'Kenya', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'KI', 'Kiribati', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'XK', 'Kosovo', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'KW', 'Kuwait', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'KG', 'Kyrgyzstan', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'LA', 'Laos', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'LV', 'Latvia', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'LB', 'Lebanon', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'LS', 'Lesotho', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'LR', 'Liberia', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'LY', 'Libya', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'LI', 'Liechtenstein', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'LT', 'Lithuania', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'LU', 'Luxembourg', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'MO', 'Macao', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'MK', 'Macedonia', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'MG', 'Madagascar', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'MW', 'Malawi', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'MY', 'Malaysia', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'MV', 'Maldives', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'ML', 'Mali', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'MT', 'Malta', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'MH', 'Marshall Islands', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'MQ', 'Martinique', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'MR', 'Mauritania', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'MU', 'Mauritius', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'YT', 'Mayotte', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'MX', 'Mexico', 'City', 'State', 'Postal Code', '{{ Street1 }}' + @crlf + '{{ Street2 }}' + @crlf + '{{ Zip }} {{ City }}, {{ State }}' + @crlf + '{{ Country }}' UNION ALL
	SELECT 'FM', 'Micronesia', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'MD', 'Moldova', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'MC', 'Monaco', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'MN', 'Mongolia', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'ME', 'Montenegro', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'MS', 'Montserrat', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'MA', 'Morocco', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'MZ', 'Mozambique', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'MM', 'Myanmar', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'NA', 'Namibia', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'NR', 'Nauru', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'NP', 'Nepal', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'NL', 'Netherlands', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'AN', 'Netherlands Antilles', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'NC', 'New Caledonia', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'NZ', 'New Zealand', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'NI', 'Nicaragua', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'NE', 'Niger', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'NG', 'Nigeria', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'NU', 'Niue', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'NF', 'Norfolk Island', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'KP', 'North Korea', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'MP', 'Northern Mariana Islands', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'NO', 'Norway', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'OM', 'Oman', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'PK', 'Pakistan', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'PW', 'Palau', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'PS', 'Palestinian Territory', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'PA', 'Panama', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'PG', 'Papua New Guinea', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'PY', 'Paraguay', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'PE', 'Peru', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'PH', 'Philippines', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'PN', 'Pitcairn', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'PL', 'Poland', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'PT', 'Portugal', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'PR', 'Puerto Rico', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'QA', 'Qatar', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'CG', 'Republic of the Congo', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'RE', 'Reunion', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'RO', 'Romania', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'RU', 'Russia', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'RW', 'Rwanda', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'BL', 'Saint Barthelemy', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'SH', 'Saint Helena', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'KN', 'Saint Kitts and Nevis', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'LC', 'Saint Lucia', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'MF', 'Saint Martin', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'PM', 'Saint Pierre and Miquelon', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'VC', 'Saint Vincent and the Grenadines', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'WS', 'Samoa', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'SM', 'San Marino', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'ST', 'Sao Tome and Principe', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'SA', 'Saudi Arabia', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'SN', 'Senegal', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'RS', 'Serbia', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'CS', 'Serbia and Montenegro', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'SC', 'Seychelles', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'SL', 'Sierra Leone', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'SG', 'Singapore', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'SX', 'Sint Maarten', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'SK', 'Slovakia', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'SI', 'Slovenia', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'SB', 'Solomon Islands', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'SO', 'Somalia', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'ZA', 'South Africa', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'GS', 'South Georgia and the South Sandwich Islands', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'KR', 'South Korea', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'SS', 'South Sudan', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'ES', 'Spain', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'LK', 'Sri Lanka', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'SD', 'Sudan', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'SR', 'Suriname', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'SJ', 'Svalbard and Jan Mayen', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'SZ', 'Swaziland', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'SE', 'Sweden', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'CH', 'Switzerland', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'SY', 'Syria', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'TW', 'Taiwan', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'TJ', 'Tajikistan', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'TZ', 'Tanzania', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'TH', 'Thailand', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'TG', 'Togo', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'TK', 'Tokelau', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'TO', 'Tonga', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'TT', 'Trinidad and Tobago', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'TN', 'Tunisia', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'TR', 'Turkey', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'TM', 'Turkmenistan', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'TC', 'Turks and Caicos Islands', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'TV', 'Tuvalu', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'VI', 'U.S. Virgin Islands', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'UG', 'Uganda', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'UA', 'Ukraine', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'AE', 'United Arab Emirates', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'GB', 'United Kingdom', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'US', 'United States', 'City', 'State', 'Zip', '{{ Street1 }}' + @crlf + '{{ Street2 }}' + @crlf + '{{ City }}, {{ State }} {{ Zip }}' UNION ALL
	SELECT 'UM', 'United States Minor Outlying Islands', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'UY', 'Uruguay', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'UZ', 'Uzbekistan', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'VU', 'Vanuatu', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'VA', 'Vatican', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'VE', 'Venezuela', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'VN', 'Vietnam', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'WF', 'Wallis and Futuna', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'EH', 'Western Sahara', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'YE', 'Yemen', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'ZM', 'Zambia', NULL, NULL, NULL, NULL UNION ALL
	SELECT 'ZW', 'Zimbabwe', NULL, NULL, NULL, NULL
	ORDER BY [Country]

OPEN country_cursor

FETCH NEXT FROM country_cursor
INTO @CountryCode, @Country, @CityLabel, @StateLabel, @ZipLabel, @AddressFormat

WHILE @@FETCH_STATUS = 0
BEGIN

    INSERT INTO #codeTable
    SELECT '            RockMigrationHelper.UpdateDefinedValueByName("D7979EA1-44E9-46E2-BF37-DDAF7F741378", "' +
        @CountryCode +  '", "' +
        @Country +  '", ' +
		CAST(@Order AS varchar) + ', false );' + ' // ' + @Country

	SET @Order = @Order + 1

	IF @CityLabel IS NOT NULL
	BEGIN
		INSERT INTO #codeTable
		SELECT '            RockMigrationHelper.AddDefinedValueAttributeValueByName("D7979EA1-44E9-46E2-BF37-DDAF7F741378", "' +
			@CountryCode +  '", "CityLabel", @"' +
			@CityLabel +  '" );' + ' // ' + @Country + ' City Label'
	END
		
	IF @StateLabel IS NOT NULL
	BEGIN
		INSERT INTO #codeTable
		SELECT '            RockMigrationHelper.AddDefinedValueAttributeValueByName("D7979EA1-44E9-46E2-BF37-DDAF7F741378", "' +
			@CountryCode +  '", "StateLabel", @"' +
			@StateLabel +  '" );' + ' // ' + @Country + ' State Label'
	END
		
	IF @ZipLabel IS NOT NULL
	BEGIN
		INSERT INTO #codeTable
		SELECT '            RockMigrationHelper.AddDefinedValueAttributeValueByName("D7979EA1-44E9-46E2-BF37-DDAF7F741378", "' +
			@CountryCode +  '", "PostalCodeLabel", @"' +
			@ZipLabel +  '" );' + ' // ' + @Country + ' PostalCode Label'
	END

	IF @AddressFormat IS NOT NULL
	BEGIN
		INSERT INTO #codeTable
		SELECT '            RockMigrationHelper.AddDefinedValueAttributeValueByName("D7979EA1-44E9-46E2-BF37-DDAF7F741378", "' +
			@CountryCode +  '", "AddressFormat", @"' +
			@AddressFormat +  '" );' + ' // ' + @Country + ' Address Format'
	END

		
	FETCH NEXT FROM country_cursor
	INTO @CountryCode, @Country, @CityLabel, @StateLabel, @ZipLabel, @AddressFormat


END

CLOSE country_cursor
DEALLOCATE country_cursor

SELECT CodeText [Migration] FROM #codeTable

IF OBJECT_ID('tempdb..#codeTable') IS NOT NULL
    DROP TABLE #codeTable