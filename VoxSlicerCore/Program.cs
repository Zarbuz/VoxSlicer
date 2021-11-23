using FileToVoxCore.Schematics;
using FileToVoxCore.Vox;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;

namespace VoxSlicerCore
{
	class Program
	{
		static void Main(string[] args)
		{
			VoxReader reader = new VoxReader();
			VoxWriter writer = new VoxWriter();

			DisplayInformations();

			if (args.Length < 2)
			{
				Console.WriteLine("[ERROR] Missing arguments");
				Console.WriteLine("Usage: VoxSlicer.exe SIZE FILE");
				return;
			}

			try
			{
				short size = Convert.ToInt16(args[0]);
				if (size <= 10 || size > 256)
				{
					Console.WriteLine("[ERROR] Size must be between 10 and 256");
					return;
				}

				VoxModel model = reader.LoadModel(args[1]);

				if (model == null)
				{
					Console.WriteLine("[ERROR] Failed to load model");
					return;
				}

				Schematic.CHUNK_SIZE = size;
				DirectoryInfo directory = Directory.CreateDirectory(Path.GetFileNameWithoutExtension(args[1]));
				foreach (VoxelData data in model.VoxelFrames)
				{
					int sizeX = (int)Math.Ceiling((decimal)data.VoxelsWide / size);
					int sizeY = (int)Math.Ceiling((decimal)data.VoxelsTall / size);
					int sizeZ = (int)Math.Ceiling((decimal)data.VoxelsDeep / size);

					Schematic[,,] schematics = new Schematic[sizeX, sizeY, sizeZ];
					Color[] colors = model.Palette;
					for (int y = 0; y < data.VoxelsTall; y++)
					{
						for (int z = 0; z < data.VoxelsDeep; z++)
						{
							for (int x = 0; x < data.VoxelsWide; x++)
							{
								int posX = x / size;
								int posY = y / size;
								int posZ = z / size;

								schematics[posX, posY, posZ] ??= new Schematic();
								int indexColor = data.Get(x, y, z);
								Color color = colors[indexColor];

								if (indexColor != 0)
								{
									schematics[posX, posY, posZ].AddVoxel(x, y, z, color);
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
								if (schematics[x, y, z].GetAllVoxels().Count != 0)
								{
									var rotation = model.TransformNodeChunks.First().RotationAt();
									string name = $"{Path.GetFileNameWithoutExtension(args[1])}-{x}-{y}-{z}.vox";
									Console.WriteLine("[INFO] Started to process: " + name);
									string outputPath = Path.Combine(directory.FullName, name);
									writer.WriteModel(size, outputPath, model.Palette.ToList(), schematics[x, y, z], rotation);
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

		private static void DisplayInformations()
		{
			Console.WriteLine("[INFO] VoxSlicer v" + Assembly.GetExecutingAssembly().GetName().Version);
			Console.WriteLine("[INFO] Author: @Zarbuz. Contact : https://twitter.com/Zarbuz");
		}
	}
}
