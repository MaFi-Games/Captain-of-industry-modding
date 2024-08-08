using System;

namespace ProgramableNetwork
{
    public class ModuleLayout
    {
        public ModuleLayout(int x, int y, LayoutSlot[,] layout)
        {
            X = x;
            Y = y;
            Layout = layout;
        }

        public enum LayoutSlot
        {
            Edge = 0, Module = 1, Railing = 2, Cable = 4
        }

        public int X { get; }
        public int Y { get; }
        public LayoutSlot[,] Layout { get; }

        public static ModuleLayout Parse(params string[] layout)
        {
            LayoutSlot[,] layoutSlots = new LayoutSlot[layout[0].Length, layout.Length];
            for (int y = 0; y < layout.Length; y++)
            {
                for (int x = 0; x < layout[y].Length; x++)
                {
                    if (layout[y][x] == 'C')
                    {
                        layoutSlots[x, y] = LayoutSlot.Cable;
                    }
                    else if (layout[y][x] == 'M')
                    {
                        layoutSlots[x, y] = LayoutSlot.Module;
                    }
                    else if (layout[y][x] == 'R')
                    {
                        layoutSlots[x, y] = LayoutSlot.Railing;
                    }
                    else
                    {
                        layoutSlots[x, y] = LayoutSlot.Edge;
                    }
                }
            }
            return new ModuleLayout(layout[0].Length, layout.Length, layoutSlots);
        }
    }
}