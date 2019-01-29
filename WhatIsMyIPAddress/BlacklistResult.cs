using System.Collections.Generic;
using System.Linq;

namespace Zero.WhatIsMyIPAddress
{
    public enum DatabaseCheckResult
    {
        Good,
        Bad,
        Timeout,
        Offline
    }

    public class Database
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public DatabaseCheckResult Value { get; set; }

    }

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
