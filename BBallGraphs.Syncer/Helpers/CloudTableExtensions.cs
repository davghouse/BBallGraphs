using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BBallGraphs.Syncer.Helpers
{
    public static class CloudTableExtensions
    {
        // http://stackoverflow.com/q/24234350, https://github.com/Azure/azure-storage-net/issues/94
        public static async Task<IList<T>> ExecuteQueryAsync<T>(this CloudTable table, TableQuery<T> query)
            where T : ITableEntity, new()
        {
            var results = new List<T>(query.TakeCount ?? 0);
            int effectiveTakeCount = query.TakeCount ?? int.MaxValue;
            TableContinuationToken continuationToken = null;

            do
            {
                var segment = await table.ExecuteQuerySegmentedAsync(query, continuationToken);
                continuationToken = segment.ContinuationToken;
                results.AddRange(segment.Results);
            } while (continuationToken != null && results.Count < effectiveTakeCount);

            if (results.Count > effectiveTakeCount)
            {
                results.RemoveRange(index: effectiveTakeCount, count: results.Count - effectiveTakeCount);
            }

            return results;
        }
    }
}
