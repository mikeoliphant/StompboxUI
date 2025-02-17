using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace StompboxAPI
{
    public class NetworkClient
    {
        public bool Running { get; private set; }
        public bool Connected { get; private set; }
        public string LastError { get; private set; }

        TcpClient client;
        NetworkStream stream;
        Thread runThread = null;

        public Action<String> LineHandler { get; set; }
        public Action<String> DebugAction { get; set; }

        Action<bool> resultCallback = null;

        public NetworkClient()
        {
        }

        public bool Start(string serverName, int port, Action<bool> resultCallback)
        {
            this.resultCallback = resultCallback;

            try
            {
                Debug("** Connecting to " + serverName + ":" + port);

                client = new TcpClient();

                var connectionTask = client.ConnectAsync(serverName, port).ContinueWith(task =>
                    {
                        return task.IsFaulted ? null : client;
                    }, TaskContinuationOptions.ExecuteSynchronously);
                var timeoutTask = Task.Delay(5000)
                    .ContinueWith<TcpClient>(task => null, TaskContinuationOptions.ExecuteSynchronously);
                var resultTask = Task.WhenAny(connectionTask, timeoutTask).Unwrap();

                var resultTcpClient = resultTask.GetAwaiter().GetResult();

                if (client.Connected)
                {
                    stream = client.GetStream();

                    Connected = true;

                    Debug("** Connected!");

                    runThread = new Thread(RunServer);
                    runThread.Start();                    
                }

                resultCallback(client.Connected);

                return client.Connected;
            }
            catch (Exception ex)
            {
                LastError = ex.ToString();

                Debug("** Connect failed with: " + LastError);

                resultCallback(false);
            }

            return false;
        }

        void Debug(string debugStr)
        {
            if (DebugAction != null)
                DebugAction(debugStr);
        }

        public void Stop()
        {
            client.Close();

            if (runThread != null)
                runThread.Join();
        }

        public void SendData(String data)
        {
            if (!Running)
                return;

            try
            {
                Byte[] byteData = System.Text.Encoding.ASCII.GetBytes(data);

                stream.Write(byteData, 0, data.Length);
            }
            catch
            {
            }
        }

        void RunServer()
        {
            try
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    Running = true;

                    resultCallback(true);  // Do this here to make sure we don't send any messages before we are ready to get the response

                    do
                    {
                        string line = reader.ReadLine();

                        if (String.IsNullOrEmpty(line))
                        {
                            if (client.Client.Poll(0, SelectMode.SelectRead))
                            {
                                byte[] buff = new byte[1];

                                if (client.Client.Receive(buff, SocketFlags.Peek) == 0)
                                {
                                    break;
                                }
                            }
                        }
                        else
                        {
                            if (LineHandler != null)
                                LineHandler(line);
                        }
                    }
                    while (true);
                }
            }
            catch (Exception ex)
            {
            }

            Debug("** Connection ended");

            Running = false;
            Connected = false;
        }
    }
}
