
IF (EXISTS(SELECT * FROM [__MigrationHistory]))
BEGIN
	DROP TABLE [dbo].[__MigrationHistory]
END

DROP TABLE [dbo].[Validation_TempId]

DROP TABLE [dbo].[Validation_WrongValue]


DROP TABLE [dbo].[Validation_ValidationRule_ErrorType]

DROP TABLE [dbo].[Validation_Errortype]

DROP TABLE [dbo].[Validation_ValidationRule]

DROP TABLE [dbo].[Validation_Column]


DROP TABLE [dbo].[Validation_Log]

DROP TABLE [dbo].[Validation_Source]

--drop stored procedures
DROP PROCEDURE [dbo].[Validation_USP_ExecuteValidation]

DROP PROCEDURE [dbo].[Validation_USP_Fill_and_Update_WrongValues]
DROP PROCEDURE [dbo].[Validation_USP_FillWrongValues]
DROP PROCEDURE [dbo].[Validation_USP_Update_corrected_Values]
