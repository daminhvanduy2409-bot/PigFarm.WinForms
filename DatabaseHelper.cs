using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace PigFarm.WinForms
{
    internal static class DatabaseHelper
    {
        private static SqlConnection GetConnection()
            => new SqlConnection(AppConfig.ConnectionString);

        public static DataTable Query(string sql, params SqlParameter[] parameters)
        {
            using var conn = GetConnection();
            using var cmd = new SqlCommand(sql, conn);
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            conn.Open();
            var dt = new DataTable();
            new SqlDataAdapter(cmd).Fill(dt);
            return dt;
        }

        public static int Execute(string sql, params SqlParameter[] parameters)
        {
            using var conn = GetConnection();
            using var cmd = new SqlCommand(sql, conn);
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            conn.Open();
            return cmd.ExecuteNonQuery();
        }

        public static object? Scalar(string sql, params SqlParameter[] parameters)
        {
            using var conn = GetConnection();
            using var cmd = new SqlCommand(sql, conn);
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            conn.Open();
            var result = cmd.ExecuteScalar();
            return result == DBNull.Value ? null : result;
        }

        public static SqlParameter P(string name, object? value)
            => new SqlParameter(name, value ?? DBNull.Value);
    }
}
