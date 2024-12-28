using Mafi;
using Mafi.Core;
using Mafi.Core.Entities;
using Mafi.Core.Input;
using Mafi.Unity;
using Mafi.Unity.Entities;
using Mafi.Unity.InputControl;
using Mafi.Unity.InputControl.Cursors;
using Mafi.Unity.InputControl.Inspectors;
using System.Linq;
using UnityEngine;

namespace ProgramableNetwork
{
    [GlobalDependency(RegistrationMode.AsAllInterfaces, false, false)]
    public class ControllerInspector : EntityInspector<Controller, ControllerView>
    {
        private readonly ControllerView m_windowView;
        private readonly AudioSource m_invalidOpSound;
        private bool m_highlightSearched;
        private IRenderedEntity m_hoveredEntity;

        public ControllerInspector(
            InspectorContext context,
            CursorManager cursorManager,
            CursorPickingManager cursorPickingManager,
            ShortcutsManager shortcutsManager,
            //TerrainCursor terrainCursor,
            NewInstanceOf<EntityHighlighter> entityHighlighter,
            NewInstanceOf<EntityHighlighter> entityHighlighterSelectable
            ) : base(context)
        {
            m_windowView = new ControllerView(this);
            CursorManager = cursorManager;
            CursorPickingManager = cursorPickingManager;
            //TerrainCursor = terrainCursor;
            EntityHighlighter = entityHighlighter.Instance;
            EntityHighlighterSelectable = entityHighlighterSelectable.Instance;
            ShortcutsManager = shortcutsManager;
            m_invalidOpSound = Context.Builder.AudioDb.GetSharedAudio(Context.Builder.Audio.InvalidOp);
        }

        public CursorManager CursorManager { get; }
        public CursorPickingManager CursorPickingManager { get; }
        //public TerrainCursor TerrainCursor { get; }
        public EntityHighlighter EntityHighlighter { get; }
        public EntityHighlighter EntityHighlighterSelectable { get; }
        public ShortcutsManager ShortcutsManager { get; }
        public EntitySelector EntitySelectionInput { get; set; }

        protected override ControllerView GetView()
        {
            return m_windowView;
        }

        public override bool InputUpdate(IInputScheduler inputScheduler)
        {
            if (EntitySelectionInput != null)
            {
                //var position = TerrainCursor.Tile3f.ToVector3();
                // set line view endpoint

                if (ShortcutsManager.IsPrimaryActionDown)
                {
                    Tile3f source = SelectedEntity.Position3f;

                    Option<IRenderedEntity> pickedEntity = CursorPickingManager.PickEntity<IRenderedEntity>(e => EntitySelectionInput.EntityFilter((Entity)e));
                    if (pickedEntity.HasValue
                        && pickedEntity.Value.HasPosition(out Tile3f target)
                        && (source - target).Length <= EntitySelectionInput.EntitySearchDistance
                        )
                    {
                        EntitySelectionInput.Entity = pickedEntity.Value;
                        EntitySelectionInput.Refresh();
                        EntitySelectionInput = null;
                        return true;
                    }
                    m_invalidOpSound.Play();
                    return true;
                }
                if (ShortcutsManager.IsSecondaryActionDown)
                {
                    EntitySelectionInput.Refresh();
                    EntitySelectionInput = null;
                    return true;
                }
            }
            return base.InputUpdate(inputScheduler);
        }

        public override void RenderUpdate(GameTime gameTime)
        {
            base.RenderUpdate(gameTime);

            if (EntitySelectionInput != null)
            {
                if (!m_highlightSearched)
                {
                    m_highlightSearched = true;
            
                    Tile3f source = SelectedEntity.Position3f;
                    Fix32 innerDistance = EntitySelectionInput.EntitySearchDistance;
                    Fix32 ringDistance = EntitySelectionInput.EntitySearchDistance + 2;
            
                    Context.EntitiesManager.GetAllEntitiesOfType<Entity>()
                        .Where(e => EntitySelectionInput.EntityFilter(e))
                        .Where(e => e is IEntityWithPosition && e is IRenderedEntity)
                        .Cast<IEntityWithPosition>()
                        .Where(e => (source - e.Position3f).Length <= innerDistance)
                        .Cast<IRenderedEntity>()
                        .Call(e => EntityHighlighterSelectable.Highlight(e, ColorRgba.Green))
                        .ToList();
                }
            
                Option<IRenderedEntity> pickedEntity = CursorPickingManager.PickEntity<IRenderedEntity>(e => EntitySelectionInput.EntityFilter((Entity)e));
            
                if (pickedEntity.IsNone || m_hoveredEntity != null)
                {
                    EntityHighlighter.RemoveHighlight(m_hoveredEntity);
                }
            
                if (pickedEntity.HasValue)
                {
                    m_hoveredEntity = pickedEntity.Value;
                    Tile3f source = SelectedEntity.Position3f;
            
                    if (!pickedEntity.Value.HasPosition(out Tile3f target)
                        || (source - target).Length > EntitySelectionInput.EntitySearchDistance)
                    {
                        EntityHighlighter.HighlightOnly(m_hoveredEntity, ColorRgba.Red);
                    }
                    else
                    {
                        EntityHighlighter.HighlightOnly(m_hoveredEntity, ColorRgba.LightBlue);
                    }
                    return;
                }
            }
            if (EntitySelectionInput == null && m_hoveredEntity != null)
            {
                EntityHighlighter.RemoveHighlight(m_hoveredEntity);
                m_hoveredEntity = null;
            }
            if (EntitySelectionInput == null && m_highlightSearched)
            {
                EntityHighlighterSelectable.ClearAllHighlights();
                m_highlightSearched = false;
            }
        }

        protected override void OnDeactivated()
        {
            base.OnDeactivated();
            //EntitySelectionInput = null;
        }
    }
}
