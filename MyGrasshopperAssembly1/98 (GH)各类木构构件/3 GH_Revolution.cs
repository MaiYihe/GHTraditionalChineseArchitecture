using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using OutPutElement;
using Rhino.Geometry;
using UtilityElement.ReferenceDatum;

namespace MyGrasshopperAssembly1
{
    public class GH_Revolution : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public GH_Revolution()
          : base("GH_Revolution", "Nickname",
              "Description",
              "Yhm Toolbox", "几何生成_木构构件")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("定位轴", "DWZ", "...", GH_ParamAccess.list);
            pManager.AddTextParameter("名称", "NAME", "...", GH_ParamAccess.item);

            pManager.AddNumberParameter("偏移", "OFFSET", "...", GH_ParamAccess.item);

            pManager.AddNumberParameter("柱径D", "", "...", GH_ParamAccess.item);
            pManager.AddCurveParameter("旋转面", "RevolutionFace，在XY平面上", "...", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("图元信息", "TYXX", "...", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<BaseAxis> baseAxisList = new List<BaseAxis>();
            string name = "";
            double offset = 0.0;
            double zhuJing = 0.0;
            Curve curveToRevolute = default(Curve);
            if (!DA.GetDataList(0, baseAxisList) ||
                !DA.GetData(1, ref name) ||
                !DA.GetData(2, ref offset) ||
                !DA.GetData(3, ref zhuJing) ||
                !DA.GetData(4, ref curveToRevolute)) return;

            RevolvedTimber revolvedTimber = new RevolvedTimber(curveToRevolute, zhuJing, name);


            //偏移
            Transform upTransform = Transform.Translation(new Vector3d(0, offset, 0));
            GeometryInformation tmpGeometryInformation = new GeometryInformation(revolvedTimber.TimberBrep, null, Plane.WorldXY, "");
            GeometryInformation transformedGI = tmpGeometryInformation.Duplicate();
            transformedGI.Transform(upTransform);

            //返回
            List<Transform> backTransformList = baseAxisList
                .Select(x => Transform.PlaneToPlane(new Plane(Point3d.Origin, -Plane.WorldZX.YAxis, Plane.WorldZX.XAxis), x.referencePlane))
                .ToList();
            List<GeometryInformation> backGeometryInformationList = new List<GeometryInformation>();
            for (int i = 0; i < backTransformList.Count; i++)
            {
                GeometryInformation tmpBackGI = transformedGI.Duplicate();
                tmpBackGI.Transform(backTransformList[i]);
                backGeometryInformationList.Add(tmpBackGI);
            }

            DA.SetDataList(0, backGeometryInformationList);
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
            get { return new Guid("0AB55D1D-B1E6-4492-AD26-4DCA974655B4"); }
        }
    }
}