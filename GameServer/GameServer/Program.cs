// See https://aka.ms/new-console-template for more information
namespace GameServer;

public class Program
{
    public static void Main(string[] args)
    {
        Server server = new Server();
        server.Start(Constants.PORT_NUM);
    }
}
