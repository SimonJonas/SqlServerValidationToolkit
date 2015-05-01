
------------------------------------------------------
------------------------------------------------------
----------------Procedures
------------------------------------------------------
------------------------------------------------------








---------------------------------------------------------------[Validation_USP_Update_corrected_Values]------------------------------------------------------------------

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.[Validation_USP_Update_corrected_Values]') AND type in (N'P'))
DROP PROCEDURE dbo.[Validation_USP_Update_corrected_Values];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Simon Gubler
-- Create date: 01. 09. 2011
-- Description:	Updates the WrongValue-Table with corrected values. Assumes that TempId has been filled with the wrong entries
-- =============================================
CREATE PROCEDURE [Validation_USP_Update_corrected_Values] 
	@ValidationRuleId int

AS
BEGIN


	--Automatically correct Values that have been corrected for this column and for this errorgroup
	UPDATE wv 
	SET [Is_Corrected]=1
	FROM
	[Validation_WrongValue] wv
	JOIN Validation_ValidationRule vr ON wv.ValidationRule_fk = vr.ValidationRule_id
	JOIN Validation_Column c ON c.Column_id = vr.Column_fk
	WHERE
	vr.ValidationRule_id = @ValidationRuleId
	AND [Is_Corrected] = 0
	AND 
	(
		--If the entry-Id is in [Validation_TempId], then the entry is still uncorrected
		[Id] NOT IN (
	
			--All TempIds with the same Error, for example "too small"
			SELECT [Id] FROM [Validation_TempId] tmp 
			WHERE 
				tmp.[ErrorType_fk] = wv.ErrorType_fk AND 
				tmp.ValidationRule_fk = @ValidationRuleId
		)
		OR
		--if the rule has become inactive, the wrong entry is corrected
		vr.IsActive=0
	)

END

GO




---------------------------------------------------------------[Validation_USP_FillWrongValue]------------------------------------------------------------------

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.[Validation_USP_FillWrongValues]') AND type in (N'P'))
DROP PROCEDURE dbo.[Validation_USP_FillWrongValues];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Simon Gubler
-- Create date: 01. 09. 2011
-- Description:	Fills the WrongValue-Table with new wrong entries. 
-- =============================================
CREATE PROCEDURE [Validation_USP_FillWrongValues] 
	@ValidationRuleId int,
	@Log_id bigint

AS
BEGIN

--Is used to format the Date accordingly
--for more information, see: http://msdn.microsoft.com/en-us/library/ms187928.aspx
DECLARE @DateFormatNumberInConvert varchar(10)
SELECT @DateFormatNumberInConvert = CASE WHEN c.[Type] LIKE '%date%' THEN
	',113'
ELSE
	''
END
FROM Validation_Column c 
JOIN Validation_ValidationRule vr ON vr.Column_fk=c.Column_id
WHERE vr.ValidationRule_id=@ValidationRuleId

DECLARE @Select_for_WrongValues varchar(MAX)

--the query joins the TempId-table with the source based on the Id
--where no uncorrected wrongValue exists with the same errorType, column, id and errorGroup.
SELECT @Select_for_WrongValues = 'SELECT
temp.ValidationRule_fk
, [Id]
, CONVERT(varchar(MAX),'+c.Name+@DateFormatNumberInConvert+')
, ' + CASE 
			WHEN (vr.discriminator='Comparison') 
		THEN 
			'CONVERT(varchar(MAX),'+vr.ComparedColumn+@DateFormatNumberInConvert+')' 
		ELSE 
			'NULL' 
		END + '
, [ErrorType_fk]
, ' + CONVERT(varchar(30),@Log_id) + '
, 0 AS lCorrected, 0 as lIgnore
FROM [Validation_TempId] AS temp JOIN ['+s.Name+'] ON (temp.[Id]=['+s.Name+'].['+s.Id_Name+'])' + 
' WHERE NOT EXISTS(
	SELECT * FROM [Validation_WrongValue] AS wrong
	WHERE wrong.[ValidationRule_fk]=temp.[ValidationRule_fk] AND wrong.[ErrorType_fk]=temp.[ErrorType_fk] AND wrong.[Id]='+s.Id_Name+' AND wrong.[Is_Corrected]=0 
)'
FROM
Validation_Source s 
JOIN Validation_Column c ON c.Source_fk=s.Source_id
JOIN Validation_ValidationRule vr ON vr.Column_fk = c.Column_id
WHERE vr.ValidationRule_id=@ValidationRuleId

print 'insert into WrongValues'
--SELECT @Select_for_WrongValues

DECLARE @InsertStatement varchar(MAX) = 'INSERT INTO [Validation_WrongValue] (ValidationRule_fk, Id, Value, Value2, ErrorType_fk, Log_id, Is_Corrected, Ignore) ' + @Select_for_WrongValues
BEGIN TRY
	-- We insert 
	-- - the ValidationRule-Id
	-- - The Value and the additional values
	-- - The Errortype
	-- - The Log-Entry
	-- - A 0 to indicate that the Entry is not yet corrected, since it is new
	-- We only insert Entries that are not already in WrongValues
	EXEC (@InsertStatement)
