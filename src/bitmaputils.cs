using System;
using System.Drawing;

class BitmapUtils {
	public static Bitmap shittyCopy(Image bitmap){
		Bitmap bm = new Bitmap(bitmap.Width, bitmap.Height);
		using(Graphics g = Graphics.FromImage(bm)){
			g.DrawImage(bitmap, 0, 0, new RectangleF(new Point(0, 0), new Size(bitmap.Width, bitmap.Height)), GraphicsUnit.Pixel);
		};
		return bm;
	}
	public static Image drawMessage(string message, int size = 96)
	{
		Image img = new Bitmap(1200, 350);
		Graphics graph = Graphics.FromImage(img);
		graph.Clear(Color.Gray);
		StringFormat idiot = new StringFormat();
		idiot.Alignment = StringAlignment.Center;
		idiot.LineAlignment = StringAlignment.Center;
		graph.DrawString(message, new Font("Arial", size), new SolidBrush(Color.FromArgb(48, 48, 48)), new PointF(600, 175), idiot);
		return img;
	}
}