using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using Priority_Queue;


namespace TSP
{

    class ProblemAndSolver
    {

        private class TSPSolution
        {
            /// <summary>
            /// we use the representation [cityB,cityA,cityC] 
            /// to mean that cityB is the first city in the solution, cityA is the second, cityC is the third 
            /// and the edge from cityC to cityB is the final edge in the path.  
            /// You are, of course, free to use a different representation if it would be more convenient or efficient 
            /// for your data structure(s) and search algorithm. 
            /// </summary>
            public ArrayList
                Route;

            /// <summary>
            /// constructor
            /// </summary>
            /// <param name="iroute">a (hopefully) valid tour</param>
            public TSPSolution(ArrayList iroute)
            {
                Route = new ArrayList(iroute);
            }

            /// <summary>
            /// Compute the cost of the current route.  
            /// Note: This does not check that the route is complete.
            /// It assumes that the route passes from the last city back to the first city. 
            /// </summary>
            /// <returns></returns>
            public double costOfRoute()
            {
                // go through each edge in the route and add up the cost. 
                int x;
                City here;
                double cost = 0D;

                for (x = 0; x < Route.Count - 1; x++)
                {
                    here = Route[x] as City;
                    cost += here.costToGetTo(Route[x + 1] as City);
                }

                // go from the last city to the first. 
                here = Route[Route.Count - 1] as City;
                cost += here.costToGetTo(Route[0] as City);
                return cost;
            }
        }

        #region Private members 

        /// <summary>
        /// Default number of cities (unused -- to set defaults, change the values in the GUI form)
        /// </summary>
        // (This is no longer used -- to set default values, edit the form directly.  Open Form1.cs,
        // click on the Problem Size text box, go to the Properties window (lower right corner), 
        // and change the "Text" value.)
        private const int DEFAULT_SIZE = 25;

        /// <summary>
        /// Default time limit (unused -- to set defaults, change the values in the GUI form)
        /// </summary>
        // (This is no longer used -- to set default values, edit the form directly.  Open Form1.cs,
        // click on the Time text box, go to the Properties window (lower right corner), 
        // and change the "Text" value.)
        private const int TIME_LIMIT = 60;        //in seconds

        private const int CITY_ICON_SIZE = 5;


        // For normal and hard modes:
        // hard mode only
        private const double FRACTION_OF_PATHS_TO_REMOVE = 0.20;

        /// <summary>
        /// the cities in the current problem.
        /// </summary>
        private City[] Cities;
        /// <summary>
        /// a route through the current problem, useful as a temporary variable. 
        /// </summary>
        private ArrayList Route;
        /// <summary>
        /// best solution so far. 
        /// </summary>
        private TSPSolution bssf; 

        /// <summary>
        /// how to color various things. 
        /// </summary>
        private Brush cityBrushStartStyle;
        private Brush cityBrushStyle;
        private Pen routePenStyle;


        /// <summary>
        /// keep track of the seed value so that the same sequence of problems can be 
        /// regenerated next time the generator is run. 
        /// </summary>
        private int _seed;
        /// <summary>
        /// number of cities to include in a problem. 
        /// </summary>
        private int _size;

        /// <summary>
        /// Difficulty level
        /// </summary>
        private HardMode.Modes _mode;

        /// <summary>
        /// random number generator. 
        /// </summary>
        private Random rnd;

        /// <summary>
        /// time limit in milliseconds for state space search
        /// can be used by any solver method to truncate the search and return the BSSF
        /// </summary>
        private int time_limit;

        // class for holding Branch and Bound state
        private class BBState
        {
            public double[,] costMatrix;
            public int[] inEdges;
            public int[] outEdges;
            public double cost;
            public int numEdges;
            public int numAddedEdges;

