using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Netbar_manager_console {
    class Util {
        public static String getFileMD5(string path) {
            string strResult = "";
            string strHashData = "";

            byte[] arrbytHashValue;
            System.IO.FileStream oFileStream = null;

            System.Security.Cryptography.MD5CryptoServiceProvider oMD5Hasher =
                       new System.Security.Cryptography.MD5CryptoServiceProvider();

            try {
                oFileStream = new System.IO.FileStream(path, System.IO.FileMode.Open,
                      System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite);
                arrbytHashValue = oMD5Hasher.ComputeHash(oFileStream);//计算指定Stream 对象的哈希值
                oFileStream.Close();
                //由以连字符分隔的十六进制对构成的String，其中每一对表示value 中对应的元素；例如“F-2C-4A”
                strHashData = System.BitConverter.ToString(arrbytHashValue);
                //替换-
                strHashData = strHashData.Replace("-", "");
                strResult = strHashData;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
 
            return strResult;
        }

        public static void debug_info(string s){
            Console.WriteLine("[DEBUG]: "+s);
        }
        public static void info(string s) {
            Console.WriteLine("[INFO]: "+s);
        }
        public static void error_info(string s) {
            Console.WriteLine("[ERROR]: "+s);
        }
    }
}
