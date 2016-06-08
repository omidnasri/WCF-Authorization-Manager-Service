//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

using System;
using System.IO;
using System.IO.Compression;
using System.ServiceModel.Channels;

namespace GlobalCommonLib
{
    //This class is used to create the custom encoder (GZipMessageEncoder)
    internal class GZipMessageEncoderFactory : MessageEncoderFactory
    {
        MessageEncoderFactory innerFactory;
        MessageEncoder encoder;
        CompressionAlgorithm compressionAlgorithm;

        //The GZip encoder wraps an inner encoder
        //We require a factory to be passed in that will create this inner encoder
        public GZipMessageEncoderFactory(MessageEncoderFactory messageEncoderFactory, CompressionAlgorithm compressionAlgorithm)
        {
            if (messageEncoderFactory == null)
                throw new ArgumentNullException("messageEncoderFactory", "A valid message encoder factory must be passed to the CompressionEncoder");
            encoder = new GZipMessageEncoder(messageEncoderFactory.Encoder, compressionAlgorithm);
            this.compressionAlgorithm = compressionAlgorithm;
            this.innerFactory = messageEncoderFactory;
        }

        //The service framework uses this property to obtain an encoder from this encoder factory
        public override MessageEncoder Encoder
        {
            get { return encoder; }
        }

        public override MessageVersion MessageVersion
        {
            get { return encoder.MessageVersion; }
        }

        public override MessageEncoder CreateSessionEncoder()
        {
            return new GZipMessageEncoder(this.innerFactory.CreateSessionEncoder(), this.compressionAlgorithm);
        }

        //This is the actual GZip encoder
        class GZipMessageEncoder : MessageEncoder
        {
            const string GZipContentType = "application/x-gzip";
            const string DeflateContentType = "application/x-deflate";

            //This implementation wraps an inner encoder that actually converts a WCF Message
            //into textual XML, binary XML or some other format. This implementation then compresses the results.
            //The opposite happens when reading messages.
            //This member stores this inner encoder.
            MessageEncoder innerEncoder;

            CompressionAlgorithm compressionAlgorithm;

