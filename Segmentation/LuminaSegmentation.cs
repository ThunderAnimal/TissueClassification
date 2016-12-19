/*
 * @author Stephan Wienert, Sebastian Lohmann
 */
//#reference System.dll
//#reference System.Drawing.dll
//#reference System.Windows.Forms.dll

//#reference #Accessory.dll
//#reference #Accessory.Imaging.dll
//#reference #Accessory.CognitionMaster.exe
//#reference #Accessory.CognitionMaster.DefaultPlugins.dll

using System;
using System.Drawing;
using System.Collections.Generic;

using SharpAccessory.Threading;
using SharpAccessory.VisualComponents;

using SharpAccessory.Imaging.Filters;
using SharpAccessory.Imaging.Automation;
using SharpAccessory.Imaging.Processors;
using SharpAccessory.Imaging.Segmentation;
using SharpAccessory.Imaging.Classification;
using SharpAccessory.Imaging.Classification.Features.Color;
using SharpAccessory.Imaging.Classification.Features.Gradient;

//using SharpAccessory.CognitionMaster.DefaultPlugins;


public class LuminaSegmentation:LocalProcessBase
{
	private const int MAX_CONTOURLENGTH=1000;


	public LuminaSegmentation() : base("lumina") { }
	
	
	public override ProcessResult Execute(ProcessExecutionParams p)
	{
		GrayscaleProcessor gp=new GrayscaleProcessor(new Bitmap(p.Bitmap), RgbToGrayscaleConversion.None);
		
		BitmapProcessor bp=new BitmapProcessor(p.Bitmap.Clone() as Bitmap);
		
		for(int dy=0; dy<gp.Height; dy++) for(int dx=0; dx<gp.Width; dx++)
		{
			int pixel=(int)bp.GetPixel(dx, dy);
			
			int r=(pixel&0x00ff0000)>>16;
			int g=(pixel&0x0000ff00)>>08;
			int b=(pixel&0x000000ff)>>00;
			
			int max=Math.Max(Math.Max(r, g), b);
			int min=Math.Min(Math.Min(r, g), b);
			
			int range=max-min;
			
			int level=(r+g+b)/3;
			
			int val=level-range;
			
			if(val>255) val=255;
			if(val<0) val=0;
			
			gp.SetPixel(dx, dy, (UInt32)level);
		}
		
		GrayscaleProcessor gpSobel=(GrayscaleProcessor)gp.Clone();
		
		new SobelEdgeDetector().Execute(gpSobel);
		
		ObjectLayer l1stLevel=Execute1stLevelSegmentation(gp, gpSobel);
		
		ObjectLayer l2ndLevel=Execute2ndLevelSegmentation(l1stLevel, gp);
		
		gpSobel.WriteBack=false;
		gp.WriteBack=false;
		
		gpSobel.Dispose();
		gp.Dispose();
		bp.Dispose();
		
		gpSobel.Bitmap.Dispose();
		gp.Bitmap.Dispose();
		bp.Bitmap.Dispose();
		
		l1stLevel.Name="1st Level";
		l2ndLevel.Name="2nd Level";
		
		return new ProcessResult(new ObjectLayer[]{l1stLevel, l2ndLevel});
	}
	
	
	private ObjectLayer Execute2ndLevelSegmentation(ObjectLayer l1stLevel, GrayscaleProcessor p)
	{
		ObjectLayer layer=l1stLevel.CreateAbove(obj=>
		{
			return true;
		});
		
		double[] background=GetBackgroundHistogram(layer, p);
		double[] foreground=GetForegroundHistogram(layer, p);
		
		while(true)
		{
			bool removed=false;
			
			layer=layer.CreateAbove(obj=>
			{
				int[] histogram=GetHistogram(obj, p);
				
				double ratioForeground=GetRatioForeground(histogram, foreground, background);
				
				if(ratioForeground>0.5) return true;
				
				for(int i=0; i<256; i++)
				{
					int val=histogram[i];
					
					background[i]+=val;
					foreground[i]-=val;
				}
				
				removed=true;
				
				return false;
			});
			
			if(!removed) break;
		}
		
		return layer;
	}
	
	
	private double GetRatioForeground(int[] histogram, double[] foreground, double[] background)
	{
		double sumForeground=0.0;
		double sumBackground=0.0;
		
		for(int i=0; i<256; i++)
		{
			int value=histogram[i];
			
			double cleanForeground=foreground[i]-value;
			
			if(cleanForeground>background[i])
			{
				sumForeground+=value;
			}
			else sumBackground+=value;
		}
		
		return sumForeground/(sumForeground+sumBackground);
	}
	
	
	private int[] GetHistogram(ImageObject obj, GrayscaleProcessor p)
	{
		Rectangle boundingBox=obj.Contour.FindBoundingBox();
		
		int y2=boundingBox.Bottom;
		int x2=boundingBox.Right;
		int y1=boundingBox.Y;
		int x1=boundingBox.X;
		
		int[] histogram=new int[256];
		
		for(int dy=y1; dy<y2; dy++) for(int dx=x1; dx<x2; dx++)
		{
			if(obj.Layer.Map[dx, dy]!=obj.Id) continue;
			
			histogram[p[dx, dy]]++;
		}
		
		return histogram;
	}
	
	
	private double[] GetForegroundHistogram(ObjectLayer layer, GrayscaleProcessor p)
	{
		double[] histogram=new double[256];
		
		for(int dy=0; dy<p.Height; dy++) for(int dx=0; dx<p.Width; dx++)
		{
			if(layer.Map[dx, dy]==0) continue;
			
			histogram[p[dx, dy]]++;
		}
		
		return histogram;
	}
	
	
	private double[] GetBackgroundHistogram(ObjectLayer layer, GrayscaleProcessor p)
	{
		double[] histogram=new double[256];
		
		for(int dy=0; dy<p.Height; dy++) for(int dx=0; dx<p.Width; dx++)
		{
			if(layer.Map[dx, dy]!=0) continue;
			
			histogram[p[dx, dy]]++;
		}
		
		return histogram;
	}
	
	
	private ObjectLayer Execute1stLevelSegmentation(GrayscaleProcessor p, GrayscaleProcessor pSobel)
	{
		ContourBasedSegmentation cbs=new ContourBasedSegmentation();
		
		cbs.CreatePrimarySegmentation(p, MAX_CONTOURLENGTH);
		
		cbs.EvaluateContours(pSobel);
		
		ObjectLayer layer=cbs.CreateLayer();
		
		layer=layer.CreateAbove(obj=>
		{
			return GetContourGradient(obj, p)>0;
		});
		
		return layer;
	}
	
	
	private double GetContourGradient(ImageObject obj, GrayscaleProcessor p)
	{
		int[] nX={ +1, 0, -1, 0 };
		int[] nY={ 0, +1, 0, -1 };
		
		int height=p.Height;
		int width=p.Width;
		
		double sumInner=0.0, sumOuter=0.0;
		double numInner=0.0, numOuter=0.0;
		
		for(int i=0; i<obj.Contour.Length; i++)
		{
			Point pI=obj.Contour[i];
			
			for(int j=0; j<4; j++)
			{
				int x=pI.X+nX[j];
				int y=pI.Y+nY[j];
				
				if(x<0 || y<0 || x>=width || y>=height) continue;
				
				if(obj.Contour.Contains(x, y)) continue;
				
				UInt32 intensity=p.GetPixel(x, y);
				
				if(obj.Layer.Map[x, y]==obj.Id)
				{
					sumInner+=intensity;
					numInner++;
				}
				else
				{
					sumOuter+=intensity;
					numOuter++;
				}
			}
		}
		
		double meanInner=sumInner/numInner;
		double meanOuter=sumOuter/numOuter;
		
		return meanInner-meanOuter;
	}

}