            public BBState(int edges)
            {
                cost = 0;
                numEdges = edges;
                inEdges = new int[edges];
                outEdges = new int[edges];
                for (int i = 0; i < numEdges; i++)
                {
                    inEdges[i] = -1;
                    outEdges[i] = -1;
                }
                costMatrix = new double[edges, edges];
                numAddedEdges = 0;
            }
            public BBState(BBState other)
            {
                cost = other.cost;
                numEdges = other.numEdges;
                inEdges = new int[numEdges];
                outEdges = new int[numEdges];
                costMatrix = new double[numEdges, numEdges];
                for (int i = 0; i < numEdges; i++)
                {
                    inEdges[i] = other.inEdges[i];
                    outEdges[i] = other.outEdges[i];
                    for (int j = 0; j < numEdges; j++)
                    {
                        costMatrix[i, j] = other.costMatrix[i, j];
                    }
                }
                numAddedEdges = other.numAddedEdges;
            }

        }


        #endregion

        #region Public members

        /// <summary>
        /// These three constants are used for convenience/clarity in populating and accessing the results array that is passed back to the calling Form
        /// </summary>
        public const int COST = 0;           
        public const int TIME = 1;
        public const int COUNT = 2;
        
        public int Size
        {
            get { return _size; }
        }

        public int Seed
        {
            get { return _seed; }
        }
        #endregion

        #region Constructors
        public ProblemAndSolver()
        {
            this._seed = 1; 
            rnd = new Random(1);
            this._size = DEFAULT_SIZE;
            this.time_limit = TIME_LIMIT * 1000;                  // TIME_LIMIT is in seconds, but timer wants it in milliseconds

            this.resetData();
        }

        public ProblemAndSolver(int seed)
        {
            this._seed = seed;
            rnd = new Random(seed);
            this._size = DEFAULT_SIZE;
            this.time_limit = TIME_LIMIT * 1000;                  // TIME_LIMIT is in seconds, but timer wants it in milliseconds

            this.resetData();
        }

