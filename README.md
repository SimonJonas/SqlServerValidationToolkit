# SqlServerValidationToolkit
A tool for data-validation of SQL Server tables

##What it is

The tool is a small tool that allows to quickly define rules for column-values. You can use the tool to get an overview of invalid valid and to constantly corrrect them. Everytime you run the validation you have a current view of all invalid values.

It is easy to set up. Just run the exe and define the database to validate. Then import the metadata for the tables you want to validate, define rules and execute the validation. 

The metadata is stored in a local database. The connection string is stored in the database in encrypted form. It can only be read from the local computer.

The following rule-types are available:
- MinMax-rule
- Like-rule with a like-expression
- Comparison-rule to compare the value of one column with the value of another column
- CustomQuery-rule which allows you to define a custom query and define your own error types. The query returns the id of the invalid values and the errorType-code.


##Example

Below you see the configuration for a MinMax-rule for the column "Length" of the table "Babies". Valid values are between 0 and 60. Null values should be interpreted as error:
![Configuration](https://cloud.githubusercontent.com/assets/3718526/8893417/ba56b31e-338f-11e5-83bb-e7341258b392.png)

The rule in the screenshot creates the following query:
```sql
SELECT BabyID,CASE WHEN (Length < 10) THEN 'TooLow' WHEN (60 < Length) THEN 'TooHigh' WHEN (Length IS NULL) THEN 'NotEntered' END AS ErrorType_code
	FROM 
	Babies
	WHERE 
    (Length < 10 OR 60 < Length)
	OR Length IS NULL
```

For the null values there are three strategies:
- Ignore: Null-values are not validated
- InterpretAsError: Null-values have a special code 'NotEntered'
- ConvertToDefaultValue: Values are converted to the default value of the column

After the validation you can see all invalid values in the "WrongValues"-tab:
![wrongValues](https://cloud.githubusercontent.com/assets/3718526/8893464/23790f1c-3391-11e5-9ccc-0c2db82a95c2.png)
