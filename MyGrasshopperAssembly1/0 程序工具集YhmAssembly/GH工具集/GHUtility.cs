using Rhino.Geometry.Collections;
using Rhino.Geometry.Intersect;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Rhino.Geometry.BrepFace;
using Rhino.DocObjects;
using static YhmAssembly.BrepUtility;
using static YhmAssembly.listEdit;
using System.Runtime.CompilerServices;
using Grasshopper.Kernel;
using Rhino;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Runtime.InProcess;
using Rhino.Commands;
using Rhino.Display;
using Grasshopper;
using Grasshopper.Getters;
using System.Security.Cryptography;
using System.Reflection;
using Grasshopper.GUI.Canvas;
using System.Windows.Forms;
using Rhino.Runtime;

namespace YhmAssembly//我的工具箱，仅对缺省类进行操作!
{
    public class SourceAndTarget
    {
        public IGH_Param source { get; set; }//输入端参数
        public IGH_Param target { get; set; }//输出端参数
        public object mode { get; set; }
        public bool currentComponentInputConnectected { get; set; }
        public bool currentComponentOutputConnectected { get; set; }
        Grasshopper.GUI.Canvas.Interaction.IGH_MouseInteraction wireInteraction { get; set; }
        public SourceAndTarget(ref GH_Canvas canvas)
        {
            this.currentComponentInputConnectected = false;
            this.currentComponentOutputConnectected = false;
            if (canvas != null)
            {
                wireInteraction = canvas.ActiveInteraction;
                if (wireInteraction != null && wireInteraction is Grasshopper.GUI.Canvas.Interaction.GH_WireInteraction)
                {
                    Type type = typeof(Grasshopper.GUI.Canvas.Interaction.GH_WireInteraction);
                    source = type
                      .GetField("m_source", BindingFlags.NonPublic | BindingFlags.Instance)
                      .GetValue(wireInteraction) as IGH_Param;
                    target = type
                      .GetField("m_target", BindingFlags.NonPublic | BindingFlags.Instance)
                      .GetValue(wireInteraction) as IGH_Param;
                    mode = type
                      .GetField("m_mode", BindingFlags.NonPublic | BindingFlags.Instance)
                      .GetValue(wireInteraction);
                }//得到source、target
                this.source = source;
                this.target = target;
                this.mode = mode;
            }            
        }//得到canvas任意某处的source和target
        public SourceAndTarget(ref GH_Canvas canvas, GH_Component gH_Component)
        {
            this.currentComponentInputConnectected = false;
            this.currentComponentOutputConnectected = false;
            if (canvas != null)
            {
                wireInteraction = canvas.ActiveInteraction;
                if (wireInteraction != null && wireInteraction is Grasshopper.GUI.Canvas.Interaction.GH_WireInteraction)
                {
                    Type type = typeof(Grasshopper.GUI.Canvas.Interaction.GH_WireInteraction);
                    source = type
                      .GetField("m_source", BindingFlags.NonPublic | BindingFlags.Instance)
                      .GetValue(wireInteraction) as IGH_Param;
                    target = type
                      .GetField("m_target", BindingFlags.NonPublic | BindingFlags.Instance)
                      .GetValue(wireInteraction) as IGH_Param;
                    mode = type
                      .GetField("m_mode", BindingFlags.NonPublic | BindingFlags.Instance)
                      .GetValue(wireInteraction);
                }//得到source、target
            }
            else return;
            if (source != null && target != null && mode != null)
            {
                int inputCount = gH_Component.Params.Input.Count;
                int outputCount = gH_Component.Params.Output.Count;
                for (int i = 0; i < inputCount; i++)
                {
                    if (gH_Component.Params.Input[i].InstanceGuid == source.InstanceGuid
                        || gH_Component.Params.Input[i].InstanceGuid == target.InstanceGuid)
                        this.currentComponentInputConnectected = true;
                }//检测组件输入端是否参与本次连接
                for (int i = 0; i < outputCount; i++)
                {
                    if (gH_Component.Params.Output[i].InstanceGuid == source.InstanceGuid
                        || gH_Component.Params.Output[i].InstanceGuid == target.InstanceGuid)
                        this.currentComponentInputConnectected = true;
                }//检测组件输出端是否参与本次连接
            }

            if (currentComponentInputConnectected || currentComponentOutputConnectected)
            {
                this.source = source;
                this.target = target;
                this.mode = mode;
            }
            else
            {
                this.source = null;
                this.target = null;
                this.mode = null;
            }
        }//得到指定组件的source和target
    }//得到Wire的source和target参数
    public class WireConnectionDetect
    {
        public GH_Canvas canvas { get; set; }
        public Action action { get; set; }
        public WireConnectionDetect(ref GH_Canvas canvas, object sender, MouseEventArgs e, ref Action doSomething)
        {
            if (canvas != null)
                canvas.MouseDown += MouseDown;
            this.canvas = canvas;
            action = doSomething;

            if (sender != null && e != null)
            {
                MouseDown(sender, e);
                MouseUp(sender, e);
            }
        }

        Grasshopper.GUI.Canvas.Interaction.IGH_MouseInteraction wireInteraction;
        public void MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            if (canvas.ActiveInteraction != null &&
              (canvas.ActiveInteraction is Grasshopper.GUI.Canvas.Interaction.GH_WireInteraction ||
            canvas.ActiveInteraction is Grasshopper.GUI.Canvas.Interaction.GH_RewireInteraction))
            {
                canvas.MouseUp += MouseUp;
                wireInteraction = canvas.ActiveInteraction;
            }
        }
        public void MouseUp(object sender, MouseEventArgs e)
        {
            canvas.MouseUp -= MouseUp;
            if (wireInteraction != null)
            {
                if (wireInteraction is Grasshopper.GUI.Canvas.Interaction.GH_WireInteraction)
                {
                    Type type = typeof(Grasshopper.GUI.Canvas.Interaction.GH_WireInteraction);
                    IGH_Param source = type
                      .GetField("m_source", BindingFlags.NonPublic | BindingFlags.Instance)
                      .GetValue(wireInteraction) as IGH_Param;
                    IGH_Param target = type
                      .GetField("m_target", BindingFlags.NonPublic | BindingFlags.Instance)
                      .GetValue(wireInteraction) as IGH_Param;
                    object mode = type
                      .GetField("m_mode", BindingFlags.NonPublic | BindingFlags.Instance)
                      .GetValue(wireInteraction);

                    //测试guid
                    //RhinoApp.WriteLine(string.Format(
                    //     "InputInstanceGuid:{0}",
                    //     base.Params.Input[0].InstanceGuid));
                    //RhinoApp.WriteLine(string.Format(
                    //     "SourceInstanceGuid:{0}",
                    //     source.InstanceGuid));
                    //RhinoApp.WriteLine(string.Format(
                    //     "TargetInstanceGuid:{0}",
                    //     target.InstanceGuid));
                    if (source != null && target != null && mode != null && action != null)
                    {
                        action.Invoke();
                    }//说明两个参数通过wire连接上了
                }
            }
        }
    }//检测Canvas全局是否有Wire连线，检测到就传递委托（）
}

