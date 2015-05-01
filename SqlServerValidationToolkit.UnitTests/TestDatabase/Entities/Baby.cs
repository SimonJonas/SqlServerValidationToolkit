using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerValidationToolkit.UnitTests.TestDatabase.Entities
{
    public class Baby
    {
        public int BabyID { get; set; }
        public Nullable<System.DateTime> Hospital_entry { get; set; }
        public Nullable<System.DateTime> Birth_date { get; set; }
        public Nullable<decimal> Length { get; set; }
        public Nullable<int> Weight { get; set; }
        public string Email { get; set; }
    }
}
