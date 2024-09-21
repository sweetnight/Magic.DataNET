using System;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace Magic.DataNET
{
    public class SQLite
    {
        #region INTERNAL PROPERTIES
        public string DBFileName { get; set; } = string.Empty;
        private SQLiteConnection? m_dbConnection { get; set; }

        private const string DbFileCreatedMessage = "DB File is created";
        private const string DbFileExistsMessage = "DB File exists";
        private const string DbConnectResultSuccess = "DB is connected successfully.";
        private const string DbConnectResultFail = "Connect DB is failed.";
        private const string DbCloseResultSuccess = "Close DB is successful.";
        private const string DbCloseResultFail = "Close DB is failed.";
        private const string QueryResultSuccess = "Query finished successfully.";
        private const string QueryResultFail = "Failed when querying.";
        #endregion

        #region METHODS
        public CreateDatabaseFileResult CreateDatabaseFile(string dbFileName)
        {
            CreateDatabaseFileResult createDatabaseFileResult = new CreateDatabaseFileResult();

            DBFileName = @dbFileName;

            if (!File.Exists(@DBFileName))
            {
                SQLiteConnection.CreateFile(@DBFileName);

                createDatabaseFileResult.state = true;
                createDatabaseFileResult.message = DbFileCreatedMessage;
            }
            else
            {
                createDatabaseFileResult.state = false;
                createDatabaseFileResult.message = DbFileExistsMessage;
            }

            return createDatabaseFileResult;
        } // end of method

        public ConnectDBResult ConnectDB()
        {
            ConnectDBResult connectDBResult = new ConnectDBResult();

            try
            {
                m_dbConnection = new SQLiteConnection($"Data Source={DBFileName};Version=3;");
                m_dbConnection.Open();

                connectDBResult.state = true;
                connectDBResult.message = DbConnectResultSuccess;
            }
            catch (Exception ex)
            {
                connectDBResult.state = false;
                connectDBResult.message = DbConnectResultFail + $" Exception: {ex.Message}";

                Debug.WriteLine(ex.Message);
            }

            return connectDBResult;
        } // end of function

        public CloseDBResult CloseDB()
        {
            CloseDBResult closeDBResult = new CloseDBResult();

            try
            {
                m_dbConnection?.Close();
                closeDBResult.state = true;
                closeDBResult.message = DbCloseResultSuccess;
            }
            catch (Exception ex)
            {
                closeDBResult.state = false;
                closeDBResult.message = DbCloseResultFail + $" Exception: {ex.Message}";

                Debug.WriteLine(ex.Message);
            }

            return closeDBResult;
        } // end of function

        public QueryResult Query(string query)
        {
            QueryResult queryResult = new QueryResult();

            try
            {
                using (SQLiteCommand command = new SQLiteCommand(query, m_dbConnection))
                {
                    command.ExecuteNonQuery();
                }

                queryResult.state = true;
                queryResult.message = QueryResultSuccess;
            }
            catch (Exception ex)
            {
                queryResult.state = false;
                queryResult.message = QueryResultFail + $" Exception: {ex.Message}";

                Debug.WriteLine(ex.Message);
            }

            return queryResult;
        } // end of function

        public ReadQueryResult ReadQuery_(string query)
        {

            ReadQueryResult readQueryResult = new ReadQueryResult();

            try
            {
                using (SQLiteCommand command = new SQLiteCommand(query, m_dbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        DataTable dt = new DataTable();
                        dt.Load(reader);

                        // Handle datetime parsing errors
                        foreach (DataRow row in dt.Rows)
                        {
                            foreach (DataColumn column in dt.Columns)
                            {
                                if (column.DataType == typeof(string) && DateTime.TryParseExact(row[column].ToString(), "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                                {
                                    row[column] = parsedDate;
                                }
                                else
                                {
                                    // Log the error and keep the original value if it's not a datetime string
                                    Debug.WriteLine($"Error parsing datetime in column '{column.ColumnName}' for row '{row[column]}'. Keeping original value.");
                                }
                            }
                        }

                        readQueryResult.Data = dt;
                        readQueryResult.Count = dt.Rows.Count;
                        readQueryResult.State = true;
                        readQueryResult.Message = "Query finished successfully";
                    }
                }
            }
            catch (Exception ex)
            {
                readQueryResult.Data = null;
                readQueryResult.Count = 0;
                readQueryResult.State = false;
                readQueryResult.Message = ex.Message;

                Debug.WriteLine(ex.Message);
            }

            return readQueryResult;

        } // end of method

        public ReadQueryResult ReadQuery(string query)
        {

            ReadQueryResult readQueryResult = new ReadQueryResult();

            try
            {
                using (SQLiteCommand command = new SQLiteCommand(query, m_dbConnection))
                {

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        DataTable dt = new DataTable();
                        dt.Load(reader);

                        readQueryResult.Data = dt;
                        readQueryResult.Count = dt.Rows.Count;
                        readQueryResult.State = true;
                        readQueryResult.Message = "Query finished successfully";
                    }
                }
            }
            catch (Exception ex)
            {
                readQueryResult.Data = null;
                readQueryResult.Count = 0;
                readQueryResult.State = false;
                readQueryResult.Message = ex.Message;

                Debug.WriteLine(ex.Message);
            }

            return readQueryResult;

        } // end of method

        public ScalarQueryResult ScalarQuery(string query)
        {
            ScalarQueryResult scalarQueryResult = new ScalarQueryResult();

            try
            {
                using (SQLiteCommand command = new SQLiteCommand(query, m_dbConnection))
                {
                    object result = command.ExecuteScalar();
                    scalarQueryResult.Data = result;
                    scalarQueryResult.State = true;

                    if (result == null)
                    {
                        scalarQueryResult.Message = "No data returned by query";
                    }
                    else
                    {
                        scalarQueryResult.Message = "Query finished successfully";
                    }
                }
            }
            catch (Exception ex)
            {
                scalarQueryResult.Data = null;
                scalarQueryResult.State = false;
                scalarQueryResult.Message = ex.Message;

                Debug.WriteLine(ex.Message);
            }

            return scalarQueryResult;
        } // end of method

        public async Task<ReadQueryResult> ReadQueryAsync(string query)
        {
            ReadQueryResult readQueryResult = new ReadQueryResult();

            try
            {
                using (SQLiteCommand command = new SQLiteCommand(query, m_dbConnection))
                using (DbDataReader reader = await command.ExecuteReaderAsync())
                {
                    if (reader is SQLiteDataReader sqliteReader)
                    {
                        DataTable dt = new DataTable();
                        dt.Load(sqliteReader);

                        readQueryResult.Data = dt;
                        readQueryResult.Count = dt.Rows.Count;
                        readQueryResult.State = true;
                        readQueryResult.Message = "Query finished successfully";
                    }
                    else
                    {
                        // Handle the case where the reader is not SQLiteDataReader
                        throw new InvalidOperationException("Unexpected data reader type.");
                    }
                }
            }
            catch (Exception ex)
            {
                readQueryResult.Data = null;
                readQueryResult.Count = 0;
                readQueryResult.State = false;
                readQueryResult.Message = $"Failed when querying. Exception: {ex.Message}";

                Debug.WriteLine(ex.Message);
            }

            return readQueryResult;
        } // end of method

        public IsTableExistsResult IsTableExists(string tableName)
        {

            IsTableExistsResult isTableExistsResult = new IsTableExistsResult();

            // Query untuk mengecek apakah tabel sudah ada.
            string query = $"SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = '{tableName}'";

            try
            {
                using (SQLiteCommand command = new SQLiteCommand(query, m_dbConnection))
                {
                    // ExecuteScalar digunakan karena kita hanya memerlukan satu nilai (jumlah tabel yang ditemukan).
                    int tableCount = Convert.ToInt32(command.ExecuteScalar());

                    if (tableCount == 0)
                    {
                        isTableExistsResult.state = false;
                        isTableExistsResult.message = "Table does not exist";
                    }
                    else
                    {
                        isTableExistsResult.state = true;
                        isTableExistsResult.message = "Table exists";
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exception if needed.
                isTableExistsResult.state = false;
                isTableExistsResult.message = $"Error checking table existence. Exception: {ex.Message}";
            }

            return isTableExistsResult;

        } // end of method

        public bool IsColumnExists(string tableName, string columnName)
        {
            IsTableExistsResult tableExists = this.IsTableExists(tableName);

            if (!tableExists.state) return false;

            string query = $"SELECT COUNT(*) FROM pragma_table_info('{tableName}') WHERE name = '{columnName}';";

            try
            {
                using (SQLiteCommand command = new SQLiteCommand(query, m_dbConnection))
                {

                    // ExecuteScalar digunakan karena kita hanya memerlukan satu nilai (jumlah tabel yang ditemukan).
                    return Convert.ToInt32(command.ExecuteScalar()) > 0;

                }
            }
            catch
            {
                // Handle exception if needed.
                return false;
            }

        } // end of method

        public bool IsDataExists(string tableName)
        {

            string query = $"SELECT EXISTS(SELECT 1 FROM {tableName})";
            ScalarQueryResult result = ScalarQuery(query);

            if (result.State && result.Data != null)
            {
                return Convert.ToInt32(result.Data) == 1;
            }
            else
            {
                Console.WriteLine(result.Message);
                return false;
            }

        } // end of method

        public string[] GetColumnNames(string tableName, string? excludedColumn = null)
        {

            List<string> columns = new List<string>();
            string query = $"PRAGMA table_info({tableName})";

            try
            {
                using (SQLiteCommand command = new SQLiteCommand(query, m_dbConnection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string? columnName = reader["name"].ToString();
                            if (!string.IsNullOrEmpty(columnName) && (excludedColumn == null || !string.Equals(columnName, excludedColumn, StringComparison.OrdinalIgnoreCase)))
                            {
                                columns.Add(columnName);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return columns.ToArray();

        } // end of method

        #endregion

        #region RESULT OBJECT CLASSES
        public class CreateDatabaseFileResult
        {
            public bool state { get; set; } = false;
            public string message { get; set; } = string.Empty;
        } // end of class

        public class ConnectDBResult
        {
            public bool state { get; set; }
            public string message { get; set; } = string.Empty;
        } // end of class

        public class CloseDBResult
        {
            public bool state { get; set; }
            public string message { get; set; } = string.Empty;
        } // end of class

        public class QueryResult
        {
            public bool state { get; set; } = false;
            public string message { get; set; } = string.Empty;
        } // end of class

        public class ReadQueryResult
        {
            public DataTable? Data { get; set; } = new DataTable();
            public int Count { get; set; } = 0;
            public bool State { get; set; } = false;
            public string Message { get; set; } = string.Empty;
        } // end of method

        public class ScalarQueryResult
        {
            public object? Data { get; set; } = new object();
            public bool State { get; set; }
            public string Message { get; set; } = string.Empty;
        }

        public class IsTableExistsResult
        {
            public bool state { get; set; }
            public string message { get; set; } = string.Empty;
        } // end of class
        #endregion
    } // end of class
} // end of namespace
