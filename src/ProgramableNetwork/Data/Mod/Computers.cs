using Mafi;
using Mafi.Base;
using Mafi.Collections;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core.Entities.Static;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Factory.Transports;
using Mafi.Core.Gfx;
using Mafi.Core.Mods;
using Mafi.Core.Ports.Io;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;
using Mafi.Core.Terrain;

namespace ProgramableNetwork
{
    public partial class NewIds
    {
        public partial class Computers
        {
            public static readonly StaticEntityProto.ID Computer = new StaticEntityProto.ID("ProgramableNetwork_Computer");
            public static readonly StaticEntityProto.ID Database = new StaticEntityProto.ID("ProgramableNetwork_Database");
        }
        public partial class Transport
        {
            public static readonly StaticEntityProto.ID Cable_T1 = new StaticEntityProto.ID("ProgramableNetwork_Cable_T1");
            public static readonly IoPortShapeProto.ID CablePort = new IoPortShapeProto.ID("ProgramableNetwork_IoPortShape_Cable");
        }
    }

    public partial class NewAssets
    {
        public partial class Computers
        {
            public partial class Icons
            {
                public static readonly string Computer = "Assets/ProgramableNetwork/Computer/Icon.png";
                public static readonly string Database = "Assets/ProgramableNetwork/Computer/Icon.png";
            }

            public static readonly string Computer = "Assets/ProgramableNetwork/Computer/Computer.prefab";
            public static readonly string Database = "Assets/ProgramableNetwork/Computer/Computer.prefab";
        }
    }

    internal class Computers : AValidatedData
    {
        protected override void RegisterDataInternal(ProtoRegistrator registrator)
        {
            var category = registrator.PrototypesDb.Get<ToolbarCategoryProto>(Ids.ToolbarCategories.Machines).ToImmutableArray();

            registrator.PrototypesDb.Add(new IoPortShapeProto(
                id: NewIds.Transport.CablePort,
                strings: Proto.Str.Empty,
                layoutChar: '.',
                allowedProductType: ProtocolProductProto.ProductType,
                graphics: new IoPortShapeProto.Gfx(
                    "Assets/Base/Transports/Pipes/Port.prefab",
                    "Assets/Base/Transports/Pipes/Port-Lod3.prefab"
                )
            ));

            registrator.PrototypesDb.Add(new TransportProto(
                id: NewIds.Transport.Cable_T1,
                strings: Proto.CreateStr(NewIds.Transport.Cable_T1, "Cable", "The cable transfers signals by directionally and hold its value until the source is not stopped or signal is not cleared"),
                surfaceRelativeHeight: 0.5.Tiles().ThicknessTilesF,
                maxQuantityPerTransportedProduct: 1.Quantity(),
                transportedProductsSpacing: 5f.Tiles(),
                speedPerTick: RelTile1f.FromTilesPerSecond(10),
                zStepLength: new RelTile1i(0),
                needsPillarsAtGround: true,
                canBeBuried: true,
                tileSurfaceWhenOnGround: registrator.PrototypesDb.Get<TerrainTileSurfaceProto>(Ids.TerrainTileSurfaces.DefaultConcrete),
                maxPillarSupportRadius: new RelTile1i(6),
                portsShape: registrator.PrototypesDb.Get<IoPortShapeProto>(NewIds.Transport.CablePort).ValueOrThrow("Missing port shape"),
                baseElectricityCost: 1.Kw(),
                cornersSharpnessPercent: 55.Percent(),
                allowMixedProducts: true,
                isBuildable: true,
                costs: ((EntityCostsTpl)(new EntityCostsTpl.Builder()).Electronics(2).Priority(4)).MapToEntityCosts(registrator),
                lengthPerCost: new RelTile1i(6),
                constructionDurationPerProduct: 0.2.Seconds(),
                nextTier: Option<TransportProto>.None, // todo
                maintenanceProduct: registrator.PrototypesDb.Get<VirtualProductProto>(Ids.Products.MaintenanceT1).ValueOrThrow("Missing mintenance type"),
                maintenancePerTile: 1.Quantity(),
                graphics: new TransportProto.Gfx(
                    crossSection: createCrossSection(0.05.Tiles(), 0f, out float maxUv),
                    renderProducts: false,
                    samplesPerCurvedSegment: 3,
                    materialPath: "Assets/Base/Transports/Pipes/Pipes.mat",
                    transportUvLength: 6.0.Tiles(),
                    renderTransportedProducts: false,
                    soundOnBuildPrefabPath: "Assets/Base/Transports/Audio/PipePlaced.prefab",
                    flowIndicator: Option.None,
                    verticalConnectorPrefabPath: "Assets/Base/Transports/Pipes/Vertical_Connector.prefab",
                    pillarAttachments: new Dict<TransportPillarAttachmentType, string>
                    {
                        {
                            TransportPillarAttachmentType.FlatToFlat_Straight,
                            "Assets/Base/Transports/Pipes/T1-FlatToFlat_Straight.prefab"
                        },
                        {
                            TransportPillarAttachmentType.FlatToFlat_Turn,
                            "Assets/Base/Transports/Pipes/T1-FlatToFlat_Turn.prefab"
                        },
                        {
                            TransportPillarAttachmentType.RampDownToRampUp_Turn,
                            "Assets/Base/Transports/Pipes/T1-RampDownToRampUp_Turn.prefab"
                        },
                        {
                            TransportPillarAttachmentType.FlatToRampUp_Straight,
                            "Assets/Base/Transports/Pipes/T1-FlatToRampUp_Straight.prefab"
                        },
                        {
                            TransportPillarAttachmentType.FlatToRampUp_Turn,
                            "Assets/Base/Transports/Pipes/T1-FlatToRampUp_Turn.prefab"
                        },
                        {
                            TransportPillarAttachmentType.FlatToRampDown_Straight,
                            "Assets/Base/Transports/Pipes/T1-FlatToRampDown_Straight.prefab"
                        },
                        {
                            TransportPillarAttachmentType.FlatToRampDown_Turn,
                            "Assets/Base/Transports/Pipes/T1-FlatToRampDown_Turn.prefab"
                        },
                        {
                            TransportPillarAttachmentType.FlatToVertical,
                            "Assets/Base/Transports/Pipes/T1-FlatToVertical.prefab"
                        },
                        {
                            TransportPillarAttachmentType.VerticalToVertical,
                            "Assets/Base/Transports/Pipes/T1-VerticalToVertical.prefab"
                        },
                        {
                            TransportPillarAttachmentType.FlatToVertical_Down,
                            "Assets/Base/Transports/Pipes/T1-FlatToVertical_Down.prefab"
                        }
                    },
                    uvShiftY: 0,
                    crossSectionScale: Percent.Hundred,
                    crossSectionRadius: 1,
                    instancedRenderingData: Option.None,
                    usePerProductColoring: false,
                    customIconPath: Option.None,
                    useInstancedRendering: false,
                    maxRenderedLod: int.MaxValue
                )
            ));

            registrator.PrototypesDb.Add(new ComputerProto(
                id: NewIds.Computers.Computer,
                strings: Proto.CreateStr(NewIds.Computers.Computer, "Computer", "Handles basic operations and automatization"),
                layout: registrator.LayoutParser.ParseLayoutOrThrow(
                    new EntityLayoutParams(
                        portsCanOnlyConnectToTransports: false
                    ),
                    "   A.+   ",
                    "B.+[1]+.D",
                    "   C.+   "
                    ),
                costs: ((EntityCostsTpl)Costs.Build.CP2(4)).MapToEntityCosts(registrator),
                operationCount: 10,
                graphics: new LayoutEntityProto.Gfx(
                    prefabPath: NewAssets.Computers.Computer,
                    customIconPath: NewAssets.Computers.Icons.Computer,
                    categories: category
                )
            ));

            registrator.PrototypesDb.Add(new ComputerProto(
                id: NewIds.Computers.Database,
                strings: Proto.CreateStr(NewIds.Computers.Database, "Database", "Holds only variables"),
                layout: registrator.LayoutParser.ParseLayoutOrThrow("[1]"),
                costs: ((EntityCostsTpl)Costs.Build.CP2(4)).MapToEntityCosts(registrator),
                operationCount: 20,
                variables: 20,
                iddlePower: 0.Kw(),
                workingPower: 1.Kw(),
                instructionLevel: 0,
                graphics: new LayoutEntityProto.Gfx(
                    prefabPath: NewAssets.Computers.Database,
                    customIconPath: NewAssets.Computers.Icons.Database,
                    categories: category
                )
            ));
        }

