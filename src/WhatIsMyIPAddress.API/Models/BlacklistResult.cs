using System.Collections.Generic;
using System.Linq;

namespace WhatIsMyIPAddress.API.Models
{
    public class BlacklistResult
    {
        public double GoodPercent
        {
            get
            {
                return Databases.Count(db => db.Value == DatabaseCheckResult.Good) / (double)Databases.Count * 100;
            }
        }

        public double BadPercent
        {
            get
            {
                return Databases.Count(db => db.Value == DatabaseCheckResult.Bad) / (double)Databases.Count * 100;
            }
        }

        public List<Database> Databases { get; set; }

    }
}
