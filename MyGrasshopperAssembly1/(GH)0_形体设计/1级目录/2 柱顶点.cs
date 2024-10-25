using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;
using YhmAssembly;

namespace MyGrasshopperAssembly1
{
    public class 柱顶点 : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public 柱顶点()
          : base("柱顶点", "Nickname",
              "Description",
              "Yhm Toolbox", "形体设计")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("步架","BJ","...", GH_ParamAccess.item);
            pManager.AddNumberParameter("柱总高","ZZG","到檩下",GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("柱顶点","ZDD", "到梁架下", GH_ParamAccess.list);
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double BJ = new double();
            List<double> ZZG = new List<double>();
            DA.GetData(0,ref BJ);//步架
            DA.GetDataList(1, ZZG);//柱总高

            ZZG.Reverse();//倒转列表
            NegIndexList<Tuple<Double, Double, Double>> XYZ = new NegIndexList<Tuple<Double, Double, Double>>();

            for (int i = 0;i < ZZG.Count; i++)
            {
                if(i == 0)
                {
                   XYZ.PositiveAdd(Tuple.Create(0.0, 0.0, ZZG[i]));
                }
                else
                {
                    XYZ.PositiveAdd(Tuple.Create(i * BJ, 0.0, ZZG[i]));
                    XYZ.NegtiveAdd(Tuple.Create(-i * BJ, 0.0, ZZG[i]));
                }
            }
            for (int i =0;i<XYZ.Count;i++)
            {

            }
            int watershed = 0;
            DA.SetDataList(0,XYZ.ToNormal(out watershed).Select(tuple => new Point3d(tuple.Item1, tuple.Item2, tuple.Item3)));
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
            get { return new Guid("81AB1B3A-5A8A-49CE-A555-3C3BE7FC0147"); }
        }
    }
}