using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using IOComponents;
using Rhino.Geometry;
using UtilityElement.Metadeta.Dimensions;
using YhmAssembly;
using System.Windows.Forms;
using System.Linq;

namespace MyGrasshopperAssembly1
{
    public class 柱径权衡_无斗拱亭子 : ZuiComponent
    {
        /// <summary>
        /// Initializes a new instance of the 柱径权衡 class.
        /// </summary>
        public 柱径权衡_无斗拱亭子()
          : base("柱径权衡_无斗拱亭子", "Nickname",
              "Description",
              "Yhm Toolbox", "构件权衡")
        {
        }

        private static readonly ParamDefinition[] inputs = new ParamDefinition[]
        {
            new ParamDefinition(new Param_GenericObject
            {
                Name = "檐柱径D",
                NickName = "D",
                Description = "",
                Access = GH_ParamAccess.item,
                Optional = true,
            }),
        };
        private static readonly ParamDefinition[] outputs = new ParamDefinition[]
        {
            new ParamDefinition(new Param_GenericObject
            {
                Name = "檐柱半径",
                NickName = "",
                Description = "",
                Access = GH_ParamAccess.item,
                Optional = false
            }),
            new ParamDefinition(new Param_GenericObject
            {
                Name = "雷公柱半径",
                NickName = "",
                Description = "",
                Access = GH_ParamAccess.item,
                Optional = false
            }),
            new ParamDefinition(new Param_GenericObject
            {
                Name = "重檐金柱半径",
                NickName = "",
                Description = "",
                Access = GH_ParamAccess.item,
                Optional = false
            }),
            new ParamDefinition(new Param_GenericObject
            {
                Name = "童柱半径",
                NickName = "",
                Description = "",
                Access = GH_ParamAccess.item,
                Optional = false
            }),
            new ParamDefinition(new Param_GenericObject
            {
                Name = "檐柱半径",
                NickName = "",
                Description = "",
                Access = GH_ParamAccess.item,
                Optional = false
            }),
        };
        public override GH_Exposure Exposure => GH_Exposure.last;
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>

        private List<List<(string, Object)>> dataLists { get; set; }//menu下的所有层级
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
                    ("檐柱半径",D/2),
                    ("雷公柱半径",D/2),
                };
            List<(string, Object)> dataList1 = new List<(string, object)>
                {
                    ("重檐金柱半径",1.2*D/2),
                    ("童柱半径",0.8*D/2),
                };
            List<(string, Object)> dataList2 = new List<(string, object)>
                {
                    ("垂柱半径",0.8*D/2),
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

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            GH_Component currentComponent = this;//获取当前组件
            MenuManager menuManager = new MenuManager(ref currentComponent, ref menu);//新建空菜单
            string[] menuItemNames = new string[]
            {
                "檐柱、雷公柱半径",
                "重檐金柱、童柱半径",
                "垂柱半径"
            };//菜单内容
            menuManager.menuItemNames = menuItemNames;//赋予名称的同时创建出GH中的菜单与eventHandlers、menuState

            for (int i = 0; i < menuItemNames.Count(); i++)
            {
                OutputParamsManager outputParamsManager = new OutputParamsManager(ref currentComponent, dataLists[i]);
                Action action = () => outputParamsManager.ResetOutputsParams();
                menuManager.SetMenuItemAction(i, action);//为menu自定义的第i项设置操作方法
            }
            menuManager.UpdateMenu();
        }

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
            get { return new Guid("7D3891C5-A92C-4165-979B-C50CA71BEF05"); }
        }

        protected override ParamDefinition[] Inputs => inputs;

        protected override ParamDefinition[] Outputs => outputs;
    }
}