using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.ServiceModel.Channels;
using System.Text;
using System.Xml;

namespace GlobalCommonLib
{
    public class CryptographerBase
    {
        public const string KeyElementName = "EncryptionKey";
        public const string EncryptedElementName = "Encrypted";
        //The elements that will be encrypted when the contentEncryption is set to "Credentials" or "All".
        public const string CredentialsElementName = "Credentials";
        public const string AllElementName = "s:Envelope";
        public const int AesKeySize = 256; //The minimum size of the key is 128 bits, and the maximum size is 256 bits. [2]
        public const int RsaKeySize = 1024; //The RSACryptoServiceProvider supports key lengths from 384 bits to 16384 bits in increments of 8 bits if you have the Microsoft Enhanced Cryptographic Provider installed. It supports key lengths from 384 bits to 512 bits in increments of 8 bits if you have the Microsoft Base Cryptographic Provider installed.[1]
        protected const bool Content = false;//Encrypt only the content (true) or the node also (false); it seems not to function on true.?!?
        //on server: generates a new public/private key at its instantiation
        //on client: must be initiated with the public key of the server 
        public static RSACryptoServiceProvider RsaServiceProvider { get; private set; }
        //this is necessary on a multithreading environment to pair the request and reply (both on server and client)
        //on client: contains the message id and the key used to encrypt the request message; it will be used to decrypt the reply message.
        //on server: contains the message is and the key extracted from the encrypted message; it will be used to encrypt the reply message.
        //protected static ConcurrentDictionary<string, byte[]> AesKeys { get; private set; }
        static CryptographerBase()
        {
            RsaServiceProvider = new RSACryptoServiceProvider(RsaKeySize);
            //AesKeys = new ConcurrentDictionary<string, byte[]>();
        }
    }

    public class Cryptographer : CryptographerBase
    {
        public static void Encrypt(XmlDocument xmlDoc, string elementToEncrypt)
        {
            XmlNodeList elementsToEncrypt = xmlDoc.GetElementsByTagName(elementToEncrypt);
            if (elementsToEncrypt.Count == 0) return;
            var aesServiceProvider = new AesCryptoServiceProvider {KeySize = 256};
            aesServiceProvider.GenerateKey();
            XmlNode idNode = xmlDoc.GetElementsByTagName("a:MessageID")[0];
            string id = string.Empty;
            if (null != idNode)
                id =  BitConverter.ToString( Encoding.UTF8.GetBytes(idNode.InnerText));
             id += "||" + BitConverter.ToString(aesServiceProvider.Key);
            //AesKeys.TryAdd(id, );
            var xmlElementToEncrypt = (XmlElement)elementsToEncrypt[0];
            var encryptedXml = new EncryptedXml();
            byte[] encryptedElement = encryptedXml.EncryptData(xmlElementToEncrypt, aesServiceProvider, Content);
            var encryptedData = new EncryptedData
                                    {
                                        Type = EncryptedXml.XmlEncElementUrl,
                                        EncryptionMethod = new EncryptionMethod(EncryptedXml.XmlEncAES256Url)
                                    };
            var encryptedKey = new EncryptedKey
                                   {
                                       CipherData =
                                           new CipherData(EncryptedXml.EncryptKey(aesServiceProvider.Key,
                                                                                  RsaServiceProvider, Content)),
                                       EncryptionMethod = new EncryptionMethod(EncryptedXml.XmlEncRSA15Url)
                                   };
            encryptedData.KeyInfo = new KeyInfo();
            encryptedKey.KeyInfo.AddClause(new KeyInfoName(KeyElementName));
            encryptedData.KeyInfo.AddClause(new KeyInfoEncryptedKey(encryptedKey));
            encryptedData.CipherData.CipherValue = encryptedElement;
            encryptedData.Id = id;
            EncryptedXml.ReplaceElement(xmlElementToEncrypt, encryptedData, Content);
        }
        public static void Decrypt(XmlDocument xmlDoc)
        {

            XmlNodeList encryptedElements = xmlDoc.GetElementsByTagName("EncryptedData");
            if (encryptedElements.Count == 0)
                return;

            var aesServiceProvider = new AesCryptoServiceProvider();
            var encryptedElement = (XmlElement)encryptedElements[0];

            var encryptedData = new EncryptedData();
            encryptedData.LoadXml(encryptedElement);

            var keyinString = encryptedData.Id.Split(new string[] {"||"}, StringSplitOptions.None).ElementAt(1);
            byte[] key = keyinString.Split('-').Select(b => Convert.ToByte(b, 16)).ToArray();
            //AesKeys.TryRemove(encryptedData.Id, out key);
            aesServiceProvider.Key = key;


            var encryptedXml = new EncryptedXml();
            encryptedXml.ReplaceData(encryptedElement, encryptedXml.DecryptData(encryptedData, aesServiceProvider));
        }
        public static ArraySegment<byte> EncryptBuffer(ArraySegment<byte> buffer, BufferManager bufferManager, int messageOffset, string elementToEncrypt = "s:Envelope")
        {
            var xmlDoc = new XmlDocument();

            using (var memoryStream = new MemoryStream(buffer.Array, buffer.Offset, buffer.Count))
            {
                xmlDoc.Load(memoryStream);                
            }

            Cryptographer.Encrypt(xmlDoc, elementToEncrypt);
            byte[] encryptedBytes = Encoding.UTF8.GetBytes(xmlDoc.OuterXml);
            byte[] bufferedBytes = bufferManager.TakeBuffer(encryptedBytes.Length);
            Array.Copy(encryptedBytes, 0, bufferedBytes, 0, encryptedBytes.Length);
            bufferManager.ReturnBuffer(buffer.Array);

            var byteArray = new ArraySegment<byte>(bufferedBytes, messageOffset, encryptedBytes.Length);

            return byteArray;
        }
        public static ArraySegment<byte> DecryptBuffer(ArraySegment<byte> buffer, BufferManager bufferManager)
        {
            var xmlDoc = new XmlDocument();
            using (var memoryStream = new MemoryStream(buffer.Array, buffer.Offset, buffer.Count))
            {
                xmlDoc.Load(memoryStream);
            }

            Cryptographer.Decrypt(xmlDoc);
            byte[] decryptedBytes = Encoding.UTF8.GetBytes(xmlDoc.OuterXml);

            byte[] bufferedBytes = bufferManager.TakeBuffer(decryptedBytes.Length);
            Array.Copy(decryptedBytes, 0, bufferedBytes, 0, decryptedBytes.Length);
            bufferManager.ReturnBuffer(buffer.Array);
            var byteArray = new ArraySegment<byte>(bufferedBytes, 0, decryptedBytes.Length);

            string s = Encoding.UTF8.GetString(byteArray.Array, byteArray.Offset, byteArray.Count);
            return byteArray;

        }
    }
}