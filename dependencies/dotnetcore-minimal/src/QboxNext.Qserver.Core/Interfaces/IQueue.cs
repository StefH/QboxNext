namespace QboxNext.Qserver.Core.Interfaces
{
    public interface IQueue<T>
    {
        void Enqueue(string queueName, T item);
        T Dequeue(string queueName);
    }
}
