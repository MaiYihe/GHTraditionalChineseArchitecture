using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilityElement.ReferenceDatum;
using static YhmAssembly.CurveUtility;
using UtilityElement.Metadeta;
using System.Reflection.Metadata;
using Grasshopper;
using Rhino.Geometry.Intersect;
using Rhino;
using YhmAssembly;
using System.Diagnostics.Metrics;
using System.Security.Cryptography;
using Microsoft.VisualBasic;
using static Rhino.Render.TextureGraphInfo;

namespace UtilityElement //对辅助类进行操作的工具集
{
    public static class BaseCurveUtility
    {
        //curve转定位基线
        public static BaseCurve CurveToBaseCurve(Curve curve)
        {
            Plane referencePlane = new Plane();
            Plane extrudePlane = new Plane();
            referencePlane = GetPlanes(curve, 0.5)[0];
            extrudePlane = GetPlanes(curve, 0)[2];
            BaseCurve baseCurve = new BaseCurve(curve, referencePlane, referencePlane.ZAxis, extrudePlane);
            return baseCurve;
        }

        public static List<BaseCurve> CurvesToBaseCurves(List<Curve> curves)
        {
            List<BaseCurve> baseCurves = new List<BaseCurve>();
            Plane referencePlane = new Plane();
            Plane extrudePlane = new Plane();

            foreach (Curve curve in curves)
            {
                referencePlane = GetPlanes(curve, 0.5)[0];
                extrudePlane = GetPlanes(curve, 0)[2];
                BaseCurve baseCurve1 = new BaseCurve(curve, referencePlane, referencePlane.ZAxis, extrudePlane);
                baseCurves.Add(baseCurve1);
            }
            return baseCurves;
        }
        public static List<BaseCurve> CurvesToBaseCurves(bool isSelected, List<Curve> curves)
        {
            List<BaseCurve> baseCurves = new List<BaseCurve>();
            Plane referencePlane = new Plane();
            Plane extrudePlane = new Plane();

            foreach (Curve curve in curves)
            {
                referencePlane = GetPlanes(curve, 0.5)[0];
                extrudePlane = GetPlanes(curve, 0)[2];
                BaseCurve baseCurve1 = new BaseCurve(isSelected, curve, referencePlane, referencePlane.ZAxis, extrudePlane);
                baseCurves.Add(baseCurve1);
            }
            return baseCurves;
        }
        public static Curve Vector3dToLine(Vector3d vector3D, Point3d from)//point3d是锚点
        {
            Point3d to = from;
            to.Transform(Transform.Translation(vector3D));
            Line line = new Line(from, to);
            return line.ToNurbsCurve();
        }

