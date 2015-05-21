# SqlServerValidationToolkit
A tool for data-validation of SQL Server tables

It is a small tool that allows to quickly define rules for column-values. The following rule-types are available:
- MinMax-rule
- Like-rule with a like-expression
- Comparison-rule to compare the value of one column with the value of another column
- CustomQuery-rule which allows you to define a custom query and define your own error types. The query returns the id of the invalid values and the errorType-id.

After defining the rule, you can view all wrong values with the error and the id. You can then either correct the value in the database or ignore it. Ignored values are filtered by default.

The tool is easy to set up. Just run the exe and define the database to install the toolkit. Then import the tables you want to validate, define rules and execute the validation. 

The toolkit will install 8 tables and 3 stored procedures, each starting with "Validation_". After installing, you can also uninstall the tables and stored procedures via the toolkit configuration with the Settings->Uninstall-command.
