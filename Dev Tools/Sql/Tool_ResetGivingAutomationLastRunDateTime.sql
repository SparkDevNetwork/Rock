-- Reset Last Time for Giving Classications so that the GivingAutomation job will rebuild all Giving Classifications
-- This is handy if you populate FinancialTransactions with dates that are older than this LastRunDateTime 
-- After running this script, you'll have to Refresh the cache or restart RockWeb.
UPDATE [Attribute]
SET [DefaultValue] = JSON_MODIFY([DefaultValue], '$.GivingClassificationSettings.LastRunDateTime', '')
WHERE [Key] = 'core_GivingAutomationConfiguration'



