using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using IOComponents;
using Rhino.Geometry;
using UtilityElement.Metadeta.Dimensions;
using System.Windows.Forms;
using YhmAssembly;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Rhino;
using System.ComponentModel;
using System.Linq;
using GH_IO.Serialization;

namespace MyGrasshopperAssembly1
{
    public class 梁高宽权衡_普通屋架 : ZuiComponent
    {
        /// <summary>
        /// Initializes a new instance of the 柱径权衡 class.
        /// </板高宽权衡>
        public 梁高宽权衡_普通屋架()
          : base("梁高宽权衡_普通屋架_屋架无斗栱", "Nickname",
              "Description",
              "Yhm Toolbox", "构件权衡")
        {
        }

        private static readonly ParamDefinition[] inputs = new ParamDefinition[1]
        {
            new ParamDefinition(new Param_GenericObject
            {
                Name = "柱径D",
                NickName = "D",
                Description = "D，默认单位为m",
                Access = GH_ParamAccess.item,
                Optional = true
            }),
        };

        private static readonly ParamDefinition[] outputs = new ParamDefinition[]
        {
            new ParamDefinition(new Param_GenericObject
            {
                Name = "三架梁尺寸",
                NickName = "",
                Description = "",
                Access = GH_ParamAccess.item,
                Optional = false
            }),
            new ParamDefinition(new Param_GenericObject
            {
                Name = "五架梁尺寸",
                NickName = "",
                Description = "",
                Access = GH_ParamAccess.item,
                Optional = false
            }),
            new ParamDefinition(new Param_GenericObject
            {
                Name = "抱头梁尺寸",
                NickName = "",
                Description = "",
                Access = GH_ParamAccess.item,
                Optional = false
            }),
            new ParamDefinition(new Param_GenericObject
            {
                Name = "随梁尺寸",
                NickName = "",
                Description = "",
                Access = GH_ParamAccess.item,
                Optional = false
            }),
            new ParamDefinition(new Param_GenericObject
            {
                Name = "脊瓜柱尺寸",
                NickName = "",
                Description = "",
                Access = GH_ParamAccess.item,
                Optional = false
            }),
            new ParamDefinition(new Param_GenericObject
            {
                Name = "三架梁下金瓜柱尺寸",
                NickName = "",
                Description = "",
                Access = GH_ParamAccess.item,
                Optional = false
            }),
            new ParamDefinition(new Param_GenericObject
            {
                Name = "五架梁下金瓜柱尺寸",
                NickName = "",
                Description = "",
                Access = GH_ParamAccess.item,
                Optional = false
            }),
            new ParamDefinition(new Param_GenericObject
            {
                Name = "三架梁下柁墩尺寸",
                NickName = "",
                Description = "",
                Access = GH_ParamAccess.item,
                Optional = false
            }),
            new ParamDefinition(new Param_GenericObject
            {
                Name = "五架梁下柁墩尺寸",
                NickName = "",
                Description = "",
                Access = GH_ParamAccess.item,
                Optional = false
            }),
        };

        public override GH_Exposure Exposure => GH_Exposure.primary;

        private List<List<(string, Object)>> dataLists { get; set; }
        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Component currentComponent = this;
            Double D = 0.0;
            DA.GetData(0, ref D);
            dataLists = new List<List<(string, Object)>>();
            //nickName,outputResult,menu管理
            List<(string, Object)> dataList0 = new List<(string, object)>
                {
                    ("三架梁尺寸",new SquareProfile(D, D, 1.25 * D, 0.95 * D)),
                    ("五架梁尺寸",new SquareProfile(D, D, 1.5 * D, 1.2 * D)),
                    ("抱头梁尺寸",new SquareProfile(0, D, 1.4 * D, 1.1 * D)),
                    ("随梁尺寸",new SquareProfile(0, 0, D, 0.8 * D))
                };
            List<(string, Object)> dataList1 = new List<(string, object)>
                {
                    ("脊瓜柱尺寸",new SquareProfile(0, 0, 0.8 * 0.95 * D, 0.8 * D)),
                    ("三架梁下金瓜柱尺寸",new SquareProfile(0, 0, 0.8 * 0.95 * D, D)),
                    ("五架梁下金瓜柱尺寸",new SquareProfile(0, 0, 0.8 * 1.2 * D, D)),
                };
            List<(string, Object)> dataList2 = new List<(string, object)>
                {
                    ("三架梁下柁墩尺寸",new SquareProfile(0, 0, 0.8 * 0.95 * D, 2 * D)),
                    ("五架梁下柁墩尺寸",new SquareProfile(0, 0, 0.8 * 1.2 * D, 2 * D)),
                };
            dataLists.Add(dataList0);
            dataLists.Add(dataList1);
            dataLists.Add(dataList2);
            this.dataLists = dataLists;
            //SolveInstance的输出参数管理
            OutputParamsManager outputParamsManager = new OutputParamsManager(ref currentComponent, ref DA);
            outputParamsManager.AddToDictionary(dataList0, dataList1, dataList2);
            outputParamsManager.LookupDictionaryAndOutput();//找到对映的字典并输出
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            GH_Component currentComponent = this;//获取当前组件
            MenuManager menuManager = new MenuManager(ref currentComponent, ref menu);//新建空菜单
            string[] menuItemNames = new string[]
            {
                "梁尺寸",
                "瓜柱尺寸",
                "柁墩尺寸"
            };//菜单内容
            menuManager.menuItemNames = menuItemNames;//赋予名称的同时创建出GH中的菜单与eventHandlers、menuState

            for(int i =0;i< menuItemNames.Count(); i++)
            {
                OutputParamsManager outputParamsManager = new OutputParamsManager(ref currentComponent, dataLists[i]);
                Action action = () => outputParamsManager.ResetOutputsParams();
                menuManager.SetMenuItemAction(i, action);//为menu自定义的第i项设置操作方法
            }
            menuManager.UpdateMenu();
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
            get { return new Guid("ED2A407E-C088-4195-977F-F83186DCC4C7"); }
        }

        protected override ParamDefinition[] Inputs => inputs;

        protected override ParamDefinition[] Outputs => outputs;
    }
}