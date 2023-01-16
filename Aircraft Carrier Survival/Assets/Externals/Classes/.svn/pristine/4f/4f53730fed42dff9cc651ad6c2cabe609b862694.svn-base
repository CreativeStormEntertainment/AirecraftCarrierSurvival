using System;
using System.IO;

public interface IGameNetworkManager
{
    void PongReceived(string adress);
    void ConnectionIncoming(IntPtr guid);
    void ConnectionEstablished(IntPtr guid);
    void ParseIncommingData(IntPtr data, UInt64 length = 0);
    void SetUserName(string name);
    void GetUserData(MemoryStream memStream);
}
