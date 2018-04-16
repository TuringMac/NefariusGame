using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NefariusWebApp
{
    public class TableManager //TODO Move to Core
    {
        static List<Table> TableList { get; set; } = new List<Table>();
        public static Table GetTable(string pTableName)
        {
            lock (TableList)
            {
                var table = TableList.SingleOrDefault(tbl => tbl.TableName == pTableName);
                if (table == null)
                {
                    table = new Table(pTableName);
                    TableList.Add(table);
                }

                return table;
            }
        }
    }
}
