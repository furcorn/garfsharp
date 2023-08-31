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
		public Gimmick(string name, Func<Bitmap, Image, CurrentComicInfo, DateTime, Bitmap> callback){
			this.name = name;
			this.contentlabel = "&" + name;
			this.doIt = callback;
			this.enabled = false;
		}
		public readonly string name; //{ get; set; }
		public readonly string contentlabel; //{ get; set; }
		public bool enabled { get; set; }
		public Func<Bitmap, Image, CurrentComicInfo, DateTime, Bitmap> doIt { get; set; }
}

public class PanelReplace : Gimmick {
	
	public string replacePath;

	public PanelReplace(string name, string replacePath) : base(name){
			this.replacePath = replacePath;
			this.doIt = delegate(Bitmap bm, Image img, CurrentComicInfo curcomic, DateTime date){
				if(File.Exists(this.replacePath)){
					using(Bitmap pipeunscale = new Bitmap(this.replacePath)){
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

public class BorderCrop : Gimmick {

	public int top;
	public int left;
	public int bottom;
	public int right;

	public BorderCrop(string name, int top, int left, int bottom, int right) : base(name) {
		this.top = top;
		this.left = left;
		this.bottom = bottom;
		this.right = right;
		this.doIt = delegate(Bitmap bm, Image img, CurrentComicInfo curcomic, DateTime date){
			bm = new Bitmap(this.getWidth(img, curcomic, date), this.getHeight(img, curcomic, date));
			using(Graphics g = Graphics.FromImage(bm)){
				g.DrawImage(img, 0, 0, new RectangleF(new Point(this.getX(), this.getY()), bm.Size), GraphicsUnit.Pixel);
			}
			return bm;
		};
	}

	public virtual int getX(){
		return this.left;
	}

	public virtual int getY(){
		return this.top;
	}

	public virtual int getWidth(Image img, CurrentComicInfo curcomic, DateTime date){ // BorderCrop does not need curcomic nor date, but PanelCrop (which inherits BorderCrop) will need curcomic and HeathcliffCrop will need date
		return img.Width - this.right - this.left;
	}

	public virtual int getHeight(Image img, CurrentComicInfo curcomic, DateTime date){
		return img.Height - this.bottom - this.top;
	}
}

public class PanelCrop : BorderCrop {
	
	public int panels;

	public PanelCrop(string name, int panels) : base(name, 0, 0, 0, 0){
		this.panels = 2;
	}

	public override int getWidth(Image img, CurrentComicInfo curcomic, DateTime date){
		return (int)((float)img.Width*(((float)this.panels)/curcomic.comic.numPanels));
	}
}

public class SimpleCrop : BorderCrop {
	public int width;
	public int height;

	public SimpleCrop(string name, int x, int y, int width, int height) : base(name, x, y, 0, 0){
		this.width = width;
		this.height = height;
	}

	public override int getWidth(Image img, CurrentComicInfo curcomic, DateTime date){
		if(this.width > 0){
			return this.width;
		}
		else{
			return img.Width;
		}
	}

	public override int getHeight(Image img, CurrentComicInfo curcomic, DateTime date){
		if(this.height > 0){
			return this.height;
		}
		else{
			return img.Height;
		}
	}
}

public class HeathcliffCrop : SimpleCrop {

	public HeathcliffCrop(string name) : base(name, 0, 0, 0, 0) { }

	public override int getHeight(Image img, CurrentComicInfo curcomic, DateTime date){
		// yandev mode activate
		if(date.DayOfWeek == DayOfWeek.Sunday){
			return img.Height;
		}
		if(img.Height >= 660){
			return 660;
		}
		else if(img.Height >= 500){
			return 500;
		}
		else if(img.Height >= 330){
			return 330;
		}
		else{
			return img.Height;
		}
	}
}

public class Gimmicks {
	public Gimmicks()
	{
		Gimmick pipe = new PanelReplace("Pipe", "./resource/pipe.png");
		Gimmick deflated = new PanelReplace("Deflated", "./resource/deflated.png");			
		Gimmick window = new PanelReplace("Window", "./resource/window.png");
		Gimmick twopanel = new PanelCrop("Two panels", 2);
		Gimmick nocaption = new HeathcliffCrop("No caption");
		// Gimmick twopanel = new Gimmick("Two panels", delegate(Bitmap bm, Image img, CurrentComicInfo curcomic){
		// 	float width = (float)img.Width*(2.0f/curcomic.comic.numPanels); // JUST USE DECIMALS ALREADY!!! STOP DOING THE FUCKING THING IN INTEGERS
		// 	bm = new Bitmap((int)width, img.Height);
		// 	using(Graphics g = Graphics.FromImage(bm)){
		// 		g.DrawImage(img, 0, 0, new RectangleF(new Point(0, 0), bm.Size), GraphicsUnit.Pixel);
		// 	};
		// 	return bm;
		// }); 
		// Need a better way to do this.... it's just the same function but with different paths
		this.gimmicks = new List<Gimmick> {
			twopanel,
			pipe,
			deflated,
			window,
			nocaption
		};
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