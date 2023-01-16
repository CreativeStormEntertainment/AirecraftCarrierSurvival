using UnityEditor;
using UnityEngine;
using System;
using System.Runtime.InteropServices;

public class RakNetStructs
{

}

[StructLayout(LayoutKind.Sequential)]
public struct RakNetPacket
{
    public IntPtr guid;
    public UInt64 length;
    public IntPtr data;
    public IntPtr packetRef;
    public string adress;
    public UInt16 port;
}

[StructLayout(LayoutKind.Sequential)]
public struct RakNetGUID
{
    public UInt64 g;
    public UInt16 systemIndex;
}