        //只适用于直线，本质上是对BaseCurve进行缩放
        public static List<BaseCurve> CurveCutBaseCurve(Curve cutter, BaseCurve baseCurve)
        {
            CurveIntersections intersections = Intersection.CurveCurve(cutter,
                baseCurve.curve, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
            Point3d[] tmpP = intersections.Select(x => x.PointA).ToArray();

            //排序点，长度列表对映的点
            List<double> distance = tmpP
                .Select(x => baseCurve.curve.PointAtStart.DistanceTo(x))
                .ToList();
            List<Point3d> sortedPoints = listEdit.Sort(distance, tmpP);
            sortedPoints.Insert(0, baseCurve.curve.PointAtStart);

            //得到各段长度列表
            List<double> lengthList = sortedPoints
                .Skip(1)
                .Select((x, i) => sortedPoints[i].DistanceTo(sortedPoints[i + 1]))
                .ToList();
            lengthList.Add(baseCurve.curve.PointAtEnd.DistanceTo(sortedPoints[sortedPoints.Count - 1]));

            //各段位移列表
            List<Vector3d> v = sortedPoints
                .Select((x, i) => new Vector3d(x - sortedPoints[0]))
                .ToList();

            //缩放，位移
            List<BaseCurve> resultBaseCurves = new List<BaseCurve>();
            for (int i = 0; i < lengthList.Count; i++)
            {
                BaseCurve tmpB = baseCurve.Duplicate();
                tmpB.Scale1D(lengthList[i] / baseCurve.curveLength, 0);
                tmpB.Transform(Transform.Translation(v[i]));
                resultBaseCurves.Add(tmpB);
            }

            return resultBaseCurves;
        }
        public static List<BaseCurve> CurvesCutBaseCurve(Curve[] cutters, BaseCurve baseCurve)
        {
            List<Point3d> cutterPoints = new List<Point3d>();
            for (int i = 0; i < cutters.Count(); i++)
            {
                CurveIntersections intersections = Intersection.CurveCurve(cutters[i],
               baseCurve.curve, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
                List<Point3d> tmpPs = intersections.Select(x => x.PointA).ToList();
                cutterPoints.AddRange(tmpPs);
            }
            List<BaseCurve> resultBaseCurves = PointsSplitBaseCurve(cutterPoints.ToArray(), baseCurve);
            return resultBaseCurves;
        }
        public static List<BaseCurve> PointsSplitBaseCurve(Point3d[] cutterPoints, BaseCurve baseCurve)
        {
            Point3d[] tmpP = cutterPoints;

            //排序点，长度列表对映的点
            List<double> distance = tmpP
                .Select(x => baseCurve.curve.PointAtStart.DistanceTo(x))
                .ToList();
            List<Point3d> sortedPoints = listEdit.Sort(distance, tmpP);
            sortedPoints.Insert(0, baseCurve.curve.PointAtStart);

            //得到各段长度列表
            List<double> lengthList = sortedPoints
                .Skip(1)
                .Select((x, i) => sortedPoints[i].DistanceTo(sortedPoints[i + 1]))
                .ToList();
            lengthList.Add(baseCurve.curve.PointAtEnd.DistanceTo(sortedPoints[sortedPoints.Count - 1]));

            //各段位移列表
            List<Vector3d> v = sortedPoints
                .Select((x, i) => new Vector3d(x - sortedPoints[0]))
                .ToList();

            //缩放，位移
            List<BaseCurve> resultBaseCurves = new List<BaseCurve>();
            for (int i = 0; i < lengthList.Count; i++)
            {
                BaseCurve tmpB = baseCurve.Duplicate();
                tmpB.Scale1D(lengthList[i] / baseCurve.curveLength, 0);
                tmpB.Transform(Transform.Translation(v[i]));
                resultBaseCurves.Add(tmpB);
            }
            return resultBaseCurves;
        }
        public static BaseCurve CurveSplitAndGetCoreBaseCurve(Curve cutter, BaseCurve baseCurve)
        {
            Point3d originalCenter = baseCurve.curve.PointAt(0.5);
            BaseCurve tmpBaseCurve = baseCurve.Duplicate();
            List<BaseCurve> splitedBaseCurves = CurveCutBaseCurve(cutter, baseCurve);

            var groupedKeyValues = splitedBaseCurves
                .GroupBy(x => x.curve.PointAt(0.5).DistanceTo(originalCenter))
                .OrderBy(x => x.Key);

            BaseCurve coreBaseCurve = new BaseCurve();
            var lastGroup = groupedKeyValues.FirstOrDefault();
            if (lastGroup != null)
            {
                var firstItemInLastGroup = lastGroup.FirstOrDefault();
                coreBaseCurve = firstItemInLastGroup as BaseCurve;
            }
            return coreBaseCurve;
        }
        public static BaseCurve CurvesSplitAndGetCoreBaseCurve(Curve[] cutters, BaseCurve baseCurve)
        {
            BaseCurve tmpBaseCurve = baseCurve;
            for (int i = 0; i < cutters.Length; i++)
            {
                tmpBaseCurve = CurveSplitAndGetCoreBaseCurve(cutters[i], tmpBaseCurve);
            }
            return tmpBaseCurve;
        }
        public static List<BaseCurve> PolygonShapeGetInternalIntersectionLine(List<BaseCurve> baseCurves)
        {
            List<BaseCurve> coreBaseCurves = new List<BaseCurve>();
            for (int i = 0; i < baseCurves.Count; i++)
            {
                List<BaseCurve> tmpCutters = baseCurves.Select(x => x.Duplicate()).ToList();
                tmpCutters.RemoveAt(i);
                coreBaseCurves.Add(BaseCurveUtility.CurvesSplitAndGetCoreBaseCurve(tmpCutters.Select(x => x.curve).ToArray(), baseCurves[i]));
            }
            return coreBaseCurves;
        }

    }//定位基线工具
    public static class BaseFaceUtility
    {
        //将Surface转化为BaseSurface
        public static BaseFace SurfaceToBaseSurface(Surface surface)
        {
            //uv中心点uvIntersectPoint，中心点法向量uvIntersectionNormal
            AreaMassProperties amp = AreaMassProperties.Compute(surface);
            Point3d centeriod = amp.Centroid;
            double u = 0.0; double v = 0.0;
            surface.ClosestPoint(centeriod, out u, out v);
            BrepFace brepFace = surface.ToBrep().Faces[0];
            Surface untrimedSurface = brepFace.UnderlyingSurface();

            //找到更长的一条
            Curve uCurve = untrimedSurface.IsoCurve(0, u);
            Curve vCurve = untrimedSurface.IsoCurve(1, v);
            Curve longerCurve = default(Curve);
            if (uCurve.GetLength() > vCurve.GetLength()) longerCurve = uCurve;
            else longerCurve = vCurve;
            //Point3d centerPoint = longerCurve.PointAt(0.5);
            //double u0 = 0.0; double v0 = 0.0;
            //if (!surface.ClosestPoint(centerPoint, out u0, out v0)) return null;


            Vector3d normal = surface.NormalAt(u, v);

            Plane tmpPlane = new Plane(centeriod, normal);

            //保证向量一定在正方向
            Plane referencePlane = new Plane(centeriod, normal);
            if (normal * Vector3d.ZAxis < 0)
            {
                normal = -normal;
                referencePlane = new Plane(centeriod, normal);
            }

            Vector3d vv = new Vector3d(longerCurve.PointAtStart - longerCurve.PointAtEnd);
            double rotationAngle = Vector3d.VectorAngle(referencePlane.XAxis, vv);
            Transform rotationTransform = Transform.Rotation(-rotationAngle, referencePlane.ZAxis, referencePlane.Origin);
            referencePlane.Transform(rotationTransform);

            BaseFace baseFace = new BaseFace(surface.ToBrep(), referencePlane, normal);
            return baseFace;
        }
    }
    public static class TimeberBrepUtility
    {
        //将简单的Brep转化为标注辅助线
        public static AnnotationGuideCurves SimpleBrepToAnnotationGuideline(Brep brep, List<Plane> planes)
        {
            AnnotationGuideCurves annotationGuideline = new AnnotationGuideCurves();
            Curve[] curves;
            Point3d[] points;

            if (Rhino.Geometry.Intersect.Intersection.BrepPlane(brep, planes[0], Utility.DocumentTolerance(), out curves, out points))
            {
                annotationGuideline.topViewCurves = curves?.ToList();
            }

            if (Rhino.Geometry.Intersect.Intersection.BrepPlane(brep, planes[1], Utility.DocumentTolerance(), out curves, out points))
            {
                annotationGuideline.frontViewCurves = curves?.ToList();
            }

            if (Rhino.Geometry.Intersect.Intersection.BrepPlane(brep, planes[2], Utility.DocumentTolerance(), out curves, out points))
            {
                annotationGuideline.rightViewCurves = curves?.ToList();
            }

            return annotationGuideline;
        }
    }
}
