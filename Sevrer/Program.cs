using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Sevrer
{
    public enum MsgType { INFO,WARNING,ERROR }
    class Program
    {
        static bool flag = true;

        static void Main(string[] args)
        {
            bool ServerState = false;
            Thread ServerThread = new Thread(new ParameterizedThreadStart(StartListening));
            ServerThread.Start();
            
            while (flag)
            {

                Console.Write(">");
                string cmd = Console.ReadLine();
                switch (cmd)
                {
                    case "exit":
                        OnWriteConsole("Are you sure that you want to shutdown the server?(y/n)",MsgType.WARNING);
                        Console.Write(">");
                        string asw = Console.ReadLine();
                        
                        if (asw != "y")
                            continue;
                        ServerThread.Abort();
                        OnWriteConsole("Waiting for ListeningThread to return...");
                        ServerThread.Join();
                        break;
                    default:
                        if(cmd!="")
                        OnWriteConsole("\""+cmd+"\" is an Unknown Command.",MsgType.ERROR);
                        break;
                }
            }
            OnWriteConsole("Press anykey to continue...");
            Console.ReadLine();
        }
        public static void OnWriteConsole(string str,MsgType mt)
        {
            switch (mt)
            {
                case MsgType.INFO:
                    Console.WriteLine("[Info]   ->" + str);
                    break;
                case MsgType.WARNING:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("[WARNING]->");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine(str);
                    break;
                case MsgType.ERROR:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("[WARNING]->");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine(str);
                    break;
            }
        }
        public static void OnWriteConsole(string str)
        {
            Console.WriteLine("[Info]   ->" + str);
        }
        public class StateObject
        {
            // Client socket.
            public Socket workSocket = null;
            // Size of receive buffer.
            public const int BufferSize = 256;
            // Receive buffer.
            public byte[] buffer = new byte[BufferSize];
            // Received data string.
            public StringBuilder sb = new StringBuilder();
        }
        // State object for reading client data asynchronously 
        // Thread signal. 
        public static ManualResetEvent allDone = new ManualResetEvent(false);
        public static void StartListening(object state)
        {
            // Data buffer for incoming data. 
            byte[] bytes = new Byte[1024];
            // Establish the local endpoint for the socket. 
            // The DNS name of the computer 
            // running the listener is "host.contoso.com". 

            //IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            IPHostEntry ipHostInfo = Dns.Resolve("127.0.0.1");
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);
            // Create a TCP/IP socket. 
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            // Bind the socket to the local endpoint and listen for incoming connections. 
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);
                
                while (true)
                {
                    // Set the event to nonsignaled state. 
                    allDone.Reset();
                    // Start an asynchronous socket to listen for connections. 
                    OnWriteConsole("Waiting for a connection.");
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
                    // Wait until a connection is made before continuing. 
                    allDone.WaitOne();
                }
            }
            catch (Exception e)
            {
                OnWriteConsole(e.ToString(), MsgType.ERROR);
                flag = false;
            }
            OnWriteConsole("Server Crashed.", MsgType.ERROR);
        }
        public static void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue. 
            allDone.Set();
            // Get the socket that handles the client request. 
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);
            // Create the state object. 
            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
        }
        public static void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;
            // Retrieve the state object and the handler socket 
            // from the asynchronous state object. 
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;
            // Read data from the client socket. 
            int bytesRead = handler.EndReceive(ar);
            if (bytesRead > 0)
            {
                // There    might be more data, so store the data received so far. 
                state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
                // Check for end-of-file tag. If it is not there, read 
                // more data. 
                content = state.sb.ToString();
                if (content.IndexOf("<EOF>") > -1)
                {
                    // All the data has been read from the 
                    // client. Display it on the console. 
                    Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",
                    content.Length, content);
                    // Echo the data back to the client. 
                    Send(handler, "Server return :" + content);
                }
                else
                {
                    // Not all data received. Get more. 
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
                }
            }
        }
        private static void Send(Socket handler, String data)
        {
            // Convert the string data to byte data using ASCII encoding. 
            byte[] byteData = Encoding.ASCII.GetBytes(data);
            // Begin sending the data to the remote device. 
            handler.BeginSend(byteData, 0, byteData.Length, 0,
            new AsyncCallback(SendCallback), handler);
        }
        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object. 
                Socket handler = (Socket)ar.AsyncState;
                // Complete sending the data to the remote device. 
                int bytesSent = handler.EndSend(ar);
                OnWriteConsole("Sent "+bytesSent+" bytes to client.");
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
            catch (Exception e)
            {
                OnWriteConsole(e.ToString(),MsgType.ERROR);
                flag = false;
            }
        }
    }
}
