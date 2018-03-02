using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace NefariusCore
{
    [DataContract]
    public class Effect
    {
        [DataMember]
        public string direction { get; set; } // get/drop
        [DataMember]
        public string item { get; set; } // coin/spy/invention
        [DataMember]
        public string count { get; set; } // fixed("1","2","3")/"spy"/"invented"/"inventions"

        public void Apply(Player pPlayer)
        {
            if (string.Equals(direction, "get"))
            {
                if (string.Equals(direction, "coin"))
                {
                    if (string.Equals(count, "spy"))
                    {

                    }
                    else if (string.Equals(count, "invented"))
                    {

                    }
                    else if (string.Equals(count, "inventions"))
                    {

                    }
                    else
                    {
                        decimal.TryParse(count, out decimal n);
                    }
                }
                else if (string.Equals(direction, "spy"))
                {
                    if (string.Equals(count, "spy"))
                    {

                    }
                    else if (string.Equals(count, "invented"))
                    {

                    }
                    else if (string.Equals(count, "inventions"))
                    {

                    }
                    else
                    {
                        decimal.TryParse(count, out decimal n);
                    }
                }
                else if (string.Equals(direction, "invention"))
                {
                    if (string.Equals(count, "spy"))
                    {

                    }
                    else if (string.Equals(count, "invented"))
                    {

                    }
                    else if (string.Equals(count, "inventions"))
                    {

                    }
                    else
                    {
                        decimal.TryParse(count, out decimal n);
                    }
                }
                else
                    throw new Exception("Wrong effect item");
            }
            else if (string.Equals(direction, "drop"))
            {
                if (string.Equals(direction, "coin"))
                {
                    if (string.Equals(count, "spy"))
                    {

                    }
                    else if (string.Equals(count, "invented"))
                    {

                    }
                    else if (string.Equals(count, "inventions"))
                    {

                    }
                    else
                    {
                        decimal.TryParse(count, out decimal n);
                    }
                }
                else if (string.Equals(direction, "spy"))
                {
                    if (string.Equals(count, "spy"))
                    {

                    }
                    else if (string.Equals(count, "invented"))
                    {

                    }
                    else if (string.Equals(count, "inventions"))
                    {

                    }
                    else
                    {
                        decimal.TryParse(count, out decimal n);
                    }
                }
                else if (string.Equals(direction, "invention"))
                {
                    if (string.Equals(count, "spy"))
                    {

                    }
                    else if (string.Equals(count, "invented"))
                    {

                    }
                    else if (string.Equals(count, "inventions"))
                    {

                    }
                    else
                    {
                        decimal.TryParse(count, out decimal n);
                    }
                }
                else
                    throw new Exception("Wrong effect item");
            }
            else
                throw new Exception("Wrong effect direction");
        }
    }
}
/*
 * получить/отдать
 * монету/шпиона/карту
 * фиксированное количиство/по числу шпионов/по числу созданных изобретений/по числу изобретений в руке
*/
