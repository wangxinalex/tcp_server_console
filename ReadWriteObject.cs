using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace server_async {
    class ReadWriteObject {
        public TcpClient client{get;set;}
        public NetworkStream netStream{get;set;}
        public byte[] readBytes{set;get;}
        public byte[] writeBytes{set;get;}
        public ReadWriteObject(TcpClient client) {
            this.client = client;
            this.netStream = client.GetStream();
            this.readBytes = new byte[client.ReceiveBufferSize];
            this.writeBytes = new byte[client.SendBufferSize];
        }

        public void InitReadArray() { 
            readBytes = new byte[client.ReceiveBufferSize];
        }
        public void InitWriteArray() { 
            writeBytes = new byte[client.SendBufferSize];
        }
    }
}
