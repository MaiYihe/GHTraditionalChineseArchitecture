using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using UtilityElement.Metadeta.Dimensions;

namespace MyGrasshopperAssembly1
{
    public class 梁高宽权衡_无斗拱亭子 : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the 梁高宽权衡_小式亭子 class.
        /// </summary>
        public 梁高宽权衡_无斗拱亭子()
          : base("梁高宽权衡_无斗拱亭子", "Nickname",
              "Description",
              "Yhm Toolbox", "构件权衡")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.last;
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("檐柱径D", "", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("长短扒梁半高差", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("长扒梁", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("短扒梁", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("多边形扒梁", "", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double D = 0;            
            if (!DA.GetData(0, ref D)) return;

            SquareProfile changPaLiang = new SquareProfile(0, 0, 1.3 * D, 1.05 * D);
            SquareProfile duanPaLiang = new SquareProfile(0, 0, 1.05 * D, 0.9 * D);
            SquareProfile duoBianXingPaLiang = new SquareProfile(0, 0, 1.4 * D, D);
            double halfHeight = (1.3 * D - 1.05 * D) / 2;

            DA.SetData(0, halfHeight);
            DA.SetData(1, changPaLiang);
            DA.SetData(2, duanPaLiang);
            DA.SetData(3, duoBianXingPaLiang);
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
            get { return new Guid("4C58D8B5-C287-41B5-AC88-374C18C47DB1"); }
        }
    }
}