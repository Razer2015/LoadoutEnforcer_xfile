using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Reflection;
using System.Web;

public class xfileFIN
{
    public static Dictionary<String, String> Select_Weapons(String base_path)
    {
        Dictionary<String, String> weapons = new Dictionary<String, String>();
        using (SQLiteConnection connect = new SQLiteConnection(@"Data Source=" + Path.Combine(base_path, @"LoadoutEnforcer\compact.db3") + ";Version=3;"))
        {
            connect.Open();
            using (SQLiteCommand fmd = connect.CreateCommand())
            {
                fmd.CommandText = @"SELECT * FROM compact_weapons ORDER BY categoryType ASC;";
                fmd.CommandType = CommandType.Text;
                SQLiteDataReader r = fmd.ExecuteReader();
                while (r.Read())
                {
                    String value = String.Format("{0}|{1}", Convert.ToString(r["categoryType"]), Convert.ToString(r["name_rn"]));
                    if (r["rcon"] != DBNull.Value)
                        value += "|" + Convert.ToString(r["rcon"]);
                    weapons.Add(Convert.ToString(r["key"]), value);
                }
            }
        }
        return (weapons);
    }

    public static Dictionary<String, String> Select_Accessories(String base_path)
    {
        Dictionary<String, String> accessories = new Dictionary<String, String>();
        using (SQLiteConnection connect = new SQLiteConnection(@"Data Source=" + Path.Combine(base_path, @"LoadoutEnforcer\compact.db3") + ";Version=3;"))
        {
            connect.Open();
            using (SQLiteCommand fmd = connect.CreateCommand())
            {
                fmd.CommandText = @"SELECT * FROM (SELECT * FROM compact_weaponaccessory ORDER BY name_rn ASC) ORDER BY category_rn;";
                fmd.CommandType = CommandType.Text;
                SQLiteDataReader r = fmd.ExecuteReader();
                while (r.Read())
                {
                    accessories.Add(Convert.ToString(r["key"]), String.Format("{0}|{1}|{2}", Convert.ToString(r["category_rn"]), Convert.ToString(r["name_rn"]), Convert.ToString(r["slug"])));
                }
            }
        }
        return (accessories);
    }

    public static Dictionary<String, String> Select_Kititems(String base_path)
    {
        Dictionary<String, String> kititems = new Dictionary<String, String>();
        using (SQLiteConnection connect = new SQLiteConnection(@"Data Source=" + Path.Combine(base_path, @"LoadoutEnforcer\compact.db3") + ";Version=3;"))
        {
            connect.Open();
            using (SQLiteCommand fmd = connect.CreateCommand())
            {
                fmd.CommandText = @"SELECT * FROM (SELECT * FROM compact_kititems ORDER BY name_rn ASC) ORDER BY category_rn;";
                fmd.CommandType = CommandType.Text;
                SQLiteDataReader r = fmd.ExecuteReader();
                while (r.Read())
                {
                    String value = String.Format("{0}|{1}", Convert.ToString(r["category_rn"]), Convert.ToString(r["name_rn"]));
                    if (r["rcon"] != DBNull.Value)
                        value += "|" + Convert.ToString(r["rcon"]);
                    kititems.Add(Convert.ToString(r["key"]), value); 
                }
            }
        }
        return (kititems);
    }

    public static String HTMLDecode(string encoded)
    {
        return (HttpUtility.HtmlDecode(encoded));
    }
}