using System;
using System.Text;
using System.Threading;
using System.Net.WebSockets;
using UnityEngine;

public class MouseDataClass
{
    public int mouseX;
    public int mouseY;

}

public class WebSocketClient : MonoBehaviour
{
    public float mouseDataX;
    public float mouseDataY;

    Uri u = new Uri("ws://127.0.0.1:8080");
    ClientWebSocket cws = null;
    ArraySegment<byte> buf = new ArraySegment<byte>(new byte[1024]);

    float resetTime = 0;

    void Start()
    {
        Connect();
        resetTime = Time.time;
    }

    async void Connect()
    {
        cws = new ClientWebSocket();
        try
        {
            await cws.ConnectAsync(u, CancellationToken.None);
            if (cws.State == WebSocketState.Open) Debug.Log("connected");
            SayHello();
            GetStuff();
        }
        catch (Exception e) { Debug.Log("woe " + e.Message); }
    }

    async void SayHello()
    {
        ArraySegment<byte> b = new ArraySegment<byte>(Encoding.UTF8.GetBytes("hello"));
        await cws.SendAsync(b, WebSocketMessageType.Text, true, CancellationToken.None);
    }

    async void GetStuff()
    {
        WebSocketReceiveResult r = await cws.ReceiveAsync(buf, CancellationToken.None);
        Debug.Log("Got: " + Encoding.UTF8.GetString(buf.Array, 0, r.Count));
        string inputJson = Encoding.UTF8.GetString(buf.Array, 0, r.Count);
        try
        {
            var myObj = JsonUtility.FromJson<MouseDataClass>(inputJson);
            mouseDataX = myObj.mouseX;
            mouseDataY = myObj.mouseY;
        }
        catch (Exception e)
        {
            Debug.Log("Got wrong data!!!");

        }

        GetStuff();
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
        if (Time.time - resetTime > 3 && cws.State == WebSocketState.Open)
        {
            SayHello();
            resetTime = Time.time;
        }
    }

    private void OnApplicationQuit()
    {
        cws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed in server by the client", CancellationToken.None);
    }
}