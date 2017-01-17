/*
 * @author Sebastian Lohmann
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Glaukopis.SharpAccessoryIntegration.Segmentation;
using SharpAccessory.CognitionMaster.Plugging;
using SharpAccessory.Imaging.Automation;
using SharpAccessory.Imaging.Classification;
using SharpAccessory.Imaging.Segmentation;
using SharpAccessory.VisualComponents.Dialogs;

namespace Segmentation {
    [PluginDefaultEnabled(true)]
	class SegmentationPlugin:Plugin {
		private Func<ObjectLayer> _createCoreObjectLayer;
		private Func<ObjectLayer> _createLuminaObjectLayer;
		private Func<ObjectLayer> _createSimpleLuminaObjectLayer;
		private ObjectLayer _cellCores,_lumina,_simpleLumina, _newCellCores, _newLumina;

        private RichTextBox _debugBox;

        public void  LogOutput(String msg)
        {
            _debugBox.Text += "\n" + msg;
        }
        
        public void ClearOutput()
        {
            _debugBox.Text = "";
        } 

		protected override void OnLoad(EventArgs e){
			base.OnLoad(e);
			CreateTabContainer("Segmentation");
			TabContainer.Enabled=true;
            _debugBox = new RichTextBox { Text = "Debug Output\n==============================================", Parent = TabContainer, Dock = DockStyle.Fill, Enabled=true, ReadOnly=true };
            (new Button { Text="execute segmentations",Parent=TabContainer,Dock=DockStyle.Top }).Click+=delegate {
				if(null==DisplayedImage) return;
				var progressDialog=new ProgressDialog { Message="executing segmentations",ProgressBarStyle=ProgressBarStyle.Marquee,AllowCancel=false,BackgroundTask=()=> {
                    ClearOutput();
                    LogOutput("Calc Layers:");
					_cellCores=_createCoreObjectLayer();
                    //_lumina = _createLuminaObjectLayer();
                    _simpleLumina = _createSimpleLuminaObjectLayer();

                    LogOutput(ObjectLayerrUtils.GetFeatureValueList(_cellCores, "area").Count.ToString());
                    LogOutput("Calc center point cores...");
				    _newCellCores = ObjectLayerrUtils.SetClass(ObjectLayerrUtils.AddFeatureCenterPoint(ObjectLayerrUtils.AddFeatureFormFactor(_cellCores)),new Class("cellCores", Color.Red));
                    LogOutput("Calc cores in near for lumina");
                    _newLumina = (ObjectLayerrUtils.AddFeatureCoresInNear(ObjectLayerrUtils.AddFeatureFormFactor(_simpleLumina), _newCellCores));
                    
					SetLayers(new [] { _cellCores,/*_lumina,*/_simpleLumina, _newCellCores, _newLumina, ObjectLayerrUtils.MergeObjectLayers(_newLumina, _newCellCores)});
                    LogOutput("FINISH");
                }
                };
				progressDialog.CenterToScreen();
				progressDialog.ShowDialog();
			};
           
			_createCoreObjectLayer=()=>{
                LogOutput("Create Layer Cores");
				var process=new CellCoresHE();
				var processResult=process.Execute(new ProcessExecutionParams(DisplayedImage));
				var layer=processResult.Layers.Last();
                LogOutput("Add area and range");
			    layer = ObjectLayerrUtils.AddFeatureRange(ObjectLayerrUtils.AddFeatureArea(layer));
				layer.Name="cores";
				return layer;
			};
			_createLuminaObjectLayer=()=>{
                LogOutput("Create Layer Lumina");
                var process=new LuminaSegmentation();
				var processResult=process.Execute(new ProcessExecutionParams(DisplayedImage));
				var layer=processResult.Layers.Last();
                LogOutput("Add area and range");
                layer = ObjectLayerrUtils.AddFeatureRange(ObjectLayerrUtils.AddFeatureArea(layer));
                layer.Name="lumina";
				return layer;
			};
			_createSimpleLuminaObjectLayer=()=>{
                LogOutput("Create Layer simple Lumina");
                var segmentation=new SimpleLuminaSegmentation();
				var layer=segmentation.Execute(DisplayedImage);
                LogOutput("Add area and range");
                layer = ObjectLayerrUtils.AddFeatureRange(ObjectLayerrUtils.AddFeatureArea(layer));
                LogOutput("Improve Segmentaion Lumina");
                layer = segmentation.ImprObjectLayer(layer);
				layer.Name="simple lumina";
				return layer;
			};
		}
    }

}
