using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types.Transforms;
using Rhino.Geometry;
using TimberElement;
using UtilityElement.Metadeta;
using UtilityElement.Metadeta.Dimensions;
using UtilityElement.ReferenceDatum;
using OutPutElement;
using System.Linq;

namespace MyGrasshopperAssembly1
{
    public class GH_CircleSection : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public GH_CircleSection()
          : base("GH_CircleSection", "Nickname",
              "Description",
              "Yhm Toolbox", "几何生成_木构构件")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("定位基线", "DWJX", "...", GH_ParamAccess.list);

            pManager.AddTextParameter("名称", "NAME", "...", GH_ParamAccess.item);
            pManager.AddTextParameter("上浮/中心/下沉", "", "...", GH_ParamAccess.item);
            pManager.AddNumberParameter("偏移", "OFFSET", "...", GH_ParamAccess.item);

            pManager.AddNumberParameter("开端出头", "CT0", "...", GH_ParamAccess.item);
            pManager.AddNumberParameter("尾端出头", "CT1", "...", GH_ParamAccess.item);

            pManager.AddNumberParameter("半径", "R", "...", GH_ParamAccess.item);
            pManager.AddNumberParameter("金盘占比", "JP", "...", GH_ParamAccess.item);

            pManager.AddBooleanParameter("改动基准", "GD", "...", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("图元信息", "TYXX", "...", GH_ParamAccess.list);
            pManager.AddGenericParameter("顶部线", "DWJX", "...", GH_ParamAccess.item);
            pManager.AddGenericParameter("中部线", "DWJX", "...", GH_ParamAccess.item);
            pManager.AddGenericParameter("底部线", "DWJX", "...", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<BaseCurve> baseCurves = new List<BaseCurve>();
            String state = "";
            String name = "";
            Double offset = 0.0;
            Double expansion0 = 0.0;
            Double expansion1 = 0.0;
            Double radius = 0.0;
            Double jinPan = 0.0;//金盘占比
            Boolean isDatumChanged = false;

            DA.GetDataList(0, baseCurves);
            DA.GetData(1,ref name);
            DA.GetData(2,ref state);
            DA.GetData(3,ref offset);
            DA.GetData(4, ref expansion0);
            DA.GetData(5, ref expansion1);
            DA.GetData(6, ref radius);
            DA.GetData(7, ref jinPan);
            DA.GetData(8,ref isDatumChanged);

            //需要输出的结果
            List<GeometryInformation> geometryInformationList = new List<GeometryInformation> ();
            List<BaseCurve> middleBaseCurves = new List<BaseCurve>();
            List<BaseCurve> topBaseCurves = new List<BaseCurve>();
            List<BaseCurve> bottomBaseCurves = new List<BaseCurve>();

            //创建图元信息与边界条件
            CircleSectionTimber circleSectionTimber = new CircleSectionTimber();
            GeometryInformation geometryInformation = new GeometryInformation();
            GeometryInformation tmpGeometryInformation = new GeometryInformation();
            BaseCurve middleBaseCurve = new BaseCurve();
            BaseCurve topBaseCurve = new BaseCurve();
            BaseCurve bottomBaseCurve = new BaseCurve();
            BaseCurve tmpMiddleBaseCurve = new BaseCurve();
            BaseCurve tmpTopBaseCurve = new BaseCurve();
            BaseCurve tmpBottomBaseCurve = new BaseCurve();

            //在XY平面上创建，复制位移生成多项
            ////图元信息，在XY平面上创建一项
            //创建圆剖面尺寸
            CircularProfile circularProfile = new CircularProfile(expansion0, expansion1, radius, jinPan);
            //取一项,在XY平面上创建CircleSectionTimber
            if (baseCurves.Count == 0 || baseCurves[0] == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "未输入定位基线！");
                return;
            }
            circleSectionTimber = new CircleSectionTimber(circularProfile, baseCurves[0].curveLength, name);

            if (circleSectionTimber.isExist == false)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "参数错误！");
                return;
            }
            //获取到CircleSectionTimber的几何图元、标注辅助线、定位平面、尺寸信息描述
            Brep timberBrep = circleSectionTimber.TimberBrep;
            AnnotationGuideCurves annotationGuideCurves = circleSectionTimber.AnnotationGuideCurves;
            Plane referncePlane = circleSectionTimber.baseCurveInfluenced.referencePlane;
            String description = circleSectionTimber.description;

            //将获取到的信息赋予geometryInformation
            geometryInformation = new GeometryInformation(timberBrep, annotationGuideCurves, referncePlane, description, circleSectionTimber.baseCurveInfluenced);
            ////位移成为多项
            //得到XY平面到各项的Transform
            List<Transform> transforms = baseCurves.Select(curve => Transform.PlaneToPlane(Plane.WorldXY, curve.extrudePlane)).ToList();

            //上浮/中心/下沉
            Vector3d v = circleSectionTimber.baseCurveInfluenced.positiveDirection;//位移矢量
            Double dis = circleSectionTimber.circularProfile.height / 2;
            if (state != null)
            {
                if (state == "上浮")
                {
                    v.Y = dis;
                }
                else if (state == "中心")
                {
                    v.Y = 0;
                }
                else if (state == "下沉")
                {
                    v.Y = -dis;
                }

            }

            ////边界条件，在XY平面上创建一项
            Vector3d halfHeight = new Vector3d(v);
            halfHeight.Y = dis;

            if(isDatumChanged == true)
            {
                middleBaseCurve = circleSectionTimber.baseCurveInfluenced.Duplicate();
                topBaseCurve = circleSectionTimber.baseCurveInfluenced.Duplicate();
                bottomBaseCurve = circleSectionTimber.baseCurveInfluenced.Duplicate();
            }
            else
            {
                middleBaseCurve = circleSectionTimber.baseCurve.Duplicate();
                topBaseCurve = circleSectionTimber.baseCurve.Duplicate();
                bottomBaseCurve = circleSectionTimber.baseCurve.Duplicate();
            }

            topBaseCurve.Transform(Transform.Translation(halfHeight));
            bottomBaseCurve.Transform(Transform.Translation(-halfHeight));

            //将geometryInformation与边界条件向正方向移动
            v.Y += offset;
            geometryInformation.Transform(Transform.Translation(v));
            topBaseCurve.Transform(Transform.Translation(v));
            middleBaseCurve.Transform(Transform.Translation(v));
            bottomBaseCurve.Transform(Transform.Translation(v));
            //从XY平面位移生成其他项
            foreach (Transform transform in transforms)
            {
                tmpGeometryInformation = geometryInformation.Duplicate();
                tmpGeometryInformation.Transform(transform);
                geometryInformationList.Add(tmpGeometryInformation);

                tmpMiddleBaseCurve = middleBaseCurve.Duplicate();
                tmpMiddleBaseCurve.Transform(transform);
                middleBaseCurves.Add(tmpMiddleBaseCurve);

                tmpTopBaseCurve = topBaseCurve.Duplicate();
                tmpTopBaseCurve.Transform(transform);
                topBaseCurves.Add(tmpTopBaseCurve);

                tmpBottomBaseCurve = bottomBaseCurve.Duplicate();
                tmpBottomBaseCurve.Transform(transform);
                bottomBaseCurves.Add(tmpBottomBaseCurve);
            }            

            DA.SetDataList(0, geometryInformationList);
            DA.SetDataList(1, topBaseCurves);
            DA.SetDataList(2, middleBaseCurves);
            DA.SetDataList(3, bottomBaseCurves);
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
            get { return new Guid("D8771618-40B4-4B3E-8EE0-D11FFF9EDB11"); }
        }
    }
}