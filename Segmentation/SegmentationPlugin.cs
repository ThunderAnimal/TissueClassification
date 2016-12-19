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
    using System.Collections.Generic;

    [PluginDefaultEnabled(true)]
	class SegmentationPlugin:Plugin {
		private Func<ObjectLayer> createCoreObjectLayer;
		private Func<ObjectLayer> createLuminaObjectLayer;
		private Func<ObjectLayer> createSimpleLuminaObjectLayer;
		private ObjectLayer cellCores,lumina,simpleLumina;

        private RichTextBox debugBox;

        public void  LogOutput(String Msg)
        {
            debugBox.Text += "\n" + Msg;
        }
        
        public void clearOutput()
        {
            debugBox.Text = "";
        } 

		protected override void OnLoad(EventArgs e){
			base.OnLoad(e);
			this.CreateTabContainer("Segmentation");
			this.TabContainer.Enabled=true;
            this.debugBox = new RichTextBox { Text = "Debug Output\n==============================================", Parent = this.TabContainer, Dock = DockStyle.Fill, Enabled=true, ReadOnly=true };
            (new Button { Text="execute segmentations",Parent=this.TabContainer,Dock=DockStyle.Top }).Click+=delegate {
				if(null==this.DisplayedImage) return;
				var progressDialog=new ProgressDialog { Message="executing segmentations",ProgressBarStyle=ProgressBarStyle.Marquee,AllowCancel=false,BackgroundTask=()=> {
                    clearOutput();
					this.cellCores=this.createCoreObjectLayer();
					this.lumina=this.createLuminaObjectLayer();
					this.simpleLumina=this.createSimpleLuminaObjectLayer();
                    this.addCoresCountInNear(simpleLumina, cellCores);
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

        public ObjectLayer addCoresCountInNear(ObjectLayer lumina, ObjectLayer cores)
        {
            const int near = 10;
            int maxX, maxY, minX, minY;

            this.LogOutput("Calc Cores near Lumina....");
            this.LogOutput("Map Width: " + cores.Map.Width.ToString());
            this.LogOutput("Map Height: " + cores.Map.Width.ToString());
            this.LogOutput("Near Tolleranz: " + near.ToString());
            this.LogOutput("");

            this.LogOutput("Anzahl cellCores All: " + cores.Objects.Count);

            //In neue LIste überführen um das Feature Zelleker nerbby zu speichern
            List<ImageObjectLumina> luminaList = new List<ImageObjectLumina>();
            foreach (ImageObject luminaImageObject in lumina.Objects.AsEnumerable<ImageObject>())
            {
                luminaList.Add(new ImageObjectLumina(luminaImageObject.Id, luminaImageObject.Contour, luminaImageObject.Class));
            }
                 
            //Ermittlung der Zellkern Nachbarn       
            foreach(ImageObject cellImageObject in cores.Objects.AsEnumerable<ImageObject>())
            {
                foreach(System.Drawing.Point point in cellImageObject.Contour.GetPoints())
                {
                    minX = point.X - near;
                    minY = point.Y - near;
                    maxX = point.X + near;
                    maxY = point.Y + near;

                    if (point.X - near < 0)
                        minX = 0;
                    if (point.Y - near < 0)
                        minY = 0;
                    if (point.X + near > lumina.Map.Width)
                        maxX = lumina.Map.Width;
                    if (point.Y + near > lumina.Map.Height)
                        maxY = lumina.Map.Height;

                    //Find Cores in MAP
                    for (int x = minX; x < maxX; x++)
                    {
                        for (int y = minY; y < maxY; y++)
                        {
                            if(lumina.Map[x,y] != 0)
                            {
                               foreach(ImageObjectLumina luminaImageObject in luminaList)
                                {
                                    if(luminaImageObject.Id == lumina.Map[x, y])
                                    {
                                        luminaImageObject.addNearCellCores(cellImageObject);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            foreach (ImageObjectLumina luminaImageObject in luminaList)
            {
                this.LogOutput(luminaImageObject.Id.ToString() + " Count near Cellcores: " + luminaImageObject.countNearCellCores());
            }

            return lumina;
        }
    }
}
