CREATE PROC [dbo].[cust_rock_sp_update_all]

AS

PRINT 'Updating Defined Types...'
EXEC [cust_rock_sp_update_defined_type_all]

PRINT 'Updating Defined Values...'
EXEC [cust_rock_sp_update_defined_value_all]

PRINT 'Updating Attributes...'
EXEC [cust_rock_sp_update_attribute_all]

PRINT 'Updating People...'
EXEC [cust_rock_sp_update_person_all]

PRINT 'Updating Attribute Values...'
EXEC [cust_rock_sp_update_attribute_value_all]

