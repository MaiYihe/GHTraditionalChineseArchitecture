using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using OutPutElement;
using Rhino.Geometry;
using TimberElement;
using UtilityElement.ReferenceDatum;

namespace MyGrasshopperAssembly1
{
    public class GH_Face : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public GH_Face()
          : base("GH_Face", "Nickname",
              "Description",
              "Yhm Toolbox", "几何生成_木构构件")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("定位基面", "DWJM", "...", GH_ParamAccess.list);

            pManager.AddTextParameter("名称", "NAME", "...", GH_ParamAccess.item);
            pManager.AddTextParameter("上浮/中心/下沉", "", "...", GH_ParamAccess.item);
            pManager.AddNumberParameter("偏移", "OFFSET", "...", GH_ParamAccess.item);

            pManager.AddNumberParameter("厚度", "houDu", "...", GH_ParamAccess.item);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("图元信息", "TYXX", "...", GH_ParamAccess.list);
            pManager.AddGenericParameter("顶部面", "DWJM", "...", GH_ParamAccess.list);
            pManager.AddGenericParameter("中部面", "DWJM", "...", GH_ParamAccess.list);
            pManager.AddGenericParameter("底部面", "DWJM", "...", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<BaseFace> baseFaces = new List<BaseFace>();
            string name = "";
            string state = "";
            double offset = 0.0;
            double houDu = 0.0;
            if (!DA.GetDataList(0, baseFaces) ||
                !DA.GetData(1, ref name) ||
                !DA.GetData(2, ref state) ||
                !DA.GetData(3, ref offset) ||
                !DA.GetData(4, ref houDu)) return;

            if (houDu <= 0)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "厚度不正确！");
                return;
            }

            BaseFace orientedFace = baseFaces[0].Duplicate();
            Transform xform = Transform.PlaneToPlane(baseFaces[0].referencePlane, Plane.WorldXY);
            orientedFace.Transform(xform);
            FaceTimber faceTimber = new FaceTimber(orientedFace, houDu, name);
            GeometryInformation geometryInformation = new GeometryInformation(faceTimber.TimberBrep
                , null
                , faceTimber.baseFace.referencePlane
                , faceTimber.description);
            //上浮/中心/下沉
            Vector3d v = faceTimber.baseFace.positiveDirection;//位移矢量
            double dis = faceTimber.width / 2;
            if (state != null)
            {
                if (state == "上浮")
                {
                    v.Z = dis;
                }
                else if (state == "中心")
                {
                    v.Z = 0;
                }
                else if (state == "下沉")
                {
                    v.Z = -dis;
                }
            }
            //偏移
            v.Z += offset;
            Transform yform = Transform.Translation(v);
            GeometryInformation transformedGeometryInformation = geometryInformation.Duplicate();
            transformedGeometryInformation.Transform(yform);

            BaseFace middleBaseFace = orientedFace.Duplicate();
            middleBaseFace.Transform(yform);
            BaseFace topBaseFace = middleBaseFace.Duplicate();
            topBaseFace.Transform(Transform.Translation(new Vector3d(0, 0, faceTimber.width / 2)));
            BaseFace bottomBaseFace = middleBaseFace.Duplicate();
            bottomBaseFace.Transform(Transform.Translation(new Vector3d(0, 0, -faceTimber.width / 2)));


            //原路返回
            List<Transform> zformList = baseFaces.Select(x => Transform.PlaneToPlane(Plane.WorldXY, x.referencePlane)).ToList();
            List<GeometryInformation> backGeometryInformationList = new List<GeometryInformation>();
            List<BaseFace> topBaseFaces = new List<BaseFace>();
            List<BaseFace> middleBaseFaces = new List<BaseFace>();
            List<BaseFace> bottomBaseFaces = new List<BaseFace>();
            for (int i = 0; i < zformList.Count; i++)
            {
                GeometryInformation tmpG = transformedGeometryInformation.Duplicate();
                tmpG.Transform(zformList[i]);
                backGeometryInformationList.Add(tmpG);
                BaseFace tmpTop = topBaseFace.Duplicate();
                BaseFace tmpMiddle = middleBaseFace.Duplicate();
                BaseFace tmpBottom = bottomBaseFace.Duplicate();
                tmpTop.Transform(zformList[i]);
                tmpMiddle.Transform(zformList[i]);
                tmpBottom.Transform(zformList[i]);
                topBaseFaces.Add(tmpTop);
                middleBaseFaces.Add(tmpMiddle);
                bottomBaseFaces.Add(tmpBottom);
            }


            DA.SetDataList(0, backGeometryInformationList);
            DA.SetDataList(1, topBaseFaces);
            DA.SetDataList(2, middleBaseFaces);
            DA.SetDataList(3, bottomBaseFaces);
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
            get { return new Guid("CF622CE5-82EF-4432-A754-9826ACAE28AE"); }
        }
    }
}