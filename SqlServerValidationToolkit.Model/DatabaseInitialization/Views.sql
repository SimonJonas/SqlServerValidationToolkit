CREATE VIEW Validation_ColumnsWithWrongValues
AS
SELECT 
	DISTINCT -- distinct because the entry could contain multiple wrong values
	wv.[Id],
	c.Name as columnName,
	s.Name as sourceName
  FROM [dbo].[Validation_WrongValue] wv
  JOIN dbo.Validation_ValidationRule vr ON wv.ValidationRule_fk = vr.ValidationRule_id
  JOIN dbo.Validation_Column c ON vr.Column_fk = c.Column_id
  JOIN dbo.Validation_Source s ON c.Source_fk = s.Source_id
  --only show values that were not ignored by the user
  WHERE wv.Ignore=0
