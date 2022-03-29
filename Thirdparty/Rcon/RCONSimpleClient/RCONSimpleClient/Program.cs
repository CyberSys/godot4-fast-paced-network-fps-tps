﻿using System;
using RCONServerLib;

namespace RCONSimpeClient
{
    public static class Program
    {
        private static bool _authProcessed;

        public static void Main(string[] args)
        {
            var ip = "127.0.0.1";
            var port = 27020;
            var password = "changeme";

            if (args.Length == 3)
            {
                ip = args[0];
                int.TryParse(args[1], out port);
                password = args[2];
            }

            var client = new RemoteConClient()
            {
                UseUtf8 = true,
            };
            client.OnLog += message => { Console.WriteLine("Client Log: {0}", message); };
            client.OnAuthResult += result => { _authProcessed = true; };
            client.OnConnectionStateChange += state =>
            {
                Console.WriteLine("Connection changed: " + state);
                if (state == RemoteConClient.ConnectionStateChange.Connected) client.Authenticate(password);
            };

            client.Connect(ip, port);
            while (true)
            {
                if (!client.Connected)
                {
                    Console.ReadKey();
                    client.Connect(ip, port);
                    continue;
                }

                if (_authProcessed && !client.Authenticated)
                {
                    _authProcessed = false;
                    Console.WriteLine("Password: ");
                    var enteredPwd = Console.ReadLine();
                    client.Authenticate(enteredPwd);
                    continue;
                }

                if (!client.Authenticated)
                    continue;

                var cmd = Console.ReadLine();
                if (cmd == "exit" || cmd == "quit")
                {
                    client.Disconnect();
                    break;
                }

                client.SendCommand(cmd, result => { Console.WriteLine("CMD Result: " + result); });
            }
        }
    }
}