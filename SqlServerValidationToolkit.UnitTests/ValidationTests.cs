using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServerValidationToolkit.Model.Context;
using SqlServerValidationToolkit.Model.DatabaseInitialization;
using SqlServerValidationToolkit.Model.Entities;
using SqlServerValidationToolkit.Model.Entities.Rule;
using SqlServerValidationToolkit.UnitTests.Properties;
using SqlServerValidationToolkit.UnitTests.TestDatabase;
using SqlServerValidationToolkit.UnitTests.TestDatabase.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerValidationToolkit.UnitTests
{
    [TestClass]
    public class ValidationTests
    {
        [TestInitialize]
        public void Initialize()
        {

            
            
            Initializer initializer = new Initializer();


            using (var ctx = new SqlServerValidationToolkitContext(Settings.Default.ConnectionString))
            {
                initializer.AddValidation(ctx);

                ctx.Database.Initialize(false);

                //var ctxBabies = new TestDatabaseContext(Settings.Default.ConnectionString);
                //ctxBabies.Database.Initialize(false);

                ctx.Validate();
            }
        }

        [TestMethod]
        public void CorrectMultipleValues()
        {
            var ctx = new SqlServerValidationToolkitContext(Settings.Default.ConnectionString);

            Assert.IsTrue(WrongLengthEntriesExisting(ctx));

            var ctxBabies = new TestDatabaseContext(Settings.Default.ConnectionString);

            CorrectLength(ctxBabies);
            ctx.Validate();

            Assert.IsFalse(WrongLengthEntriesExisting(ctx));
        }

        [TestMethod]
        public void CorrectSingleValue()
        {

            var firstWrongWeightBabyId = ApplyCorrection(
                (wrongValue) =>
                    wrongValue.Validation_ValidationRule.Column.Name == "Weight",
                (ctxB, baby) =>
                {
                    baby.Weight = 1000;
                    ctxB.SaveChanges();
                });

            AssertWrongValueIsCorrected(firstWrongWeightBabyId, "Weight");
        }

        [TestMethod]
        public void IgnoreWrongValue()
        {

            var ctx = new SqlServerValidationToolkitContext(Settings.Default.ConnectionString);

            var wrongV = (from wV in ctx.WrongValues
                          where wV.Validation_ValidationRule.Column.Name == "Weight"
                          select wV).First();

            //setting lcorrected to true marks the entry as corrected
            wrongV.Ignore = true;

            Console.WriteLine("ignore Errortype {0} on column {1} for Baby {2}", wrongV.ErrorType_fk, wrongV.Validation_ValidationRule.Column.Column_id, wrongV.Id);
            ctx.SaveChanges();

            ctx.Validate();

            var newWrongValue = ctx.WrongValues.Single(wrongValue =>
                    wrongValue.Validation_ValidationRule.Column.Name == "Weight" &&
                    wrongValue.Id == wrongV.Id &&
                    wrongValue.ErrorType_fk == wrongV.ErrorType_fk);

            //the wrong value is still ignored
            Assert.IsTrue(newWrongValue.Ignore);
        }

        /// <summary>
        /// Selects the Baby-Id from the WrongValues-View based on the predicate,
        /// finds the Baby, applies the correction and executes CheckColumn-Procedure
        /// </summary>
        /// <param name="pred">predicate to find the Id of the Baby with the wrong value</param>
        /// <param name="correction">Correction-Action</param>
        /// <returns>Baby-Id</returns>
        private static long ApplyCorrection(Expression<Func<WrongValue, bool>> pred, Action<TestDatabaseContext, Baby> correction)
        {
            var ctx = new SqlServerValidationToolkitContext(Settings.Default.ConnectionString);

            foreach (var wv in ctx.WrongValues)
            {
                Console.WriteLine(wv);
            }

            var firstWrongWeightBabyId = ctx.WrongValues.Where(pred).Select(wrongValue => wrongValue.Id).First();


            var ctxBabies = new TestDatabaseContext(Settings.Default.ConnectionString);
            var babyWithWrongWeight = ctxBabies.Babies.Find(firstWrongWeightBabyId);

            correction(ctxBabies, babyWithWrongWeight);

            ctx.Validate();
            return firstWrongWeightBabyId;
        }

        [TestMethod]
        public void DeleteEntryWithWrongValue()
        {
            var firstWrongWeightBabyId = ApplyCorrection(
                (wrongValue) =>
                    wrongValue.Validation_ValidationRule.Column.Name == "Weight",
                (ctxB, baby) =>
                {
                    ctxB.Babies.Remove(baby);
                    ctxB.SaveChanges();
                });


            AssertWrongValueIsCorrected(firstWrongWeightBabyId, "Weight");
        }

        [TestMethod]
        public void CorrectEntry_SetItToNull_NullIsAllowed()
        {
            var babyId = ApplyCorrection(
                (wrongValue) =>
                    wrongValue.Validation_ValidationRule.Column.Name == "Length",
                (ctxB, baby) =>
                {
                    //setting the length to null will mark the entry as resolved
                    baby.Length = null;
                    ctxB.SaveChanges();
                });

            AssertWrongValueIsCorrected(babyId, "Length");
        }
        private static void AssertWrongValueExists(long babyId, string columnName)
        {
            var ctx = new SqlServerValidationToolkitContext(Settings.Default.ConnectionString);
            Assert.IsTrue(
                ctx.WrongValues.Any(wrongValue =>
                    wrongValue.Validation_ValidationRule.Column.Name == columnName &&
                    wrongValue.Id == babyId), string.Format("No wrong value exists for baby {0} and column {1}", babyId, columnName));
        }

        private static void AssertWrongValueIsCorrected(long babyId, string columnName)
        {
            var ctx = new SqlServerValidationToolkitContext(Settings.Default.ConnectionString);
            Assert.IsFalse(
                ctx.WrongValues.Any(wrongValue =>
                    wrongValue.Validation_ValidationRule.Column.Name == columnName &&
                    wrongValue.Id == babyId));
        }

        [TestMethod]
        public void CheckLikeExpression()
        {
            var ctx = new SqlServerValidationToolkitContext(Settings.Default.ConnectionString);

            Assert.IsTrue(WrongEmailEntriesExisting(ctx));
        }

        [TestMethod]
        public void CorrectLikeExpression()
        {
            var babyId = ApplyCorrection(
                (wrongValue) =>
                    wrongValue.Validation_ValidationRule.Column.Name == "Email",
                (ctxB, baby) =>
                {
                    baby.Email = "corrected@mail.com";
                    ctxB.SaveChanges();
                });

            AssertWrongValueIsCorrected(babyId, "Email");
        }
        [TestMethod]
        public void ChangeLikeValidationRule()
        {
            var babyId = ApplyCorrection(
                (wrongValue) =>
                    wrongValue.Validation_ValidationRule.Column.Name == "Email",
                (ctxB, baby) =>
                {
                    using (var ctx = new SqlServerValidationToolkitContext(Settings.Default.ConnectionString))
                    {
                        var emailRule = ctx.ValidationRules.OfType<LikeRule>().Single(vr => vr.Column.Name == "Email");
                        emailRule.LikeExpression = baby.Email;
                        emailRule.CompiledQuery = emailRule.Query;
                        ctx.SaveChanges();
                    }
                });

            AssertWrongValueIsCorrected(babyId, "Email");
        }

        [TestMethod]
        public void CheckCompareColumn()
        {
            var ctx = new SqlServerValidationToolkitContext(Settings.Default.ConnectionString);

            Assert.IsTrue(WrongHospitalEntryEntriesExisting(ctx));
        }

        [TestMethod]
        public void CorrectCompareColumn()
        {
            var babyId = ApplyCorrection(
                (wrongValue) =>
                    wrongValue.Validation_ValidationRule.Column.Name == "Hospital_Entry",
                (ctxB, baby) =>
                {
                    baby.Hospital_entry = baby.Birth_date.Value.AddDays(-1);
                    ctxB.SaveChanges();
                });

            AssertWrongValueIsCorrected(babyId, "Hospital_Entry");
        }

        [TestMethod]
        public void CorrectEntry_SetItToNull_NullIsNotAllowed_MinMax()
        {
            var babyId = ApplyCorrection(
                (wrongValue) =>
                    wrongValue.Validation_ValidationRule.Column.Name == "Weight",
                (ctxB, baby) =>
                {
                    //precondition: weight is not null
                    Assert.IsNotNull(baby.Weight);

                    //setting the weight to null is also an error
                    baby.Weight = null;
                    ctxB.SaveChanges();
                });

            AssertWrongValueExists(babyId, "Weight");
        }
        [TestMethod]
        public void CorrectEntry_SetItToNull_NullIsNotAllowed_Compare()
        {
            string columnName = "Hospital_entry";
            using (var ctx = new SqlServerValidationToolkitContext(Settings.Default.ConnectionString))
            {
                var column = ctx.Columns.Single(c => c.Name == columnName);
                var vr = column.ValidationRules.Single();
                vr.NullValueTreatment = NullValueTreatment.InterpretAsError;

                vr.CompiledQuery = vr.Query;
                ctx.SaveChanges();

            }
            int babyId;
            using (var ctx = new TestDatabaseContext(Settings.Default.ConnectionString))
            {
                var baby = ctx.Babies.First(b => b.Hospital_entry == null);
                babyId = baby.BabyID;
            }
            using (var ctx = new SqlServerValidationToolkitContext(Settings.Default.ConnectionString))
            {
                bool babyEntryExisted = WrongBabyEntryExists(columnName, babyId, ctx);
                ctx.Validate();
                bool babyEntryExists = WrongBabyEntryExists(columnName, babyId, ctx);
                Assert.IsFalse(babyEntryExisted);
                Assert.IsTrue(babyEntryExists);
            }


            //AssertWrongValueExists(babyId, "Hospital_Entry");
        }

        private static bool WrongBabyEntryExists(string columnName, int babyId, SqlServerValidationToolkitContext ctx)
        {
            return ctx.WrongValues.Any(wv => wv.Id == babyId && wv.Validation_ValidationRule.Column.Name == columnName);
        }

        [TestMethod]
        public void CorrectEntry_SetItToNull_NullIsNotAllowed_Like()
        {
            string columnName = "Email";
            using (var ctx = new SqlServerValidationToolkitContext(Settings.Default.ConnectionString))
            {
                var column = ctx.Columns.Single(c => c.Name == columnName);
                var vr = column.ValidationRules.Single();
                vr.NullValueTreatment = NullValueTreatment.InterpretAsError;

                vr.CompiledQuery = vr.Query;
                ctx.SaveChanges();

            }
            int babyId;
            using (var ctx = new TestDatabaseContext(Settings.Default.ConnectionString))
            {
                var baby = ctx.Babies.First(b => b.Email != null);
                baby.Email = null;
                ctx.SaveChanges();
                babyId = baby.BabyID;
            }
            using (var ctx = new SqlServerValidationToolkitContext(Settings.Default.ConnectionString))
            {
                bool babyEntryExisted = WrongBabyEntryExists(columnName, babyId, ctx);
                ctx.Validate();
                bool babyEntryExists = WrongBabyEntryExists(columnName, babyId, ctx);
                Assert.IsFalse(babyEntryExisted);
                Assert.IsTrue(babyEntryExists);
            }
        }

        [TestMethod]
        public void CheckCustomQuery()
        {
            var ctx = new SqlServerValidationToolkitContext(Settings.Default.ConnectionString);

            Assert.IsTrue(WrongBirthDateEntriesExisting(ctx));
        }

        private static void CorrectLength(TestDatabaseContext ctxBabies)
        {
            ctxBabies.Babies.ToList().ForEach(b =>
            {
                if (b.Length < 10)
                    b.Length = 10;

                if (b.Length > 60)
                    b.Length = 60;
            });
            ctxBabies.SaveChanges();
        }

        private static bool WrongLengthEntriesExisting(SqlServerValidationToolkitContext ctx)
        {
            return WrongEntriesExisting(ctx, "Length");
        }
        private static bool WrongEmailEntriesExisting(SqlServerValidationToolkitContext ctx)
        {
            return WrongEntriesExisting(ctx, "Email");
        }
        private static bool WrongHospitalEntryEntriesExisting(SqlServerValidationToolkitContext ctx)
        {
            return WrongEntriesExisting(ctx, "Hospital_entry");
        }
        private static bool WrongBirthDateEntriesExisting(SqlServerValidationToolkitContext ctx)
        {
            return WrongEntriesExisting(ctx, "Birth_date");
        }

        private static bool WrongEntriesExisting(SqlServerValidationToolkitContext ctx, string columnName)
        {
            return ctx.WrongValues.Any(wrongValue => wrongValue.Validation_ValidationRule.Column.Name == columnName);
        }

    }
}
