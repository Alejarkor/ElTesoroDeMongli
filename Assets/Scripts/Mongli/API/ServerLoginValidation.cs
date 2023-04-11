using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;

public class ServerLoginValidation : MonoBehaviour
{
    private HttpListener listener;
    private Thread listenerThread;

    void Start()
    {
        listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:3000/");
        listener.Start();
        listenerThread = new Thread(ListenerCallback);
        listenerThread.Start();
    }

    private void ListenerCallback()
    {
        while (true)
        {
            HttpListenerContext context = listener.GetContext();
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            // Leer y procesar el token enviado por el cliente
            StreamReader reader = new StreamReader(request.InputStream);
            string token = reader.ReadToEnd();
            Debug.Log("Token recibido: " + token);
            // Aquí puedes validar el token y tomar decisiones en función de él

            // Enviar una respuesta al cliente
            string responseString = "OK"; // o "NOK" si la validación falla
            byte[] buffer = Encoding.UTF8.GetBytes(responseString);

            // Añadir encabezados CORS
            response.AddHeader("Access-Control-Allow-Origin", "*");
            response.AddHeader("Access-Control-Allow-Methods", "POST, GET, OPTIONS");
            response.AddHeader("Access-Control-Allow-Headers", "Content-Type");


            response.ContentLength64 = buffer.Length;
            Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();
        }
    }

    void OnDestroy()
    {
        listener.Stop();
        listenerThread.Abort();
    }
}
