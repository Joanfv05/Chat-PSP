using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Collections.Generic;

public class BasicWebSocketServer : MonoBehaviour
{
    private WebSocketServer wss;

    void Start()
    {
        wss = new WebSocketServer(7777);
        wss.AddWebSocketService<ChatBehavior>("/");
        wss.Start();
        Debug.Log("Servidor WebSocket iniciado en ws://127.0.0.1:7777/");
    }

    void OnDestroy()
    {
        if (wss != null)
        {
            wss.Stop();
            wss = null;
            Debug.Log("Servidor WebSocket detenido.");
        }
    }
}

public class ChatBehavior : WebSocketBehavior
{
    private static Dictionary<string, string> userColors = new Dictionary<string, string>();
    private static readonly string[] colors = { "#FF5733", "#33FF57", "#3357FF", "#F5B041", "#9B59B6" };
    private static int colorIndex = 0;
    private static int userCount = 1; // Contador para los nombres de los usuarios
    private string userId;

    protected override void OnOpen()
    {
        // Asignar un nombre de usuario basado en el contador (Usuario 1, Usuario 2, etc.)
        userId = "Usuario " + userCount;
        userCount++; // Incrementar el contador para el próximo usuario

        if (!userColors.ContainsKey(userId))
        {
            userColors[userId] = colors[colorIndex % colors.Length];
            colorIndex++;
        }

        // Notificar a todos que un usuario se ha unido, usando el nombre legible
        BroadcastMessage($"<color={userColors[userId]}><b>{userId}</b></color> se ha unido al chat.");
    }

    protected override void OnMessage(MessageEventArgs e)
    {
        if (!string.IsNullOrEmpty(e.Data))
        {
            // Mostrar el mensaje con el nombre del usuario y color
            BroadcastMessage($"<color={userColors[userId]}><b>{userId}</b></color>: <color=#000000>{e.Data}</color>");
        }
    }

    protected override void OnClose(CloseEventArgs e)
    {
        // Notificar a todos que el usuario se ha desconectado
        BroadcastMessage($"<color={userColors[userId]}><b>{userId}</b></color> se ha desconectado.");
        userColors.Remove(userId);
    }

    private void BroadcastMessage(string message)
    {
        Sessions.Broadcast(message);
    }
}
