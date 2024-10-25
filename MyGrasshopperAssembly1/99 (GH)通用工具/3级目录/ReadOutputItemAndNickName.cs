using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using Grasshopper.Kernel.Attributes;
using System.ComponentModel;
using YhmAssembly;
using Grasshopper.GUI.Canvas;
using Rhino.Render.CustomRenderMeshes;
using Grasshopper;
using Grasshopper.Kernel.Types;
using Rhino;
using GH_IO.Serialization;

namespace MyGrasshopperAssembly1
{
    public class ReadOutputItemAndNickName : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ReadOutputNickName class.
        /// </summary>
        public ReadOutputItemAndNickName()
          : base("ReadOutputItemAndNickName", "Nickname",
              "Description",
              "Yhm Toolbox", "通用工具")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("ItemInfo", "II", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Item", "I", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("NickName", "NN", "", GH_ParamAccess.item);
        }

        public override GH_Exposure Exposure => GH_Exposure.tertiary;

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            object obj = default(object);
            DA.GetData(0, ref obj);

            GH_Canvas gH_Canvas = Instances.ActiveCanvas;
            SourceAndTarget sourceAndTarget = new SourceAndTarget(ref gH_Canvas,this);
            if (sourceAndTarget.source !=null)
            {
                IGH_Param source = sourceAndTarget.source;
                if (sourceAndTarget.currentComponentInputConnectected == true) inputNickName = source.NickName;
            }
            DA.SetData(0, obj);
            DA.SetData(1, inputNickName);
        }

        // 创建组件的属性
        public override void CreateAttributes()
        {
            base.CreateAttributes();            
        }

        String inputNickName = default(String);
        public override bool Write(GH_IWriter writer)
        {
            writer.SetString("inputNickName", inputNickName); // 保存参数值
            return base.Write(writer);
        }

        // 从 Grasshopper 文件加载参数值
        public override bool Read(GH_IReader reader)
        {
            reader.TryGetString("inputNickName", ref inputNickName); // 从文件中读取参数值
            return base.Read(reader);
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
                //string inputString = "3222s";
                //System.Drawing.Bitmap bitmap = StringToBitmap(inputString, 15, 15);
                //return bitmap;
                //Rectangle rec = new Rectangle(0, 0, (int)widthInRhinoUnits, (int)heightInRhinoUnits);
                //Bounds = rec;
                return null;
            }
        }
        //public static System.Drawing.Bitmap StringToBitmap(string text, int width, int height)
        //{
        //    //// 创建位图对象
        //    //System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(width, height);

        //    //// 创建绘图对象
        //    //using (Graphics graphics = Graphics.FromImage(bitmap))
        //    //{
        //    //    // 设置文本颜色、字体、大小等
        //    //    Font font = new Font("Arial", width / 2);
        //    //    Brush brush = Brushes.Black;

        //    //    // 清空图像
        //    //    graphics.Clear(Color.White);

        //    //    // 测量文本大小
        //    //    SizeF textSize = graphics.MeasureString(text, font);

        //    //    // 计算文本起始位置，使其位于 Bitmap 的正中心
        //    //    float x = (width - textSize.Width) / 2;
        //    //    float y = (height - textSize.Height) / 2;

        //    //    // 在图像上绘制文本
        //    //    graphics.DrawString(text, font, brush, new PointF(x, y));
        //    //}

        //    //return bitmap;
        //}

        //protected override void Layout(int width, int height)
        //{
        //    // 将 Bitmap 的大小转换为 Grasshopper 单位（这里简单地假设 1 像素等于 1.0 的 Rhino 单位）
        //    int widthInPixels = width;
        //    int heightInPixels = height;

        //    double widthInRhinoUnits = widthInPixels;
        //    double heightInRhinoUnits = heightInPixels;

        //    // 设置组件的大小
        //    Rectangle rec = new Rectangle(0, 0, (int)widthInRhinoUnits, (int)heightInRhinoUnits);
        //    Bounds = rec;

        //    // 设置组件中的参数布局等
        //    AdjustParameterLayout();
        //}

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("9E6C44FD-4CD3-4B35-8CCF-6B3381E0EE21"); }
        }
    }
}