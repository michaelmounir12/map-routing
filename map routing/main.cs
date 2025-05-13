using map_routing;

using map_routing;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace map_routing
{
    class Program
    {
        static void Main(string[] args)
        {
            int caseCount = args.Length / 3;
            for (int i = 0; i < caseCount; i++)
            {
                string mapPath = args[i * 3];
                string queryPath = args[i * 3 + 1];
                string outputPath = args[i * 3 + 2];

                Console.WriteLine($"Processing case {i + 1}/{caseCount}: {Path.GetFileName(mapPath)}, {Path.GetFileName(queryPath)} in {Path.GetFileName(outputPath)}");

                var swAll = Stopwatch.StartNew();
                var IdToCoor = new Dictionary<int, (double, double)>();
                var edges = new Dictionary<int, List<Tuple<int, double, double>>>();
                int v_count = -1;

                Reader.ReadMapWithVerticesAndEdges(mapPath, edges, IdToCoor, ref v_count);
                var queries = Reader.ReadAndParseQueries(queryPath);

                // Solve queries in parallel
                var swCompute = Stopwatch.StartNew();
                var results = new Models.Result[queries.Count];
                Parallel.For(0, queries.Count, j =>
                {
                    var (point1, point2, R) = queries[j];
                    results[j] = MainLogic.Solver(point1, point2, R, v_count, IdToCoor, edges);
                });
                swCompute.Stop();

                using var writer = new StreamWriter(outputPath);
                foreach (var result in results)
                {
                    writer.WriteLine(string.Join(" ", result.Path));
                    writer.WriteLine($"{result.Time:F2} mins");
                    writer.WriteLine($"{result.TotalDist:F2} km");
                    writer.WriteLine($"{result.TotalWalk:F2} km");
                    writer.WriteLine($"{result.VehicleDist:F2} km");
                    writer.WriteLine();
                }
                writer.WriteLine($"{swCompute.ElapsedMilliseconds} ms");
                writer.WriteLine($"{swAll.ElapsedMilliseconds} ms");


                if (results.Length > 0 && queries.Count > 0)
                {
                    try
                    {
                        var firstQuery = queries[0];
                        var firstResult = results[0];

                        var bitmap = MapVisualizer.VisualizePath(
                            firstQuery.Item1,
                            firstQuery.Item2,
                            IdToCoor,
                            edges,
                            firstResult.Path
                        );

                        string visualizationPath = Path.Combine(
                            Path.GetDirectoryName(outputPath),
                            Path.GetFileNameWithoutExtension(outputPath) + "_visualization.png"
                        );

                        bitmap.Save(visualizationPath, ImageFormat.Png);
                        Console.WriteLine($"Visualization saved to: {visualizationPath}");

                        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                        {
                            try
                            {
                                Process.Start(new ProcessStartInfo
                                {
                                    FileName = visualizationPath,
                                    UseShellExecute = true
                                });
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Could not open visualization: {ex.Message}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Visualization failed: {ex.Message}");
                    }
                }

            }
        }
    }
}