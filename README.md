# SqlServerValidationToolkit
A tool for data-validation of SQL Server tables

It is a small tool that allows to quickly define rules for column-values. You can use the tool to get an overview of invalid valid and to constantly corrrect them. Everytime you run the validation you have a current view of all invalid values.

The following rule-types are available:
- MinMax-rule
- Like-rule with a like-expression
- Comparison-rule to compare the value of one column with the value of another column
- CustomQuery-rule which allows you to define a custom query and define your own error types. The query returns the id of the invalid values and the errorType-code.

After defining the rule, you can view all wrong values with the error and the id. You can then either correct the value in the database or ignore it. Ignored values are filtered by default.

The tool is easy to set up. Just run the exe and define the database to validate. Then import the metadata for the tables you want to validate, define rules and execute the validation. 

The metadata is stored in a local database. The connection string is stored in the database in encrypted form. It can only be read from the local computer.

