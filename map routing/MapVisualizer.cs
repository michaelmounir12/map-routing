using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

public static class MapVisualizer
{
    public static Bitmap VisualizePath(
        Tuple<double, double> startPoint,
        Tuple<double, double> endPoint,
        Dictionary<int, (double x, double y)> idToCoor,
        Dictionary<int, List<Tuple<int, double, double>>> edges,
        List<int> pathVertices,
        int imageWidth = 800,
        int imageHeight = 600)
    {
        // hanghayr input format zy MapDrawer
        var start = (startPoint.Item1, startPoint.Item2);
        var end = (endPoint.Item1, endPoint.Item2);

        var simpleEdges = edges.ToDictionary(
            e => e.Key,
            e => e.Value.Select(t => t.Item1).ToList()
        );
        //ha ncall el DrawMap method 

        return MapDrawer.DrawMap(
            start,
            end,
            idToCoor,
            simpleEdges,
            pathVertices,
            imageWidth,
            imageHeight
        );
    }
}

public class MapDrawer
{   //hnakhod el mathematical w graph data b3den ha nghayarhom le visual map image
    public static Bitmap DrawMap(
        (double x, double y) start,
        (double x, double y) end,
        Dictionary<int, (double x, double y)> points,
        Dictionary<int, List<int>> connections,
        List<int> path,
        int width = 800,
        int height = 600)
    {
        //  hanehseb drawing area
        var (minx, maxx, miny, maxy) = FindDrawingArea(start, end, points);

        //  hanehseb scaling
        var scale = CalculateScale(minx, maxx, miny, maxy, width, height);

        //  Create image
        var image = new Bitmap(width, height);

        //  Draw kolo
        using (var g = Graphics.FromImage(image))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.White);

            DrawConnections(g, points, connections, minx, miny, scale, height);

            if (path != null && path.Count > 0)
                DrawPath(g, points, path, minx, miny, scale, height);

            DrawWalkingPaths(g, start, end, points, path, minx, miny, scale, height);
            DrawAllPoints(g, points, minx, miny, scale, height);

            if (path != null)
                HighlightPathPoints(g, points, path, minx, miny, scale, height);

