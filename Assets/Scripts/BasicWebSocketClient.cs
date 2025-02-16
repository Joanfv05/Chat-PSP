using UnityEngine;
using TMPro;
using UnityEngine.UI;
using WebSocketSharp;
using System;
using System.Collections.Generic;

public class BasicWebSocketClient : MonoBehaviour
{
    public TMP_Text chatDisplay;
    public TMP_InputField inputField;
    public Button sendButton;
    public ScrollRect scrollRect;
    
    private WebSocket ws;
    private Queue<Action> _actionsToRun = new Queue<Action>();

    void Start()
    {
        ConnectToServer();
        sendButton.onClick.AddListener(SendMessage);
        inputField.onSubmit.AddListener(delegate { SendMessage(); });
        inputField.Select();
        inputField.ActivateInputField();
    }

    void ConnectToServer()
    {
        if (ws != null)
        {
            ws.Close();
            ws = null;
        }

        ws = new WebSocket("ws://127.0.0.1:7777/");

        ws.OnOpen += (sender, e) =>
        {
            EnqueueUTAction(() => Debug.Log("WebSocket conectado correctamente."));
        };

        ws.OnMessage += (sender, e) =>
        {
            EnqueueUTAction(() => AddMessageToChat(e.Data));
        };

        ws.OnError += (sender, e) =>
        {
            EnqueueUTAction(() => Debug.LogError("Error en el WebSocket: " + e.Message));
        };

        ws.OnClose += (sender, e) =>
        {
            EnqueueUTAction(() => Debug.Log("WebSocket cerrado. Código: " + e.Code + ", Razón: " + e.Reason));
            ws = null;
        };

        ws.ConnectAsync();
    }

    public void SendMessage()
    {
        if (!string.IsNullOrEmpty(inputField.text) && ws != null && ws.ReadyState == WebSocketState.Open)
        {
            ws.Send(inputField.text);
            inputField.text = "";
            inputField.ActivateInputField();
        }
        else
        {
            Debug.LogWarning("No se puede enviar el mensaje. WebSocket no conectado.");
        }
    }

    void AddMessageToChat(string message)
    {
        chatDisplay.text += "\n" + message;
        LayoutRebuilder.ForceRebuildLayoutImmediate(chatDisplay.rectTransform);
        ScrollToBottom();
    }

    void ScrollToBottom()
    {
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    void Update()
    {
        while (_actionsToRun.Count > 0)
        {
            Action action = _actionsToRun.Dequeue();
            action?.Invoke();
        }
    }

    private void EnqueueUTAction(Action action)
    {
        lock (_actionsToRun)
        {
            _actionsToRun.Enqueue(action);
        }
    }

    void OnDestroy()
    {
        if (ws != null)
        {
            ws.Close();
            ws = null;
            Debug.Log("WebSocket cerrado correctamente.");
        }
    }
}