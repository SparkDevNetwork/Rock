/* General purpose adhoc SQLs for looking at column metadata */

/* show the length of the various NVARCHAR columns.  Handy for ensuring consistency */
select table_name, column_name, CHARACTER_MAXIMUM_LENGTH from INFORMATION_SCHEMA.COLUMNS where DATA_TYPE = 'nvarchar' order by COLUMN_NAME 

/* show the name of the various columns. Handy for examining consistency */
select column_name, TABLE_NAME from INFORMATION_SCHEMA.COLUMNS order by COLUMN_NAME 
