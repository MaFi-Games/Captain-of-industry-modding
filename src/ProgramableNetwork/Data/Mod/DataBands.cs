using Mafi.Core.Mods;
using Mafi.Core.Entities;
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
    public class DataBands : AValidatedData
    {
        public static readonly Proto.ID UnknownDataBand = new Proto.ID("ProgramableNetwork_DataBand_Unknown");
        public static DataBandProto UnknownDataBandProto { get; private set; }

        protected override void RegisterDataInternal(ProtoRegistrator registrator)
        {
            registrator.PrototypesDb.Add(UnknownDataBandProto = new DataBandProto(
                id: UnknownDataBand,
                strings: new Proto.Str(
                    name: Loc.Str(UnknownDataBand.Value + "_name", "Unknown", "unkonwn band name"),
                    descShort: Loc.Str(UnknownDataBand.Value + "_shortDesc", "Received signal is unrecognizable", "unkonwn band description")
                ),
                channels: 0,
                channelDisplay: (i) => "~"));

            // known signals
            registrator.PrototypesDb.Add(new DataBandProto(
                id: UnknownDataBand,
                strings: new Proto.Str(
                    name: Loc.Str("ProgramableNetwork_DataBand_FM_name", "FM", "Commonly known radio signal name"),
                    descShort: Loc.Str("ProgramableNetwork_DataBand_FM_shortDesc", "Statndard Frquency Modulated signal used in classic radios. The channels are from 85.5 to 108.0 kHz and steping by 500 Hz (total 45 channels)", "Commonly known radio signal description")
                ),
                channels: 45,
                channelDisplay: (i) => (i * (0.5f).ToFix32() + (85.5f).ToFix32()).ToStringRounded(1) + " kHz",
                buttons: (builder, container, dataBand, refresh) =>
                {
                    builder.NewBtnGeneral("NstartkHz")
                        .SetText("|<")
                        .OnClick(() =>
                        {
                            dataBand.Channel = 0;
                            refresh();
                        })
                        .SetSize(20, 20)
                        .AppendTo(container);
                    builder.NewBtnGeneral("N-5kHz")
                        .SetText("<<")
                        .OnClick(() =>
                        {
                            dataBand.Channel -= 10;
                            if (dataBand.Channel < 0)
                                dataBand.Channel += 45;
                            refresh();
                        })
                        .SetSize(20, 20)
                        .AppendTo(container);

                    builder.NewBtnGeneral("N-0.5kHz")
                        .SetText("<")
                        .OnClick(() =>
                        {
                            dataBand.Channel -= 1;
                            if (dataBand.Channel < 0)
                                dataBand.Channel += 45;
                            refresh();
                        })
                        .SetSize(20, 20)
                        .AppendTo(container);

                    builder.NewBtnGeneral("N+0.5kHz")
                        .SetText(">")
                        .OnClick(() =>
                        {
                            dataBand.Channel += 1;
                            if (dataBand.Channel > 44)
                                dataBand.Channel -= 45;
                            refresh();
                        })
                        .SetSize(20, 20)
                        .AppendTo(container);

                    builder.NewBtnGeneral("N+5kHz")
                        .SetText(">>")
                        .OnClick(() =>
                        {
                            dataBand.Channel += 10;
                            if (dataBand.Channel > 44)
                                dataBand.Channel -= 45;
                            refresh();
                        })
                        .SetSize(20, 20)
                        .AppendTo(container);

                    builder.NewBtnGeneral("NendkHz")
                        .SetText(">|")
                        .OnClick(() =>
                        {
                            dataBand.Channel = 44;
                            refresh();
                        })
                        .SetSize(20, 20)
                        .AppendTo(container);
                }
                ));
        }
    }
}
