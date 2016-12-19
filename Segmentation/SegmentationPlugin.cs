/*
 * @author Sebastian Lohmann
 */
namespace Segmentation {
	using SharpAccessory.CognitionMaster.Plugging;
	using System;
	using System.Linq;
	using System.Windows.Forms;
	using SharpAccessory.Imaging.Automation;
	using SharpAccessory.VisualComponents.Dialogs;
	using Glaukopis.SharpAccessoryIntegration.Segmentation;
	using SharpAccessory.Imaging.Segmentation;
	[PluginDefaultEnabled(true)]
	class SegmentationPlugin:Plugin {
		private Func<ObjectLayer> createCoreObjectLayer;
		private Func<ObjectLayer> createLuminaObjectLayer;
		private Func<ObjectLayer> createSimpleLuminaObjectLayer;
		private ObjectLayer cellCores,lumina,simpleLumina;
		protected override void OnLoad(EventArgs e){
			base.OnLoad(e);
			this.CreateTabContainer("Segmentation");
			this.TabContainer.Enabled=true;
			(new Button { Text="execute segmentations",Parent=this.TabContainer,Dock=DockStyle.Top }).Click+=delegate {
				if(null==this.DisplayedImage) return;
				var progressDialog=new ProgressDialog { Message="executing segmentations",ProgressBarStyle=ProgressBarStyle.Marquee,AllowCancel=false,BackgroundTask=()=> {
					this.cellCores=this.createCoreObjectLayer();
					this.lumina=this.createLuminaObjectLayer();
					this.simpleLumina=this.createSimpleLuminaObjectLayer();
					this.SetLayers(new [] { this.cellCores,this.lumina,this.simpleLumina });
				}};
				progressDialog.CenterToScreen();
				progressDialog.ShowDialog();
			};
			this.createCoreObjectLayer=()=>{
				var process=new CellCoresHE();
				var processResult=process.Execute(new ProcessExecutionParams(this.DisplayedImage));
				var layer=processResult.Layers.Last();
				layer.Name="cores";
				return layer;
			};
			this.createLuminaObjectLayer=()=>{
				var process=new LuminaSegmentation();
				var processResult=process.Execute(new ProcessExecutionParams(this.DisplayedImage));
				var layer=processResult.Layers.Last();
				layer.Name="lumina";
				return layer;
			};
			this.createSimpleLuminaObjectLayer=()=>{
				var segmentation=new SimpleLuminaSegmentation();
				var layer=segmentation.Execute(this.DisplayedImage);
				layer.Name="simple lumina";
				return layer;
			};
		}
	}
}
