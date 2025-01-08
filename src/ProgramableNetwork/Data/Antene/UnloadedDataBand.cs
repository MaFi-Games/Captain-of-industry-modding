using Mafi;
using Mafi.Collections;
using Mafi.Core.Entities;
using Mafi.Serialization;
using System.Collections.Generic;
using System.IO;

namespace ProgramableNetwork
{
    public class UnloadedDataBand : IDataBand
    {
        private string protoId;
        private byte[] bandData;

        public UnloadedDataBand(string protoId, byte[] bandData)
        {
            this.protoId = protoId;
            this.bandData = bandData;
        }

        public EntityContext Context { get; set; }

        public DataBandProto Prototype { get; set; }

        public IEnumerable<IDataBandChannel> Channels { get; set; }

        public void CreateChannel()
        {
            // nothing to do
        }

        public void Update()
        {
            // nothing to do
        }

        public IDataBand Deserialize(EntityContext context, int loadedSaveVersion)
        {
            BlobReader blobReader = new BlobReader(new MemoryStream(bandData, 0, bandData.Length), loadedSaveVersion);

            DataBandProto dataBandProto = context.ProtosDb.Get<DataBandProto>(new Mafi.Core.Prototypes.Proto.ID(protoId)).Value;
            IDataBand dataBand = dataBandProto.Deserializer.Invoke(blobReader);
            blobReader.FinalizeLoading(Option.None);

            Log.Info($"Databand {protoId} was deserialized");

            return dataBand;
        }

        public void initContext()
        {
            // nothing to do
        }

        public void RemoveChannel(IDataBandChannel channel)
        {
            // nothing to do
        }
    }
}