using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using UtilityElement.Metadeta.Dimensions;

namespace MyGrasshopperAssembly1
{
    public class 梁高宽权衡_卷棚 : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the 柱径权衡 class.
        /// </板高宽权衡>
        public 梁高宽权衡_卷棚()
          : base("梁高宽权衡_卷棚_屋架无斗栱", "Nickname",
              "Description",
              "Yhm Toolbox", "构件权衡")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("柱径D", "D，默认单位为m", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("四架梁尺寸", "", "...", GH_ParamAccess.item);
            pManager.AddGenericParameter("六架梁尺寸", "", "...", GH_ParamAccess.item);
            pManager.AddGenericParameter("脊瓜柱尺寸", "", "...", GH_ParamAccess.item);
            pManager.AddGenericParameter("四架梁下金瓜柱尺寸", "", "...", GH_ParamAccess.item);
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Double D = 0.0;
            SquareProfile siJiaLiangDimension, liuJiaLiangDimension;
            DA.GetData(0, ref D);
            siJiaLiangDimension = new SquareProfile(0, 0, 1.4 * D, 1.1 * D);
            liuJiaLiangDimension = new SquareProfile(0, 0, 1.5 * D, 1.2 * D);
            DA.SetData(0, siJiaLiangDimension);
            DA.SetData(1, liuJiaLiangDimension);
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
            get { return new Guid("ED2A407E-C088-4195-977F-F83186DCC6D8"); }
        }
    }
}