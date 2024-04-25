using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;

public static class UserUtils {
    public static int? getUserIdByUsername(string username) {
        using (var connection = new SqliteConnection(DBInfo.DataBaseName)) {
            try {
                connection.Open();

                using (var command = connection.CreateCommand()) {

                    var query = $@"
                        SELECT 
	                        id
                        FROM
	                        users as us
                        WHERE 
	                        us.username = '{username}'
                    ";
                    command.CommandText = query;
                    using (var reader = command.ExecuteReader()) {
                        if (reader.HasRows) {
                            reader.Read();
                            return Convert.ToInt32(reader["id"]);
                        }
                        else {
                            return null;
                        }
                    }

                }
            }
            catch (Exception ex) {
                Debug.LogError(ex);
                return null;
            }
            finally {
                connection.Close();
            }
        }
    }
}