            //We require an inner encoder to be supplied (see comment above)
            internal GZipMessageEncoder(MessageEncoder messageEncoder, CompressionAlgorithm compressionAlgorithm)
                : base()
            {
                if (messageEncoder == null)
                    throw new ArgumentNullException("messageEncoder", "A valid message encoder must be passed to the CompressionEncoder");
                innerEncoder = messageEncoder;
                this.compressionAlgorithm = compressionAlgorithm;
            }
            public override string ContentType
            {
                get 
                {
                    return this.compressionAlgorithm == CompressionAlgorithm.GZip ?
                        GZipContentType : DeflateContentType;
                }
            }
            public override string MediaType
            {
                get { return this.ContentType; }
            }
            //SOAP version to use - we delegate to the inner encoder for this
            public override MessageVersion MessageVersion
            {
                get { return innerEncoder.MessageVersion; }
            }
            //Helper method to compress an array of bytes
            static ArraySegment<byte> CompressBuffer(ArraySegment<byte> buffer, BufferManager bufferManager, int messageOffset, CompressionAlgorithm compressionAlgorithm)
            {
                buffer = Cryptographer.EncryptBuffer(buffer, bufferManager, messageOffset);

                var memoryStream = new MemoryStream();
                
                using (Stream compressedStream = compressionAlgorithm == CompressionAlgorithm.GZip ? 
                    (Stream)new GZipStream(memoryStream, CompressionMode.Compress, true) :
                    (Stream)new DeflateStream(memoryStream, CompressionMode.Compress, true))
                {
                    compressedStream.Write(buffer.Array, buffer.Offset, buffer.Count);
                }

                byte[] compressedBytes = memoryStream.ToArray();
                int totalLength = messageOffset + compressedBytes.Length;
                byte[] bufferedBytes = bufferManager.TakeBuffer(totalLength);

                Array.Copy(compressedBytes, 0, bufferedBytes, messageOffset, compressedBytes.Length);

                bufferManager.ReturnBuffer(buffer.Array);
                buffer = new ArraySegment<byte>(bufferedBytes, messageOffset, compressedBytes.Length);
               
                return buffer;
            }
            //Helper method to decompress an array of bytes
            static ArraySegment<byte> DecompressBuffer(ArraySegment<byte> buffer, BufferManager bufferManager, CompressionAlgorithm compressionAlgorithm)
            {
                var memoryStream = new MemoryStream(buffer.Array, buffer.Offset, buffer.Count);
                var decompressedStream = new MemoryStream();
                int totalRead = 0;
                const int blockSize = 1024;
                byte[] tempBuffer = bufferManager.TakeBuffer(blockSize);
                using (Stream compressedStream = compressionAlgorithm == CompressionAlgorithm.GZip ?
                    (Stream)new GZipStream(memoryStream, CompressionMode.Decompress) :
                    (Stream)new DeflateStream(memoryStream, CompressionMode.Decompress))
                {
                    while (true)
                    {
                        int bytesRead = compressedStream.Read(tempBuffer, 0, blockSize);
                        if (bytesRead == 0)
                            break;
                        decompressedStream.Write(tempBuffer, 0, bytesRead);
                        totalRead += bytesRead;
                    }
                }
                bufferManager.ReturnBuffer(tempBuffer);

                byte[] decompressedBytes = decompressedStream.ToArray();
                byte[] bufferManagerBuffer = bufferManager.TakeBuffer(decompressedBytes.Length + buffer.Offset);
                Array.Copy(buffer.Array, 0, bufferManagerBuffer, 0, buffer.Offset);
                Array.Copy(decompressedBytes, 0, bufferManagerBuffer, buffer.Offset, decompressedBytes.Length);

                buffer = new ArraySegment<byte>(bufferManagerBuffer, buffer.Offset, decompressedBytes.Length);
                buffer = Cryptographer.DecryptBuffer(buffer, bufferManager);
                bufferManager.ReturnBuffer(buffer.Array);

                return buffer;
            }
            //One of the two main entry points into the encoder. Called by WCF to decode a buffered byte array into a Message.
            public override Message ReadMessage(ArraySegment<byte> buffer, BufferManager bufferManager, string contentType)
            {
                //Decompress the buffer
                ArraySegment<byte> decompressedBuffer = DecompressBuffer(buffer, bufferManager, this.compressionAlgorithm);
                //Use the inner encoder to decode the decompressed buffer
                Message returnMessage = innerEncoder.ReadMessage(decompressedBuffer, bufferManager);
                returnMessage.Properties.Encoder = this;
                return returnMessage;
            }
            //One of the two main entry points into the encoder. Called by WCF to encode a Message into a buffered byte array.
            public override ArraySegment<byte> WriteMessage(Message message, int maxMessageSize, BufferManager bufferManager, int messageOffset)
            {
                //Use the inner encoder to encode a Message into a buffered byte array
                ArraySegment<byte> buffer = innerEncoder.WriteMessage(message, maxMessageSize, bufferManager, 0);
                //Compress the resulting byte array
                //System.Console.WriteLine("Original size: {0}", buffer.Count);
                buffer = CompressBuffer(buffer, bufferManager, messageOffset, this.compressionAlgorithm);
                //System.Console.WriteLine("Compressed size: {0}", buffer.Count);
                return buffer;
            }
            public override Message ReadMessage(System.IO.Stream stream, int maxSizeOfHeaders, string contentType)
            {
                //Pass false for the "leaveOpen" parameter to the GZipStream constructor.
                //This will ensure that the inner stream gets closed when the message gets closed, which
                //will ensure that resources are available for reuse/release.
                Stream compressedStream = compressionAlgorithm == CompressionAlgorithm.GZip ?
                                    (Stream)new GZipStream(stream, CompressionMode.Decompress, false) :
                                    (Stream)new DeflateStream(stream, CompressionMode.Decompress, false);
                return innerEncoder.ReadMessage(compressedStream, maxSizeOfHeaders);
            }
            public override void WriteMessage(Message message, System.IO.Stream stream)
            {
                using (Stream compressedStream = compressionAlgorithm == CompressionAlgorithm.GZip ?
                    (Stream)new GZipStream(stream, CompressionMode.Decompress) :
                    (Stream)new DeflateStream(stream, CompressionMode.Decompress))
                {
                    innerEncoder.WriteMessage(message, compressedStream);
                }

                // innerEncoder.WriteMessage(message, gzStream) depends on that it can flush data by flushing 
                // the stream passed in, but the implementation of GZipStream.Flush will not flush underlying
                // stream, so we need to flush here.
                stream.Flush();
            }
        }
    }
}