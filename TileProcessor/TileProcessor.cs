/*
 * @author Sebastian Lohmann
 */
namespace TileProcessor {
	using System;
	using System.Drawing;
	using System.IO;
	using System.Linq;
	using Glaukopis.Core.Analysis;
	using Glaukopis.SharpAccessoryIntegration;
	using Glaukopis.SlideProcessing;
	using SharpAccessory.Imaging.Filters;
	internal class TileProcessor {
		private static void Main(string[] args) {
			foreach(var slideName in Util.GetSlideFilenames(args)){
				using(var slideCache=new SlideCache(slideName)){
					var hValues=File.Exists(slideCache.DataPath+"hValues.xml")?SlidePartitioner<double>.Load(slideCache.DataPath+"hValues.xml"):new SlidePartitioner<double>(slideCache.Slide,1f,new Size(2000,2000));
					var eValues=File.Exists(slideCache.DataPath+"eValues.xml")?SlidePartitioner<double>.Load(slideCache.DataPath+"eValues.xml"):hValues.Duplicate<double>();
					var hRange=new Range<double>();
					var eRange=new Range<double>();
					foreach(var tile in hValues.Values){
						using(Bitmap tileImage=slideCache.Slide.GetImagePart(tile),hImage=tileImage.Clone() as Bitmap,eImage=tileImage.Clone() as Bitmap){
							var gpH=new ColorDeconvolution().Get1stStain(hImage,ColorDeconvolution.KnownStain.HaematoxylinEosin);
							var gpE=new ColorDeconvolution().Get2ndStain(eImage,ColorDeconvolution.KnownStain.HaematoxylinEosin);
							var hSum=0u;
							var eSum=0u;
							var cnt=0;
							foreach(var grayscalePixel in gpH.Pixels()){
								hSum+=grayscalePixel.V;
								eSum+=gpE.GetPixel(grayscalePixel.X,grayscalePixel.Y);
								cnt++;
							}
							var meanH=(double)hSum/(double)cnt;
							tile.Data=meanH;
							hRange.Add(meanH);
							var meanE=(double)eSum/(double)cnt;
							eValues[tile.Index].Data=meanE;
							eRange.Add(meanE);
							gpH.Dispose();
							gpE.Dispose();
							if(slideCache.Slide.GetAnnotationsInArea(tile.SourceArea).Any()){
								var tileCache=slideCache.GetTileCache(tile.Index);
								tileCache.SetImage("rgb",tileImage);
								tileCache.SetImage("h",gpH.Bitmap);
								tileCache.SetImage("e",gpE.Bitmap);
							}
						}
						Console.WriteLine(slideCache.SlideName+"-"+tile.Index+" done");
					}
					var range=new Range<double>{hRange.Minimum,hRange.Maximum,eRange.Minimum,eRange.Maximum};
					Func<double,Color> toColor=v=>{
						var c=(int)Math.Round(range.Normalize(v)*255d);
						return Color.FromArgb(c,c,c);
					};
					slideCache.SetImage("hValues",hValues.GenerateHeatMap(toColor));
					slideCache.SetImage("eValues",eValues.GenerateHeatMap(toColor));
					slideCache.SetImage("overview",slideCache.Slide.GetImagePart(0,0,slideCache.Slide.Size.Width,slideCache.Slide.Size.Height,hValues.Columns,hValues.Rows));
					hValues.Save(slideCache.DataPath+"hValues.xml");
					eValues.Save(slideCache.DataPath+"eValues.xml");
				}
			}
		}
	}
}