        TransportCrossSection createCrossSection(RelTile1f radius, float uvShiftY, out float maxUv)
        {
            var TRANSPORT_UV_LENGTH = 6.0.Tiles();

            ImmutableArrayBuilder<CrossSectionVertex> immutableArrayBuilder = new ImmutableArrayBuilder<CrossSectionVertex>(9);
            AngleDegrees1f angleDegrees1f = -AngleDegrees1f.Deg360 / 8;
            RelTile1f lengthTiles = (radius.Value * new Tile2f(1, 0) - radius.Value * new Tile2f(angleDegrees1f.DirectionVector)).LengthTiles;
            Assert.That(8 * lengthTiles).IsLess(TRANSPORT_UV_LENGTH);
            for (int i = 0; i <= 8; i++)
            {
                Vector2f directionVector = (i * angleDegrees1f - AngleDegrees1f.Deg90).DirectionVector;
                float num4 = (float)i * lengthTiles.Value.ToFloat() / TRANSPORT_UV_LENGTH.Value.ToFloat();
                immutableArrayBuilder[i] = new CrossSectionVertex(new RelTile2f(radius.Value * directionVector), directionVector, num4 + uvShiftY);
            }
            maxUv = immutableArrayBuilder.Last.TextureCoordY;
            Assert.That(maxUv).IsLess(1f, "MAX UV is outside of texture.");
            return new TransportCrossSection(ImmutableArray.Create(immutableArrayBuilder.GetImmutableArrayAndClear()), ImmutableArray<ImmutableArray<CrossSectionVertex>>.Empty);
        }
    }
}
