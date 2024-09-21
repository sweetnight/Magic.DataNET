using Magic.DataNET;
using System;
using System.Data;
using System.Linq;

namespace Magic.DataNET
{
    public class Options
    {
        public SQLite SQLite { get; set; } = new SQLite();

        public void CreateOptionsTable()
        {
            // check dulu udah ada table options?
            string query = "SELECT count(*) FROM sqlite_master WHERE type = 'table' AND name = 'options'";
            SQLite.ReadQueryResult optionsTableExist = SQLite.ReadQuery(query);

            // 0 => tabel tidak ditemukan. 1 => tabel ditemukan.
            // kalau sudah ada, return saja, karena udah ada key, jangan sampai bikin key enkripsi baru, nanti invalid semua datanya kalau pake key baru.

            if(optionsTableExist.Data != null && optionsTableExist.Data.Rows[0][0].ToString() == 1.ToString()) return;

            query = "CREATE TABLE IF NOT EXISTS options (" +
                                "option_id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                                "option_name TEXT NOT NULL, " +
                                "option_value TEXT NOT NULL" +
                            ")";

            SQLite.Query(query);

            CreateEncryptionKey();

        } // end of function

        private void CreateEncryptionKey()
        {
            string? oldKey = get_option("key", "null");

            if (oldKey != "null") return;

            string newKey = CreateRandomString(32);

            update_option("key", newKey);
        } // end of method

        public string? get_option(string option_name, string? default_value = null)
        {

            if (option_name == "")
            {
                return "null";
            }

            string query = "SELECT option_value FROM options WHERE option_name LIKE '" + option_name + "' ORDER BY option_id ASC LIMIT 1";
            SQLite.ReadQueryResult queryResult = SQLite.ReadQuery(query);

            if (queryResult.Data == null || queryResult.Data.Rows.Count == 0) return default_value;

            return queryResult.Data.Rows[0].Field<string>("option_value");

        } // end of method

        public string update_option(string option_name, string option_value)
        {
            if (option_name == "")
            {
                return "invalid";
            }

            string query;
            string? option_value_from_table = get_option(option_name);

            if (option_value_from_table == null)
            {
                query = "INSERT INTO options (option_name, option_value) values ('" + option_name + "', '" + option_value + "')";
                SQLite.Query(query);

                return "inserted";
            }

            query = "UPDATE options SET option_value = '" + option_value + "' WHERE option_name LIKE '" + option_name + "'";
            SQLite.Query(query);

            return "updated";
        } // end of method

        public string delete_option(string option_name)
        {
            if (option_name == "")
            {
                return "invalid";
            }

            string query;
            string? option_value_from_table = get_option(option_name);

            if (option_value_from_table == "null")
            {
                return "null";
            }

            query = "DELETE FROM options WHERE option_name LIKE '" + option_name + "'";
            SQLite.Query(query);

            return "deleted";
        } // end of method

        public string get_option_encryption(string option_name, string default_value = "")
        {
            if (option_name == "")
            {
                return "null";
            }

            string? encryptionKey = get_option("key");
            string option_name_encrypted = Encryption.Encrypt(option_name, encryptionKey == null ? string.Empty : encryptionKey);

            string query = "SELECT option_value FROM options WHERE option_name LIKE '" + option_name_encrypted + "' ORDER BY option_id ASC LIMIT 1";
            SQLite.ReadQueryResult reader = SQLite.ReadQuery(query);

            if (reader.Data == null || reader.Data.Rows.Count == 0)
            {
                if (default_value == "")
                {
                    return "null";
                }
                else
                {
                    return default_value;
                }

            }

            string? option_value_encrypted = reader.Data.Rows[0].Field<string>("option_value");

            return Encryption.Decrypt(option_value_encrypted == null ? string.Empty : option_value_encrypted, encryptionKey == null ? string.Empty : encryptionKey);

        } // end of method

        public string update_option_encryption(string option_name, string option_value)
        {
            if (option_name == "")
            {
                return "invalid";
            }

            string? encryptionKey = get_option("key");
            string option_name_encrypted = Encryption.Encrypt(option_name, encryptionKey == null ? string.Empty : encryptionKey);
            string option_value_encrypted = Encryption.Encrypt(option_value, encryptionKey == null ? string.Empty : encryptionKey);

            string query;
            string option_value_from_table = get_option_encryption(option_name);

            if (option_value_from_table == "null")
            {
                query = "INSERT INTO options (option_name, option_value) " +
                            "values ('" + option_name_encrypted + "', '" + option_value_encrypted + "')";
                SQLite.Query(query);

                return "inserted";
            }

            query = "UPDATE options SET option_value = '" + option_value_encrypted + "' " +
                        "WHERE option_name LIKE '" + option_name_encrypted + "'";
            SQLite.Query(query);

            return "updated";
        } // end of method

        public string delete_option_encryption(string option_name)
        {
            if (option_name == "")
            {
                return "invalid";
            }

            string? encryptionKey = get_option("key");
            string option_name_encrypted = Encryption.Encrypt(option_name, encryptionKey == null ? string.Empty : encryptionKey);

            string query;
            string option_value_from_table = get_option_encryption(option_name);

            if (option_value_from_table == "null")
            {
                return "null";
            }

            query = "DELETE FROM options WHERE option_name LIKE '" + option_name_encrypted + "'";
            SQLite.Query(query);

            return "deleted";
        } // end of method

        private static string CreateRandomString(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890";

            string newKey = new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());

            return newKey;
        } // end of method
    } // end of class
} // end of namespace
