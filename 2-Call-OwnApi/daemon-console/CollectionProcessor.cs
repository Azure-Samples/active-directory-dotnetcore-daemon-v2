using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace daemon_console
{
    public static class CollectionProcessor<T>
    {
        /// <summary>
        /// Processes the Ms Graph collection page.
        /// </summary>
        /// <param name="graphServiceClient">The graph service client.</param>
        /// <param name="collectionPage">The collection page.</param>
        /// <param name="maxRows">Max rows to fetch from this multi-page call. -1 means the entire result-set</param>
        /// <returns></returns>
        public static async Task<List<T>> ProcessGraphCollectionPageAsync(GraphServiceClient graphServiceClient, ICollectionPage<T> collectionPage, int maxRows = -1)
        {
            List<T> allItems = new List<T>();
            bool breaktime = false;

            var pageIterator = PageIterator<T>.CreatePageIterator(graphServiceClient, collectionPage, (item) =>
            {
                allItems.Add(item);
                //Debug.WriteLine($"1.allItems.Count-{allItems.Count}");

                if (maxRows != -1 && allItems.Count >= maxRows)
                {
                    breaktime = true;
                    return false;
                }

                return true;
            });

            // Start iteration
            await pageIterator.IterateAsync();

            while (pageIterator.State != PagingState.Complete)
            {
                //Debug.WriteLine($"2.allItems.Count-{allItems.Count}");

                if (breaktime)
                {
                    break;
                }

                // Keep iterating till complete.
                await pageIterator.ResumeAsync();
            }

            return allItems;
        }
    }
}
