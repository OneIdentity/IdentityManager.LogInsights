using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogfileMetaAnalyser.Helpers
{
	public static class Extensions
	{
		/// <summary>
		/// Partition an async enumerable into chunks that do not exceed the specified size.
		/// </summary>
		/// <param name="src">The source enumerable.</param>
		/// <param name="size">Maximum size.</param>
		/// <typeparam name="T">Type of elements in the enumerable.</typeparam>
		/// <returns>Enumerable of partitions.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Raised when size is lower or equal to zero.</exception>
		public static async IAsyncEnumerable<IReadOnlyList<T>> Partition<T>(this IAsyncEnumerable<T> src, int size)
		{
			if (size < 1)
				throw new ArgumentOutOfRangeException("src", size, "Size has to be greater than zero.");

			// Build lists and return
			List<T> ret = null;

			await foreach (var item in src.ConfigureAwait(false))
			{
				ret ??= new List<T>(size);

				ret.Add(item);

				if (ret.Count == size)
				{
					yield return ret;
					ret = null;
				}
			}

			if (ret != null)
				yield return ret;
		}
	}
}