        public ProblemAndSolver(int seed, int size)
        {
            this._seed = seed;
            this._size = size;
            rnd = new Random(seed);
            this.time_limit = TIME_LIMIT * 1000;                        // TIME_LIMIT is in seconds, but timer wants it in milliseconds

            this.resetData();
        }
        public ProblemAndSolver(int seed, int size, int time)
        {
            this._seed = seed;
            this._size = size;
            rnd = new Random(seed);
            this.time_limit = time*1000;                        // time is entered in the GUI in seconds, but timer wants it in milliseconds

            this.resetData();
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Reset the problem instance.
        /// </summary>
        private void resetData()
        {

            Cities = new City[_size];
            Route = new ArrayList(_size);
            bssf = null;

            if (_mode == HardMode.Modes.Easy)
            {
                for (int i = 0; i < _size; i++)
                    Cities[i] = new City(rnd.NextDouble(), rnd.NextDouble());
            }
            else // Medium and hard
            {
                for (int i = 0; i < _size; i++)
                    Cities[i] = new City(rnd.NextDouble(), rnd.NextDouble(), rnd.NextDouble() * City.MAX_ELEVATION);
            }

            HardMode mm = new HardMode(this._mode, this.rnd, Cities);
            if (_mode == HardMode.Modes.Hard)
            {
                int edgesToRemove = (int)(_size * FRACTION_OF_PATHS_TO_REMOVE);
                mm.removePaths(edgesToRemove);
            }
            City.setModeManager(mm);

            cityBrushStyle = new SolidBrush(Color.Black);
            cityBrushStartStyle = new SolidBrush(Color.Red);
            routePenStyle = new Pen(Color.Blue,1);
            routePenStyle.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
        }

        private void initBSSF()
        {
            int x;
            ArrayList init = new ArrayList();
            List<int> fix = new List<int>();
            // this is the trivial solution. 
            for (x = 0; x < Cities.Length; x++)
            {
                init.Add(Cities[Cities.Length - x - 1]);
                if (x != 0 && Cities[Cities.Length - x].costToGetTo(Cities[Cities.Length - x - 1]) == double.PositiveInfinity)
                    fix.Add(x); // fix city at location x.
            }
            Random m = new Random();
            for (int i = 0; i < fix.Count; i++)
            {
                City fixPrev = init[(fix[i] - 1) % Cities.Length] as City;
                City fixMid = init[fix[i]] as City;
                City fixNext = init[(fix[i] + 1) % Cities.Length] as City;
                City swapPrev;
                City swapMid;
                City swapNext;
                int swapIndex;
                do
                {
                    swapIndex = m.Next(1, Cities.Length - 1);
                    swapPrev = init[(swapIndex - 1) % Cities.Length] as City;
                    swapMid = init[swapIndex % Cities.Length] as City;
                    swapNext = init[(swapIndex + 1) % Cities.Length] as City;
                } while (fixPrev.costToGetTo(swapMid) == double.PositiveInfinity ||
                swapMid.costToGetTo(fixNext) == double.PositiveInfinity ||
                swapPrev.costToGetTo(fixMid) == double.PositiveInfinity ||
                fixMid.costToGetTo(swapNext) == double.PositiveInfinity);

                // found an agreeable swap, fix.
                init[fix[i]] = swapMid;
                init[swapIndex] = fixNext;
            }

            // call this the best solution so far.  bssf is the route that will be drawn by the Draw method. 
            bssf = new TSPSolution(init);


            // try 2 other random solutions
            for (int j = 0; j < 2; j++)
            {
                // randomize
                for (int i = 1; i < Cities.Length; i++)
                {
                    City fixPrev = init[(i - 1) % Cities.Length] as City;
                    City fixMid = init[i] as City;
                    City fixNext = init[(i + 1) % Cities.Length] as City;
                    City swapPrev;
                    City swapMid;
                    City swapNext;
                    int swapIndex;
                    do
                    {
                        swapIndex = m.Next(1, Cities.Length - 1);
                        swapPrev = init[swapIndex - 1 % Cities.Length] as City;
                        swapMid = init[swapIndex % Cities.Length] as City;
                        swapNext = init[swapIndex + 1 % Cities.Length] as City;
                    } while (fixPrev.costToGetTo(swapMid) == double.PositiveInfinity ||
                    swapMid.costToGetTo(fixNext) == double.PositiveInfinity ||
                    swapPrev.costToGetTo(fixMid) == double.PositiveInfinity ||
                    fixMid.costToGetTo(swapNext) == double.PositiveInfinity);

                    // found an agreeable swap, fix.
                    init[i] = swapMid;
                    init[swapIndex] = fixNext;
                }
                TSPSolution tmp = new TSPSolution(init);
                if (tmp.costOfRoute() < bssf.costOfRoute())
                    bssf = tmp;
            }
        }

        private void rowAndColumnReduction(BBState cost)
        {
            // for each row
            for (int i = 0; i < cost.numEdges; i++)
            {
                // skip row if all infinities because edge already included
                if (cost.outEdges[i] != -1) continue;

                // find the min
                double min = double.PositiveInfinity;
                for (int j = 0; j < cost.numEdges; j++)
                {
                    if (cost.costMatrix[i, j] < min) min = cost.costMatrix[i, j];
                }

                // if the min is not 0 (already reduced)
                if (min != 0)
                {
                    // add to cost
                    cost.cost += min;
                    // subtract from all other entries
                    for (int j = 0; j < cost.numEdges; j++)
                    {
                        cost.costMatrix[i, j] -= min;
                    }
                }
            }

            // Repeat for each column
            for (int i = 0; i < cost.numEdges; i++)
            {
                // skip row if all infinities because edge already included
                if (cost.inEdges[i] != -1) continue;

                // find the min
                double min = double.PositiveInfinity;
                for (int j = 0; j < cost.numEdges; j++)
                {
                    if (cost.costMatrix[j, i] < min) min = cost.costMatrix[j, i];
                }

                // if the min is not 0 (already reduced)
                if (min != 0)
                {
                    // add to cost
                    cost.cost += min;
                    // subtract from all other entries
                    for (int j = 0; j < cost.numEdges; j++)
                    {
                        cost.costMatrix[j, i] -= min;
                    }
                }
            }
        }

        private double worstCase(BBState cost, int city1, int city2)
        {
            // will be min in row + min in column
            double ret = 0;
            double min = double.PositiveInfinity;
            for (int i = 0; i < cost.numEdges; i++)
            {
                if (i != city2 && cost.costMatrix[city1, i] < min)
                    min = cost.costMatrix[city1, i];
            }
            ret += min;
            min = double.PositiveInfinity;
            for (int j = 0; j < cost.numEdges; j++)
            {
                if (j != city1 && cost.costMatrix[j, city2] < min)
                    min = cost.costMatrix[j, city2];
            }
            ret += min;
            ret += cost.cost;
            return ret;
        }

        private BBState includeEdge(BBState cost, int city1, int city2)
        {
            BBState ret = new BBState(cost);
            ret.cost += cost.costMatrix[city1, city2];

            // infinite out row
            for (int i = 0; i < ret.numEdges; i++)
            {
                ret.costMatrix[i, city2] = double.PositiveInfinity;
            }

            // infinite out column
            for (int j = 0; j < ret.numEdges; j++)
            {
                ret.costMatrix[city1, j] = double.PositiveInfinity;
            }

            // update in edges and out edges
            ret.outEdges[city1] = city2;
            ret.inEdges[city2] = city1;
            ret.numAddedEdges++;

            // prevent cycles
            if (ret.numAddedEdges < ret.numEdges - 1)
            {
                // first, find the city in this subset that doesn't have an outgoing edge yet
                int lastCity = city1;
                while (ret.outEdges[lastCity] != -1)
                {
                    lastCity = ret.outEdges[lastCity];
                }

                // now, take that city and make sure it won't try to go into anything
                // already in the cycle.
                int prevCity = ret.inEdges[city2];
                while (prevCity != -1)
                {
                    // walk up the line of cities we just added
                    ret.costMatrix[lastCity, prevCity] = double.PositiveInfinity;
                    prevCity = ret.inEdges[prevCity];
                }
            }

            // call reduce
            rowAndColumnReduction(ret);
            return ret;
        }


        #endregion

        #region Public Methods

        /// <summary>
        /// make a new problem with the given size.
        /// </summary>
        /// <param name="size">number of cities</param>
        public void GenerateProblem(int size, HardMode.Modes mode)
        {
            this._size = size;
            this._mode = mode;
            resetData();
        }

        /// <summary>
        /// make a new problem with the given size, now including timelimit paremeter that was added to form.
        /// </summary>
        /// <param name="size">number of cities</param>
        public void GenerateProblem(int size, HardMode.Modes mode, int timelimit)
        {
            this._size = size;
            this._mode = mode;
            this.time_limit = timelimit*1000;                                   //convert seconds to milliseconds
            resetData();
        }

        /// <summary>
        /// return a copy of the cities in this problem. 
        /// </summary>
        /// <returns>array of cities</returns>
        public City[] GetCities()
        {
            City[] retCities = new City[Cities.Length];
            Array.Copy(Cities, retCities, Cities.Length);
            return retCities;
        }

        /// <summary>
        /// draw the cities in the problem.  if the bssf member is defined, then
        /// draw that too. 
        /// </summary>
        /// <param name="g">where to draw the stuff</param>
        public void Draw(Graphics g)
        {
            float width  = g.VisibleClipBounds.Width-45F;
            float height = g.VisibleClipBounds.Height-45F;
            Font labelFont = new Font("Arial", 10);

            // Draw lines
            if (bssf != null)
            {
                // make a list of points. 
                Point[] ps = new Point[bssf.Route.Count];
                int index = 0;
                foreach (City c in bssf.Route)
                {
                    if (index < bssf.Route.Count -1)
                        g.DrawString(" " + index +"("+c.costToGetTo(bssf.Route[index+1]as City)+")", labelFont, cityBrushStartStyle, new PointF((float)c.X * width + 3F, (float)c.Y * height));
                    else 
                        g.DrawString(" " + index +"("+c.costToGetTo(bssf.Route[0]as City)+")", labelFont, cityBrushStartStyle, new PointF((float)c.X * width + 3F, (float)c.Y * height));
                    ps[index++] = new Point((int)(c.X * width) + CITY_ICON_SIZE / 2, (int)(c.Y * height) + CITY_ICON_SIZE / 2);
                }

                if (ps.Length > 0)
                {
                    g.DrawLines(routePenStyle, ps);
                    g.FillEllipse(cityBrushStartStyle, (float)Cities[0].X * width - 1, (float)Cities[0].Y * height - 1, CITY_ICON_SIZE + 2, CITY_ICON_SIZE + 2);
                }

                // draw the last line. 
                g.DrawLine(routePenStyle, ps[0], ps[ps.Length - 1]);
            }

            // Draw city dots
            foreach (City c in Cities)
            {
                g.FillEllipse(cityBrushStyle, (float)c.X * width, (float)c.Y * height, CITY_ICON_SIZE, CITY_ICON_SIZE);
            }

        }

        /// <summary>
        ///  return the cost of the best solution so far. 
        /// </summary>
        /// <returns></returns>
        public double costOfBssf ()
        {
            if (bssf != null)
                return (bssf.costOfRoute());
            else
                return -1D; 
        }

        /// <summary>
        /// This is the entry point for the default solver
        /// which just finds a valid random tour 
        /// </summary>
        /// <returns>results array for GUI that contains three ints: cost of solution, time spent to find solution, number of solutions found during search (not counting initial BSSF estimate)</returns>
        public string[] defaultSolveProblem()
        {
            int i, swap, temp, count=0;
            string[] results = new string[3];
            int[] perm = new int[Cities.Length];
            Route = new ArrayList();
            Random rnd = new Random();
            Stopwatch timer = new Stopwatch();

            timer.Start();

            do
            {
                for (i = 0; i < perm.Length; i++)                                 // create a random permutation template
                    perm[i] = i;
                for (i = 0; i < perm.Length; i++)
                {
                    swap = i;
                    while (swap == i)
                        swap = rnd.Next(0, Cities.Length);
                    temp = perm[i];
                    perm[i] = perm[swap];
                    perm[swap] = temp;
                }
                Route.Clear();
                for (i = 0; i < Cities.Length; i++)                            // Now build the route using the random permutation 
                {
                    Route.Add(Cities[perm[i]]);
                }
                bssf = new TSPSolution(Route);
                count++;
            } while (costOfBssf() == double.PositiveInfinity);                // until a valid route is found
            timer.Stop();

            results[COST] = costOfBssf().ToString();                          // load results array
            results[TIME] = timer.Elapsed.ToString();
            results[COUNT] = count.ToString();

            return results;
        }

        /// <summary>
        /// performs a Branch and Bound search of the state space of partial tours
        /// stops when time limit expires and uses BSSF as solution
        /// </summary>
        /// <returns>results array for GUI that contains three ints: cost of solution, time spent to find solution, number of solutions found during search (not counting initial BSSF estimate)</returns>
        public string[] bBSolveProblem()
        {
            string[] results = new string[3];

            // start our timer
            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            timer.Start();

            int solNumber = 0;
            int statesCreated = 0;
            int branchesConsidered = 0;
            int statesStored = 0;
            int statesPruned = 0;

            // generate bssf
            initBSSF();
            double bsf = costOfBssf();
            double initial = bsf;

            // generate initial cost matrix
            BBState s = new BBState(Cities.Length);
            for (int i = 0; i < Cities.Length; i++)
            {
                for (int j = 0; j < Cities.Length; j++)
                {
                    if (i == j)
                        s.costMatrix[i, j] = double.PositiveInfinity;
                    else
                        s.costMatrix[i, j] = Cities[i].costToGetTo(Cities[j]);
                }
            }
            rowAndColumnReduction(s);

            // create priority queue and init with initial state
            SimplePriorityQueue<BBState> pq = new SimplePriorityQueue<BBState>();
            pq.Enqueue(s, 0);

            // loop - while !empty and cost < bssf{   
            while (pq.Count > 0 && timer.ElapsedMilliseconds <= time_limit)
            {
                BBState cur = pq.Dequeue();
                if (cur.cost > bsf)
                {
                    statesPruned++;
                    continue;
                }

                BBState nextInc = cur;
                BBState nextEx;
                int cityOut = -1;
                int cityIn = -1;
                double improvement = double.NegativeInfinity;

                // for all edges
                for (int i = 0; i < cur.numEdges; i++)
                {
                    for (int j = 0; j < cur.numEdges; j++)
                    {
                        // if edge is 0
                        if (cur.costMatrix[i, j] == 0)
                        {
                            // calculate best if included
                            BBState tmp = includeEdge(cur, i, j);
                            branchesConsidered++;

                            // and improvement over exclude
                            double tmpImprov = worstCase(cur, i, j) - tmp.cost;

                            // and if keep if best improvement so far
                            if (improvement < tmpImprov)
                            {
                                nextInc = tmp;
                                cityOut = i;
                                cityIn = j;
                                improvement = tmpImprov;
                            }
                        }
                    }
                }

                if (nextInc.cost < bsf)
                {
                    // is this state a complete solution?
                    if (nextInc.numAddedEdges == nextInc.numEdges)
                    {
                        // transform into bssf
                        ArrayList route = new ArrayList();
                        int city = 0;
                        do
                        {
                            route.Add(Cities[city]);
                            city = nextInc.outEdges[city];
                        } while (city != 0);

                        // update
                        bssf = new TSPSolution(route);
                        bsf = costOfBssf();
                        solNumber++;
                    }
                    else
                    {
                        // we've found the state with the best improvement
                        // so calculate create the exclude state;
                        nextEx = new BBState(cur);
                        nextEx.costMatrix[cityOut, cityIn] = double.PositiveInfinity;
                        rowAndColumnReduction(nextEx);

                        // enqueue both of the new states
                        pq.Enqueue(nextInc, Convert.ToInt32(nextInc.cost / (nextInc.numAddedEdges + 1)));
                        statesCreated += 2;

                        // enqueue if not infinite
                        if (nextEx.cost < bsf)
                            pq.Enqueue(nextEx, Convert.ToInt32(nextEx.cost / (nextInc.numAddedEdges + 1)));
                        else
                            statesPruned++;

                        // die with soemthing if we never actually expanded the state
                        if (nextInc == cur) throw new NotSupportedException();
                    }
                    if (pq.Count > statesStored)
                    {
                        statesStored = pq.Count;
                    }
                }
            } // end while loop



            results[COST] = bssf.costOfRoute().ToString();    // load results into array here, replacing these dummy values
            results[TIME] = Convert.ToString(timer.Elapsed);
            results[COUNT] = "-1";

            return results;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////
        // These additional solver methods will be implemented as part of the group project.
        ////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// finds the greedy tour starting from each city and keeps the best (valid) one
        /// </summary>
        /// <returns>results array for GUI that contains three ints: cost of solution, time spent to find solution, number of solutions found during search (not counting initial BSSF estimate)</returns>
        public string[] greedySolveProblem()
        {
            string[] results = new string[3];

            // TODO: Add your implementation for a greedy solver here.

            results[COST] = "not implemented";    // load results into array here, replacing these dummy values
            results[TIME] = "-1";
            results[COUNT] = "-1";

            return results;
        }

        public string[] fancySolveProblem()
        {
            string[] results = new string[3];

            // TODO: Add your implementation for your advanced solver here.

            results[COST] = "not implemented";    // load results into array here, replacing these dummy values
            results[TIME] = "-1";
            results[COUNT] = "-1";

            return results;
        }
        #endregion
    }

}
