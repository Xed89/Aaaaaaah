using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices.JavaScript;

namespace RaylibWasm;

partial class PeerJsInterop
{
  [JSImport("Prompt", "PeerJsInterop")]
  public static partial string Prompt(string message, string defaultValue);

  [JSImport("CreatePeerJs", "PeerJsInterop")]
  private static partial int CreatePeerJs(string id);
  [JSImport("ConnectTo", "PeerJsInterop")]
  public static partial void ConnectTo(string id);
  [JSImport("SendData", "PeerJsInterop")]
  public static partial void SendData(int connectionId, byte[] data);
  [JSExport()]
  public static void OnOpen()
  {
    PeerJsInstance.OnOpen();
  }

  [JSExport()]
  public static void OnConnect(int connectionId)
  {
    PeerJsInstance.OnConnect(connectionId);
  }
  [JSExport()]
  public static void OnData(int connectionId, byte[] data)
  {
    PeerJsInstance.OnData(connectionId, data);
  }

  private static PeerJs PeerJsInstance;
  public static PeerJs Create(string id)
  {
    CreatePeerJs(id);
    PeerJsInstance = new PeerJs();
    return PeerJsInstance;
  }
}

class PeerJs
{
  private List<int> ConnectionIds = new();
  public PeerJs()
  {

  }

  public List<byte[]> DataQueue = new();

  public void OnOpen()
  {
    Console.WriteLine("On Open received");
  }

  public void OnConnect(int connectionId)
  {
    Console.WriteLine("Connection received #" + connectionId);
    ConnectionIds.Add(connectionId);
  }

  public void OnData(int connectionId, byte[] data)
  {
    Console.WriteLine("Data received #" + connectionId + " - " + data.Length);
    DataQueue.Add(data);
  }

  public void ConnectTo(string id)
  {
    PeerJsInterop.ConnectTo(id);
  }

  public void SendData(byte[] data)
  {
    if (ConnectionIds.Count == 0) return;
    PeerJsInterop.SendData(ConnectionIds[0], data);
  }
}