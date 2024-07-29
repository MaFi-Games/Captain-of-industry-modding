

using System;
using Mafi;
using Mafi.Core;
using Mafi.Core.Buildings.Settlements;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Static;
using Mafi.Core.Population;
using Mafi.Core.Products;
using Mafi.Core.PropertiesDb;
using Mafi.Core.Prototypes;
using Mafi.Core.Stats;
using Mafi.Core.Terrain;
using Mafi.Core.World;

[GlobalDependency(RegistrationMode.AsEverything, false, false)]
public class FixSavedGames : IInitializer
{
	private readonly SettlementsManager m_settlementsManager;
    private readonly TerrainManager m_terrainManager;
    private readonly EntitiesBuilder m_entitiesBuilder;
    private readonly IEntitiesManager m_entitiesManager;

	private readonly IConstructionManager m_constructionManager;

	private readonly IProductsManager m_productsManager;

	private readonly PopsHealthManager m_healthManager;

	private readonly UpointsManager m_upointsManager;

	private readonly ProtosDb m_protosDb;

	private readonly UnlockedProtosDb m_unlockedProtosDb;

	private readonly IPropertiesDb m_propsDb;

	private readonly StatsManager m_statsManager;

	private readonly IWorldMapManager m_mapManager;

    public bool IsBeingLoaded { get; }

	public FixSavedGames(SettlementsManager settlementsManager, EntitiesBuilder entitiesBuilder, TerrainManager terrainManager, IEntitiesManager entitiesManager, IConstructionManager constructionManager, IProductsManager productsManager, PopsHealthManager healthManager, UpointsManager upointsManager, ProtosDb protosDb, UnlockedProtosDb unlockedProtosDb, IPropertiesDb propsDb, StatsManager statsManager, IWorldMapManager mapManager)
	{
		m_settlementsManager = settlementsManager;
		m_terrainManager = terrainManager;
		m_entitiesBuilder = entitiesBuilder;
		m_entitiesManager = entitiesManager;
		m_constructionManager = constructionManager;
		m_productsManager = productsManager;
		m_healthManager = healthManager;
		m_upointsManager = upointsManager;
		m_protosDb = protosDb;
		m_unlockedProtosDb = unlockedProtosDb;
		m_propsDb = propsDb;
		m_statsManager = statsManager;
		m_mapManager = mapManager;
		IsBeingLoaded = true;

		Log.Info("[ConstructionSite]: Save game fixer created.");
	}

	void IInitializer.DoOnNewGameOnly(Action action)
	{
		Log.Info("[ConstructionSite]: Save game fix initialized on new game, so do nothing.");
	}

	private void SavedGameFixer()
	{
	}

	void IInitializer.DoOnNewGameOrAfterLoad(Action action)
	{
		Log.Info($"[{ProgramableNetwork.ModDefinition.ModName}]: Save game fix on loaded game.");
		SavedGameFixer();
	}
}
