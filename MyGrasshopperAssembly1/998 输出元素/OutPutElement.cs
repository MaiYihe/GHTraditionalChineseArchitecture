using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UtilityElement.Metadeta;
using UtilityElement.ReferenceDatum;
using YhmAssembly;

namespace OutPutElement
{
    public class GeometryInformation
    {
        public Brep brep { get; set; }//几何图元
        public AnnotationGuideCurves annotationGuideCurves { get; set; }//标注辅助线
        public Plane referencePlane { get; set; }//定位平面
        public String description { get; set; }//构件尺寸
        public BaseCurve baseCurve { get; set; }//定位基线
        public GeometryInformation Duplicate()
        {
            GeometryInformation geometryInformation = new GeometryInformation(brep, annotationGuideCurves, referencePlane, description, baseCurve);
            return geometryInformation;
        }//复制
        public void Transform(Transform xform)
        {
            brep = GeometryUtility.TransformGeometry(brep, xform) as Brep;
            if (annotationGuideCurves != null) annotationGuideCurves.Transform(xform);
            if (referencePlane != null) referencePlane = (Plane)GeometryUtility.TransformGeometry(referencePlane, xform);
            if (baseCurve != null) baseCurve.Transform(xform);
        }//位移

        //Construct
        public GeometryInformation()
        {
        }
        public GeometryInformation(Brep brep, AnnotationGuideCurves annotationGuideCurves, Plane referencePlane, string description, BaseCurve baseCurve)
        {
            if (brep != null) this.brep = brep.Duplicate() as Brep;
            if (annotationGuideCurves != null) this.annotationGuideCurves = annotationGuideCurves.Duplicate();
            if (referencePlane != null) this.referencePlane = referencePlane.Clone();
            this.description = description;
            if (baseCurve != null) this.baseCurve = baseCurve.Duplicate();
        }
        public GeometryInformation(Brep brep, AnnotationGuideCurves annotationGuideCurves, Plane referencePlane, string description)
        {
            this.brep = brep.Duplicate() as Brep;
            if (annotationGuideCurves != null) this.annotationGuideCurves = annotationGuideCurves.Duplicate();
            if (referencePlane != null) this.referencePlane = referencePlane.Clone();
            this.description = description;
        }
    }//图元信息
}
