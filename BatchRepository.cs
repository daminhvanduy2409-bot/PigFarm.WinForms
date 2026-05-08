using System;
using System.Collections.Generic;
using System.Data;

namespace PigFarm.WinForms
{
    internal class BatchModel
    {
        public int BatchID { get; set; }
        public string BatchCode { get; set; } = "";
        public DateTime ImportDate { get; set; }
        public int InitialCount { get; set; }
        public decimal InitialAvgWeight { get; set; }
        public decimal? ExportAvgWeight { get; set; }
        public string Stage { get; set; } = "";
        public string Barn { get; set; } = "";
        public string Status { get; set; } = "Active";
        public string Note { get; set; } = "";
    }

    internal static class BatchRepository
    {
        public static List<BatchModel> GetAll(string statusFilter = "All")
        {
            string where = statusFilter == "All" ? "" : " WHERE Status=@s";
            var dt = DatabaseHelper.Query(
                $"SELECT * FROM Batch{where} ORDER BY ImportDate DESC",
                statusFilter == "All"
                    ? Array.Empty<Microsoft.Data.SqlClient.SqlParameter>()
                    : new[] { DatabaseHelper.P("@s", statusFilter) });

            var list = new List<BatchModel>();
            foreach (DataRow r in dt.Rows) list.Add(Map(r));
            return list;
        }

        public static DataTable GetAllAsTable(string statusFilter = "All")
        {
            string where = statusFilter == "All" ? "" : " WHERE Status=@s";
            return DatabaseHelper.Query(
                $@"SELECT BatchID,BatchCode AS [Mã Batch],
                          CONVERT(VARCHAR,ImportDate,103) AS [Ngày Vào],
                          InitialCount AS [SL Vào], InitialAvgWeight AS [TL Vào],
                          Barn AS [Chuồng], Stage AS [GĐ], Status AS [Trạng Thái], Note AS [Ghi Chú]
                   FROM Batch{where} ORDER BY ImportDate DESC",
                statusFilter == "All"
                    ? Array.Empty<Microsoft.Data.SqlClient.SqlParameter>()
                    : new[] { DatabaseHelper.P("@s", statusFilter) });
        }

        public static int Insert(BatchModel b)
        {
            return DatabaseHelper.Execute(
                @"INSERT INTO Batch(BatchCode,ImportDate,InitialCount,InitialAvgWeight,
                                    ExportAvgWeight,Stage,Barn,Status,Note)
                  VALUES(@code,@date,@cnt,@wt,@ewt,@stage,@barn,@status,@note)",
                DatabaseHelper.P("@code", b.BatchCode),
                DatabaseHelper.P("@date", b.ImportDate),
                DatabaseHelper.P("@cnt", b.InitialCount),
                DatabaseHelper.P("@wt", b.InitialAvgWeight),
                DatabaseHelper.P("@ewt", b.ExportAvgWeight),
                DatabaseHelper.P("@stage", b.Stage),
                DatabaseHelper.P("@barn", b.Barn),
                DatabaseHelper.P("@status", b.Status),
                DatabaseHelper.P("@note", b.Note));
        }

        public static int Update(BatchModel b)
        {
            return DatabaseHelper.Execute(
                @"UPDATE Batch SET BatchCode=@code, ImportDate=@date, InitialCount=@cnt,
                    InitialAvgWeight=@wt, ExportAvgWeight=@ewt, Stage=@stage,
                    Barn=@barn, Status=@status, Note=@note
                  WHERE BatchID=@id",
                DatabaseHelper.P("@id", b.BatchID),
                DatabaseHelper.P("@code", b.BatchCode),
                DatabaseHelper.P("@date", b.ImportDate),
                DatabaseHelper.P("@cnt", b.InitialCount),
                DatabaseHelper.P("@wt", b.InitialAvgWeight),
                DatabaseHelper.P("@ewt", b.ExportAvgWeight),
                DatabaseHelper.P("@stage", b.Stage),
                DatabaseHelper.P("@barn", b.Barn),
                DatabaseHelper.P("@status", b.Status),
                DatabaseHelper.P("@note", b.Note));
        }

        public static int Delete(int batchId)
        {
            return DatabaseHelper.Execute(
                "DELETE FROM Batch WHERE BatchID=@id",
                DatabaseHelper.P("@id", batchId));
        }

        public static DataTable GetComboData(string statusFilter = "Active")
        {
            string where = statusFilter == "All" ? "" : " WHERE Status=@s";
            return DatabaseHelper.Query(
                $"SELECT BatchID, BatchCode + ' [' + Barn + ']' AS Label FROM Batch{where} ORDER BY BatchCode",
                statusFilter == "All"
                    ? Array.Empty<Microsoft.Data.SqlClient.SqlParameter>()
                    : new[] { DatabaseHelper.P("@s", statusFilter) });
        }

        private static BatchModel Map(DataRow r) => new BatchModel
        {
            BatchID = (int)r["BatchID"],
            BatchCode = r["BatchCode"].ToString()!,
            ImportDate = (DateTime)r["ImportDate"],
            InitialCount = (int)r["InitialCount"],
            InitialAvgWeight = (decimal)r["InitialAvgWeight"],
            ExportAvgWeight = r["ExportAvgWeight"] == DBNull.Value ? null : (decimal?)r["ExportAvgWeight"],
            Stage = r["Stage"]?.ToString() ?? "",
            Barn = r["Barn"]?.ToString() ?? "",
            Status = r["Status"].ToString()!,
            Note = r["Note"]?.ToString() ?? ""
        };
    }
}