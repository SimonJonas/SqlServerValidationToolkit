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
        SqlServerValidationToolkitContext _ctx;
        string _connectionString;

        public Validator()
        {
            string decryptedConnectionString = GetDecryptedConnectionString();
            _connectionString = decryptedConnectionString;
            _ctx = SqlServerValidationToolkitContext.Create(_connectionString);
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
            return _ctx.WrongValues.Where(wrongValue =>
                wrongValue.Validation_ValidationRule.Column == column
                &&
                wrongValue.Errortype == errorType);
        }

        public void LoadSources()
        {
            _ctx.Sources.ToList();
        }

        public ObservableCollection<Source> Sources
        {
            get
            {
                return _ctx.Sources.Local;
            }
        }

        public void Add(Source newSource)
        {
            _ctx.Sources.Add(newSource);
        }

        public void Remove(Source source)
        {
            if (source.Source_id != 0)
            {
                _ctx.Sources.Remove(source);
            }
        }

        public void Save(IEnumerable<Source> sources)
        {
            _ctx.SaveChanges();
            foreach (var s in this.Sources)
            {
                _ctx.Entry(s).Reload();
            }
        }


        public void Remove(Column column)
        {
            if (column.Column_id != 0)
            {
                _ctx.Columns.Remove(column);
            }
        }

        public void ExecuteValidation()
        {
            _ctx.Validate();
        }

        public IEnumerable<ErrorType> ErrorTypes
        {
            get
            {
                return _ctx.Errortypes.ToList();
            }
        }

        public IEnumerable<WrongValue> WrongValues
        {
            get
            {
                return _ctx.WrongValues.OrderBy(wv => wv.Id).ThenBy(wv => wv.Validation_ValidationRule.Column.Source.Name).ThenBy(wv => wv.Validation_ValidationRule.Column.Name).ToList();
            }
        }

        public bool IsSavable
        {
            get
            {
                return _ctx.ChangeTracker.HasChanges();
            }
        }

        public void Remove(ValidationRule vr)
        {
            _ctx.ValidationRules.Remove(vr);
        }

        public void Refresh()
        {
            _ctx.Dispose();
            _ctx = SqlServerValidationToolkitContext.Create();
            _ctx.WrongValues.ToList();
            _ctx.Errortypes.ToList();
            LoadSources();
        }

        public void AssignErrorTypes(ValidationRule vr)
        {
            ErrorTypeAssigner a = new ErrorTypeAssigner();
            a.LoadErrorTypes(_ctx);
            a.AddErrorTypes(vr);
        }

        public void Dispose()
        {
            _ctx.Dispose();
        }

        public void Uninstall()
        {
            string c = Resources.UninstallToolkit;
            _ctx.Database.ExecuteSqlCommand(c);
        }
    }
}
