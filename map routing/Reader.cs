using System;
using System.Collections.Generic;
using System.IO;

public class Reader
{
    public static List<(Tuple<double, double>, Tuple<double, double>, long)> ReadAndParseQueries(string queriesPath)
    {
        var tupleList = new List<(Tuple<double, double>, Tuple<double, double>, long)>();
        using var reader = new StreamReader(queriesPath);
        if (!long.TryParse(reader.ReadLine(), out long qCount))
            throw new InvalidDataException("First line must be the number of queries.");

        for (long i = 0; i < qCount; i++)
        {
            var parts = reader.ReadLine()?.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts?.Length == 5 &&
                double.TryParse(parts[0], out double fx) &&
                double.TryParse(parts[1], out double fy) &&
                double.TryParse(parts[2], out double tx) &&
                double.TryParse(parts[3], out double ty) &&
                long.TryParse(parts[4], out long R))
            {
                tupleList.Add((Tuple.Create(fx, fy), Tuple.Create(tx, ty), R));
            }
            else
            {
                throw new InvalidDataException($"Invalid query at line {i + 2}.");
            }
        }
        return tupleList;
    }

    public static void ReadMapWithVerticesAndEdges(
        string mapPath,
        Dictionary<int, List<Tuple<int, double, double>>> edges,
        Dictionary<int, (double, double)> IdToCoor, ref int v_count)
    {
        using var reader = new StreamReader(mapPath);
        if (!int.TryParse(reader.ReadLine(), out int vCount))
            throw new InvalidDataException("First line must be the number of vertices.");
        v_count = vCount;

        for (int i = 0; i < vCount; i++)
        {
            var parts = reader.ReadLine()?.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts?.Length >= 3 &&
                int.TryParse(parts[0], out int id) &&
                double.TryParse(parts[1], out double x) &&
                double.TryParse(parts[2], out double y))
            {
                IdToCoor[id] = (x, y);
            }
            else throw new InvalidDataException($"Invalid vertex at line {i + 2}.");
        }

        if (!long.TryParse(reader.ReadLine(), out long eCount))
            throw new InvalidDataException("Expected edge count after vertices.");

        for (int i = 0; i < eCount; i++)
        {
            var parts = reader.ReadLine()?.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts?.Length >= 4 &&
                int.TryParse(parts[0], out int from) &&
                int.TryParse(parts[1], out int to) &&
                double.TryParse(parts[2], out double len) &&
                double.TryParse(parts[3], out double spd))
            {
                if (!edges.ContainsKey(from)) edges[from] = new List<Tuple<int, double, double>>();
                edges[from].Add(Tuple.Create(to, len, spd));
                if (!edges.ContainsKey(to)) edges[to] = new List<Tuple<int, double, double>>();
                edges[to].Add(Tuple.Create(from, len, spd));
            }
            else throw new InvalidDataException($"Invalid edge at line {vCount + i + 3}.");
        }
    }
}