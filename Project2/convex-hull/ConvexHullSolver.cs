using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Drawing;
using _1_convex_hull;

namespace _2_convex_hull
{
    class ConvexHullSolver
    {
        static System.Drawing.Graphics _g;
        System.Windows.Forms.PictureBox pictureBoxView;
        public static ConvexHullSolver _instance;

        public ConvexHullSolver(System.Drawing.Graphics g, System.Windows.Forms.PictureBox pictureBoxView)
        {
            _instance = this; // make a sort of singleton so we can draw right from the CHull
            _g = g;
            this.pictureBoxView = pictureBoxView;
        }

        public static System.Drawing.Graphics getGraphics()
        {
            return _g;
        }

        public void Refresh()
        {
            // Use this especially for debugging and whenever you want to see what you have drawn so far
            pictureBoxView.Refresh();
        }

        public void Pause(int milliseconds)
        {
            // Use this especially for debugging and to animate your algorithm slowly
            pictureBoxView.Refresh();
            System.Threading.Thread.Sleep(milliseconds);
        }

        public void Solve(List<System.Drawing.PointF> pointList)
        {
            List<PointF> sortedPoints = pointList.OrderBy(p => p.X).ToList();
                // the complexity of OrderBy isn't given in documentation, but 
                //  I assume Microsoft isn't putting crap code in C#, so it must be no 
                //  worse than O(nlogn)
            CHull convexHull = getHull(sortedPoints);
            convexHull.drawHull();
        }

        private CHull getHull(List<System.Drawing.PointF> pointList)
        {
            // recursive method: split the hull in half, call getHull on each half, merge.
            // base case: 2 or 3 points.  just return a new CHull
            // otherwise: split list into L and R
            //              call getHull on each list to get LHull and RHull
            //              call LHull.merge(RHull);
            
            if (pointList.Count < 4)
            {
                // base case
                return new CHull(pointList);
            }

            else
            {
                // we don't need to worry about the EXACT midpoint (no special handling of odd-length list)
                // because call tree doesn't need to be balanced
                List<PointF> LPoints = new List<PointF>(pointList.Take<PointF>(pointList.Count / 2));
                List<PointF> RPoints = new List<PointF>(pointList.Skip<PointF>(pointList.Count / 2));

                CHull LHull = getHull(LPoints);
                CHull RHull = getHull(RPoints);

                return LHull.merge(RHull);

                /*
                 * This algorithm gets two subproblems of 1/2 size, and then joins them in (worst-case) quadratic time.
                 */
            }
        }
    }
}
