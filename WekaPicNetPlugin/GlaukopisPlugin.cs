/*
 * @author Sebastian Lohmann
 */
namespace Glaukopis.CognitionMasterIntegration {
	using System.Collections.Generic;
	using System.Linq;
	using SharpAccessory.CognitionMaster.Plugging;
	using SharpAccessory.Imaging.Segmentation;
	/// <summary>
	/// Erweiterung von <see cref="Plugin"/>
	/// </summary>
	public class GlaukopisPlugin:Plugin {
		/// <summary>
		/// fügt den übergebenen <see cref="ObjectLayer"/> hinzu
		/// </summary>
		/// <param name="layer"><see cref="ObjectLayer"/></param>
		/// <param name="replaceLayerWithSameName">falls true werden vorhandene <see cref="ObjectLayer"/>s mit gleichem Namen ersetzt</param>
		protected void addLayer(ObjectLayer layer,bool replaceLayerWithSameName=false){
			if(null==layer) return;
			var existentLayers=this.GetLayers().ToList();
			if(replaceLayerWithSameName){
				var oldLayers=existentLayers.Where(l=>layer.Name==l.Name).ToList();
				foreach(var oldLayer in oldLayers) existentLayers.Remove(oldLayer);
			}
			existentLayers.Add(layer);
			this.SetLayers(existentLayers.ToArray());
		}
		/// <summary>
		/// fügt die übergebenen <see cref="ObjectLayer"/>s hinzu
		/// </summary>
		/// <param name="layers"><see cref="ObjectLayer"/>s</param>
		/// <param name="replaceLayerWithSameName">falls true werden vorhandene <see cref="ObjectLayer"/>s mit gleichem Namen ersetzt</param>
		protected void addLayer(IEnumerable<ObjectLayer> layers,bool replaceLayerWithSameName=false){
			var existentLayers=this.GetLayers().ToList();
			var newLayers=layers.ToList();
			if(replaceLayerWithSameName){
				var oldLayers=new List<ObjectLayer>();
				foreach(var objectLayer in newLayers) oldLayers.AddRange(existentLayers.Where(l=>objectLayer.Name==l.Name));
				foreach(var oldLayer in oldLayers) existentLayers.Remove(oldLayer);
			}
			existentLayers.AddRange(newLayers);
			this.SetLayers(existentLayers.ToArray());
		}
		/// <summary>
		/// ersetzt die vorhandenen <see cref="ObjectLayer"/>s durch den übergebenen
		/// </summary>
		/// <param name="layer"><see cref="ObjectLayer"/></param>
		protected void replaceLayer(ObjectLayer layer){
			this.SetLayers(null==layer?null:new[]{layer});
		}
	}
}
