using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using YhmAssembly;
using UtilityElement.ReferenceDatum;
using UtilityElement;

namespace MyGrasshopperAssembly1
{
    public class 竖直贯通_无斗栱屋架 : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the 竖直贯通 class.
        /// </summary>
        public 竖直贯通_无斗栱屋架()
          : base("竖直贯通_无斗栱屋架", "Nickname",
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
            pManager.AddIntegerParameter("檩索引", "LSY", "檩对称", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("竖直贯通线", "ZZGTX", "", GH_ParamAccess.list);
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
            DA.GetDataList(0, columnVertex);
            DA.GetData(1, ref index);
            //输出的数据
            List<Curve> curves = new List<Curve>();
            List<BaseCurve> baseCurves = new List<BaseCurve>();
            if (index < 0 || index > columnVertex.Count / 2)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "索引超出范围！");
                return;
            }

            NegIndexList<Point3d> columnVertexNeg = new NegIndexList<Point3d>(columnVertex);
            Transform xform = Transform.ProjectAlong(Plane.WorldXY, -Plane.WorldXY.ZAxis);
            Point3d p0 = new Point3d((Point3d)columnVertexNeg[-index]);
            p0.Transform(xform);
            Point3d p1 = new Point3d((Point3d)columnVertexNeg[index]);
            p1.Transform(xform);
            Curve curve0 = new Line((Point3d)columnVertexNeg[-index], p0).ToNurbsCurve();
            Curve curve1 = new Line((Point3d)columnVertexNeg[index], p1).ToNurbsCurve();
            if (index == 0)
            {
                curves.Add(curve0);
            }//index=0时输出中间一项
            else if (index > 0)
            {
                curves.Add(curve0);
                curves.Add(curve1);
            }//否则输出两项
            baseCurves = BaseCurveUtility.CurvesToBaseCurves(curves);
            //输出
            DA.SetDataList(0, baseCurves);
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
            get { return new Guid("D7D11ED6-BBA3-4625-993B-065EE0EB6A51"); }
        }
    }
}