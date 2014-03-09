using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Netbar_manager_console {

    class Server_service {
        private ArrayList client_list;
        private TcpListener listener;
        private Thread processor;
        enum Return_code{OK, PARAMETER_ERROR, LEAVED};
        string menu_path = ".\\menu.xml";
        string menu_md5 = null;

        public Server_service() {
            menu_md5 = Util.getFileMD5(menu_path);
            Util.info("Newest menu id = "+menu_md5);
            client_list = new ArrayList();
            processor = new Thread(new ThreadStart(start_listening));
            processor.Start();
        }

        private void start_listening() {
            Util.info("Server start listening");
            IPAddress ipAddress = IPAddress.Any;
            listener = new TcpListener(ipAddress, 2500);
            listener.Start();
            while (true) {
                try {
                    TcpClient tcp_client = listener.AcceptTcpClient();
                    Thread client_service_thread = null;
                    Service_client client = new Service_client(this, tcp_client );
                    client_service_thread = new Thread(new ThreadStart(client.service_client_start));
                    client_service_thread.Start();

                } catch (Exception ex) {
                    Util.error_info(ex.Message);
                }

            }
        }
       
        class Service_client{
            Server_service service;

            public Server_service Service {
                get { return service; }
                set { service = value; }
            }
            TcpClient tcp_client;
            public Service_client(Server_service service,TcpClient tcp_client ) {
                this.Tcp_client = tcp_client;
                this.Service = service;
            }
            Thread this_thread;

            public Thread This_thread {
                get { return this_thread; }
                set { this_thread = value; }
            }

            public TcpClient Tcp_client {
                get { return tcp_client; }
                set { tcp_client = value; }
            }
            public void service_client_start() {
                
                Util.info("service client start");
                TcpClient this_tcp_client = Tcp_client;
                NetworkStream ns = this_tcp_client.GetStream();
                StreamReader sr = new StreamReader(ns);
                StreamWriter sw = new StreamWriter(ns) { AutoFlush = true};
                bool alive = true;
                while (alive) {
                    string command = null;
                    try {
                        command = sr.ReadLine();
                    } catch (IOException ex) {
                        Util.error_info("Client exited abnormally");
                        return;
                    }
                    Util.info("Received command "+command);
                    string[] tokens = command.Trim().Split('|');
                    Return_code check_code = check_command(tokens);
                    if (check_code != Return_code.OK) {
                        sw.WriteLine("Invalid command or wrong parameters");
                        continue;
                    }
                    Return_code handle_code = handle_command(this.Service, tokens, this_tcp_client, sw, sr);
                    if (handle_code == Return_code.LEAVED) {
                        alive = false;
                    } else if (handle_code == Return_code.PARAMETER_ERROR) {
                        Util.error_info("Parameter Wrong");
                    }

                }
            }
            public Return_code check_command(string[] tokens) {
                if (tokens == null || tokens[0] == null) {
                    Util.error_info("No valid commands");
                    return Return_code.PARAMETER_ERROR;
                }
                string err_message = null;
                switch (tokens[0]) { 
                    case "CONNECT":
                        if (tokens.Length < 2 || tokens[1].Trim().Length==0) {
                            err_message = ("No user name");
                        }
                        break;
                    case "SYNC":
                        if (tokens[1] == null) {
                            err_message = "No md5 code";  
                        }
                        break;
                    case "LIST":
                        break;
                    default:
                        err_message = "invalid command";
                        break;
                }
                if (err_message != null) {
                    Util.error_info(err_message);
                    return Return_code.PARAMETER_ERROR;
                }
                return Return_code.OK;
            }

            public Return_code handle_command(Server_service service, string[] tokens, TcpClient tcp_client, StreamWriter sw, StreamReader sr) {
                if (tokens[0] == "CONNECT") {
                    string user_name = tokens[1];
                    Client_model c = new Client_model(user_name, tcp_client);
                    service.client_list.Add(c);
                    string response = "CONNECT_ACK";
                    sw.WriteLine( response);
                } else if(tokens[0]=="LIST"){
                    if (service.client_list.Count == 0) {
                        sw.WriteLine("No Users");
                        return Return_code.OK;
                    }
                    string list_of_user = "[";
                    for (int i = 0; i < service.client_list.Count;i++ ) {
                        string user_name = ((Client_model)service.client_list[i]).Name;
                        list_of_user+=(user_name+",");
                    }
                    list_of_user = list_of_user.Substring(0, list_of_user.Length - 1) + "]";
                    sw.WriteLine(list_of_user);
                } else if (tokens[0] == "SYNC") {
                    string client_md5 = tokens[1];
                    if (!client_md5.Equals(service.menu_md5)) {
                        sw.WriteLine("NEED_SYNC");
                        if (sr.ReadLine() == "PREPARED") {
                            Util.info("Client prepared to receive");
                            send_menu(service.menu_path, tcp_client);
                        }
                    } else {
                        sw.WriteLine("NOT_NEED_SYNC");
                    }
                        
                }
                return Return_code.OK;
            }

            private void send_menu(string menu_path, TcpClient tcp_client) {
                Util.info("Menu send start");
                NetworkStream ns = tcp_client.GetStream();
                FileStream fs = new FileStream(menu_path, FileMode.Open);
                int size = 0;//初始化读取的流量为0   
                long len = 0;//初始化已经读取的流量   
                while (len < fs.Length) {
                    byte[] buffer = new byte[512];
                    size = fs.Read(buffer, 0, buffer.Length);
                    ns.Write(buffer, 0, size);
                    len += size;
                }
                fs.Flush();
                ns.Flush();
                fs.Close();
                Util.info("Menu send completed");
            }

        }
    }
}
