using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using IOComponents;
using Rhino.Geometry;
using UtilityElement;
using UtilityElement.ReferenceDatum;
using YhmAssembly;

namespace MyGrasshopperAssembly1
{
    public class 取竖段线_无斗拱亭子 : ZuiComponent
    {
        /// <summary>
        /// Initializes a new instance of the 取柱线 class.
        /// </summary>
        public 取竖段线_无斗拱亭子()
          : base("取竖段线_无斗拱亭子", "Nickname",
              "Description",
              "Yhm Toolbox", "形体设计")
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.last;
        private static readonly ParamDefinition[] inputs = new ParamDefinition[]
        {
            new ParamDefinition(new Param_GenericObject
            {
                Name = "面阔",
                NickName = "以外接圆半径记",
                Description = "",
                Access = GH_ParamAccess.item,
                Optional = true,
            }),
            new ParamDefinition(new Param_GenericObject
            {
                Name = "柱高",
                NickName = "",
                Description = "",
                Access = GH_ParamAccess.item,
                Optional = true,
            }),
            new ParamDefinition(new Param_GenericObject
            {
                Name = "多边形边数",
                NickName = "输入整数",
                Description = "",
                Access = GH_ParamAccess.item,
                Optional = true,
            }),
        };
        private static readonly ParamDefinition[] outputs = new ParamDefinition[]
        {
            new ParamDefinition(new Param_GenericObject
            {
                Name = "柱顶点",
                NickName = "",
                Description = "",
                Access = GH_ParamAccess.list,
                Optional = false
            }),
            new ParamDefinition(new Param_GenericObject
            {
                Name = "竖段线",
                NickName = "",
                Description = "",
                Access = GH_ParamAccess.list,
                Optional = false
            }),
            new ParamDefinition(new Param_GenericObject
            {
                Name = "柱底点",
                NickName = "",
                Description = "",
                Access = GH_ParamAccess.list,
                Optional = false
            }),
        };


        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            object miankuoObj = null; object ZGObj = null; object nObj = null;
            if (!DA.GetData(0, ref miankuoObj) || !DA.GetData(1, ref ZGObj) || !DA.GetData(2, ref nObj)) return;
            DA.GetData(0, ref miankuoObj);
            DA.GetData(1, ref ZGObj);
            DA.GetData(2, ref nObj);
            GH_Convert.ToDouble(miankuoObj, out double miankuo, GH_Conversion.Both);
            GH_Convert.ToDouble(ZGObj, out double ZG, GH_Conversion.Both);
            GH_Convert.ToInt32(nObj, out int n, GH_Conversion.Both);

            if (miankuo == 0 || ZG == 0 || n < 3)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "输入参数不正确！");
                return;
            }
            Polyline polyline = Polyline.CreateInscribedPolygon(new Circle(miankuo / 2), n);
            Point3d[] polylinePoint = polyline.ToArray();

            Polyline upPolyline = polyline.Duplicate();
            upPolyline.Transform(Transform.Translation(new Vector3d(0, 0, ZG)));
            Point3d[] upPointlinePoint = upPolyline.ToArray();

            List<Curve> curveList = polylinePoint.Select((x, i) => new Line(x, upPointlinePoint[i]).ToNurbsCurve() as Curve).ToList();
            List<BaseCurve> baseCurveList = BaseCurveUtility.CurvesToBaseCurves(curveList);

            //旋转baseCurveList以适应多边形
            baseCurveList = AdjuestStraightBaseCurvesToCenter(baseCurveList);

            upPolyline.RemoveAt(upPolyline.Count - 1);
            baseCurveList.RemoveAt(baseCurveList.Count - 1);
            polyline.RemoveAt(polyline.Count - 1);
            DA.SetDataList(0, upPolyline.ToList());//顶部点
            DA.SetDataList(1, baseCurveList);
            DA.SetDataList(2, polyline.ToList());//底部点
        }

        public static List<BaseCurve> AdjuestStraightBaseCurvesToCenter(List<BaseCurve> baseCurveList)
        {
            List<Point3d> startPoints = baseCurveList.Select(x => x.curve.PointAtStart).ToList();
            Point3d centriod = Point3dUtility.Point3dAverage(startPoints);
            List<Vector3d> vectorsToAlign = startPoints.Select(x => new Vector3d(centriod - x)).ToList();

            List<Vector3d> axises = baseCurveList.Select((x, i) => -Vector3d.CrossProduct(vectorsToAlign[i], x.extrudePlane.YAxis)).ToList();
            List<Transform> alignList = baseCurveList
                .Select((x, i) => Transform.Rotation(Vector3d.VectorAngle(vectorsToAlign[i], x.extrudePlane.YAxis), axises[i], x.extrudePlane.Origin))
                .ToList();
            for (int i = 0; i < baseCurveList.Count; i++)
            {
                baseCurveList[i].Transform(alignList[i]);
            }
            return baseCurveList;
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("A3499E7A-9D7F-4258-8110-9E0DC63B96B8"); }
        }

        protected override ParamDefinition[] Inputs => inputs;

        protected override ParamDefinition[] Outputs => outputs;
    }
}