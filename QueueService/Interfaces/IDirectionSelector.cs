namespace QueueService.Interfaces
{
    public interface IDirectionSelector
    {
        ISenderBuilder ISendTo(string queueName);

        IReceiveTypeSelector IReceiveFrom(string queueName);
    }
}
