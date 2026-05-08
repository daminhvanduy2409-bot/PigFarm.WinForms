using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;

namespace PigFarm.WinForms
{
    internal class BatchKpi
    {
        public int BatchID { get; set; }
        public string BatchCode { get; set; } = "";
        public string Barn { get; set; } = "";
        public string Stage { get; set; } = "";
        public string Status { get; set; } = "";
        public DateTime ImportDate { get; set; }
        public int InitialCount { get; set; }
        public decimal InitialAvgWeight { get; set; }
        public int CurrentCount { get; set; }
        public decimal LatestAvgWeight { get; set; }
        public decimal TotalFeedKg { get; set; }
        public decimal FeedKgThisMonth { get; set; }
        public int TotalDead { get; set; }
        public decimal MortalityPct { get; set; }
        public decimal BiomassGainKg { get; set; }
        public int DaysOnFeed { get; set; }
        public decimal? FCR { get; set; }
        public decimal? ADG_gPerDay { get; set; }
        public DateTime? LastSnapshotDate { get; set; }
        public decimal CurrentInventoryWeightKg { get; set; }
        public decimal TotalMovedWeightKg { get; set; }
        public decimal InitialBiomassKg { get; set; }
    }

    internal static class KpiService
    {
        public static List<BatchKpi> GetAllKpi(string statusFilter = "All")
        {
            string where = statusFilter == "All" ? "" : " WHERE Status = @status";
            string sql = $"SELECT * FROM vw_BatchKPI{where} ORDER BY ImportDate DESC";

            var list = new List<BatchKpi>();
            var p = statusFilter == "All"
                ? Array.Empty<SqlParameter>()
                : new[] { DatabaseHelper.P("@status", statusFilter) };

            var dt = DatabaseHelper.Query(sql, p);
            foreach (DataRow r in dt.Rows)
                list.Add(MapRow(r));

            return list;
        }

        public static BatchKpi? GetKpi(int batchId)
        {
            var dt = DatabaseHelper.Query(
                "SELECT * FROM vw_BatchKPI WHERE BatchID = @id",
                DatabaseHelper.P("@id", batchId));
            return dt.Rows.Count == 0 ? null : MapRow(dt.Rows[0]);
        }

        public static DashboardSummary GetDashboardSummary()
        {
            var kpis = GetAllKpi("Active");
            var summary = new DashboardSummary();
            summary.ActiveBatches = kpis.Count;

            foreach (var k in kpis)
            {
                summary.TotalActivePigs += k.CurrentCount;
                summary.TotalFeedMonth += k.FeedKgThisMonth;
                if (k.FCR.HasValue) summary.FcrValues.Add(k.FCR.Value);
                if (k.ADG_gPerDay.HasValue) summary.AdgValues.Add(k.ADG_gPerDay.Value);
                summary.TotalDead += k.TotalDead;
                summary.TotalInitial += k.InitialCount;
            }

            summary.AvgFcr = summary.FcrValues.Count > 0
                ? Math.Round(summary.FcrValues.Average(), 2) : 0;
            summary.AvgAdg = summary.AdgValues.Count > 0
                ? Math.Round(summary.AdgValues.Average(), 1) : 0;
            summary.MortalityPct = summary.TotalInitial > 0
                ? Math.Round(summary.TotalDead * 100.0m / summary.TotalInitial, 2) : 0;

            return summary;
        }

        // Feed trend: last 30 days, all active batches
        public static DataTable GetFeedTrend30Days()
        {
            return DatabaseHelper.Query(
                @"SELECT CONVERT(VARCHAR,FeedDate,103) AS [Ngay],
                         SUM(QuantityKg) AS [TongCam]
                  FROM FeedDaily
                  WHERE FeedDate >= DATEADD(DAY,-29,CAST(GETDATE() AS DATE))
                  GROUP BY FeedDate
                  ORDER BY FeedDate");
        }

        // Growth trend: snapshots across active batches
        public static DataTable GetGrowthTrend(int batchId)
        {
            return DatabaseHelper.Query(
                @"SELECT CONVERT(VARCHAR,SnapshotDate,103) AS [Ngay],
                         AvgWeight AS [TLTB]
                  FROM BatchSnapshot
                  WHERE BatchID = @id
                  ORDER BY SnapshotDate",
                DatabaseHelper.P("@id", batchId));
        }

        private static BatchKpi MapRow(DataRow r)
        {
            return new BatchKpi
            {
                BatchID = (int)r["BatchID"],
                BatchCode = r["BatchCode"].ToString()!,
                Barn = r["Barn"]?.ToString() ?? "",
                Stage = r["Stage"]?.ToString() ?? "",
                Status = r["Status"].ToString()!,
                ImportDate = (DateTime)r["ImportDate"],
                InitialCount = (int)r["InitialCount"],
                InitialAvgWeight = (decimal)r["InitialAvgWeight"],
                CurrentCount = (int)r["CurrentCount"],
                LatestAvgWeight = (decimal)r["LatestAvgWeight"],
                TotalFeedKg = (decimal)r["TotalFeedKg"],
                FeedKgThisMonth = (decimal)r["FeedKgThisMonth"],
                TotalDead = (int)r["TotalDead"],
                MortalityPct = (decimal)r["MortalityPct"],
                BiomassGainKg = (decimal)r["BiomassGainKg"],
                DaysOnFeed = (int)r["DaysOnFeed"],
                FCR = r["FCR"] == DBNull.Value ? null : (decimal?)r["FCR"],
                ADG_gPerDay = r["ADG_gPerDay"] == DBNull.Value ? null : (decimal?)r["ADG_gPerDay"],
                LastSnapshotDate = r["LastSnapshotDate"] == DBNull.Value ? null : (DateTime?)r["LastSnapshotDate"],
                CurrentInventoryWeightKg = (decimal)r["CurrentInventoryWeightKg"],
                TotalMovedWeightKg = (decimal)r["TotalMovedWeightKg"],
                InitialBiomassKg = (decimal)r["InitialBiomassKg"],
            };
        }
    }

    internal class DashboardSummary
    {
        public int ActiveBatches { get; set; }
        public int TotalActivePigs { get; set; }
        public decimal TotalFeedMonth { get; set; }
        public decimal AvgFcr { get; set; }
        public decimal AvgAdg { get; set; }
        public decimal MortalityPct { get; set; }
        public int TotalDead { get; set; }
        public int TotalInitial { get; set; }
        public List<decimal> FcrValues { get; } = new();
        public List<decimal> AdgValues { get; } = new();
    }
}