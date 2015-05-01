using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServerValidationToolkit.Model.Context;
using SqlServerValidationToolkit.UnitTests.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerValidationToolkit.UnitTests
{
    [TestClass]
    public class ValidationRuleTests
    {
        [TestMethod]
        public void TestCustomConnectionString()
        {
            using (SqlServerValidationToolkitContext entities = new SqlServerValidationToolkitContext("data source=localhost;initial catalog=ValidationToolkit_UnitTest;integrated security=True"))
            {
                entities.Columns.ToList().ForEach(c => Console.WriteLine(c.Column_id));
            }
        }

        [TestMethod]
        public void TestIsActive()
        {
            Initializer initializer = new Initializer();

            using (var ctx = new SqlServerValidationToolkitContext(Settings.Default.ConnectionString))
            {
                initializer.AddValidation(ctx);

                ctx.Validate();

                ctx.ValidationRules.ToList().ForEach(vr => vr.IsActive = false);

                ctx.SaveChanges();

                ctx.Validate();

                Assert.IsTrue(ctx.WrongValues.All(wv => wv.Is_Corrected));
            }

        }
    }
}
