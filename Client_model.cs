using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Netbar_manager_console {
    class Client_model {
        public Client_model(string name, TcpClient tcp_client){
            this.Name = name;
            this.Tcp_client = tcp_client;
        }
        private string name;

        public string Name {
            get { return name; }
            set { name = value; }
        }
        private TcpClient tcp_client;

        public TcpClient Tcp_client {
            get { return tcp_client; }
            set { tcp_client = value; }
        }
        private Thread thread;

        public Thread Thread {
            get { return thread; }
            set { thread = value; }
        }


    }
}
