using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;

namespace NetworkRouting
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void clearAll()
        {
            startNodeIndex = -1;
            stopNodeIndex = -1;
            sourceNodeBox.Clear();
            sourceNodeBox.Refresh();
            targetNodeBox.Clear();
            targetNodeBox.Refresh();
            arrayTimeBox.Clear();
            arrayTimeBox.Refresh();
            heapTimeBox.Clear();
            heapTimeBox.Refresh();
            differenceBox.Clear();
            differenceBox.Refresh();
            pathCostBox.Clear();
            pathCostBox.Refresh();
            arrayCheckBox.Checked = false;
            arrayCheckBox.Refresh();
            return;
        }

        private void clearSome()
        {
            arrayTimeBox.Clear();
            arrayTimeBox.Refresh();
            heapTimeBox.Clear();
            heapTimeBox.Refresh();
            differenceBox.Clear();
            differenceBox.Refresh();
            pathCostBox.Clear();
            pathCostBox.Refresh();
            return;
        }

        private void generateButton_Click(object sender, EventArgs e)
        {
            int randomSeed = int.Parse(randomSeedBox.Text);
            int size = int.Parse(sizeBox.Text);

            Random rand = new Random(randomSeed);
            seedUsedLabel.Text = "Random Seed Used: " + randomSeed.ToString();

            clearAll();
            this.adjacencyList = generateAdjacencyList(size, rand);
            List<PointF> points = generatePoints(size, rand);
            resetImageToPoints(points);
            this.points = points;
        }

        // Generates the distance matrix.  Values of -1 indicate a missing edge.  Loopbacks are at a cost of 0.
        private const int MIN_WEIGHT = 1;
        private const int MAX_WEIGHT = 100;
        private const double PROBABILITY_OF_DELETION = 0.35;

        private const int NUMBER_OF_ADJACENT_POINTS = 3;

        private List<HashSet<int>> generateAdjacencyList(int size, Random rand)
        {
            List<HashSet<int>> adjacencyList = new List<HashSet<int>>();

            for (int i = 0; i < size; i++)
            {
                HashSet<int> adjacentPoints = new HashSet<int>();
                while (adjacentPoints.Count < 3)
                {
                    int point = rand.Next(size);
                    if (point != i) adjacentPoints.Add(point);
                }
                adjacencyList.Add(adjacentPoints);
            }

            return adjacencyList;
        }

        private List<PointF> generatePoints(int size, Random rand)
        {
            List<PointF> points = new List<PointF>();
            for (int i = 0; i < size; i++)
            {
                points.Add(new PointF((float) (rand.NextDouble() * pictureBox.Width), (float) (rand.NextDouble() * pictureBox.Height)));
            }
            return points;
        }

        private void resetImageToPoints(List<PointF> points)
        {
            pictureBox.Image = new Bitmap(pictureBox.Width, pictureBox.Height);
            Graphics graphics = Graphics.FromImage(pictureBox.Image);
            Pen pen;

            if (points.Count < 100)
                pen = new Pen(Color.Blue);
            else
                pen = new Pen(Color.LightBlue);
            foreach (PointF point in points)
            {
                graphics.DrawEllipse(pen, point.X, point.Y, 2, 2);
            }

            this.graphics = graphics;
            pictureBox.Invalidate();
        }

        // These variables are instantiated after the "Generate" button is clicked
        private List<PointF> points = new List<PointF>();
        private Graphics graphics;
        private List<HashSet<int>> adjacencyList;

        private Double getWeight(int a, int b)
        {
            PointF p1 = points.ElementAt(a);
            PointF p2 = points.ElementAt(b);

            double dx = p1.X - p2.X;
            double dy = p1.Y - p2.Y;

            return Math.Sqrt(dx * dx + dy * dy);
        }

        // Use this to generate paths (from start) to every node; then, just return the path of interest from start node to end node
        private void solveButton_Click(object sender, EventArgs e)
        {
            // This was the old entry point, but now it is just some form interface handling
            bool ready = true;

            if(startNodeIndex == -1)
            {
                sourceNodeBox.Focus();
                sourceNodeBox.BackColor = Color.Red;
                ready = false;
            }
            if(stopNodeIndex == -1)
            {
                if(!sourceNodeBox.Focused)
                    targetNodeBox.Focus();
                targetNodeBox.BackColor = Color.Red;
                ready = false;
            }
            if (points.Count > 0)
            {
                resetImageToPoints(points);
                paintStartStopPoints();
            }
            else
            {
                ready = false;
            }
            if(ready)
            {
                clearSome();
                solveButton_Clicked();  // Here is the new entry point
            }
        }

        private void solveButton_Clicked()
        {
            // *** Implement this method, use the variables "startNodeIndex" and "stopNodeIndex" as the indices for your start and stop points, respectively ***
            // we'll run Dijkstra's with a default 

            Stopwatch timer = new Stopwatch();

            double heapResult = Dijkstra(timer, false);
            double heapTime = timer.ElapsedMilliseconds;

            double arrayResult = 0.0;
            double arrayTime = 0.0;

            if (arrayCheckBox.Checked)
            {
                timer.Reset();
                arrayResult = Dijkstra(timer, true);
                arrayTime = timer.ElapsedMilliseconds;
            }

            heapTimeBox.Text = heapTime.ToString();
            arrayTimeBox.Text = arrayTime.ToString();
            differenceBox.Text = (arrayTime - heapTime).ToString();

            if ((heapResult == arrayResult) || (arrayResult == 0))
            {
                pathCostBox.Text = heapResult.ToString();
            }
            else
            {
                pathCostBox.Text = "Heap and array disagree";
            }
            
        }

        private double Dijkstra(Stopwatch s, Boolean isArray)
        {
            /*
            procedure dijkstra(G, l, s)
            Input: Graph G = (V, E), directed or undirected;
                    positive edge lengths { le: e ∈ E}; vertex s ∈ V
            Output: For all vertices u reachable from s, dist(u) is set
                    to the distance from s to u.
            
            for all u ∈ V :
            dist(u) = ∞
            prev(u) = nil
            dist(s) = 0
            H = makequeue(V)(using dist - values as keys)
                    while H is not empty:
                        u = deletemin(H)
                    for all edges (u, v) ∈ E:
                    if dist(v) > dist(u) + l(u, v):
                       dist(v) = dist(u) + l(u, v)
                        prev(v) = u
                        decreasekey(H, v)
*/
            s.Start();
            int numberOfNodes = points.Count;

            PQueue queue;

            if (isArray)
            {
                queue = new AQueue(numberOfNodes);
            }
            else
            {
                //queue = new AQueue(numberOfNodes);

                queue = new BQueue(numberOfNodes);
            }

            Double[] dist = new Double[numberOfNodes];
            int[] prev = new int[numberOfNodes];

            for (int i = 0; i < numberOfNodes; i++)
            {
                dist[i] = Double.MaxValue;
                prev[i] = -1;
            }

            dist[startNodeIndex] = 0;
            queue.makeQueue();
            queue.reduceKey(startNodeIndex, 0);

            while(queue.notEmpty())
            {
                int u = queue.deleteMin();

                HashSet<int> E = adjacencyList.ElementAt(u);
                foreach (int v in E)
                {
                    Double nextHop = dist[u] + getWeight(u, v);
                    if (dist[v] > nextHop)
                    {
                        dist[v] = nextHop;
                        prev[v] = u;
                        queue.reduceKey(v, nextHop);
                    }
                }
            }

            s.Stop();

            // if it's not an array, go ahead and draw the path
            if (!isArray)
            {

                Pen p = new Pen(Color.Black, 1);
                Font arialFont = new Font("Arial", 11);

                int node = stopNodeIndex;
                int parent = prev[stopNodeIndex];

                while (parent != -1)
                {
                    graphics.DrawLine(p, points[node], points[parent]);

                    RectangleF rectf = new RectangleF(70, 90, 80, 80);//x, y, width, height
                    rectf.X = Math.Abs(points[node].X + points[parent].X) / 2;
                    rectf.Y = Math.Abs(points[node].Y + points[parent].Y) / 2;
                    graphics.DrawString(((int) getWeight(node, parent)).ToString(), arialFont, Brushes.Black, rectf);

                    node = parent;

                    parent = prev[parent];

               
                }
            }

            return dist[stopNodeIndex];
        }

        public class Node
        {
            public int pointsIndex;
            public int position;
            public double distance;
            public Node(int ptIndex)
            {
                pointsIndex = ptIndex;
                position = -1;
                distance = 0;
            }
        }

        public interface PQueue
        {
            //void insert(Double value);
            void makeQueue();
            void reduceKey(int key, Double newVal);
            int deleteMin();
            bool notEmpty();
        }

        public class BQueue : PQueue
        {
            private int parentOf(int node)
            {
                return node / 2;
            }

            private int leftChildOf(int node)
            {
                return 2 * node;
            }

            private int rightChildOf(int node)
            {
                return 2 * node + 1;
            }

            Node[] node_values;
            Double[] weights;
            List<Node> nodes;
            
            int occupied;
            int size;


            public BQueue(int size)
            {
                this.size = size;

                node_values = new Node[size];

                weights = new Double[size];
                nodes = new List<Node>();

                occupied = 0;  // kinda a pointer to last empty space
            }

            void Swap(int position1, int position2)
            {
                Node temp = node_values[position1];
                node_values[position1] = node_values[position2];
                node_values[position2] = temp;
                node_values[position1].position = position1;
                node_values[position2].position = position2;

                Double temp2 = weights[position1];
                weights[position1] = weights[position2];
                weights[position2] = temp2;
            }

            public void insert(Node item, double priority)
            {
                // Add the item to the heap in the end position of the array (i.e. as a leaf of the tree)
                int position = occupied++;
                node_values[position] = item;
                item.position = position;
                weights[position] = priority;
                // Move it upward into position, if necessary
                bubbleUp(position);
                nodes.Add(item);
            }

            void bubbleUp(int position)
            {
                while ((position > 0) && (weights[parentOf(position)] > weights[position]))
                {
                    int original_parent_pos = parentOf(position);
                    Swap(position, original_parent_pos);
                    position = original_parent_pos;
                }
            }

            public void makeQueue()
            {
                for (int i = 0; i < size; i++)
                {   
                    insert(new Node(i), Double.MaxValue);
                }
            }

            public void reduceKey(int key, Double newVal)
            {
                // reduce the key, 
                // then "bubble up" like in insert

                // find and reduce/update key
                int position = nodes[key].position;
                while ((position > 0) && (weights[parentOf(position)] > newVal))
                {
                    int original_parent_pos = parentOf(position);
                    Swap(original_parent_pos, position);
                    position = original_parent_pos;
                }
                weights[position] = newVal;
            }

            public int deleteMin()
            {
                nodes[node_values[0].pointsIndex].distance = weights[node_values[0].position];
                //data[0].distance = distances[data[0].position];
                Node minNode = node_values[0];
                Swap(0, occupied - 1);
                occupied--;
                siftDown(0);
                return minNode.pointsIndex;
            }

            public bool notEmpty()
            {
                return occupied != 0;
            }

            public void siftDown(int position)
            {
                /*
                 *  l ← Left-child-index(i)
                    r ← Right-child-index(i)
                    if l < heap-size[A] and A[l] > A[i] then
                        greatest ← l
                    else
                        greatest ← i
                    end if
                    if r < heap-size[A] and A[r] > A[greatest] then
                        greatest ← r
                    end if
                    if greatest ≠ i then
                        Swap(A[i],A[greatest])
                        Max-Heapify(A, greatest)
                    end if
                */

                int lchild = leftChildOf(position);//look at it's left child and get its value
                int rchild = rightChildOf(position); ;//look at it's left child and get its value
                int largest = 0;
                if ((lchild < occupied) && (weights[lchild] < weights[position]))
                {
                    largest = lchild;
                }
                else
                {
                    largest = position;
                }
                if ((rchild < occupied) && (weights[rchild] < weights[largest]))
                {
                    largest = rchild;
                }
                if (largest != position)
                {
                    Swap(position, largest);
                    siftDown(largest);
                }

            }
        } 

        public class AQueue : PQueue
        {
            Double[] array;
            int count;

            public AQueue(int size)
            {
                array = new Double[size];
                count = size;
            }

     /*       public void insert(Double value)
            {
                // essentially do nothing
            }   */

            public void makeQueue()
            {
                for (int i = 0; i < count; i++)
                {
                    array[i] = Double.MaxValue;
                }
            }

            public void reduceKey(int key, Double newVal)
            {
                array[key] = newVal;
            }

            public int deleteMin()
            {
                Double min = Double.MaxValue;
                int position = 0;

                for(int i = 0; i < array.Count(); i++)
                {
                    if(array[i] != -1 && array[i] < min)
                    {
                        position = i;
                        min = array[i];
                    }
                }

                count--;
                // let's mark this cell in the array as "used" so that we don't duplicate it
                array[position] = -1;

                return position;
            }

            public bool notEmpty()
            {
                return count != 0;
            }
        }

        private Boolean startStopToggle = true;
        private int startNodeIndex = -1;
        private int stopNodeIndex = -1;
        private void pictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (points.Count > 0)
            {
                Point mouseDownLocation = new Point(e.X, e.Y);
                int index = ClosestPoint(points, mouseDownLocation);
                if (startStopToggle)
                {
                    startNodeIndex = index;
                    sourceNodeBox.ResetBackColor();
                    sourceNodeBox.Text = "" + index;
                }
                else
                {
                    stopNodeIndex = index;
                    targetNodeBox.ResetBackColor();
                    targetNodeBox.Text = "" + index;
                }
                resetImageToPoints(points);
                paintStartStopPoints();
            }
        }

        private void sourceNodeBox_Changed(object sender, EventArgs e)
        {
            if (points.Count > 0)
            {
                try{ startNodeIndex = int.Parse(sourceNodeBox.Text); }
                catch { startNodeIndex = -1; }
                if (startNodeIndex < 0 | startNodeIndex > points.Count-1)
                    startNodeIndex = -1;
                if(startNodeIndex != -1)
                {
                    sourceNodeBox.ResetBackColor();
                    resetImageToPoints(points);
                    paintStartStopPoints();
                    startStopToggle = !startStopToggle;
                }
            }
        }

        private void targetNodeBox_Changed(object sender, EventArgs e)
        {
            if (points.Count > 0)
            {
                try { stopNodeIndex = int.Parse(targetNodeBox.Text); }
                catch { stopNodeIndex = -1; }
                if (stopNodeIndex < 0 | stopNodeIndex > points.Count-1)
                    stopNodeIndex = -1;
                if(stopNodeIndex != -1)
                {
                    targetNodeBox.ResetBackColor();
                    resetImageToPoints(points);
                    paintStartStopPoints();
                    startStopToggle = !startStopToggle;
                }
            }
        }
        
        private void paintStartStopPoints()
        {
            if (startNodeIndex > -1)
            {
                Graphics graphics = Graphics.FromImage(pictureBox.Image);
                graphics.DrawEllipse(new Pen(Color.Green, 6), points[startNodeIndex].X, points[startNodeIndex].Y, 1, 1);
                this.graphics = graphics;
                pictureBox.Invalidate();
            }

            if (stopNodeIndex > -1)
            {
                Graphics graphics = Graphics.FromImage(pictureBox.Image);
                graphics.DrawEllipse(new Pen(Color.Red, 2), points[stopNodeIndex].X - 3, points[stopNodeIndex].Y - 3, 8, 8);
                this.graphics = graphics;
                pictureBox.Invalidate();
            }
        }

        private int ClosestPoint(List<PointF> points, Point mouseDownLocation)
        {
            double minDist = double.MaxValue;
            int minIndex = 0;

            for (int i = 0; i < points.Count; i++)
            {
                double dist = Math.Sqrt(Math.Pow(points[i].X-mouseDownLocation.X,2) + Math.Pow(points[i].Y - mouseDownLocation.Y,2));
                if (dist < minDist)
                {
                    minIndex = i;
                    minDist = dist;
                }
            }

            return minIndex;
        }
    }
}
