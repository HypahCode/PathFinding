using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PathFinding
{
    public partial class Form1 : Form
    {
        public class TableConnection
        {
            public Node Node { get; set; }
            public float Distance { get; set; } = float.MaxValue;
            public TableConnection Previous { get; set; } = null;
            public bool IsVisibed { get; set; } = false;
            public TableConnection(Node node)
            {
                Node = node;
            }
        }

        public class NodeConnection
        {
            public Node Node { get; set; }
            public float Penalty { get; set; }

            public NodeConnection(Node n, float p)
            {
                Node = n;
                Penalty = (int)p;
            }
        }

        public class Node
        {
            public List<NodeConnection> visibleNodes = new List<NodeConnection>();
            public Point Location { get; set; }

            public string Name { get; }
            public Node(string name, Point l)
            {
                Name = name;
                Location = l;
            }

            public float Penalty(Node otherNode)
            {
                var p = new Point(Location.X - otherNode.Location.X, Location.Y - otherNode.Location.Y);
                return (float)Math.Sqrt((p.X * p.X) + (p.Y * p.Y));
            }

            public void AddVisibleNodes(IEnumerable<Node> nodes)
            {
                foreach (var n in nodes)
                {
                    visibleNodes.Add(new NodeConnection(n, Penalty(n)));
                }
            }
        }

        private List<Node> nodes = new List<Node>();

        public Form1()
        {
            InitializeComponent();

            var a = new Node("a", new Point(105, 240));
            var b = new Node("b", new Point(270, 364));
            var c = new Node("c", new Point(300, 100));
            var d = new Node("d", new Point(460, 220));
            var e = new Node("e", new Point(630, 429));
            var f = new Node("f", new Point(733, 250));
            var g = new Node("g", new Point(800, 118));
            var h = new Node("h", new Point(930, 222));
            var i = new Node("i", new Point(963, 430));

            // Different graph
            var x1 = new Node("1", new Point(300, 450));
            var x2 = new Node("2", new Point(500, 450));

            nodes.AddRange(new[] { a, b, c, d, e, f, g, h, i, x1, x2 });
            a.AddVisibleNodes(new[] { b, c });
            b.AddVisibleNodes(new[] { a, d, e });
            c.AddVisibleNodes(new[] { a, d, f, g });
            d.AddVisibleNodes(new[] { b, c, e });
            e.AddVisibleNodes(new[] { d, i, b });
            f.AddVisibleNodes(new[] { e, h });
            g.AddVisibleNodes(new[] { c, h });
            h.AddVisibleNodes(new[] { f, g, i });
            i.AddVisibleNodes(new[] { e, h });

            // Different graph, unreachable from [a..i]
            x1.AddVisibleNodes(new[] { x2 });
            x2.AddVisibleNodes(new[] { x1 });

            var bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            DrawNodes(bmp);
            DrawPath(bmp, CalculatePath(b, g), Color.Lime);
            DrawPath(bmp, CalculatePath(a, h), Color.DeepPink);
            DrawPath(bmp, CalculatePath(a, x1), Color.YellowGreen);
            DrawPath(bmp, CalculatePath(x2, d), Color.YellowGreen);
            DrawPath(bmp, CalculatePath(x1, x2), Color.LightBlue);
            pictureBox1.Image = bmp;
        }

        private List<Node> CalculatePath(Node start, Node destination)
        {
            var visited = new List<TableConnection>();
            var unvisited = new List<TableConnection>();
            foreach (var n in nodes)
            {
                unvisited.Add(new TableConnection(n));
            }
            unvisited.Find((x) => x.Node == start).Distance = 0;

            var node = start;
            while (true)
            {
                var visitor = unvisited.Find((x) => x.Node == node);
                unvisited.Remove(visitor);
                visited.Add(visitor);
                
                if (visitor.Node == destination)
                {
                    break;
                }

                foreach (var connection in node.visibleNodes)
                {
                    var next = unvisited.Find((x) => x.Node == connection.Node);
                    if (next != null)
                    {
                        var dist = visitor.Distance + connection.Penalty;
                        if (dist < next.Distance)
                        {
                            next.Distance = dist;
                            next.Previous = visitor;
                        }
                    }
                }
                unvisited.Sort((a, b) => a.Distance.CompareTo(b.Distance));
                node = unvisited[0].Node;
            }

            return GetPath(visited.Find((x) => x.Node == destination));
        }

        

        private List<Node> GetPath(TableConnection node)
        {
            var nodes = new List<Node>();
            while (node != null)
            {
                nodes.Add(node.Node);
                node = node.Previous;
            }
            nodes.Reverse();
            return nodes;
        }

        private void DrawNodes(Bitmap bmp)
        {
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Black);
                foreach (var n in nodes)
                {
                    g.DrawEllipse(new Pen(Color.Blue), new Rectangle(n.Location.X - 2, n.Location.Y - 2, 4, 4));
                    foreach (var toPath in n.visibleNodes)
                    {
                        g.DrawLine(new Pen(Color.Red), n.Location, toPath.Node.Location);
                        var center = new PointF((n.Location.X + toPath.Node.Location.X) / 2, (n.Location.Y + toPath.Node.Location.Y) / 2);
                        g.DrawString($"{toPath.Penalty}", new Font("Courier new", 10, FontStyle.Regular), new SolidBrush(Color.Red), center);
                    }
                    g.DrawString($"{n.Name}", new Font("Courier new", 12, FontStyle.Regular), new SolidBrush(Color.Yellow), n.Location.X, n.Location.Y);
                }
            }
        }

        private void DrawPath(Bitmap bmp, List<Node> path, Color color)
        {
            using (var g = Graphics.FromImage(bmp))
            {
                for (int i = 0; i < path.Count - 1; i++)
                {
                    g.DrawLine(new Pen(color), path[i].Location, path[i + 1].Location);
                }
            }
        }
    }
}
