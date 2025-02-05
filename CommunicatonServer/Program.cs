using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class TcpServer
{
    private const int Port = 12345;

    public async Task StartAsync()
    {
        TcpListener listener = new TcpListener(IPAddress.Any, Port);
        listener.Start();
        Console.WriteLine("Server started...");

        while (true)
        {
            TcpClient client = await listener.AcceptTcpClientAsync();
            _ = HandleClientAsync(client); // Spustíme asynchronní úlohu pro obsluhu klienta
        }
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        using (NetworkStream stream = client.GetStream())
        {
            byte[] buffer = new byte[1024];
            int bytesRead;

            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"Received: {message}");

                // Zpětná odpověď klientovi
                string response = $"Echo: {message}";
                byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
            }
        }

        client.Close();
    }
}

public static class Program
{
    public static async Task Main(string[] args)
    {
        TcpServer server = new TcpServer();
        await server.StartAsync(); // Spustíme server asynchronně
    }
}