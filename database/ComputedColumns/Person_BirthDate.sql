ALTER TABLE rms.dbo.Person
	DROP COLUMN BirthDate

ALTER TABLE rms.dbo.Person
	ADD BirthDate AS DATEFROMPARTS(BirthYear,BirthMonth,BirthDay) PERSISTED
