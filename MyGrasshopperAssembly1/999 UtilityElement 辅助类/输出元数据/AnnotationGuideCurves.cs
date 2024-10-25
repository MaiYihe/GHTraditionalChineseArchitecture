using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel;
using Rhino.Display;
using System.Runtime.Serialization;
using static Rhino.UI.Controls.RenderContentMenu;
using Rhino;
using YhmAssembly;

namespace UtilityElement //辅助生成类
{
    namespace Metadeta//元数据
    {
        //输出元数据-标注辅助线
        public class AnnotationGuideCurves:GH_Goo<IGH_GeometricGoo>, IGH_PreviewData
        {

            public List<Curve> topViewCurves { get; set; }
            public List<Curve> frontViewCurves { get; set; }
            public List<Curve> rightViewCurves { get; set; }

            public void Transform(Transform xform)
            {
                TransformCurves(topViewCurves, xform);
                TransformCurves(frontViewCurves, xform);
                TransformCurves(rightViewCurves, xform);
            }//Transform标注辅助线
            private void TransformCurves(List<Curve> curves, Transform xform)
            {
                if (curves == null)
                    return;

                // 对列表中的每条曲线应用变换
                foreach (var curve in curves)
                {
                    if (curve != null)
                        curve.Transform(xform);
                }
            }//Transform标注辅助线
            public override AnnotationGuideCurves Duplicate()
            {
                AnnotationGuideCurves duplicate = new AnnotationGuideCurves();
                // 复制 topViewCurves
                duplicate.topViewCurves = new List<Curve>();
                if (topViewCurves != null)
                {
                    foreach (var curve in topViewCurves)
                    {
                        duplicate.topViewCurves.Add(curve.DuplicateCurve());
                    }
                }
                // 复制 frontViewCurves
                if (frontViewCurves != null)
                {
                    duplicate.frontViewCurves = new List<Curve>();
                    foreach (var curve in frontViewCurves)
                    {
                        duplicate.frontViewCurves.Add(curve.DuplicateCurve());
                    }
                }

                // 复制 rightViewCurves
                if (rightViewCurves != null)
                {
                    duplicate.rightViewCurves = new List<Curve>();
                    foreach (var curve in rightViewCurves)
                    {
                        duplicate.rightViewCurves.Add(curve.DuplicateCurve());
                    }
                }
                return duplicate;

            }//复制标注辅助线

            public BoundingBox ClippingBox => new BoundingBox();

            public override bool IsValid => true;

            public override string TypeName => "MyCustomComponent";

            public override string TypeDescription => "Description of MyCustomComponent";

            public void DrawViewportWires(GH_PreviewWireArgs args)
            {
            }

            public void DrawViewportMeshes(GH_PreviewMeshArgs args)
            {                
            }

            public override string ToString()
            {
                return "AnnotationGuideline";
            }

            //Construction
            public AnnotationGuideCurves()
            {
            }
            public AnnotationGuideCurves(List<Curve> topViewCurves, List<Curve> frontViewCurves, List<Curve> rightViewCurves)
            {
                this.topViewCurves = topViewCurves;
                this.frontViewCurves = frontViewCurves;
                this.rightViewCurves = rightViewCurves;
            }

            public interface IOtherMetaData//其他元数据的接口
            {
                string material { get; set; }//材料
                string dimension { get; set; }//加工尺寸的标注
                double volume { get; set; }//体积
                string other { get; set; }//其他
            }
        }//标注辅助线
    }
}
