using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace server_async {
    class AsyncTcpServer {
        bool isExit = false;
        TcpListener listener;
        ArrayList client_list = new ArrayList();
        private EventWaitHandle allDone = new EventWaitHandle(false, EventResetMode.ManualReset);
        public AsyncTcpServer(){
            Thread myThread = new Thread(new ThreadStart(accept_connect));
            myThread.Start();
            handle_commands();
        }
        void handle_commands() {
            while (!isExit) {
                string command = Console.ReadLine();
                string[] tokens = command.Split(' ');
                //TODO:check grammar
                switch (tokens[0]) { 
                    case "send":
                        send_to_user(Convert.ToInt32(tokens[1]), tokens[2]); 
                        break;
                    case "quit":
                        quit_server();
                        break;
                    case "list":
                        list_users();
                        break;
                }
            }        
        
        }

        private void list_users() {
            for (int i = 0; i < client_list.Count;i++ ) {
                Console.WriteLine("List {0}: {1}", i, ((ReadWriteObject)client_list[i]).client.Client.RemoteEndPoint);
            }
        }

        private void quit_server() {
            allDone.Set();
            isExit = true;
        }

        private void send_to_user(int index, string str) {
            ReadWriteObject readWriteObject = client_list[index] as ReadWriteObject;
            send_string(readWriteObject, str);

        }
        private void accept_connect() {
            listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 2500);
            listener.Start();

            while (!isExit) {
                try {
                    allDone.Reset();
                    AsyncCallback callback = new AsyncCallback(AcceptTcpClientCallback);
                    listener.BeginAcceptTcpClient(callback, listener);
                    allDone.WaitOne();
                } catch (Exception ex) {
                    Util.error_info(ex.Message);
                    break;
                }
            }
        }
        private void AcceptTcpClientCallback(IAsyncResult ar) {
            try {
                allDone.Set();
                TcpListener myListener = ar.AsyncState as TcpListener;
                TcpClient client = myListener.EndAcceptTcpClient(ar);
                Util.info("Accpet Client: "+client.Client.RemoteEndPoint);
                ReadWriteObject readWriteObject = new ReadWriteObject(client);
                client_list.Add(readWriteObject);
                send_string(readWriteObject, "Connected");
                readWriteObject.netStream.BeginRead(readWriteObject.readBytes, 0, readWriteObject.readBytes.Length, ReadCallback, readWriteObject);
            } catch (Exception ex) {
                Util.error_info(ex.Message);
                return;
            }   
        }

        private void ReadCallback(IAsyncResult ar) {
            try {
                ReadWriteObject readWriteObject = ar.AsyncState as ReadWriteObject;
                int count = readWriteObject.netStream.EndRead(ar);
                Util.info(String.Format("From {0}: {1}", readWriteObject.client.Client.RemoteEndPoint, System.Text.Encoding.UTF8.GetString(readWriteObject.readBytes,0, count)));
                if (!isExit) {
                    readWriteObject.InitReadArray();
                    readWriteObject.netStream.BeginRead(readWriteObject.readBytes, 0, readWriteObject.readBytes.Length, ReadCallback, readWriteObject);
                }
            } catch (Exception ex) {
                Util.error_info(ex.Message);
            }
        }

        private void send_string(ReadWriteObject readWriteObject, string str) {
            try {
                readWriteObject.writeBytes = System.Text.Encoding.UTF8.GetBytes(str+"\r\n");
                readWriteObject.netStream.BeginWrite(readWriteObject.writeBytes, 0, readWriteObject.writeBytes.Length, SendCallback, readWriteObject);
                readWriteObject.netStream.Flush();
                Util.info(String.Format("Send to {0}: {1}", readWriteObject.client.Client.RemoteEndPoint, str));

            } catch (Exception ex) {
                Util.error_info(ex.Message);
            }
        }

        private void SendCallback(IAsyncResult ar) {
            ReadWriteObject readWriteObject = ar.AsyncState as ReadWriteObject;
            try {
                readWriteObject.netStream.EndWrite(ar);
            } catch (Exception ex) {
                Util.error_info(ex.Message);
            }

        }
    }
}
