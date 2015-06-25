using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpGGBotAPI;

namespace SharpGGBotApiExample
{
    class Program
    {
        static void Main(string[] args)
        {
            string login, password, authFile;
            uint uin;
            Console.Write("Podaj login botapi: ");
            login = Console.ReadLine();
            Console.Write("Podaj hasło botapi: ");
            password = Console.ReadLine();
            Console.Write("Podaj numer bota: ");
            uin = uint.Parse(Console.ReadLine());
            Console.Write("Podaj ścieżkę do pliku autoryzacyjnego: (jeśli wymagane)");
            authFile = Console.ReadLine();

            GaduGaduBot bot = new GaduGaduBot(login, password, uin, authFile);
            bot.Started += bot_Started;
            bot.Stopped += bot_Stopped;
            bot.MessageReceived += bot_MessageReceived;

            bot.Start(8000);

            uint lastUin = 0;
            while (true)
            {
                string[] tokens = Console.ReadLine().Split(':');
                string msg = tokens[0];
                if (tokens.Length > 1)
                {
                    msg = tokens[1];
                    lastUin = uint.Parse(tokens[0]);
                }

                if (lastUin == 0)
                    Console.WriteLine("Podaj numer do którego chcesz wysłać wiadomość");
                else Console.WriteLine(bot.SendMessage(lastUin, msg).BotmasterErrorCode);
            }
        }

        static void bot_Started(object sender, EventArgs e)
        {
            Console.WriteLine("Uruchomiono serwer bota");
        }

        static void bot_Stopped(object sender, EventArgs e)
        {
            Console.WriteLine("Zatrzymano serwer bota");
        }

        static void bot_MessageReceived(object sender, MessageEventArgs e)
        {
            e.Response.AppendText("Witam! - SharpGGBotAPI");
        }
    }
}
