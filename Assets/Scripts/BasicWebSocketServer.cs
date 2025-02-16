using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Collections.Generic;
using System.IO;

public class BasicWebSocketServer : MonoBehaviour
{
    private WebSocketServer wss;

    // Se ejecuta al inicio para configurar el servidor WebSocket
    void Start()
    {
        // Crea un servidor WebSocket en el puerto 7777
        wss = new WebSocketServer(7777);
        // Añade el servicio de chat al servidor
        wss.AddWebSocketService<ChatBehavior>("/");
        // Inicia el servidor
        wss.Start();
        Debug.Log("Servidor WebSocket iniciado en ws://127.0.0.1:7777/");
    }

    // Se ejecuta cuando el objeto es destruido, deteniendo el servidor
    void OnDestroy()
    {
        if (wss != null)
        {
            wss.Stop(); // Detiene el servidor WebSocket
            wss = null; // Limpia la referencia al servidor
            Debug.Log("Servidor WebSocket detenido.");
        }
    }
}

public class ChatBehavior : WebSocketBehavior
{
    // Diccionario para asignar un color único a cada usuario
    private static Dictionary<string, string> userColors = new Dictionary<string, string>();
    // Colores predefinidos para los usuarios
    private static readonly string[] colors = { "#FF5733", "#33FF57", "#3357FF", "#F5B041", "#9B59B6" };
    // Índice para controlar los colores
    private static int colorIndex = 0;
    // Contador para asignar nombres únicos a los usuarios
    private static int userCount = 1;
    private string userId; // Identificador único para cada usuario
    // Ruta del archivo donde se guardará el historial de chat
    private static string filePath = "chatHistory.txt";

    // Constructor estático que asegura que el archivo de historial exista
    static ChatBehavior()
    {
        // Si el archivo no existe, lo crea con un encabezado
        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, "Historial de mensajes:\n\n");
        }
    }

    // Se ejecuta cuando un usuario se conecta al servidor WebSocket
    protected override void OnOpen()
    {
        // Asigna un nombre de usuario único (Usuario 1, Usuario 2, etc.)
        userId = "Usuario " + userCount;
        userCount++; // Incrementa el contador para el próximo usuario

        // Asigna un color único al usuario si no tiene uno
        if (!userColors.ContainsKey(userId))
        {
            userColors[userId] = colors[colorIndex % colors.Length];
            colorIndex++; // Incrementa el índice de color
        }

        // Notifica a todos los clientes que un nuevo usuario se ha unido
        BroadcastMessage($"<color={userColors[userId]}><b>{userId}</b></color> se ha unido al chat.");
        // Guarda el mensaje de entrada al historial de chat
        SaveMessage($"{userId} se ha unido al chat.");
    }

    // Se ejecuta cuando un usuario envía un mensaje al servidor
    protected override void OnMessage(MessageEventArgs e)
    {
        if (!string.IsNullOrEmpty(e.Data))
        {
            // Formatea el mensaje para incluir el nombre del usuario y su color
            string message = $"<color={userColors[userId]}><b>{userId}</b></color>: <color=#000000>{e.Data}</color>";
            BroadcastMessage(message); // Envía el mensaje a todos los usuarios

            // Guarda el mensaje en el historial de chat
            SaveMessage($"{userId}: {e.Data}");
        }
    }

    // Se ejecuta cuando un usuario se desconecta del servidor
    protected override void OnClose(CloseEventArgs e)
    {
        // Notifica a todos los clientes que un usuario se ha desconectado
        string message = $"<color={userColors[userId]}><b>{userId}</b></color> se ha desconectado.";
        BroadcastMessage(message);

        // Guarda el mensaje de desconexión en el historial
        SaveMessage($"{userId} se ha desconectado.");

        // Elimina al usuario del diccionario de colores y decrementa el contador de usuarios
        userColors.Remove(userId);
        userCount--;
    }

    // Método para enviar un mensaje a todos los usuarios conectados
    private void BroadcastMessage(string message)
    {
        Sessions.Broadcast(message); // Envía el mensaje a todos los clientes
    }

    // Guarda los mensajes en el archivo de texto
    private void SaveMessage(string message)
    {
        try
        {
            // Abre el archivo en modo de adición (para no sobrescribir el historial)
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine(message); // Escribe el mensaje en el archivo
            }
        }
        catch (IOException ex)
        {
            // Si hay un error al guardar el mensaje, lo muestra en la consola
            Debug.LogError("Error al guardar el mensaje en el archivo: " + ex.Message);
        }
    }
}
