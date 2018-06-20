using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NefariusAngular
{
    public class TableManager //TODO Move to Core
    {
        static List<Table> TableList { get; set; } = new List<Table>();
        public static Table GetTable(string pTableName)
        {
            lock (TableList)
            {
                var table = TableList.SingleOrDefault(tbl => tbl.Name == pTableName);
                if (table == null)
                {
                    var name = pTableName;
                    if (string.IsNullOrWhiteSpace(pTableName))
                    {
                        name = Guid.NewGuid().ToString();
                    }
                    table = new Table(name);
                    TableList.Add(table);
                }

                return table;
            }
        }

        public static List<Table> GetTableList()
        {
            var result = new List<Table>();
            lock (TableList)
            {
                result = new List<Table>(TableList);
            }
            return result;
        }
    }
}
