using Rhino;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Annotation
{
    internal class LevelCurves
    {
        Curve[] Curves { get; set; }
        public LevelCurves(Curve[] curves)
        {
            this.Curves = curves;
        }
        public LevelCurves()
        {

        }

        public static LevelCurves Create(Curve[] curves, Brep box, LevelIndex levelIndex)
        {
            List<Curve> edges = new List<Curve>();
            edges.AddRange(box.Edges);
            Curve edge = edges[(int)levelIndex];

            LevelCurves levelCurves = new LevelCurves();
            for (int i = 0; i < curves.Count(); i++)
            {
                if (GetMinDistancesBetweenCurves(curves[i], edge) < RhinoDoc.ActiveDoc.ModelAbsoluteTolerance)
                {

                }
                else
                {
                    throw new Exception("");
                }
            }
            return levelCurves;
        }
        private static double GetMinDistancesBetweenCurves(Curve curveA, Curve curveB)
        {
            double tolerance = RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;
            double maxDis = 0;
            double maxDisParamA = 0;
            double maxDisParamB = 0;
            double minDis = 0;
            double minDisParamA = 0;
            double minDisParamB = 0;
            Curve.GetDistancesBetweenCurves(curveA, curveB, tolerance
                , out maxDis, out maxDisParamA, out maxDisParamB, out minDis, out minDisParamA, out minDisParamB);

            return minDis;
        }
        public enum LevelIndex
        {
            LevelZero = 0,
            LevelOne = 1,
            LevelTwo = 2,
            LevelThree = 3,
        }
    }
}
