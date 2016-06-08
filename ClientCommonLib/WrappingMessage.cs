using System;
using System.IO;
using System.ServiceModel.Channels;
using System.Text;
using System.Xml;

namespace ClientCommonLib
{

    public class WrappingMessage : Message
    {
        readonly Message _innerMsg;
        readonly MessageBuffer _msgBuffer;

        public override MessageHeaders Headers => _innerMsg.Headers;
        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
            _innerMsg.WriteBodyContents(writer);
        }
        public override MessageProperties Properties => _innerMsg.Properties;
        public override MessageVersion Version => _innerMsg.Version;

        public WrappingMessage(Message inner)
        {
            this._innerMsg = inner;
            _msgBuffer = _innerMsg.CreateBufferedCopy(int.MaxValue);
            _innerMsg = _msgBuffer.CreateMessage();
        }
        protected override void OnWriteMessage(XmlDictionaryWriter writer)
        {
            // write message to the actual encoder....
            base.OnWriteMessage(writer);
            writer.Flush();

            // write message to MemoryStream to get it's size.
            var copy = _msgBuffer.CreateMessage();
            DumpEncoderSize(writer, copy);
        }
        private static void DumpEncoderSize(System.Xml.XmlDictionaryWriter writer, Message copy)
        {
            var ms = new MemoryStream();

            string configuredEncoder = string.Empty;
            if (writer is IXmlTextWriterInitializer)
            {
                var w = (IXmlTextWriterInitializer)writer;
                w.SetOutput(ms, Encoding.UTF8, true);
                configuredEncoder = "Text";
            }
            else if (writer is IXmlMtomWriterInitializer)
            {
                var w = (IXmlMtomWriterInitializer)writer;
                w.SetOutput(ms, Encoding.UTF8, int.MaxValue, "", null, null, true, false);
                configuredEncoder = "MTOM";
            }
            else if (writer is IXmlBinaryWriterInitializer)
            {
                var w = (IXmlBinaryWriterInitializer)writer;
                w.SetOutput(ms, null, null, false);
                configuredEncoder = "Binary";
            }

            copy.WriteMessage(writer);
            writer.Flush();
            var size = ms.Position;

            Console.WriteLine("Message size using configured ({1}) encoder {0}", size, configuredEncoder);
        }
    }
}