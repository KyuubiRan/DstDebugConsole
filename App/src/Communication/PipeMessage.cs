using System.Text;

namespace App.Communication;

public class PipeMessage
{
    public enum Type : byte
    {
        None = 0,
        Attach,
        Detach,

        Log,
        Message
    }
    
    public Type MessageType { get; set; }
    public uint ProcessId { get; set; }
    public uint Size { get; set; } // Data size
    public string? Data { get; set; }

    public PipeMessage()
    {
    }
    
    public PipeMessage(byte[] buffer)
    {
        MessageType = (Type)buffer[0];
        ProcessId = BitConverter.ToUInt32(buffer, 1);
        Size = BitConverter.ToUInt32(buffer, 5);
        Data = Encoding.UTF8.GetString(buffer, 9, (int)Size);
    }

    public static implicit operator byte[](PipeMessage message)
    {
        var buffer = new byte[9 + message.Size];
        buffer[0] = (byte)message.MessageType;
        BitConverter.TryWriteBytes(buffer.AsSpan(1), message.ProcessId);
        BitConverter.TryWriteBytes(buffer.AsSpan(5), message.Size);
        Encoding.UTF8.GetBytes(message.Data, buffer.AsSpan(9));
        return buffer;
    }

    public override string ToString()
    {
        return $"Type: {MessageType}, ProcessId: {ProcessId}, Size: {Size}, Data: {Data}";
    }
}