END TRY
BEGIN CATCH


    DECLARE @ErrorMessage NVARCHAR(4000) = 'Error while inserting Wrong values: '
    DECLARE @ErrorSeverity INT;
    DECLARE @ErrorState INT;


	DECLARE @ret_string varchar (255)

	--format sttring
	SELECT @ErrorMessage = 
	'Error while inserting Wrong values for source '+s.Name+', column '+c.Name+', validationrule '+vr.discriminator
	FROM 
	Validation_Source s
	JOIN Validation_Column c ON s.Source_id=c.Source_fk
	JOIN Validation_ValidationRule vr ON vr.Column_fk=c.Column_id
	WHERE vr.ValidationRule_id = @ValidationRuleId

    SELECT 
        @ErrorMessage = @ErrorMessage+', with the statement ('+@InsertStatement+')'
	
    SELECT 
        @ErrorMessage = @ErrorMessage+ERROR_MESSAGE(),
        @ErrorSeverity = ERROR_SEVERITY(),
        @ErrorState = ERROR_STATE();

    -- Use RAISERROR inside the CATCH block to return error
    -- information about the original error that caused
    -- execution to jump to the CATCH block.
    RAISERROR (@ErrorMessage, -- Message text.
               @ErrorSeverity, -- Severity.
               @ErrorState -- State.
               );
END CATCH;



END

GO




---------------------------------------------------------------[Validation_USP_ExecuteValidation]------------------------------------------------------------------

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.[Validation_USP_ExecuteValidation]') AND type in (N'P'))
DROP PROCEDURE dbo.[Validation_USP_ExecuteValidation];
GO

-- =============================================
-- Author:		Simon Gubler
-- Create date: 25. 8. 2011
-- Description:	Adds a log-entry, checks all Columns and fills the WrongValues-Table
-- =============================================
CREATE PROCEDURE dbo.[Validation_USP_ExecuteValidation]
AS
BEGIN
	declare @Log_id int

	
	INSERT INTO [dbo].[Validation_Log]
			   ([executedAt])
		 VALUES
			   (GETDATE())

	SELECT @Log_id=@@IDENTITY

	
	DECLARE @Query varchar(max)
	DECLARE @ValidationRuleId int

	TRUNCATE TABLE [Validation_TempId]

	DECLARE queries CURSOR FOR  
			SELECT 
			vr.ValidationRule_id,
			vr.CompiledQuery
			FROM 
			[Validation_ValidationRule] as vr  
		
	OPEN queries  
	FETCH NEXT FROM queries INTO @ValidationRuleId, @Query
	WHILE @@FETCH_STATUS = 0  
	BEGIN  
		
			DECLARE @Query_with_ErrorType varchar(Max)
			SELECT  @Query_with_ErrorType = 'SELECT q.*, '+CAST(@ValidationRuleId AS VARCHAR(MAX))+' AS ValidationRule_fk FROM ('+@Query+') as q'
		
			BEGIN TRY
				-- Insert the temporary Id's 
			
				INSERT [Validation_TempId] ([Id], [ErrorType_fk], [ValidationRule_fk])
				EXEC(@Query_with_ErrorType) --the compiled query already contains the errorType

			END TRY
			BEGIN CATCH


				DECLARE @ErrorMessage NVARCHAR(4000) = 'Error while inserting temporary ids with the statement ('+@Query_with_ErrorType+')'
				DECLARE @ErrorSeverity INT;
				DECLARE @ErrorState INT;

				SELECT 
					@ErrorMessage = @ErrorMessage+ERROR_MESSAGE(),
					@ErrorSeverity = ERROR_SEVERITY(),
					@ErrorState = ERROR_STATE();

				-- Use RAISERROR inside the CATCH block to return error
				-- information about the original error that caused
				-- execution to jump to the CATCH block.
				RAISERROR (@ErrorMessage, -- Message text.
						   @ErrorSeverity, -- Severity.
						   @ErrorState -- State.
						   );
			END CATCH;

		
			EXEC [Validation_USP_Fill_and_Update_WrongValues] @ValidationRuleId, @Log_id
		
			TRUNCATE TABLE [Validation_TempId]
		FETCH NEXT FROM queries INTO @ValidationRuleId, @Query

	END  

	CLOSE queries  
	DEALLOCATE queries 

		
	--We delete the corrected values to keep the primary key constraint
	--In WrongValues we have only one value for each combination 
	--of Column, Id and Errortype.

	--We could add lCorrected to the primary key, then it could
	--work to leave the corrected values but this adds to the
	--complexity and is not needed
	DELETE [Validation_WrongValue] WHERE [Is_Corrected] = 1

END

GO


---------------------------------------------------------------[Validation_USP_Fill_and_Update_WrongValues]------------------------------------------------------------------

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.[Validation_USP_Fill_and_Update_WrongValues]') AND type in (N'P'))
DROP PROCEDURE dbo.[Validation_USP_Fill_and_Update_WrongValues];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Simon Gubler
-- Create date: 26. 8. 2011
-- Description:	Fills TempId-Table with Entries that have a NULL-Value and fills the the WrongValues-Table with Tuples out of the TempId - Table, then deletes the TempId-Table
-- =============================================
CREATE PROCEDURE [Validation_USP_Fill_and_Update_WrongValues] 
	@ValidationRuleId int,
	@Log_id bigint

AS
BEGIN

--first the wrong values are inserted, then the values are corrected because
--the wrong values of invalid rules are also inserted and corrected
EXEC [Validation_USP_FillWrongValues] @ValidationRuleId, @Log_id
EXEC [Validation_USP_Update_corrected_Values] @ValidationRuleId;




END


GO











