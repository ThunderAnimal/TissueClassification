/*
 * @author Sebastian Lohmann
 */
namespace WekaPicNet{
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.IO;
	using System.Linq;
	using System.Windows.Forms;
	using Glaukopis.Adapters.PicNetML;
	using Glaukopis.CognitionMasterIntegration;
	using SharpAccessory;
	using SharpAccessory.CognitionMaster.Plugging;
	using SharpAccessory.Imaging.Classification;
	using SharpAccessory.Imaging.Classification.Features.Color;
	using SharpAccessory.Imaging.Classification.Features.Size;
	using SharpAccessory.Imaging.Filters;
	using SharpAccessory.Imaging.Processors;
	using SharpAccessory.Imaging.Segmentation;
	using SharpAccessory.VisualComponents.Dialogs;
	using weka.classifiers;
	using weka.core;
	[PluginDefaultEnabled(true)]
	class WekaPicNetPlugin:GlaukopisPlugin{
		private readonly Dictionary<string,Class> classes=new Dictionary<string,Class>{
			{"nuclei",new Class("nuclei",Color.BlueViolet)},
			{"conglomerate",new Class("conglomerate",Color.Orange)},
			{"void",new Class("void",Color.DarkGray)},
		};
		private readonly List<string> featureNames=new List<string>{"MeanIntensity","AreaOfContour"};
		private Instances dataSet;
		private Classifier classifier;
		protected override void OnLoad(EventArgs e){
			base.OnLoad(e);
			Accessory.RedirectAssemblyBinding(@"d:\_UNI\TissueClassification\packages\PicNetML.0.0.16\lib\net45\");
			this.CreateTabContainer("Weka-PicNet");
			this.TabContainer.Enabled=true;
			(new Button{Text="apply classifier",Parent=this.TabContainer,Dock=DockStyle.Top}).Click+=delegate{
				var progressDialog=new ProgressDialog{
					Message="executing",ProgressBarStyle=ProgressBarStyle.Marquee,AllowCancel=false,BackgroundTask=()=>{
						if(null==this.classifier||null==this.dataSet)
							if(!this.loadClassifier()) return;
						if(null==this.SelectedLayer){
							if(null==this.DisplayedImage) return;
							this.addLayer(createLayer(this.DisplayedImage),true);
						}
						var layer=this.SelectedLayer.CreateAbove(io=>true);
						foreach(var io in layer.Objects) this.classifier.Classify(this.dataSet,io,n=>this.classes[n]);
						layer.Name=layer.Name+" classified";
						this.addLayer(layer,true);
					}
				};
				progressDialog.CenterToScreen();
				progressDialog.ShowDialog();
			};
			(new Button{Text="load classifier",Parent=this.TabContainer,Dock=DockStyle.Top}).Click+=delegate {this.loadClassifier();};
			(new Button{Text="save and clear data set",Parent=this.TabContainer,Dock=DockStyle.Top}).Click+=delegate{
				if(null==this.dataSet) return;
				var progressDialog=new ProgressDialog{
					Message="executing",ProgressBarStyle=ProgressBarStyle.Marquee,AllowCancel=false,BackgroundTask=()=>{
						using(var saveFileDialog=new SaveFileDialog{Filter="WEKA Attribute-Relation File (*.arff)|*.arff"}) if(DialogResult.OK==saveFileDialog.ShowDialog()) this.dataSet.SaveAsWekaArff(saveFileDialog.FileName);
						this.dataSet=null;
					}
				};
				progressDialog.CenterToScreen();
				progressDialog.ShowDialog();
			};
			(new Button{Text="add ground truth to data set",Parent=this.TabContainer,Dock=DockStyle.Top}).Click+=delegate{
				if(null==this.SelectedLayer) return;
				var progressDialog=new ProgressDialog{
					Message="executing",ProgressBarStyle=ProgressBarStyle.Marquee,AllowCancel=false,BackgroundTask=()=>{
						if(null==this.dataSet) this.dataSet=Util.CreateInstances(this.classes.Keys,this.featureNames);
						this.dataSet.AddImageObjects(this.SelectedLayer);
					}
				};
				progressDialog.CenterToScreen();
				progressDialog.ShowDialog();
			};
			(new Button{Text="classify layer by ground truth",Parent=this.TabContainer,Dock=DockStyle.Top}).Click+=delegate{
				if(null==this.DisplayedImage) return;
				if(null==this.SelectedLayer) return;
				var roiFileName=this.ImageFile.Url+".roi";
				if(!File.Exists(roiFileName)) return;
				var progressDialog=new ProgressDialog{
					Message="executing",ProgressBarStyle=ProgressBarStyle.Marquee,AllowCancel=false,BackgroundTask=()=>{
						var learningSampleFile=LearningSampleFile.FromFile(roiFileName);
						var groundTruthPositives=learningSampleFile.Samples.Select(sample=>new Point((int)sample.Features["x0"].Value,(int)sample.Features["y0"].Value)).ToList();
						var io2GroundTruth=new Dictionary<uint,int>();
						foreach(var p in groundTruthPositives){
							var id=this.SelectedLayer.Map[p.X,p.Y];
							if(!io2GroundTruth.ContainsKey(id)) io2GroundTruth.Add(id,0);
							io2GroundTruth[id]++;
						}
						foreach(var io in this.SelectedLayer.Objects){
							if(io2GroundTruth.ContainsKey(io.Id)) io.Class=1==io2GroundTruth[io.Id]?this.classes["nuclei"]:this.classes["conglomerate"];
							else io.Class=this.classes["void"];
						}
					}
				};
				progressDialog.CenterToScreen();
				progressDialog.ShowDialog();
				this.RefreshBuffer();
			};
			(new Button{Text="create layer",Parent=this.TabContainer,Dock=DockStyle.Top}).Click+=delegate{
				if(null==this.DisplayedImage) return;
				var progressDialog=new ProgressDialog{Message="executing",ProgressBarStyle=ProgressBarStyle.Marquee,AllowCancel=false,BackgroundTask=()=>this.addLayer(createLayer(this.DisplayedImage),true)};
				progressDialog.CenterToScreen();
				progressDialog.ShowDialog();
			};
		}
		private bool loadClassifier(){
			using(var openFileDialog=new OpenFileDialog{Filter="weka model file (*.model)|*.model|all files (*.*)|*.*"}) if(DialogResult.OK==openFileDialog.ShowDialog()) this.classifier=(Classifier)SerializationHelper.read(openFileDialog.FileName);
			using(var openFileDialog=new OpenFileDialog{Filter="WEKA Attribute-Relation File (*.arff)|*.arff"}) if(DialogResult.OK==openFileDialog.ShowDialog()) this.dataSet=Util.LoadInstancesFromWekaArff(openFileDialog.FileName);
			return null!=this.classifier&&null!=this.dataSet;
		}
		private static ObjectLayer createLayer(Bitmap source){
			var minContourLength=20;
			var maxContourLength=200;
			var contourBasedSegmentation=new ContourBasedSegmentation();
			using(var grayscaleProcessor=new GrayscaleProcessor(source,RgbToGrayscaleConversion.Mean){WriteBack=false}){
				contourBasedSegmentation.CreatePrimarySegmentation(grayscaleProcessor, maxContourLength, true);
				new SobelEdgeDetector().Execute(grayscaleProcessor);
				contourBasedSegmentation.EvaluateContours(grayscaleProcessor);
			}
			var layer=contourBasedSegmentation.CreateLayer(minContourLength,maxContourLength);
			layer.Name="Contour Based Segmentation (minContourLength="+minContourLength+", maxContourLength="+maxContourLength+")";
			calculateFeatures(layer,source);
			return layer;
		}
		private static void calculateFeatures(ObjectLayer objectLayer,Bitmap source){
			using(var grayscaleProcessor=new GrayscaleProcessor(source,RgbToGrayscaleConversion.Mean){WriteBack=false}) MeanIntensity.ProcessLayer(objectLayer,grayscaleProcessor);
			foreach(var imageObject in objectLayer.Objects) imageObject.Features.Add(new AreaOfContour(ContourProperties.FromContour(imageObject.Contour)));
		}
	}
}