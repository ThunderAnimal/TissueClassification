/*
 * @author Sebastian Lohmann
 */
namespace AnnotationExtractor {
	using System;
	using System.Collections.Generic;
	using Glaukopis.SlideProcessing;
	using VMscope.InteropCore.VirtualMicroscopy;
	internal static class AnnotationExtractor {
		private static void Main(string[] args) {
			foreach(var slideName in Util.GetSlideFilenames(new string[] { args[1] })){
				using(var slideCache=new SlideCache(slideName)){
					foreach(var annotation in slideCache.Slide.GetAnnotations()){
						var contained=new List<IAnnotation>();
						foreach(var candidate in slideCache.Slide.GetAnnotations()){
							if(candidate==annotation) continue;
							var rectangle=annotation.BoundingBox;
							rectangle.Intersect(candidate.BoundingBox);
							if(rectangle.IsEmpty) continue;
							if(annotation.BoundingBox.Contains(candidate.BoundingBox)) contained.Add(candidate);
						}
						var name=annotation.Name+"."+slideCache.SlideName+"."+annotation.Id;
						using(var b=annotation.Extract(1,contained)) b.Save(args[0]+"\\"+name+".png");
						Console.WriteLine(name+" exc:"+contained.Count);
					}
				}
			}
		}
	}
}
