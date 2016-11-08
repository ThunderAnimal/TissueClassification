/*
 * @author Sebastian Lohmann
 */
namespace Glaukopis.CognitionMasterPlugins{
	using CognitionMasterIntegration;
	using SlideProcessing;
	using SharpAccessory.CognitionMaster.Plugging;
	using SharpAccessory.CognitionMaster.WholeSlideImageSupport;
	using System;
	using System.Windows.Forms;
	[PluginDefaultEnabled(true)]
	public class HeatMapViewerPlugin:GlaukopisPlugin{
		private SlideDataProvider slideDataProvider;
		private HeatMapDisplay heatMapDisplay;
		protected override void OnLoad(EventArgs e){
			base.OnLoad(e);
			this.CreateTabContainer("HeatMapViewer");
			this.TabContainer.Enabled=true;
			this.heatMapDisplay=new HeatMapDisplay{Parent=this.TabContainer,Dock=DockStyle.Fill,SelectorIsVisible=true};
		}
		protected override void OnReady(EventArgs e){
			base.OnReady(e);
			this.heatMapDisplay.BindNavigation();
		}
		protected override void OnImageFileOpened(EventArgs e){
			base.OnImageFileOpened(e);
			var slide=this.ImageFile as IWholeSlideImageFile;
			if(null==slide) return;
			this.slideDataProvider?.Dispose();
			this.slideDataProvider=new SlideDataProvider(slide);
			foreach(var heatMap in this.slideDataProvider.HeatMapDescriptions) this.heatMapDisplay.AddImage(heatMap.Name,heatMap.FileName);
		}
		protected override void OnImageFileClosed(EventArgs e){
			base.OnImageFileClosed(e);
			this.slideDataProvider?.Dispose();
			this.slideDataProvider=null;
			this.heatMapDisplay.Reset();
		}
	}
}