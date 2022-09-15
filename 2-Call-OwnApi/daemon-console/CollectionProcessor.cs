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
        /// <returns></returns>
        public static async Task<List<T>> ProcessGraphCollectionPageAsync(GraphServiceClient graphServiceClient, ICollectionPage<T> collectionPage)
        {
            List<T> allItems = new List<T>();

            var pageIterator = PageIterator<T>.CreatePageIterator(graphServiceClient, collectionPage, (item) =>
            {
                //Console.WriteLine(user);
                allItems.Add(item);
                return true;
            });

            // Start iteration
            await pageIterator.IterateAsync();

            while (pageIterator.State != PagingState.Complete)
            {
                // Keep iterating till complete.
                await pageIterator.ResumeAsync();
            }

            return allItems;
        }
    }
}
