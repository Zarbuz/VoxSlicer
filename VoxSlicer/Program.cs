using System;
using System.Drawing;
using System.IO;
using VoxSlicer.Extensions;
using VoxSlicer.Schematics;
using VoxSlicer.Vox;

namespace VoxSlicer
{
    class Program
    {
        static void Main(string[] args)
        {
            VoxReader reader = new VoxReader();
            VoxWriter writer = new VoxWriter();

            if (args.Length <= 2)
            {
                Console.WriteLine("[ERROR] Missing arguments");
                Console.WriteLine("Usage: VoxSlicer.exe SIZE FILE");
                return;
            }

            try
            {
                short size = Convert.ToInt16(args[0]);
                if (size >= 126)
                {
                    Console.WriteLine("[ERROR] Size must be lower than 126");
                    return;
                }

                VoxModel model = reader.LoadModel(args[1]);
                if (model == null) return;

                DirectoryInfo directory = Directory.CreateDirectory(Path.GetFileNameWithoutExtension(args[0]));

                foreach (var data in model.voxelFrames)
                {
                    SchematicConstants.WidthSchematic = size;
                    SchematicConstants.HeightSchematic = size;
                    SchematicConstants.LengthSchematic = size;

                    int sizeX = (int)Math.Ceiling((decimal)data.VoxelsWide / size);
                    int sizeY = (int)Math.Ceiling((decimal)data.VoxelsTall / size);
                    int sizeZ = (int)Math.Ceiling((decimal)data.VoxelsDeep / size);
                    Schematic[,,] schematics = new Schematic[sizeX, sizeY, sizeZ];

                    Color[] colors = model.palette;
                    for (int y = 0; y < data.VoxelsTall; y++)
                    {
                        for (int z = 0; z < data.VoxelsDeep; z++)
                        {
                            for (int x = 0; x < data.VoxelsWide; x++)
                            {
                                int posX = x / size;
                                int posY = y / size;
                                int posZ = z / size;

                                if (schematics[posX, posY, posZ] == null)
                                {
                                    schematics[posX, posY, posZ] = new Schematic()
                                    {
                                        Blocks = new System.Collections.Generic.HashSet<Block>(),
                                        Heigth = size,
                                        Length = size,
                                        Width = size
                                    };
                                }
                                int indexColor = data.Get(x, y, z);
                                Color color = colors[indexColor];
                                if (!color.IsEmpty)
                                {
                                    schematics[posX, posY, posZ].Blocks.Add(new Block((ushort)x, (ushort)y, (ushort)z, color.ColorToUInt()));
                                }
                            }
                        }
                    }

                    for (int x = 0; x < schematics.GetLength(0); x++)
                    {
                        for (int y = 0; y < schematics.GetLength(1); y++)
                        {
                            for (int z = 0; z < schematics.GetLength(2); z++)
                            {
                                if (schematics[x, y, z].TotalCount != 0)
                                {
                                    string name = $"{Path.GetFileNameWithoutExtension(args[0])}-{x}-{y}-{z}.vox";
                                    Console.WriteLine("[INFO] Started to process: " + name);
                                    writer.WriteModel(Path.Combine(directory.FullName, name), schematics[x, y, z], 0, size);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("[ERROR] Failed to read voxel volume size");
            }
        }
    }
}
