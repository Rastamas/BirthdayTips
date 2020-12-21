using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.Configuration.Attributes;

namespace BabyTips
{
    public class Guess
    {
        [Index(0)]
        public string Name { get; set; }
        [Index(1)]
        public DateTime Birthday { get; set; }
        [Index(2)]
        public int WeightInGrams { get; set; }
        [Index(3)]
        public int HeightInCm { get; set; }

        [Ignore]
        public int Score { get; set; }

    }
}
