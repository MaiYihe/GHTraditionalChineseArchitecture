using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Annotation
{
    internal class AnnotationGuides
    {
        View topView { get; set; }
        View frontView { get; set; }
        View rightView { get; set; }
        
        public AnnotationGuides(View[] views)
        {
            topView = views[0];
            frontView = views[1];
            rightView = views[2];
        }
        public AnnotationGuides()
        {
        }


        //public static AnnotationGuides CreateFromCurves(LevelCurves[] sideCurves, Curve[] innerCurves, Plane plane)
        //{
        //    if (AllEdgesAlignSide(sideCurves, plane) == false) return;
        //    AnnotationGuides annotationGuides = new AnnotationGuides();

        //    return annotationGuides;
        //}
        //private bool AllEdgesAlignSide(Curve[] sideCurves, Plane plane)
        //{
        //    BoundingBox boundingBox = BoundingBox.Empty;
        //    for (int i = 0; i < sideCurves.Count(); i++)
        //    {
        //        BoundingBox tmpBoundingBox = sideCurves[i].GetBoundingBox(plane);
        //        boundingBox.Union(tmpBoundingBox);
        //    }

        //    List<Curve> alignedCurves = new List<Curve>();
        //    Line[] lines = boundingBox.GetEdges();
        //    for (int i = 0; i < lines.Count(); i++)
        //    {
        //        for (int j = 0; j < lines[i].Length; j++)
        //        {
        //            if (Curve.GetDistancesBetweenCurves(lines[i].ToNurbsCurve,))
        //        }
        //    }
        //}
    }
}
