using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Query_Test.Model
{
    public class Person
    {
        public string Name { get; set; }
        public int NumberOfLimbs { get; set; }
        public int? NumberOfKids { get; set; }
        public DateTime BirthDate { get; set; }
        public DateTime? DeceaseDate { get; set; }
        public double MoneyEarnedToday { get; set; }
        public double? DiaperMoney { get; set; }
    }
}
