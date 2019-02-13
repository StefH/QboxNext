using System.Collections.Generic;
using QboxNext.Qserver.Core.Interfaces;

namespace QboxNext.Qserver.Core.Utils
{
	/// <summary>
	/// MemoryQueue implements the IQueue interface in memory.
	/// </summary>
	public class MemoryQueue<T> : IQueue<T> where T : class
	{
		/// <summary>
		/// Enqueue an item.
		/// </summary>
		public void Enqueue(string queueName, T item)
		{
			Queue<T> queue;
			if (!_namedQueues.TryGetValue(queueName, out queue))
			{
				queue = new Queue<T>();
				_namedQueues.Add(queueName, queue);
			}

			queue.Enqueue(item);
		}


		/// <summary>
		/// Dequeue an item.
		/// </summary>
		/// <returns>An object if available, otherwise null.</returns>
		public T Dequeue(string queueName)
		{
			Queue<T> queue;
			if (_namedQueues.TryGetValue(queueName, out queue) && queue.Count > 0)
				return queue.Dequeue();

			return null;
		}


		/// <summary>
		/// Return the number of items in the specified queue.
		/// </summary>
		public int Count(string queueName)
		{
			Queue<T> queue;
			if (_namedQueues.TryGetValue(queueName, out queue) && queue.Count > 0)
				return queue.Count;

			return 0;
		}


		private readonly Dictionary<string, Queue<T>> _namedQueues = new Dictionary<string, Queue<T>>();
	}


}
