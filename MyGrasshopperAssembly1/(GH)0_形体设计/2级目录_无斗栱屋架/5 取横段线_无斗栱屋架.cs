using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using UtilityElement;
using UtilityElement.ReferenceDatum;
using YhmAssembly;

namespace MyGrasshopperAssembly1
{
    public class 取横段线_无斗栱屋架 : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the 取段线 class.
        /// </summary>
        public 取横段线_无斗栱屋架()
          : base("取横段线_无斗栱屋架", "Nickname",
              "Description",
              "Yhm Toolbox", "形体设计")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("柱顶点", "ZDD", "到梁架下", GH_ParamAccess.list);
            pManager.AddIntegerParameter("段索引", "DSY", "檩对称", GH_ParamAccess.item);
            pManager.AddBooleanParameter("是否对称", "SFDC", "true则取对称。默认对称", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("横段线", "HDX", "", GH_ParamAccess.list);
        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;//暴露在二级标题下

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //输入
            List<Point3d> columnVertex = new List<Point3d>();//柱顶点
            int index = 0;
            bool isMirror = true;
            DA.GetDataList(0, columnVertex);
            DA.GetData(1, ref index);
            DA.GetData(2, ref isMirror);
            //输出的数据
            List<Curve> curves = new List<Curve>();
            List<BaseCurve> baseCurves = new List<BaseCurve>();

            NegIndexList<Point3d> columnVertexNeg = new NegIndexList<Point3d>(columnVertex);
            Curve curve0 = null;
            Curve curve1 = null;

            if (index == 0 || Math.Abs(index) > columnVertex.Count / 2)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "索引超出范围！");
                return;
            }
            curve0 = GetHorizontalDCurve(columnVertexNeg, index);
            curves.Add(curve0);
            if (isMirror)
            {
                curve1 = GetHorizontalDCurve(columnVertexNeg, -index);
                curves.Add(curve1);
            }
            baseCurves = BaseCurveUtility.CurvesToBaseCurves(curves);


            //输出
            DA.SetDataList(0, baseCurves);
        }

        private Curve GetHorizontalDCurve(NegIndexList<Point3d> columnVertexNeg, int index)
        {
            Curve curve = null;
            Curve XYplaneLine = null;
            if (index != 0)
            {
                Point3d startPoint, endPoint;
                if (index > 0)
                {
                    startPoint = columnVertexNeg[index];
                    endPoint = columnVertexNeg[index - 1];
                }
                else
                {
                    startPoint = columnVertexNeg[index];
                    endPoint = columnVertexNeg[index + 1];
                }

                curve = new Line(startPoint, endPoint).ToNurbsCurve();
                curve.Reverse();
                Plane tmpP = new Plane(startPoint, Plane.WorldXY.XAxis, Plane.WorldXY.YAxis);
                XYplaneLine = Curve.ProjectToPlane(curve, tmpP);
            }
            return XYplaneLine;
        }//得到段线几何线

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
            get { return new Guid("E5DF24B0-5927-42B6-B235-37CF1E098D5B"); }
        }
    }
}