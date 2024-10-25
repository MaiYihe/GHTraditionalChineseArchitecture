using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;
using UtilityElement.Metadeta.Dimensions;

namespace MyGrasshopperAssembly1
{
    public class 降垫板高 : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the 降垫板高 class.
        /// </summary>
        public 降垫板高()
          : base("降垫板高", "Nickname",
              "Description",
              "Yhm Toolbox", "形体设计")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("柱径D", "D", "", GH_ParamAccess.item);
            pManager.AddPointParameter("柱顶点", "ZDD", "", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("降高柱顶点", "JGZDD", "", GH_ParamAccess.list);
            //pManager.AddNumberParameter("-","","", GH_ParamAccess.item);
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Double D = 0.0;
            List<Point3d> ZDD = new List<Point3d>();
            List<Point3d> newZDD = new List<Point3d>();
            List<SquareProfile> DimensionList = new List<SquareProfile>();
            DA.GetData(0, ref D);
            DA.GetDataList(1, ZDD);

            Point3d transformedPoint;
            if (ZDD.Count % 2 != 1) this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "柱顶点非奇数！");
            if (ZDD.Count < 3)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "柱顶点过少！");
                return;
            }
            for (int i = 0; i < ZDD.Count; i++)
            {
                transformedPoint = ZDD[i];
                if (i == 0 || i == ZDD.Count - 1)
                {                    
                    transformedPoint.Transform(Transform.Translation(new Vector3d(0, 0, -0.8 * D)));
                }
                //DimensionList.Add(new SquareProfile(0, 0, 0.8 * D, 0.25 * D));
                else 
                {
                    transformedPoint.Transform(Transform.Translation(new Vector3d(0, 0, -0.65 * D)));
                }
                //DimensionList.Add(new SquareProfile(0, 0, 0.65 * D, 0.25 * D));
                newZDD.Add(transformedPoint);
            }
            DA.SetDataList(0, newZDD);
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
            get { return new Guid("81267FD0-4291-4B82-8BA8-D6DD0D8F4ACD"); }
        }
    }
}