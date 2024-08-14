// windows.forms without vs designer challenge
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows;
using System.Net;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Media;
using System.Resources;

public partial class Garfield : Form
{
	// Core stuff

	public static HttpClient stripretriever;
	public static ResourceManager resources;
	public List<Comic> comics;
	public CurrentComicInfo currentcomic;
	public Gimmicks gimmicks;
	public List<string> taglines = new List<string>();
	private CancellationTokenSource ctk = new CancellationTokenSource();

	public Garfield()
	{
		resources = new ResourceManager("GarfSharp", typeof(Garfield).Assembly);
		taglines = (List<string>)resources.GetObject("taglines");
		try
		{
			string json = File.ReadAllText(@"strips.json");
			comics = JsonConvert.DeserializeObject<List<Comic>>(json);
		}
		catch (Exception suck)
		{
			MessageBox.Show(String.Format("Your strips.json is wrong.\n\n{0}\n\n...but I'll let you pass this time.", suck.ToString()), "UH OH IO!", MessageBoxButtons.OK, MessageBoxIcon.Error);
			comics = JsonConvert.DeserializeObject<List<Comic>>(resources.GetString("defaultStrips"));
		}

		// Core

		stripretriever = new HttpClient();
		currentcomic = new CurrentComicInfo(comics[0], comics[0].sources[0]);
		gimmicks = new Gimmicks();
		System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
		stripretriever.DefaultRequestHeaders.UserAgent.TryParseAdd("Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
		
		initialize_ui();

		strip_rando(null, null);
		strip_update(null, null);
	}

	private void strip_previous(object sender, EventArgs e)
	{
		try
		{
			date.Value = date.Value.AddDays(-currentcomic.comic.weekinfo.increment);
		}
		catch (ArgumentOutOfRangeException)
		{
			//let user know we are reaching oob
			SystemSounds.Beep.Play();
		}
	}
	private void strip_next(object sender, EventArgs e)
	{
		try
		{
			date.Value = date.Value.AddDays(currentcomic.comic.weekinfo.increment);
		}
		catch (ArgumentOutOfRangeException)
		{
			//let user know we are reaching oob
			SystemSounds.Beep.Play();
		}
	}

	// never heard of this approach
	// this is interesting
	// https://stackoverflow.com/questions/194863/random-date-in-c-sharp

	private void strip_rando(object sender, EventArgs e)
	{
		try
		{
			ComicSource comsrc = currentcomic.source;
			Comic com = currentcomic.comic;
			int r = (comsrc.getMaxDate(com) - comsrc.getMinDate(com)).Days;
			DateTime rd = comsrc.getMinDate(com).AddDays(new Random().Next(r));
			if(currentcomic.comic.weekinfo.dayofweek is int butt){
				date.Value = rd.AddDays(7-((int)rd.DayOfWeek-butt%7));
			}
			else{
				date.Value = rd;
			}
		}
		catch (ArgumentOutOfRangeException)
		{
			//Out of range? reroll again
			strip_rando(sender, e);
			//we dun care if argumentoutofrangeexception
		}
	}

	private void strip_save(object sender, EventArgs e)
	{
		SaveFileDialog savefile = new SaveFileDialog();
		savefile.InitialDirectory = @"C:\";
		savefile.DefaultExt = "gif";
		savefile.Filter = "GIF files (*.gif)|*.gif|PNG files (*.png)|*.png|JPEG files (*.jpg, *.jpeg)|*.jpg;*.jpeg|All files (*.*)|*.*";
		savefile.Title = "Save comic strip";
		savefile.FileName = String.Format(currentcomic.comic.fileName, date.Value);
		if (savefile.ShowDialog(this) == DialogResult.OK)
		{
			var extension = System.IO.Path.GetExtension(savefile.FileName);
			var format = System.Drawing.Imaging.ImageFormat.Gif;
			switch(extension.ToLower()){
				case ".bmp":
					format = System.Drawing.Imaging.ImageFormat.Bmp;
					break;
				case ".png":
					format = System.Drawing.Imaging.ImageFormat.Png;
					break;
				case ".jpeg":
				case ".jpg":
					format = System.Drawing.Imaging.ImageFormat.Jpeg;
					break;
				default:
					break;

			}
			System.IO.FileStream fs = (System.IO.FileStream)savefile.OpenFile();
			strip.Image.Save(fs, format);
		}
	}

	private void strip_copy(object sender, EventArgs e)
	{
		Clipboard.SetImage(strip.Image);
	}

	private async void strip_copyURL(object sender, EventArgs e)
	{
		Clipboard.SetData(DataFormats.Text, (Object)await currentcomic.source.fetchURL(stripretriever, ctk, date.Value));//(Object)String.Format(currentcomic.urlFormat, date.Value));
	}

	private async void strip_update(object sender, EventArgs e)
	{
		statusprogress.Visible = true;
		string penis = await currentcomic.source.fetchURL(stripretriever, ctk, date.Value); //String.Format(currentcomic.urlFormat, date.Value)
		Uri fuck;
		bool good = Uri.TryCreate(penis, UriKind.Absolute, out fuck);
		if(!good){
			Uri.TryCreate(Path.GetFullPath(penis), UriKind.Absolute, out fuck);
		}
		//why in the world does absoluteuri return a string and not an uri
		/*
		had to break my balls for this one. 
		none of the uri class methods don't even fucking support
		relative paths.
		*/
		Image img;
		if(fuck.IsFile){
			string path = fuck.LocalPath;
			try{
				FileStream fs = await Task.Run(() => File.OpenRead(path));//File.OpenRead(path); its asynchronous now lol
				img = Image.FromStream(fs, false, false);
			}
			catch (FileNotFoundException)
			{
				img = BitmapUtils.drawMessage("not found");
			}
			catch(IOException)
	        {
				img = BitmapUtils.drawMessage("i/o exception.\nyour file must be locked.", 36);
			}
		}
		else{
			HttpResponseMessage response = new HttpResponseMessage();
			try
			{
				//Stream stream = stripretriever.OpenRead(String.Format(currentcomic.urlFormat, date.Value));
				//ctk.Cancel();
				response = await currentcomic.source.fetch(penis, stripretriever, ctk);
				// response = await stripretriever.GetAsync(fuck, ctk.Token);
				response.EnsureSuccessStatusCode();
				Stream stream = await response.Content.ReadAsStreamAsync();
				img = Image.FromStream(stream, false, false);
			}
			catch (HttpRequestException)
			{
				//MessageBox.Show(suck.ToString());
				img = BitmapUtils.drawMessage(((int)(response.StatusCode)).ToString());
			}
			catch(ArgumentException)
			{
				img = BitmapUtils.drawMessage("an error occured while\nprocessing the image", 36);
			}
		}
		Bitmap bm = BitmapUtils.shittyCopy(img); // wish i could be using "using" here
		foreach(Gimmick gimmick in gimmicks.gimmicks){
			if(gimmick.enabled){
				img = gimmick.doIt(bm, img, currentcomic, date.Value);
				bm = BitmapUtils.shittyCopy(img);
				// just found out Bitmap extends Image. what a waste
				// THis is prone to exceptions and im not doing anything abouti t
			}
		}
		bm.Dispose();
		strip.Image = img;
		statusprogress.Visible = false;
		statusdate.Text = date.Value.ToString("d");
	}

	private void comic_update(object sender, EventArgs e, Comic comic, ComicSource source)
	{
		currentcomic = new CurrentComicInfo(comic, source);
		date.ResetBindings();
		date.Checked = true;
		//i keep getting argumentoutofrangeexceptions. lets try this
		date.MaxDate = DateTimePicker.MaximumDateTime;
		date.MinDate = DateTimePicker.MinimumDateTime;
		//reset mindate and maxdate values then set it again
		date.MaxDate = source.getMaxDate(comic);
		date.MinDate = source.getMinDate(comic);
		date.Value = date.Value;
		statuscomic.Text = String.Format("({0}) {1}", currentcomic.source.name, currentcomic.comic.name);
		strip_update(null, null);
	}

	private void strip_gimmick(object sender, EventArgs e, Gimmick gimmick){
		//i had a slightly better idea of doing this but csc hated it
		// well this is a slightly better idea of doing this now
		MenuItem gimmickItem = sender as MenuItem;
		if(gimmickItem==null) return; //wanted to add checks for tags too but i cant figure that out
		gimmickItem.Checked = !gimmickItem.Checked;
		gimmick.enabled = gimmickItem.Checked;
		strip_update(null, null);
	}

	protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
		switch (keyData)
		{
			case Keys.Right:
				strip_next(null, null);
				return true;
			case Keys.Left:
				strip_previous(null, null);
				return true;
			case Keys.R:
				strip_rando(null, null);
				return true;
		}
		return base.ProcessCmdKey(ref msg, keyData);
	}

	[STAThread]
	public static void Main()
	{
		Application.EnableVisualStyles();
		Application.Run(new Garfield());
	}
}
