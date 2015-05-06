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

namespace SqlServerValidationToolkit.Configurator.Controls.UpdateEntities
{
    class UpdateEntitiesViewViewModel
    {
        private MainWindowViewModel _mainWindowViewModel;

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
            }
            catch (DbEntityValidationException e)
            {
                Messenger.Default.Send<ValidationErrorMessage>(new ValidationErrorMessage()
                {
                    Exception = e
                });
            }

        }

        private void UpdateSources()
        {
            using (var ctx = new SqlServerValidationToolkitContext(Settings.Default.DbConnectionString))
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required))
                {
                    UpdateSources(ctx);
                    ctx.SaveChanges();
                    scope.Complete();
                }
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
            var existingSource = ctx.Sources
                .Include((s) => s.Columns)
                .SingleOrDefault(s => s.Name.Equals(table.Name));
            if (existingSource == null)
            {
                string pkColumnName = GetPrimaryKeyColumnName(ctx, table.Name);
                existingSource = new Source()
                {
                    Name = table.Name,
                    IdColumnName = pkColumnName
                };
                ctx.Sources.Add(existingSource);

            }
            return existingSource;
        }

        private static string GetPrimaryKeyColumnName(SqlServerValidationToolkitContext ctx, string name)
        {
            string query = @"SELECT column_name
FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
WHERE OBJECTPROPERTY(OBJECT_ID(constraint_name), 'IsPrimaryKey') = 1
AND table_name = '{0}'";
            string sql = string.Format(query, name);
            //TODO: Test for views
            string pkName = ctx.Database.SqlQuery<string>(sql).SingleOrDefault();
            if (pkName == null)
            {
                throw new ArgumentException(string.Format("No primary Key is defined for {0}", name));
            }
            return pkName;
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

        public void Init()
        {

            using (var ctx = new SqlServerValidationToolkitContext(Settings.Default.DbConnectionString))
            {
                var adapter = (IObjectContextAdapter)ctx;
                var connection = ((EntityConnection)adapter.ObjectContext.Connection).StoreConnection;
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

                tables.Add(GetTable(connection, row));
            }
            connection.Close();
            return tables;
        }

        private static TableViewModel GetTable(DbConnection connection, DataRow row)
        {
            var t = new TableViewModel(row[2].ToString());

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
                //if (reader.IsDBNull(3))
                //{

                //}
                //var varCharMaxLenght = reader.GetInt32(3);
                //var numericPrecition = reader.GetInt32(4);
                //var numericScale = reader.GetInt32(5);

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