            DrawSpecialPoints(g, start, end, minx, miny, scale, height);
            DrawSimpleLegend(g);
        }

        return image;
    }
    // bnshof elarea ely hatetrsem
    private static (double minx, double maxx, double miny, double maxy) FindDrawingArea(
        (double x, double y) start,
        (double x, double y) end,
        Dictionary<int, (double x, double y)> points)
    {
        double minx = points.Values.Min(p => p.x);
        double maxx = points.Values.Max(p => p.x);
        double miny = points.Values.Min(p => p.y);
        double maxy = points.Values.Max(p => p.y);

        minx = Math.Min(minx, Math.Min(start.x, end.x));
        maxx = Math.Max(maxx, Math.Max(start.x, end.x));
        miny = Math.Min(miny, Math.Min(start.y, end.y));
        maxy = Math.Max(maxy, Math.Max(start.y, end.y));

        double paddingx = (maxx - minx) * 0.1;
        double paddingy = (maxy - miny) * 0.1;

        return (minx - paddingx, maxx + paddingx, miny - paddingy, maxy + paddingy);
    }

    private static float CalculateScale(
        double minx, double maxx,
        double miny, double maxy,
        int width, int height)
    {
        double xrange = maxx - minx;
        double yrange = maxy - miny;
        return (float)Math.Min(width / xrange, height / yrange);
    }

    private static void DrawConnections(
        Graphics g,
        Dictionary<int, (double x, double y)> points,
        Dictionary<int, List<int>> connections,
        double minx, double miny,
        float scale, int height)
    {
        var pen = new Pen(Color.LightGray, 1);

        foreach (var (fromId, toIds) in connections)
        {
            var from = points[fromId];
            float fromX = (float)((from.x - minx) * scale);
            float fromY = height - (float)((from.y - miny) * scale);

            foreach (var toId in toIds)
            {
                var to = points[toId];
                float toX = (float)((to.x - minx) * scale);
                float toY = height - (float)((to.y - miny) * scale);
                g.DrawLine(pen, fromX, fromY, toX, toY);
            }
        }
    }
    // lama nersem mn point lel ba3daha
    private static void DrawPath(
        Graphics g,
        Dictionary<int, (double x, double y)> points,
        List<int> path,
        double minx, double miny,
        float scale, int height)
    {
        var pen = new Pen(Color.Red, 3);

        for (int i = 0; i < path.Count - 1; i++)
        {
            var from = points[path[i]];
            var to = points[path[i + 1]];

            float fromX = (float)((from.x - minx) * scale);
            float fromY = height - (float)((from.y - miny) * scale);
            float toX = (float)((to.x - minx) * scale);
            float toY = height - (float)((to.y - miny) * scale);

            g.DrawLine(pen, fromX, fromY, toX, toY);
        }
    }

    private static void DrawWalkingPaths(
        Graphics g,
        (double x, double y) start,
        (double x, double y) end,
        Dictionary<int, (double x, double y)> points,
        List<int> path,
        double minx, double miny,
        float scale, int height)
    {
        var pen = new Pen(Color.Green, 2) { DashStyle = DashStyle.Dash };

        float startX = (float)((start.x - minx) * scale);
        float startY = height - (float)((start.y - miny) * scale);
        float endX = (float)((end.x - minx) * scale);
        float endY = height - (float)((end.y - miny) * scale);

        if (path != null && path.Count > 0)
        {
            var first = points[path[0]];
            float firstX = (float)((first.x - minx) * scale);
            float firstY = height - (float)((first.y - miny) * scale);
            g.DrawLine(pen, startX, startY, firstX, firstY);

            var last = points[path[^1]];
            float lastX = (float)((last.x - minx) * scale);
            float lastY = height - (float)((last.y - miny) * scale);
            g.DrawLine(pen, lastX, lastY, endX, endY);
        }
        else
        {
            g.DrawLine(pen, startX, startY, endX, endY);
        }
    }

    private static void DrawAllPoints(
        Graphics g,
        Dictionary<int, (double x, double y)> points,
        double minx, double miny,
        float scale, int height)
    {
        foreach (var (x, y) in points.Values)
        {
            float drawX = (float)((x - minx) * scale);
            float drawY = height - (float)((y - miny) * scale);
            g.FillEllipse(Brushes.LightBlue, drawX - 3, drawY - 3, 6, 6);
            g.DrawEllipse(Pens.DarkBlue, drawX - 3, drawY - 3, 6, 6);
        }
    }

    private static void HighlightPathPoints(
        Graphics g,
        Dictionary<int, (double x, double y)> points,
        List<int> path,
        double minx, double miny,
        float scale, int height)
    {
        foreach (var pointId in path)
        {
            var (x, y) = points[pointId];
            float drawX = (float)((x - minx) * scale);
            float drawY = height - (float)((y - miny) * scale);
            g.FillEllipse(Brushes.Red, drawX - 5, drawY - 5, 10, 10);
            g.DrawEllipse(Pens.DarkRed, drawX - 5, drawY - 5, 10, 10);
        }
    }

    private static void DrawSpecialPoints(
        Graphics g,
        (double x, double y) start,
        (double x, double y) end,
        double minx, double miny,
        float scale, int height)
    {
        // Start point (green)
        float startX = (float)((start.x - minx) * scale);
        float startY = height - (float)((start.y - miny) * scale);
        g.FillEllipse(Brushes.Green, startX - 6, startY - 6, 12, 12);
        g.DrawEllipse(Pens.DarkGreen, startX - 6, startY - 6, 12, 12);

        // End point (orange)
        float endX = (float)((end.x - minx) * scale);
        float endY = height - (float)((end.y - miny) * scale);
        g.FillEllipse(Brushes.Orange, endX - 6, endY - 6, 12, 12);
        g.DrawEllipse(Pens.DarkOrange, endX - 6, endY - 6, 12, 12);
    }

    private static void DrawSimpleLegend(Graphics g)
    {
        g.DrawString("Start: Green", new Font("Arial", 10), Brushes.Black, 10, 10);
        g.DrawString("End: Orange", new Font("Arial", 10), Brushes.Black, 10, 30);
        g.DrawString("Path: Red", new Font("Arial", 10), Brushes.Black, 10, 50);
        g.DrawString("Walking: Green Dashed", new Font("Arial", 10), Brushes.Black, 10, 70);
    }
}