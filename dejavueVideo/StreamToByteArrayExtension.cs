using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace sakerunblobtrigger
{
    public static class StreamToByteArrayExtension
    {
        public static byte[] streamToByteArray(this Stream input)
        {
            MemoryStream ms = new MemoryStream();
            input.CopyTo(ms);
            return ms.ToArray();
        }
    }
}
