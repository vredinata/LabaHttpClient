﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Runtime.Serialization.Json;
using Newtonsoft.Json;
using System.Threading;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace laba_http_client
{
    //serializable class for authentication
    
    public class AuthData
    {
         
        public string login { get; set; }
        public string password { get; set; }
        public string token { get; set; }
       // public string data { get; set; }

        public AuthData() { }

        // can set the password in clear text, in use md5 hashing
        //public AuthData(string name, string passwd, string token, string data)
        public AuthData(string name, string passwd )
        {
            login = name;
            password = getMd5Hash(passwd);
          //  this.token = token;
         //   this.data = data;
        }

        //to obtain the password hash 
        public static string getMd5Hash(string input)
        {
            MD5 md5Hasher = MD5.Create();
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(input));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }
    }
    
    // serializable class forwarding "feature vector" from Openbr and link to the user id in the social network.
   
    public class OpbrData
    {
         
        public string link { get; set; }
        public string base64string { get; set; }
        //[DataMember(Name = "bytemas")]
        //public byte [] Bytemas { get; set; }

        public OpbrData() { }

        public OpbrData(string link, string str)
        {
            this.link = link;
            base64string = str;
        }
    }

    
    public class AesEncruptAlg
    {
        //private byte[] iv;
        ////ключ шифрования
        //private byte[] key;

        //public byte[] Iv
        //{ set
        //    { iv = value; }
        //}

        //public byte[] Key
        //{ set
        //    { key = value; }
        //}

        //public AesEncruptAlg()
        //{
        //    iv= new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        // 0x00, 0x00, 0x00, 0x00, 0x00 };
        //    key = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B,
        //0x0C, 0x0D, 0x0E, 0x0F, 0x10, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B,
        //0x0C, 0x0D, 0x0E, 0x0F, 0x10 };

        //}

        //public AesEncruptAlg(byte[] I, byte[] K)
        //{
        //    iv = I;
        //    key = K;
        //}
        //           = { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B,
        //0x0C, 0x0D, 0x0E, 0x0F, 0x10, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B,
        //0x0C, 0x0D, 0x0E, 0x0F, 0x10 };

        public static byte[] Encrypt(byte[] ENC, byte[] AES_KEY, byte[] AES_IV)
        {
            using (Aes AES = Aes.Create())
            {
                AES.KeySize = 256;
                AES.BlockSize = 128;
                AES.Padding = PaddingMode.None;

                MemoryStream MS = new MemoryStream();
                CryptoStream CS = new CryptoStream(MS, AES.CreateEncryptor(AES_KEY, AES_IV), CryptoStreamMode.Write);

                CS.Write(ENC, 0, ENC.Length);
                CS.Close();

                return MS.ToArray();
            }
        }
        public static byte[] Decrypt(byte[] DEC, byte[] AES_KEY, byte[] AES_IV)
        {
            using (Aes AES = Aes.Create())
            {
                AES.KeySize = 256;
                AES.BlockSize = 128;
                AES.Padding = PaddingMode.None;

                MemoryStream MS = new MemoryStream();
                CryptoStream CS = new CryptoStream(MS, AES.CreateDecryptor(AES_KEY, AES_IV), CryptoStreamMode.Write);

                CS.Write(DEC, 0, DEC.Length);
                CS.Close();

                return MS.ToArray();
            }
        }

    }


    class HttpsClient
    {
        //public string toadress { get; set; }
        public static ManualResetEvent allDone = new ManualResetEvent(false);
        const int BUFFER_SIZE = 1024;

        static void Main(string[] args)
        {
         //   Uri uri = new Uri("https://toadress.com");
            string sendadr = "https://toadress.com";
            string FilePath = "file_path";
            string vklink = "https://vk.com/anyuser";
            string Subsystem2Ansver;
            string filejson="";

            int EncAlgType = 0; //0 -aes
            byte[] iv = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00 };
            byte[] key = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B,
            0x0C, 0x0D, 0x0E, 0x0F, 0x10, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B,
            0x0C, 0x0D, 0x0E, 0x0F, 0x10 };


            printLog("I started to work");
            try
            {
                Subsystem2Ansver = AuthPost(sendadr); //возвращает ответ - токен?
                printLog("Subsystem2Ansver: authorization "+Subsystem2Ansver);

                if (EncAlgType == 0)  // несколько видов шифрования??
                { filejson = Convert.ToBase64String(AesEncruptAlg.Encrypt(createByteArr(FilePath), key, iv)); }
                else
                { throw new ArgumentNullException("No encription alg"); }
               // string filejson = Convert.ToBase64String( AesEncruptAlg.Encrypt(createByteArr(FilePath), key, iv));  
                OpbrData opbrdata = new OpbrData(vklink,filejson);
                string serialized = JsonConvert.SerializeObject(opbrdata);
                Subsystem2Ansver = HttpPost(sendadr, serialized);
                printLog("Subsystem2Ansver: authorization " + Subsystem2Ansver);

            }
            catch (WebException ex)
            {
                // получаем статус исключения
                WebExceptionStatus status = ex.Status;

                if (status == WebExceptionStatus.ProtocolError)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)ex.Response;
                    printLog("The error status code:" + (int)httpResponse.StatusCode + " " + httpResponse.StatusCode);
                }
            }
            catch (Exception e)
            {
                printLog(e.Message);
            }
            
            printLog("Exit");
        }

        
        public static string AuthPost(string sendadr )
        {
           // string sendadr = "https://toadress.com";
            AuthData au = new AuthData("Subsystem1", "password");
            string serialized = JsonConvert.SerializeObject(au);
            string ansver = HttpPost(sendadr, serialized);
            // printLog("Subsystem2Ansver: authorization " + Subsystem2Ansver);
            return ansver;
        }

        //public static void AsynchPost(string URI, string jsonstr, string wtoken)
        public static void AsynchPost(string URI, string jsonstr) //asynch???
        {
            // Create the request object.
            WebRequest wreq = WebRequest.Create(URI);
            wreq.ContentType = "application/json";  //"application/x-www-form-urlencoded"
            wreq.Method = "POST";
            //req.Headers.Add("Token: " + wtoken);
            //We need to count how many bytes we're sending. Post'ed Faked Forms should be name=value&
            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(jsonstr); //UTF8 - ?
            wreq.ContentLength = bytes.Length;
            //wreq.ContentLength = jsonstr.Length;

            IAsyncResult getRequestStream = wreq.BeginGetRequestStream(null, null);
            var writer = new StreamWriter(wreq.EndGetRequestStream(getRequestStream));
            writer.Write(jsonstr);
            writer.Close();

           //Посылаем запрос
            wreq.BeginGetResponse(OnAsyncCallback, wreq );
            
        }

        private static void OnAsyncCallback(IAsyncResult asyncResult)
        {
            var httpWebRequest = (HttpWebRequest)asyncResult.AsyncState;
            WebResponse response = httpWebRequest.EndGetResponse(asyncResult);
            var reader = new StreamReader(response.GetResponseStream());
            string str = reader.ReadToEnd();
            printLog("received the server response: "+str);  //как понять какой запрос? счетчик?
        }

        public static string HttpGet(string URI)
        {
            System.Net.WebRequest req = System.Net.WebRequest.Create(URI);
            System.Net.WebResponse resp = req.GetResponse();
            System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());
            return sr.ReadToEnd().Trim();
        }

        // public static HttpStatusCode HttpPost(string URI, string jsonstr, string wtoken)
        public static string HttpPost(string URI, string jsonstr)
        {
            System.Net.WebRequest req = System.Net.WebRequest.Create(URI);
             
            //Add these, as we're doing a POST
            req.ContentType = "application/json";  //"application/x-www-form-urlencoded"
            req.Method = "POST";
            //req.Headers.Add("Token: " + wtoken);
            //We need to count how many bytes we're sending. Post'ed Faked Forms should be name=value&
            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(jsonstr); //UTF8 - ?
            req.ContentLength = bytes.Length;
            System.IO.Stream os = req.GetRequestStream();
            os.Write(bytes, 0, bytes.Length); //Push it out there
            os.Close();
            System.Net.WebResponse resp = req.GetResponse();
            //HttpWebResponse httpResponse = (HttpWebResponse)resp;
            //return httpResponse.StatusCode;

            System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());
            return sr.ReadToEnd().Trim();

        }


        //возвращает размер файла по ссылке если он существует, иначе ошибка.
        public static int findFileSize(string readPath)   
        {
            int FileSize = 0;
            FileInfo FileInf = new FileInfo( readPath);
            if (FileInf.Exists)
            { FileSize = (int)FileInf.Length; }
            else { throw new ArgumentNullException("Opbr file size"); }

            return FileSize;
        }
    
        //считывает все данные бинарного файла по ссылке в массив byte[]
        public static byte[] createByteArr(string readPath) 
        {
            byte[] FileClearData;
            int fsz =  findFileSize(readPath);
            FileClearData = new byte[fsz];
            using (BinaryReader reader = new BinaryReader(File.Open( readPath, FileMode.Open)))
            {
                FileClearData = reader.ReadBytes(fsz);
            }
            return FileClearData;
        }
 
        // заносит строку mes в лог файл.
        public static void printLog(string mes)
        {
            string logPath =  "log.txt";
            string logMessage = "# " + "HttpsClient: " + DateTime.Now + " ";
            System.IO.File.AppendAllText(logPath, logMessage + mes + "\n");         
        }

    }
}
