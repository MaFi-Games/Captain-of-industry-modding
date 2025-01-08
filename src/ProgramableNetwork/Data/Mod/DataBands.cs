using Mafi.Core.Mods;
using Mafi.Core.Prototypes;
using Mafi.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mafi;
using Mafi.Unity.UserInterface;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UserInterface.Components;
using Mafi.Unity;
using Mafi.Unity.UiFramework;

namespace ProgramableNetwork
{
    public partial class DataBands : AValidatedData
    {
        public static readonly Proto.ID DataBand_Unknown = new Proto.ID("ProgramableNetwork_DataBand_Unknown");
        public static readonly Proto.ID DataBand_FM = new Proto.ID("ProgramableNetwork_DataBand_FM");

        protected override void RegisterDataInternal(ProtoRegistrator registrator)
        {
            registrator.PrototypesDb.Add(DataBandProto.Create<UnkownnDataBandType, IDataBandChannel>(
                id: DataBand_Unknown,
                strings: Proto.CreateStr(DataBand_Unknown, "Unknown", "Received signal is unrecognizable", "unkonwn band description"),
                (context, proto) => new UnkownnDataBandType(context, proto),
                channels: 0,
                (c0, c1) => false,
                UnkownnDataBandType.Serialize,
                UnkownnDataBandType.Deserialize,
                channelDisplay: (c,i) => "~"));

            // known signals
            registrator.PrototypesDb.Add(DataBandProto.Create<FMDataBand, FMDataBandChannel>(
                id: DataBand_FM,
                strings: Proto.CreateStr(DataBand_FM, "FM", "Standard Frquency Modulated signal used in classic radios. The channels are from 85.5 to 108.0 kHz and steping by 500 Hz (total 45 channels), default redirection distance is 1000 metres, Antena tower may extend it", "Commonly known radio signal description"),
                (context, proto) => new FMDataBand(context, proto),
                channels: 45,
                (c0, c1) => c0.Index != c1.Index,
                FMDataBand.Serialize,
                FMDataBand.Deserialize,
                channelDisplay: (c, i) => ((171 + i.Index).ToFix32() * 0.5f.ToFix32()).ToStringRounded(1) + " kHz",
                buttons: (builder, container, dataBand, refresh) =>
                {
                    builder.NewBtnGeneral("NstartkHz")
                        .SetText("|<")
                        .OnClick(() =>
                        {
                            dataBand.Index = 0;
                            refresh();
                        })
                        .SetSize(20, 20)
                        .AppendTo(container);
                    builder.NewBtnGeneral("N-5kHz")
                        .SetText("<<")
                        .OnClick(() =>
                        {
                            dataBand.Index -= 10;
                            if (dataBand.Index < 0)
                                dataBand.Index += 45;
                            refresh();
                        })
                        .SetSize(20, 20)
                        .AppendTo(container);

                    builder.NewBtnGeneral("N-0.5kHz")
                        .SetText("<")
                        .OnClick(() =>
                        {
                            dataBand.Index -= 1;
                            if (dataBand.Index < 0)
                                dataBand.Index += 45;
                            refresh();
                        })
                        .SetSize(20, 20)
                        .AppendTo(container);

                    builder.NewBtnGeneral("N+0.5kHz")
                        .SetText(">")
                        .OnClick(() =>
                        {
                            dataBand.Index += 1;
                            if (dataBand.Index > 44)
                                dataBand.Index -= 45;
                            refresh();
                        })
                        .SetSize(20, 20)
                        .AppendTo(container);

                    builder.NewBtnGeneral("N+5kHz")
                        .SetText(">>")
                        .OnClick(() =>
                        {
                            dataBand.Index += 10;
                            if (dataBand.Index > 44)
                                dataBand.Index -= 45;
                            refresh();
                        })
                        .SetSize(20, 20)
                        .AppendTo(container);

                    builder.NewBtnGeneral("NendkHz")
                        .SetText(">|")
                        .OnClick(() =>
                        {
                            dataBand.Index = 44;
                            refresh();
                        })
                        .SetSize(20, 20)
                        .AppendTo(container);
                }
                ));
        }
    }
}
