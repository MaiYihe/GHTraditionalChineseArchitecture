using Rhino.Geometry.Collections;
using Rhino.Geometry.Intersect;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Rhino.Geometry.BrepFace;
using Rhino.DocObjects;
using static YhmAssembly.BrepUtility;
using static YhmAssembly.listEdit;
using static YhmAssembly.CurveUtility;
using System.Runtime.CompilerServices;
using Grasshopper.Kernel;
using Rhino;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Runtime.InProcess;
using Rhino.Commands;
using Rhino.Display;
using Grasshopper;
using Grasshopper.Getters;
using System.Security.Cryptography;

namespace YhmAssembly//我的工具箱，仅对缺省类进行操作!
{
    public static class BrepUtility
    {
        public static Brep GetMiniBox(Brep Brep, Plane Plane)
        {
            Box myWorldBox;
            Brep.GetBoundingBox(Plane, out myWorldBox);
            return myWorldBox.ToBrep();
        }
        public static List<Surface> GetBrepFaces(Brep brep)
        {
            //得到brep的面，获取面的枚举器
            BrepFaceList facesArray = brep.Faces;
            IEnumerator<BrepFace> enumerator = facesArray.GetEnumerator();

            //创建Surface的列表
            List<Surface> facesList = new List<Surface>();

            //使用枚举器遍历，得到面集faceList
            while (enumerator.MoveNext())
            {
                facesList.Add(enumerator.Current.ToNurbsSurface());
                AreaMassProperties amp = AreaMassProperties.Compute(enumerator.Current);
            }


            return facesList;
        }//得到Brep上的各个Surfaces
        public static Point3d GetPlanarSurfacesCentroid(Surface surface)
        {
            Point3d p = new Point3d();
            AreaMassProperties amp = AreaMassProperties.Compute(surface);
            p = amp.Centroid;
            return p;
        }//得到surfaceList的average中心
        public static Point3d GetSurfacesUVCentroid(Brep surface)
        {
            BrepFace S1 = surface.Faces[0];
            ShrinkDisableSide disableSide = ShrinkDisableSide.ShrinkAllSides;
            S1.ShrinkFace(disableSide);
            Surface S2 = S1 as Surface;

            //Reparameterized Surface
            Interval val = new Interval(0.0, 1.0);
            S2.SetDomain(0, val);
            S2.SetDomain(1, val);
            return S2.PointAt(0.5, 0.5);
        }//得到surface的uv中心
        public static void FindHighestAndLowestPoints(List<Point3d> points, ref double highestZIndex, ref double lowestZIndex)
        {
            double highestZ = double.NegativeInfinity;
            double lowestZ = double.PositiveInfinity;
            for (int i = 0; i < points.Count; i++)
            {
                if (points[i].Z > highestZ)
                {
                    highestZ = points[i].Z;
                    highestZIndex = i;
                }
                if (points[i].Z < lowestZ)
                {
                    lowestZ = points[i].Z;
                    lowestZIndex = i;
                }

            }
        }
        public static GeometryBase ProjectGeometryToPlane(GeometryBase geometry, Plane plane, Vector3d direction)
        {
            Transform projection = Transform.ProjectAlong(plane, direction);
            GeometryBase projectedGeometry = geometry.Duplicate(); // 假设传入的geometry是Cloneable的
            projectedGeometry.Transform(projection);
            return projectedGeometry;
        }//将几何体投影到指定平面上
        private static Brep Loft(List<Curve> x)
        {
            var breps = Brep.CreateFromLoft(x, Point3d.Unset, Point3d.Unset, LoftType.Normal, false);

            return breps[0];
        }//通过loft得到cutters
        public static Brep projectToBrepCutter(Curve curve, Plane highestPlane, Plane lowestPlane)
        {
            Vector3d zAxis = new Vector3d(0, 0, 1);//Z轴的方向
            Brep brepCutter = new Brep();
            Curve highCurve = (Curve)ProjectGeometryToPlane(curve, highestPlane, zAxis);
            Curve lowCurve = (Curve)ProjectGeometryToPlane(curve, lowestPlane, zAxis);

            List<Curve> loftCurve = new List<Curve>();
            loftCurve.AddRange(new Curve[] { highCurve, lowCurve });
            Brep loftedBrep = Loft(loftCurve);

            brepCutter = loftedBrep;
            return loftedBrep;
        }
        public static List<Brep> GetCutBreps(Brep brep, Brep cutter)
        {
            List<Brep> splitedBreps = brep.Split(cutter, 0.01).ToList();

            return splitedBreps;
        }
        public static List<Curve> GetIntersectCurves(Brep brep1, Brep brep2)
        {
            Point3d[] intersectPoints;
            Curve[] intersectCurves;
            Intersection.BrepBrep(brep1, brep2, 0.01, out intersectCurves, out intersectPoints);//得到交线
            Curve[] joinedCurves = Curve.JoinCurves(intersectCurves, 0.01);//对交线进行Join，得到joinedCurves

            return joinedCurves.ToList();
        }
        public static List<Brep> SurfaceSplitByCurves(Surface surface, List<Curve> curves)
        {
            Brep val0 = Brep.CreateFromSurface(surface).Faces[0].Split(curves, 0.01);
            List<Brep> splitedSurfaces = new List<Brep>();
            for (int i = 0; i < val0.Faces.Count; i++)
            {
                splitedSurfaces.Add(val0.Faces[i].DuplicateFace(false));
            }
            return splitedSurfaces;
        }
        public static List<Brep> GetInsideSurfaces(Brep inBrep, List<Brep> splitedCutters)
        {
            List<Brep> insideSurfaces = new List<Brep>();
            //int i = 0;
            foreach (Brep splitedCutter in splitedCutters)
            {
                //i++;
                //Surface surface = splitedCutter.Faces[0];

                Point3d C1 = GetSurfacesUVCentroid(splitedCutter);
                Point3d C2 = splitedCutter.ClosestPoint(C1);
                if (inBrep.IsPointInside(C2, 0.01, true))//即保证点在Brep内
                {
                    insideSurfaces.Add(splitedCutter);
                }

                //AreaMassProperties amp = AreaMassProperties.Compute(surface);
                //this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, amp.Area.ToString());
                //this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, C2.ToString()+"CCCC"+ i.ToString());
            }

            return insideSurfaces;
        }
        public static void GetSolids(List<Brep> splitedBreps, List<Brep> surfacesToJoin, out List<Brep> solids)
        {
            solids = new List<Brep>();
            List<Brep> emptyBreps = splitedBreps;//临时可存空Brep的列表
            List<Brep> tmpBreps = new List<Brep>();//临时合并用
            Brep[] joinedBreps = new Brep[0];//存储合并的结果
            foreach (Brep surfaceToJoin in surfacesToJoin)
            {
                for (int i = 0; i < emptyBreps.Count; i++)
                {
                    tmpBreps.Add(emptyBreps[i]);
                    tmpBreps.Add(surfaceToJoin);
                    joinedBreps = Brep.JoinBreps((IEnumerable<Brep>)tmpBreps, 0.01);
                    if (joinedBreps.Count() < 2 && joinedBreps[0].IsSolid)//能join且join完是实体
                    {
                        solids.AddRange(joinedBreps);
                        emptyBreps[i] = null;
                    }
                    else if (joinedBreps.Count() < 2 && !joinedBreps[0].IsSolid)//能join但是join完以后非实体
                    {
                        emptyBreps[i] = joinedBreps[0];
                    }
                    tmpBreps.Clear();
                }
            }

        }//切出Solids
        public static Point3d GetSolidCentriod(Brep solid)
        {
            VolumeMassProperties vmp = VolumeMassProperties.Compute(solid);
            return vmp.Centroid;
        }
        public static void SortFacesOfBox(Brep brep, out Brep east, out Brep west, out Brep south, out Brep north, out Brep up, out Brep down)
        {
            BrepFaceList val0 = brep.Faces;
            List<BrepFace> valX = val0.ToList();//制作val0的拷贝
            List<BrepFace> valY = val0.ToList();//制作val0的拷贝
            List<BrepFace> valZ = val0.ToList();//制作val0的拷贝

            List<Point3d> centriods = new List<Point3d>();
            List<Tuple<Double, Double, Double>> XYZ = new List<Tuple<Double, Double, Double>>();
            for (int i = 0; i < val0.Count; i++)
            {
                AreaMassProperties amp = AreaMassProperties.Compute(val0[i]);
                centriods.Add(amp.Centroid);
                XYZ.Add(Tuple.Create(amp.Centroid.X, amp.Centroid.Y, amp.Centroid.Z));
            }
            List<Double> X = XYZ.Select(tuple => tuple.Item1).ToList();
            List<Double> Y = XYZ.Select(tuple => tuple.Item2).ToList();
            List<Double> Z = XYZ.Select(tuple => tuple.Item3).ToList();

            for (int i = 0; i < 3; i++)
            {
                valX = listEdit.Sort(X, val0.ToArray());
                valY = listEdit.Sort(Y, val0.ToArray());
                valZ = listEdit.Sort(Z, val0.ToArray());
            }

            //取出首尾项
            east = valX.LastOrDefault().DuplicateFace(false);
            west = valX.FirstOrDefault().DuplicateFace(false);
            south = valY.FirstOrDefault().DuplicateFace(false);
            north = valY.LastOrDefault().DuplicateFace(false);
            up = valZ.LastOrDefault().DuplicateFace(false);
            down = valZ.FirstOrDefault().DuplicateFace(false);
        }
        public static List<Brep> SolidIntersection(List<Brep> b1, List<Brep> b2)
        {
            List<Brep> result = new List<Brep>();
            Brep[] tmpBrep = new Brep[0];
            foreach (Brep bb1 in b1)
            {
                foreach (Brep bb2 in b2)
                {
                    tmpBrep = Brep.CreateBooleanIntersection(bb1, bb2, 0.01);
                    if (tmpBrep != null)
                    {
                        result.AddRange(tmpBrep);
                    }
                }
            }
            return result;
        }
        public static Curve GetBrepCircleSection(Brep brep)
        {
            List<Curve> wireFrame = brep.GetWireframe(-1).ToList();
            ArcCurve circleCurve = new ArcCurve();
            foreach (Curve curve in wireFrame)
            {
                if (curve.IsCircle())
                {
                    circleCurve = curve as ArcCurve;
                    break;
                }
            }
            return circleCurve;
        }//返回Brep的圆形面
    }
    public static class BrepUtility01
    {
        public static void SolidCutWithCurveALLRound(Brep originalBrep, Plane? plane, List<Curve> cutters, out List<Brep> coreBrep, out List<Brep> sideBreps, out List<Brep> brepCutters, out List<Brep> splitedBreps, out List<Curve> intersectCurves, out List<Brep> splitedCutters, out List<Brep> surfacesToJoin, out List<Brep> solids)
        {
            //得到Brep的最小包围盒miniBox
            Brep miniBox = GetMiniBox(originalBrep, (Plane)plane);
            //得到miniBox各个面
            List<Surface> facesList = GetBrepFaces(miniBox);
            //找到Surfaces的中心点
            List<Point3d> p = new List<Point3d>();
            foreach (Surface face in facesList)
            {
                Point3d p0 = GetPlanarSurfacesCentroid(face);
                p.Add(p0);
            }
            //寻找最高点与最低点，返回索引值
            double highestZIndex = -1;
            double lowestZIndex = -1;
            FindHighestAndLowestPoints(p, ref highestZIndex, ref lowestZIndex);
            //取出最高面和最低面上的Plane
            Surface highestSurface = facesList[Convert.ToInt32(highestZIndex)];
            Surface lowestSurface = facesList[Convert.ToInt32(lowestZIndex)];
            Plane highestPlane;
            Plane lowestPlane;
            highestSurface.TryGetPlane(out highestPlane, 0.01);
            lowestSurface.TryGetPlane(out lowestPlane, 0.01);

            Brep brep1 = originalBrep;
            coreBrep = new List<Brep> { originalBrep };
            sideBreps = new List<Brep>();
            Point3d corePoint = plane.Value.Origin;

            brepCutters = new List<Brep>();
            splitedBreps = new List<Brep>();
            intersectCurves = new List<Curve>();
            splitedCutters = new List<Brep>();
            surfacesToJoin = new List<Brep>();
            solids = new List<Brep>();
            List<Brep> sortedBrep = new List<Brep>();//按照距离中心点的距离排序
            List<Point3d> solidsCentriod = new List<Point3d>();
            List<Double> distance = new List<Double>();

            foreach (Curve cutter in cutters)
            {
                Brep brepCutter = projectToBrepCutter(cutter, highestPlane, lowestPlane);
                brepCutters.Add(brepCutter);//将Curve状态的Cutters转化为brepCutters；

                splitedBreps = GetCutBreps(brep1, brepCutter);
                intersectCurves = GetIntersectCurves(brep1, brepCutter);
                splitedCutters = SurfaceSplitByCurves(brepCutter.Faces[0].ToNurbsSurface(), intersectCurves);
                surfacesToJoin = GetInsideSurfaces(brep1, splitedCutters);
                //surfacesToJoin = Brep.CreatePlanarBreps(Curve.JoinCurves(intersectCurves), 0.01).ToList();
                GetSolids(splitedBreps, surfacesToJoin, out solids);
                foreach (Brep solid in solids)
                {
                    solidsCentriod.Add(GetSolidCentriod(solid));
                    distance.Add(GetSolidCentriod(solid).DistanceTo(corePoint));
                }
                sortedBrep = Sort(distance, solids.ToArray());
                DivideListWithIndex(sortedBrep, 0, out coreBrep, out sideBreps);

            }
        }
    }
    public static class CurveUtility
    {
        public static List<Plane> GetPlanes(Curve curve, double parameter)
        {
            Interval newDomain = new Interval(0, 1);
            curve.Domain = newDomain;
            List<Plane> planes = new List<Plane>();
            Point3d p = curve.PointAt(parameter);

            Plane frame = new Plane();
            Vector3d Y = curve.TangentAt(parameter);//切线向量
            curve.PerpendicularFrameAt(parameter, out frame);
            Vector3d X = frame.YAxis;//法线向量
            X.Unitize(); 
            Y.Unitize();
            Vector3d Z = Vector3d.CrossProduct(X, Y);
            planes.Add(new Plane(p, Y, Z));//定位平面
            planes.Add(new Plane(p, X, Y));
            planes.Add(new Plane(p, Z, X));//挤出平面

            return planes;
        }//得到线上的弗雷特坐标平面
        public static void ExplodeCurve(Curve curve, out List<Curve> explodedCurves, out List<Point3d> explodedPoints)
        {
            var curves = new List<Curve>();
            var points = new List<Point3d>();
            var t1 = curve.Domain.T1;
            var lastT = 0.0;
            var t = 0.0;
            while (curve.GetNextDiscontinuity(Continuity.Gsmooth_continuous, lastT, t1, out t))
            {
                var subCurve = curve.Trim(lastT, t);
                if (subCurve != null)
                {
                    curves.Add(subCurve);
                }
                points.Add(curve.PointAt(t));
                lastT = t;
            }
            if (lastT != t1)
            {
                var subCurve = curve.Trim(lastT, t1);
                if (subCurve != null)
                {
                    curves.Add(subCurve);
                }
            }
            explodedPoints = points;
            explodedCurves = curves;
        }//ExplodeCurve 爆炸线，得到不连续的各段以及角点
        public static Curve MoveCurveToPoint(Curve curveToMove, Point3d location)
        {
            Curve tmpCurve = curveToMove.DuplicateCurve();
            double t = 0.0;
            if (!tmpCurve.ClosestPoint(location, out t)) return null;
            Point3d pointAtCurve = tmpCurve.PointAt(t);
            tmpCurve.Transform(Transform.Translation(new Vector3d(location - pointAtCurve)));
            return tmpCurve;
        }//将Curve移动到point的位置
        public static Curve PlaneCutLineAndGetCoreCurve(Curve curve, params Plane[] cutters)
        {
            curve.Domain = new Interval(0, 1);
            Curve tmpCurve = curve.DuplicateCurve();
            for (int i = 0; i < cutters.Count(); i++)
            {
                Plane cutter = cutters[i];
                CurveIntersections intersections = Intersection.CurvePlane(tmpCurve, cutter, Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
                if (intersections.Count() < 1) return null;
                Point3d p = intersections[0].PointA;
                double t = 0.0;
                if (!tmpCurve.ClosestPoint(p, out t)) return null;
                Curve[] CurveArray = tmpCurve.Split(t);
                foreach (Curve item in CurveArray)
                {
                    item.Domain = new Interval(0, 1);
                }

                tmpCurve = CurveArray
                    .OrderBy(x => x.PointAt(0.5).DistanceTo(curve.PointAt(0.5)))
                    .First();
            }
            return tmpCurve;
        }//Plane剪切得到CoreCurve
        public static Curve PlaneCutLineAndGetCoreCurve(Curve curve, Point3d corePoint, params Plane[] cutters)
        {
            Curve tmpCurve = curve.DuplicateCurve();
            Curve[] CurveArray = new Curve[] { };
            for (int i = 0; i < cutters.Count(); i++)
            {
                Plane cutter = cutters[i];
                CurveIntersections intersections = Intersection.CurvePlane(tmpCurve, cutter, Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
                if (intersections.Count() > 1)
                {
                    return null;
                }
                Point3d p = intersections[0].PointA;
                double t = 0.0;
                if (!tmpCurve.ClosestPoint(p, out t))
                {
                    return null;
                }
                CurveArray = tmpCurve.Split(t);
                List<double> disList = new List<double>();
                for (int j = 0; j < CurveArray.Count(); j++)
                {
                    double tt = 0.0;
                    CurveArray[j].ClosestPoint(corePoint, out tt);

                    disList.Add(corePoint.DistanceTo(CurveArray[i].PointAt(tt)));
                }

                foreach (Curve item in CurveArray)
                {
                    item.Domain = new Interval(0, 1);
                }

                tmpCurve = disList.Zip(CurveArray.ToList(), (dis, curve) => new { Dis = dis, C = curve })
                    .OrderBy(p => p.Dis)
                    .Select(p => p.C)
                    .First();
            }

            return tmpCurve;
        }//Plane剪切得到CoreCurve,指定CorePoint
    }
    public static class PlaneUtility
    {
        public static List<Plane> PlaneToPlanes(Plane plane)
        {
            List<Plane> planes = new List<Plane>();
            Point3d p0 = plane.Origin;
            Line line = new Line(p0, plane.ZAxis);
            return GetPlanes(line.ToNurbsCurve(), 0);
        }//由单个平面得到该点其余平面
        public static Point3d subWorldPlaneForPoint3d(Plane subPlane, Point3d originalPoint)
        {
            Transform xform = Transform.PlaneToPlane(subPlane, Plane.WorldXY);
            Point3d transformedPoint3d = new Point3d(originalPoint);
            transformedPoint3d.Transform(xform);
            return transformedPoint3d;
        }
        public static List<Point3d> subWorldPlaneForPoint3ds(Plane subPlane, List<Point3d> originalPoints)
        {
            List<Point3d> subPoints = new List<Point3d>();
            for (int i = 0; i < originalPoints.Count; i++)
            {
                Point3d subPoint = subWorldPlaneForPoint3d(subPlane, originalPoints[i]);
                subPoints.Add(subPoint);
            }
            return subPoints;
        }
    }
    public static class GeometryUtility
    {
        public static Object TransformGeometry(Object geometry, Transform xform)
        {
            if (geometry is GeometryBase geometryBase && geometryBase.IsValid)
            {
                geometryBase.Transform(xform);
                return geometryBase;
            }
            else if (geometry is Plane plane && plane.IsValid)
            {
                plane.Transform(xform);
                return plane;
            }
            else if (geometry is Vector3d vector && vector.IsValid)
            {
                vector.Transform(xform);
                return vector;
            }
            return null;
        }//位移

    }
    public static class Point3dUtility
    {
        public static Point3d Point3dAverage(List<Point3d> point3Ds)
        {
            Point3d centerPoint = new Point3d();
            if (point3Ds.Any())
            {
                centerPoint = new Point3d(
                    point3Ds.Sum(p => p.X) / point3Ds.Count,
                    point3Ds.Sum(p => p.Y) / point3Ds.Count,
                    point3Ds.Sum(p => p.Z) / point3Ds.Count
                );
            }//当集合中至少有一个元素时，.Any() 返回 true，否则返回 false
            return centerPoint;
        }
    }
}
