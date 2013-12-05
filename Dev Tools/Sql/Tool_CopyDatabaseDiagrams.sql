/* SQL to help copy a diagram from one Rock Database to another */

USE [RockChMS_dev01] /* <-- set db connection to the database you want to copy the diagrams to */
GO

SET IDENTITY_INSERT sysdiagrams ON 
GO

/* run this to clean out current diagrams from target database
DELETE FROM sysdiagrams
GO
*/


INSERT sysDiagrams (name, principal_id, diagram_id, version, definition)
             select name, principal_id, diagram_id, version, definition
               from 
             [RockChMS_diagram01].dbo.sysdiagrams  /* <-- Set me to the database you want to copy the models from */
   