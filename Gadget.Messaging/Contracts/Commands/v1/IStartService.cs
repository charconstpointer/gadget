namespace Gadget.Messaging.Contracts.Commands.v1
{
    public interface IStartService
    {
        string Agent { get; }
        string ServiceName { get; }
    }
}