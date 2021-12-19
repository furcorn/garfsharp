using System;
using System.Drawing;
using System.Collections.Generic;
using System.IO;

public class Gimmick {
		public Gimmick(string name){
			this.name = name;
			this.contentlabel = "&" + name;
			this.enabled = false;
		}
		public Gimmick(string name, string label){
			this.name = name;
			this.contentlabel = label;
			this.enabled = false;
		}
		public Gimmick(string name, Func<Bitmap, Image, CurrentComicInfo, Bitmap> callback){
			this.name = name;
			this.contentlabel = "&" + name;
			this.doIt = callback;
			this.enabled = false;
		}
		public readonly string name; //{ get; set; }
		public readonly string contentlabel; //{ get; set; }
		public bool enabled { get; set; }
		public Func<Bitmap, Image, CurrentComicInfo, Bitmap> doIt { get; set; }
}

public class PanelReplace : Gimmick {
	public PanelReplace(string name, string replacePath) : base(name){
			this.doIt = delegate(Bitmap bm, Image img, CurrentComicInfo curcomic){
				string path = replacePath; //I don't want to pack it as a resource so its in a folder
				if(File.Exists(path)){
					using(Bitmap pipeunscale = new Bitmap(path)){
						int width = img.Width/(curcomic.comic.numPanels);
						if(img.Width <= 800){
							width = img.Width/2; // Two panel must be enabled
						}
						//img.Width>=1195&&img.Width<=1205?pipeunscale.Width:(int)(img.Width/3);
						Bitmap pipebm = new Bitmap(pipeunscale, new Size(width, img.Height));
						using(Graphics g = Graphics.FromImage(bm)){
							g.DrawImage(bm, 0, 0, new RectangleF(new Point(0, 0), new Size(bm.Width,bm.Height)), GraphicsUnit.Pixel);
							g.DrawImage(pipebm, bm.Width-pipebm.Width, 0, new RectangleF(new Point(0, 0), new Size(pipebm.Width,pipebm.Height)), GraphicsUnit.Pixel);
						};
						pipebm.Dispose();
					}
				}
				return bm;
			}; 
	}
}

public class Gimmicks {
	public Gimmicks()
	{
		Gimmick pipe = new PanelReplace("Pipe", "./resource/pipe.png");
		Gimmick deflated = new PanelReplace("Deflated", "./resource/deflated.png");			
		Gimmick window = new PanelReplace("Window", "./resource/window.png");
		Gimmick twopanel = new Gimmick("Two panels", delegate(Bitmap bm, Image img, CurrentComicInfo curcomic){
			float width = (float)img.Width*(2.0f/curcomic.comic.numPanels); // JUST USE DECIMALS ALREADY!!! STOP DOING THE FUCKING THING IN INTEGERS
			bm = new Bitmap((int)width, img.Height);
			using(Graphics g = Graphics.FromImage(bm)){
				g.DrawImage(img, 0, 0, new RectangleF(new Point(0, 0), bm.Size), GraphicsUnit.Pixel);
			};
			return bm;
		}); 
		// Need a better way to do this.... it's just the same function but with different paths
		this.gimmicks = new List<Gimmick> {twopanel, pipe, deflated, window};
	}
	public bool AtLeastOne(){
		foreach(Gimmick gimmick in this.gimmicks){
			if(gimmick.enabled){
				return true;
			}
		}
		return false;
	}
	public List<Gimmick> gimmicks { get; set; }
}