using SqlServerValidationToolkit.Model.Context;
using SqlServerValidationToolkit.Model.Entities;
using SqlServerValidationToolkit.Model.Entities.Rule;
using SqlServerValidationToolkit.Model.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerValidationToolkit.Model.Validation
{
    /// <summary>
    /// Provides access to the validation-procedures
    /// </summary>
    public class Validator : IDisposable
    {
        SqlServerValidationToolkitContext _ctxLocalDb;
        string _connectionStringSqlServer;

        public Validator()
        {
            string decryptedConnectionString = GetDecryptedConnectionString();
            _connectionStringSqlServer = decryptedConnectionString;
            _ctxLocalDb = SqlServerValidationToolkitContext.Create();
            LoadSources();
        }

        public static string GetDecryptedConnectionString()
        {
            string decryptedConnectionString;
            using (var ctx = SqlServerValidationToolkitContext.Create())
            {
                var database = ctx.Databases.Single();
                using (var decryptedSecureString = database.EncryptedConnectionString.DecryptString())
                {
                    decryptedConnectionString = decryptedSecureString.ToInsecureString();
                }
            }
            return decryptedConnectionString;
        }

        /// <summary>
        /// Returns the wrong entries for the column and the errortype
        /// </summary>
        /// <param name="column">the column</param>
        /// <param name="errorType">the error-type</param>
        /// <returns>wrong Entries</returns>
        public IEnumerable<WrongValue> GetWrongEntries(Column column, ErrorType errorType)
        {
            return _ctxLocalDb.WrongValues.Where(wrongValue =>
                wrongValue.Validation_ValidationRule.Column == column
                &&
                wrongValue.Errortype == errorType);
        }

        public void LoadSources()
        {
            _ctxLocalDb.Sources.ToList();
        }

        public ObservableCollection<Source> Sources
        {
            get
            {
                return _ctxLocalDb.Sources.Local;
            }
        }

        public void Add(Source newSource)
        {
            _ctxLocalDb.Sources.Add(newSource);
        }

        public void Remove(Source source)
        {
            if (source.Source_id != 0)
            {
                _ctxLocalDb.Sources.Remove(source);
            }
        }

        public void Save(IEnumerable<Source> sources)
        {
            _ctxLocalDb.SaveChanges();
            foreach (var s in this.Sources)
            {
                _ctxLocalDb.Entry(s).Reload();
            }
        }


        public void Remove(Column column)
        {
            if (column.Column_id != 0)
            {
                _ctxLocalDb.Columns.Remove(column);
            }
        }

        public void ExecuteValidation()
        {
            using (var ctxSqlServer = SqlServerValidationToolkitContext.Create(_connectionStringSqlServer))
            {
                ctxSqlServer.Validate();
            }
        }

        public IEnumerable<ErrorType> ErrorTypes
        {
            get
            {
                return _ctxLocalDb.Errortypes.ToList();
            }
        }

        public IEnumerable<WrongValue> WrongValues
        {
            get
            {
                return _ctxLocalDb.WrongValues.OrderBy(wv => wv.Id).ThenBy(wv => wv.Validation_ValidationRule.Column.Source.Name).ThenBy(wv => wv.Validation_ValidationRule.Column.Name).ToList();
            }
        }

        public bool IsSavable
        {
            get
            {
                return _ctxLocalDb.ChangeTracker.HasChanges();
            }
        }

        public void Remove(ValidationRule vr)
        {
            _ctxLocalDb.ValidationRules.Remove(vr);
        }

        public void Refresh()
        {
            _ctxLocalDb.Dispose();
            _ctxLocalDb = SqlServerValidationToolkitContext.Create();
            _ctxLocalDb.WrongValues.ToList();
            _ctxLocalDb.Errortypes.ToList();
            LoadSources();
        }

        public void AssignErrorTypes(ValidationRule vr)
        {
            ErrorTypeAssigner a = new ErrorTypeAssigner();
            a.LoadErrorTypes(_ctxLocalDb);
            a.AddErrorTypes(vr);
        }

        public void Dispose()
        {
            _ctxLocalDb.Dispose();
        }

        public void Uninstall()
        {
            string c = Resources.UninstallToolkit;
            using (var ctxSqlServer = SqlServerValidationToolkitContext.Create(_connectionStringSqlServer))
            {
                ctxSqlServer.Database.ExecuteSqlCommand(c);
            }
        }
    }
}
