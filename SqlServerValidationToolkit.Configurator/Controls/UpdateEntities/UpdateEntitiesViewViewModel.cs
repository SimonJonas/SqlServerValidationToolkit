using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Data.Entity;
using System.Data.Entity.Validation;
using GalaSoft.MvvmLight.Messaging;
using System.Transactions;
using SqlServerValidationToolkit.Model;
using SqlServerValidationToolkit.Configurator.Properties;
using SqlServerValidationToolkit.Configurator.Controls.Columns;
using System.Text.RegularExpressions;
using SqlServerValidationToolkit.Model.Context;
using SqlServerValidationToolkit.Model.Entities;
using SqlServerValidationToolkit.Model.Validation;
using log4net;

namespace SqlServerValidationToolkit.Configurator.Controls.UpdateEntities
{
    class UpdateEntitiesViewViewModel
    {
        private ILog _log = LogManager.GetLogger(typeof(UpdateEntitiesViewViewModel));
        private MainWindowViewModel _mainWindowViewModel;

        public UpdateEntitiesViewViewModel()
        {
        }

        public UpdateEntitiesViewViewModel(MainWindowViewModel mainWindowViewModel)
        {
            UpdateSourcesCommand = new RelayCommand(UpdateSourcesAndRefreh);
            _mainWindowViewModel = mainWindowViewModel;
        }

        public ICommand UpdateSourcesCommand { get; set; }

        private void UpdateSourcesAndRefreh()
        {
            try
            {

                UpdateSources();

                _mainWindowViewModel.Init();

                if (OnSourceUpdated!=null)
                {
                    OnSourceUpdated();
                }
            }
            catch (DbEntityValidationException e)
            {
                Messenger.Default.Send<ValidationErrorMessage>(new ValidationErrorMessage()
                {
                    Exception = e
                });
            }

        }

        public Action OnSourceUpdated;

        private void UpdateSources()
        {
            using (var ctx = SqlServerValidationToolkitContext.Create())
            {
                ctx.Database.Connection.Open();
                using (var scope = new TransactionScope(TransactionScopeOption.Required))
                {
                    UpdateSources(ctx);
                    ctx.SaveChanges();
                    scope.Complete();
                }
                ctx.Database.Connection.Close();
            }
        }

        private void UpdateSources(SqlServerValidationToolkitContext ctx)
        {
            foreach (var table in Tables.Where(t => t.IsSelected))
            {
                var source = GetSource(ctx, table);
                UpdateColumns(table, source);
            }
        }

        private static Source GetSource(SqlServerValidationToolkitContext ctx, TableViewModel table)
        {
            var db = ctx.Databases
                .Include((d) => d.Sources)
                .Include((d) => d.Sources.Select(s=>s.Columns)).Single();
            var existingSource = db.Sources
                
                .SingleOrDefault(s => s.Name.Equals(table.Name));
            if (existingSource == null)
            {

                string pkColumnName = table.PrimaryKeyColumnName; 
                existingSource = new Source()
                {
                    IdColumnName = pkColumnName
                };
                existingSource.SetName(ctx.Database.Connection.Database, table.Schema, table.Name);
                db.Sources.Add(existingSource);

            }
            return existingSource;
        }

        private static string GetPrimaryKeyColumnName(DbConnection connection, string name)
        {
            string query = @"SELECT column_name
FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
WHERE OBJECTPROPERTY(OBJECT_ID(constraint_name), 'IsPrimaryKey') = 1
AND table_name = '{0}'";
            string sql = string.Format(query, name);
            //TODO: Test for views
            var commands = connection.CreateCommand();
            commands.CommandText = sql;

            try
            {
                var columnName = commands.ExecuteScalar();
                if (columnName==null)
                {
                    return null;
                }
                return columnName.ToString();
            } catch (Exception e)
            {
                return null;
            }
        }

        private void UpdateColumns(TableViewModel table, Source source)
        {
            foreach (var column in table.Columns)
            {
                var existingColumn = source.Columns.SingleOrDefault(c => c.Name == column.Name);
                if (existingColumn == null && column.Name != source.IdColumnName)
                {
                    var newColumn = CreateColumn(column);


                    if (ColumnTypeOptions.ColumnTypes.Contains(newColumn.Type))
                    {
                        source.Columns.Add(newColumn);
                    }
                    else
                    {
                        //Log not added column
                        _log.WarnFormat("The column '{0}' cannot be imported because it's type '{1}' is not supported to import", column.Name, column.SqlType);
                    }
                }
                else
                {
                    CheckForChanges(existingColumn, column);
                }

            }
        }

        private void CheckForChanges(Column existingColumn, ColumnViewModel column)
        {
            //TODO: Implement
        }

        private Column CreateColumn(ColumnViewModel column)
        {
            string type = GetType(column.SqlType);

            return new Column()
            {
                Name = column.Name,
                Type = type,

            };
        }

        /// <summary>
        /// Gets the mapped type of the validation-toolkit
        /// </summary>
        /// <param name="sqlType">^sql-column-type</param>
        /// <returns>validation-toolkit-type</returns>
        private static string GetType(string sqlType)
        {
            if (sqlType.StartsWith("varchar") || sqlType.StartsWith("nvarchar"))
            {
                return "string";
            }
            if (sqlType.StartsWith("char") || sqlType.StartsWith("nchar"))
            {
                return "string";
            }
            if (sqlType.Equals("bit"))
            {
                return "int"; //TODO: Support bit
            }
            if (sqlType.Equals("bigint"))
            {
                return "int";
            }

            return sqlType;
        }

        public void Init(string connectionString)
        {
            using (var connection = SqlServerValidationToolkitContext.CreateConnection(connectionString))
            {
                Tables = GetTables(connection).OrderBy(t => t.Name).ToList();
            }
        }

        public List<TableViewModel> Tables { get; set; }


        private static List<TableViewModel> GetTables(DbConnection connection)
        {
            connection.Open();
            DataTable schema = connection.GetSchema("Tables");
            List<TableViewModel> tables = new List<TableViewModel>();
            foreach (DataRow row in schema.Rows)
            {
                TableViewModel tableViewModel = GetTable(connection, row);
                if (tableViewModel != null)
                {
                    tables.Add(GetTable(connection, row));
                }
            }
            connection.Close();
            return tables;
        }

        private static TableViewModel GetTable(DbConnection connection, DataRow row)
        {
            string name = row[2].ToString();
            string schema = row[1].ToString();
            var t = new TableViewModel(schema, name);
            t.PrimaryKeyColumnName = GetPrimaryKeyColumnName(connection, name);

            if (t.PrimaryKeyColumnName==null)
            {
                return null;
            }

            var c = connection.CreateCommand();

            string sql = string.Format(@"SELECT 
	COLUMN_NAME, 
	IS_NULLABLE, --NO/YES 
	DATA_TYPE, --int/datetime/varchar 
	CHARACTER_MAXIMUM_LENGTH, --for varchar: -1 = MAX
	NUMERIC_PRECISION, -- for numeric
	NUMERIC_SCALE -- for numeric
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = N'{0}'", t.Name);

            c.CommandText = sql;
            var reader = c.ExecuteReader();
            while (reader.Read())
            {
                string clName = reader.GetValue(0).ToString();
                var nullable = reader.GetValue(1).ToString().Equals("YES");
                string dataType = reader.GetValue(2).ToString();

                var col = new ColumnViewModel()
                {
                    Name = clName,
                    Nullable = nullable,
                    SqlType = dataType //TODO: include additional info
                };
                t.Columns.Add(col);
            }

            reader.Close();

            return t;
        }
    }
}
