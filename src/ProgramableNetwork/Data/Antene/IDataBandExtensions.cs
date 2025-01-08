using Mafi;
using Mafi.Collections;
using Mafi.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgramableNetwork
{
    public static class IDataBandExtensions
    {
        public static void Serialize(this IDataBand dataBand, BlobWriter writer)
        {
            writer.WriteString(dataBand.Prototype.Id.Value);
            MemoryBlobWriter subwriter = new MemoryBlobWriter();
            dataBand.Prototype.Serializer(dataBand, subwriter);
            subwriter.FinalizeSerialization();
            var data = subwriter.ToArray().ToArray();
            Log.Info($"Databand {dataBand.Prototype.Id.Value} was serialized with {data.Length} bytes");
            writer.WriteArray(data);
        }

        public static IDataBand ReadDataBand(this BlobReader reader)
        {
            string protoId = reader.ReadString();
            byte[] data = reader.ReadArray<byte>();
            Log.Info($"Databand {protoId} was loaded with {data.Length} bytes");
            return new UnloadedDataBand(protoId, data);
        }
    }
}
