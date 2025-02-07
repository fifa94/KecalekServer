using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class EchoServer
{
    private readonly int _port;
    private readonly IPAddress _ipAddress;
    private TcpListener _server;

    public EchoServer(string ipAddress, int port)
    {
        _ipAddress = IPAddress.Parse(ipAddress);
        _port = port;
    }

    public async Task StartAsync()
    {
        _server = new TcpListener(_ipAddress, _port);
        _server.Start();

        Console.WriteLine($"Server naslouchá na {_ipAddress}:{_port}");

        while (true)
        {
            try
            {
                TcpClient client = await _server.AcceptTcpClientAsync();
                _ = Task.Run(() => HandleClientAsync(client)); // Obsluha klienta v samostatném vlákně
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Chyba při přijímání klienta: {ex.Message}");
            }
        }
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        using (NetworkStream stream = client.GetStream())
        using (client) // Uvolnění klienta po skončení komunikace
        {
            Console.WriteLine($"Klient připojen: {client.Client.RemoteEndPoint}");

            byte[] buffer = new byte[1024];
            int bytesRead;

            try
            {
                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"Přijato: {message}");

                    // Odeslání zprávy zpět klientovi
                    byte[] response = Encoding.UTF8.GetBytes(message);
                    await stream.WriteAsync(response, 0, response.Length);
                    Console.WriteLine($"Odesláno: {message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Chyba při komunikaci s klientem: {ex.Message}");
            }
            finally
            {
                Console.WriteLine($"Klient odpojen: {client.Client.RemoteEndPoint}");
            }
        }
    }

    public void Stop()
    {
        _server?.Stop();
    }

    public static async Task Main(string[] args)
    {
        string ipAddress = "127.0.0.1"; // Nebo "0.0.0.0" pro naslouchání na všech rozhraních
        int port = 8888;

        EchoServer server = new EchoServer(ipAddress, port);
        await server.StartAsync();

        Console.WriteLine("Server běží. Stiskněte klávesu pro ukončení...");
        Console.ReadKey();

        server.Stop();
    }
}