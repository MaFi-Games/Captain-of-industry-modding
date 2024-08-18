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
    public class ProtocolProductProto : ProductProto
    {
        public static readonly ProductType ProductType = new ProductType(typeof(ProtocolProductProto));

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
    }
}
