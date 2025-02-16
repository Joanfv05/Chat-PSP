using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Collections.Generic;
using System.IO;

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
    private static string filePath = "chatHistory.txt"; // Ruta del archivo donde se guardará el historial

    // Constructor estático para inicializar el archivo
    static ChatBehavior()
    {
        // Si el archivo no existe, lo creamos. Si ya existe, lo dejamos intacto.
        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, "Historial de mensajes:\n\n");
        }
    }

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

        // Notificar a todos que un usuario se ha unido
        BroadcastMessage($"<color={userColors[userId]}><b>{userId}</b></color> se ha unido al chat.");

        // Guardar el mensaje en el historial (sin etiquetas)
        SaveMessage($"{userId} se ha unido al chat.");
    }

    protected override void OnMessage(MessageEventArgs e)
    {
        if (!string.IsNullOrEmpty(e.Data))
        {
            // Mostrar el mensaje con el nombre del usuario y color
            string message = $"<color={userColors[userId]}><b>{userId}</b></color>: <color=#000000>{e.Data}</color>";
            BroadcastMessage(message);

            // Guardar el mensaje en el historial (sin etiquetas)
            SaveMessage($"{userId}: {e.Data}");
        }
    }

    protected override void OnClose(CloseEventArgs e)
    {
        // Notificar a todos que el usuario se ha desconectado
        string message = $"<color={userColors[userId]}><b>{userId}</b></color> se ha desconectado.";
        BroadcastMessage(message);

        // Guardar el mensaje en el historial (sin etiquetas)
        SaveMessage($"{userId} se ha desconectado.");

        userColors.Remove(userId);
        userCount--;
    }

    private void BroadcastMessage(string message)
    {
        Sessions.Broadcast(message);
    }

    private void SaveMessage(string message)
    {
        // Guardar solo el usuario y el texto en el archivo de texto
        try
        {
            using (StreamWriter writer = new StreamWriter(filePath, true)) // "true" para añadir al archivo
            {
                writer.WriteLine(message);
            }
        }
        catch (IOException ex)
        {
            Debug.LogError("Error al guardar el mensaje en el archivo: " + ex.Message);
        }
    }
}
