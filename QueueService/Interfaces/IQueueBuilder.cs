namespace QueueService.Interfaces
{
    public interface IQueueBuilder
    {
        IDirectionSelector ConfigureTransport(string hostName, string vHost, string userName, string password);
    }
}
