# SqlServerValidationToolkit
A tool for data-validation of SQL Server tables

##What is it?
The SqlServerValidationToolkit allows you to quickly define rules for column-values. You can use the tool to get an overview of invalid valid and to constantly corrrect them. Everytime you run the validation you have a current view of all invalid values.

You can use the tool in a local mode where the metadata are stored locally or in a shared mode where you store them in a SQL Server database that you can access from an SSIS-package.

##Rule types
The following rule-types are available:
- MinMax-rule
- Like-rule with a like-expression
- Comparison-rule to compare the value of one column with the value of another column
- CustomQuery-rule which allows you to define a custom query and define your own error types. The query returns the id of the invalid values and the errorType-code.

##SSIS Integration
You can integrate the validation toolkit in the SSIS-package. 
- Prepare the validation toolkit: In the SqlServerValidationToolkit.Configurator.exe.config-file go to the bottom of the file. Set the "StoreMetadataInSqlServer"-setting to "True" and in the "MetadataDatabaseConnectionString" set the connection string to the metadata-database.
- Define the rules for the validation as in the local mode.
- Integrate the tool into SSIS: In the project add an "Execute Process" Task and set "-v" in the Arguments-row.  
- Handle the invalid data: For example if you want to import the data into a data warehouse you can access the view [Validation_EntriesWithWrongValues] from the metadata database to access the Id-values of the invalid entries to exclude them from the import.

##What are the alternatives?
- A simple way is to use SSIS with lookup transformations and conditional splits to weed out invalid data. This is a standard approach but if you have a lot of tables to validate, the validation logic is spread over many places. Also it means that you have to change and redeploy the SSIS package when the validation logic changes. This can be time consuming. The validation toolkit extracts the validation logic into an external component.
- The "Data validation transform"-component from PragmaticsWorks (http://pragmaticworks.com/Products/Features/Feature/DataValidationTransform) can validate regular expressions. 
- Data Quality services (https://msdn.microsoft.com/en-us/library/ff877917.aspx) is the service from Microsoft to validate data. The validation toolkit is a simple alternative.

##Where can I use it?
In a perfect world you would not need this tool. The applications would all validate the input-data themselves and give the user immediate feedback if one of the rules is broken. However sometimes applications don't validate or they don't validate the right thing and after a while your boss comes and sais the existing data needs to be shipped to a data warehouse. What do you do? You validate yourself. You write all SQL-queries that filter the wrong values in the ETL-process or you correct them one by one. 

To help you in this process the tool was written. If you use it, you will not need to write so many SQL queries and keep track of all the wrong values. The tool does that for you. You can also copy the generated SQL queries and use them in your ETL-process. And with the customQuery-rule you can validate very complex situations.

##Example

Below you see the configuration for a MinMax-rule for the column "Length" of the table "Babies". Valid values are between 0 and 60. Null values should be interpreted as error:
![Configuration](https://cloud.githubusercontent.com/assets/3718526/8893479/cd6ee726-3391-11e5-8ddd-916545af51c4.png)

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
- ConvertToDefaultValue: Values are converted to 0 for int-columns, an empty string for string-columns and the current time  for datetime-columns

After the validation you can see all invalid values in the "WrongValues"-tab:
![wrongValues](https://cloud.githubusercontent.com/assets/3718526/8893464/23790f1c-3391-11e5-9ccc-0c2db82a95c2.png)

##Why are the imported tables called Source?
Because I would like to extend the tool so that you can also validate views in a future version.

