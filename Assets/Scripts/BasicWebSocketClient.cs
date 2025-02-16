using UnityEngine;
using TMPro;
using UnityEngine.UI;
using WebSocketSharp;
using System;
using System.Collections.Generic;

public class BasicWebSocketClient : MonoBehaviour
{
    // Referencias a UI
    public TMP_Text chatDisplay; // Donde se mostrarán los mensajes del chat
    public TMP_InputField inputField; // Campo de entrada para escribir mensajes
    public Button sendButton; // Botón para enviar mensajes
    public ScrollRect scrollRect; // Control de desplazamiento del chat

    // Variables para gestionar el WebSocket y acciones pendientes
    private WebSocket ws;
    private Queue<Action> _actionsToRun = new Queue<Action>();

    // Se ejecuta al inicio
    void Start()
    {
        ConnectToServer(); // Conecta al servidor WebSocket
        sendButton.onClick.AddListener(SendMessage); // Añade un listener al botón de enviar
        inputField.onSubmit.AddListener(delegate { SendMessage(); }); // Permite enviar mensaje al presionar Enter
        inputField.Select(); // Selecciona el campo de texto
        inputField.ActivateInputField(); // Activa el campo de entrada
    }

    // Conecta al servidor WebSocket
    void ConnectToServer()
    {
        // Cierra WebSocket si ya está abierto
        if (ws != null)
        {
            ws.Close();
            ws = null;
        }

        // Inicializa el WebSocket con la URL del servidor
        ws = new WebSocket("ws://127.0.0.1:7777/");

        // Eventos para manejar la conexión
        ws.OnOpen += (sender, e) =>
        {
            EnqueueUTAction(() => Debug.Log("WebSocket conectado correctamente.")); // Mensaje de éxito
        };

        // Recibe los mensajes del servidor
        ws.OnMessage += (sender, e) =>
        {
            EnqueueUTAction(() => AddMessageToChat(e.Data)); // Añade mensaje al chat
        };

        // Maneja errores de WebSocket
        ws.OnError += (sender, e) =>
        {
            EnqueueUTAction(() => Debug.LogError("Error en el WebSocket: " + e.Message)); // Mensaje de error
        };

        // Maneja el cierre de WebSocket
        ws.OnClose += (sender, e) =>
        {
            EnqueueUTAction(() => Debug.Log("WebSocket cerrado. Código: " + e.Code + ", Razón: " + e.Reason)); // Mensaje de cierre
            ws = null;
        };

        ws.ConnectAsync(); // Conexión asíncrona
    }

    // Envía el mensaje al servidor WebSocket
    public void SendMessage()
    {
        // Verifica que haya texto en el campo y que el WebSocket esté abierto
        if (!string.IsNullOrEmpty(inputField.text) && ws != null && ws.ReadyState == WebSocketState.Open)
        {
            ws.Send(inputField.text); // Envía el mensaje
            inputField.text = ""; // Limpia el campo de texto
            inputField.ActivateInputField(); // Reactiva el campo de entrada
        }
        else
        {
            Debug.LogWarning("No se puede enviar el mensaje. WebSocket no conectado.");
        }
    }

    // Añade un mensaje al chat
    void AddMessageToChat(string message)
    {
        chatDisplay.text += "\n" + message; // Añade el mensaje al texto del chat
        LayoutRebuilder.ForceRebuildLayoutImmediate(chatDisplay.rectTransform); // Fuerza el redibujo del layout
        ScrollToBottom(); // Desplaza el chat hacia abajo
    }

    // Desplaza la vista hacia abajo para ver el último mensaje
    void ScrollToBottom()
    {
        Canvas.ForceUpdateCanvases(); // Fuerza la actualización de los elementos del canvas
        scrollRect.verticalNormalizedPosition = 0f; // Desplaza el scroll al final
    }

    // Actualiza las acciones pendientes
    void Update()
    {
        while (_actionsToRun.Count > 0)
        {
            Action action = _actionsToRun.Dequeue(); // Toma una acción pendiente
            action?.Invoke(); // Ejecuta la acción
        }
    }

    // Encola una acción para ejecutarse más tarde en el hilo principal
    private void EnqueueUTAction(Action action)
    {
        lock (_actionsToRun) // Asegura que las acciones se gestionen de manera segura
        {
            _actionsToRun.Enqueue(action); // Encola la acción
        }
    }

    // Cierra el WebSocket cuando el objeto es destruido
    void OnDestroy()
    {
        if (ws != null)
        {
            ws.Close(); // Cierra la conexión
            ws = null; // Limpia la referencia
            Debug.Log("WebSocket cerrado correctamente.");
        }
    }
}
