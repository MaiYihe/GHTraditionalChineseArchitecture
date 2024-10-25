using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using UtilityElement.Metadeta.Dimensions;

namespace MyGrasshopperAssembly1
{
    public class 枋高宽权衡 : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the 柱径权衡 class.
        /// </summary>
        public 枋高宽权衡()
          : base("枋高宽权衡_屋架无斗栱", "Nickname",
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
            pManager.AddGenericParameter("上金、脊枋尺寸", "", "...", GH_ParamAccess.item);
            pManager.AddGenericParameter("金枋尺寸", "", "...", GH_ParamAccess.item);
            pManager.AddGenericParameter("檐枋尺寸", "", "...", GH_ParamAccess.item);
            pManager.AddGenericParameter("穿插枋尺寸", "", "...", GH_ParamAccess.item);                      
            pManager.AddGenericParameter("燕尾枋尺寸_配套檐垫板", "", "...", GH_ParamAccess.item);
            pManager.AddGenericParameter("燕尾枋尺寸_配套、脊垫板", "", "...", GH_ParamAccess.item);
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Double D = 0.0;
            SquareProfile shangJinJiFangDimension, jinFangDimension, yanFangDimension,chuanChaFangDimension, yanWeiFangDimension0, yanWeiFangDimension1;
            DA.GetData(0, ref D);

            shangJinJiFangDimension = new SquareProfile(0, 0, 0.8 * D, 0.65 * D);
            jinFangDimension = new SquareProfile(0, 0, D, 0.8 * D);
            yanFangDimension = new SquareProfile(0, 0, D, 0.8 * D);
            chuanChaFangDimension = new SquareProfile(0, 0, D, 0.8 * D);            
            yanWeiFangDimension0 = new SquareProfile(0, 0, 0.8 * D, 0.25 * D);//燕尾枋尺寸_配套檐垫板
            yanWeiFangDimension1 = new SquareProfile(0, 0, 0.65 * D, 0.25 * D);//燕尾枋尺寸_配套金、脊垫板

            DA.SetData(0, shangJinJiFangDimension);
            DA.SetData(1, jinFangDimension);
            DA.SetData(2, yanFangDimension);
            DA.SetData(3, chuanChaFangDimension);
            DA.SetData(4, yanWeiFangDimension0);
            DA.SetData(5, yanWeiFangDimension1);
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
            get { return new Guid("ED2A407E-C088-4195-977F-F83186DCC4C4"); }
        }
    }
}