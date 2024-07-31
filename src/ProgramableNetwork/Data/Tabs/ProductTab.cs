using Mafi;
using Mafi.Base;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Dynamic;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Factory.Transports;
using Mafi.Core.Products;
using Mafi.Unity;
using Mafi.Unity.InputControl;
using Mafi.Unity.InputControl.Inspectors;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UserInterface;
using Mafi.Unity.UserInterface.Components;
using Mafi.Unity.UserInterface.Style;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProgramableNetwork
{
    public class ProductTab : StackContainer, IRefreshable
    {
        private readonly UiBuilder m_builder;
        private readonly MemoryPointer m_input;
        private readonly Func<ProductProto, bool> m_filter;
        private readonly Action m_refresh;
        private readonly ItemDetailWindowView m_window;
        private readonly InspectorContext m_inspectorContext;
        private readonly StackContainer m_btnPreviewHolder;
        private Btn m_btnPreview;
        private ProductPicker m_produckPicker;

        public ProductTab(UiBuilder builder, Computer computer, InstructionProto instruction,
            MemoryPointer input, Action refresh, ItemDetailWindowView parentWindow, InspectorContext inspectorContext)
            : base(builder, "product_" + DateTime.Now.Ticks)
        {
            m_builder = builder;
            m_input = input;
            m_filter = instruction.ProductFilter;
            m_refresh = refresh;
            m_window = parentWindow;
            m_inspectorContext = inspectorContext;

            m_btnPreviewHolder = m_builder
                .NewStackContainer("picker_holder_" + DateTime.Now.Ticks)
                .SetSize(40, 40)
                .AppendTo(this);

            m_btnPreview = m_builder
                .NewBtnGeneral("picker_" + DateTime.Now.Ticks)
                .SetButtonStyle(m_builder.Style.Global.ImageBtn)
                .SetSize(40, 40)
                .SetIcon(m_builder.Style.Icons.Empty)
                .OnClick(FindProduct)
                .AppendTo(m_btnPreviewHolder);

            SetSizeMode(SizeMode.Dynamic);
            this.SetHeight(40);

            Refresh();
        }

        private void FindProduct()
        {
            if (m_produckPicker == null)
            {
                m_produckPicker = new ProductPicker(this);
                m_window.SetupInnerWindowWithButton(m_produckPicker, m_btnPreviewHolder, m_btnPreview, () => {
                    try {
                        m_btnPreviewHolder.ClearAndDestroyAll();
                        m_btnPreview = new Btn(m_builder, "picker_" + DateTime.Now.Ticks)
                            .SetButtonStyle(m_builder.Style.Global.ImageBtn)
                            .SetSize(40, 40)
                            .SetIcon(m_builder.Style.Icons.Empty)
                            .OnClick(FindProduct)
                            .AppendTo(m_btnPreviewHolder);
                    }
                    catch (Exception)
                    {
                        // gui issue
                    }
                }, () => { });
                m_window.OnHide += m_produckPicker.Hide;
            }

            m_produckPicker.Show();
        }

        public void Refresh()
        {
            if (m_input.Type == InstructionProto.InputType.Product)
                m_btnPreview.SetIcon(m_input.Product.IconPath);
            else
                m_btnPreview.SetIcon(m_builder.Style.Icons.Empty);
        }

        private class ProductPicker : WindowView
        {
            private ProductTab m_productTab;
            private GridContainer m_grid;

            public ProductPicker(ProductTab productTab)
                :base("product", FooterStyle.Round, false)
            {
                this.m_productTab = productTab;
            }

            protected override void BuildWindowContent()
            {
                this.m_headerText.SetText(NewIds.Texts.PointerTypes[InstructionProto.InputType.Product]);

                SetContentSize(500, 460);

                var scroll = Builder
                    .NewScrollableContainer("items-scroll")
                    .AddVerticalScrollbar()
                    .SetSize(500, 460)
                    .PutTo(GetContentPanel());

                GetContentPanel()
                    .SetSize(500, 460)
                    .SetBackground(Builder.Style.EntitiesMenu.MenuBg);

                m_grid = Builder
                    .NewGridContainer("items-grid")
                    .SetDynamicHeightMode(10)
                    .SetCellSize(new Vector2(40, 40))
                    .SetCellSpacing(10)
                    .SetBackground(Builder.Style.EntitiesMenu.MenuBg)
                    .SetInnerPadding(Offset.All(5));

                scroll.AddItem(m_grid);

                OnShowStart += RefreshProducts;
            }

            private void RefreshProducts()
            {
                m_grid.ClearAllAndDestroy();

                var protos = m_productTab.m_inspectorContext
                    .ProtosDb.All<ProductProto>()
                    .Where(proto => proto.IsAvailable)
                    .Where(m_productTab.m_filter)
                    .ToList();

                foreach (var proto in protos)
                {
                    Builder
                        .NewBtnGeneral("item_" + proto.Strings.Name.Id)
                        .SetButtonStyle(Builder.Style.Global.ImageBtn)
                        .SetSize(40, 40)
                        .SetIcon(proto.IconPath)
                        .SetBackgroundColor(new ColorRgba(0, 0))
                        .OnClick(() =>
                        {
                            m_productTab.m_input.Product = proto;
                            m_productTab.m_produckPicker.Hide();
                            m_productTab.m_refresh();
                        })
                        .AddToGrid(m_grid);
                }
            }
        }
    }
}