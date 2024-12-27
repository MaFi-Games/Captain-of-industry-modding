using Mafi;
using Mafi.Core.Localization.Quantity;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgramableNetwork
{
    public class ProtocolProductProto : ProductProto, IComparable<ProtocolProductProto>
    {
        public static readonly ProductType ProductType = new ProductType(typeof(ProtocolProductProto));

        public new static readonly ProtocolProductProto Phantom = Proto.RegisterPhantom(new ProtocolProductProto(new ID(ProductProto.PHANTOM_PRODUCT_ID.Value + "PROTOCOL__"), Str.Empty, Gfx.Empty));

        public ProtocolProductProto(ID id, Str strings, Gfx graphics)
            : base(id,
                   strings,
                   maxQuantityPerTransportedProduct: Quantity.One,
                   isStorable: false,
                   canBeDiscarded: true,
                   isWaste: false,
                   graphics,
                   doNotNormalize: true,
                   isExcludedFromStats: true,
                   radioactivity: 0,
                   pinToHomeScreenByDefault: false,
                   isRecyclable: false,
                   doNotTrackSourceProducts: true,
                   sourceProduct: null,
                   sourceProductQuantity: null,
                   quantityFormatter: null,
                   new List<Tag>())
        {
        }

        public int CompareTo(ProtocolProductProto other)
        {
            return CompareTo((Proto)other);
        }

        public override string ToString()
        {
            return $"{base.Id} (protocol)";
        }
    }
}
