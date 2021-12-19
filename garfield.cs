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
using System.Reflection;
using System.Text.RegularExpressions;
using System.Media;
public class Garfield : Form
{
	/*public class Comic
	{
		public Comic(string name, DateTime minDate, DateTime maxDate, string urlFormat, string fileName, int dayIncrement)
		{
			this.name = name;
			this.minDate = minDate;
			this.maxDate = maxDate;
			this.urlFormat = urlFormat;
			this.fileName = fileName;
		}
		[JsonConstructor]
		public Comic(string name, string minDate, string maxDate, string urlFormat, string fileName, int dayIncrement, int? weekday, bool isHtml, string realUrlRegex, int realMatchGroup, int numPanels)
		{
			this.name = name;
			this.minDate = DateTime.Parse(minDate);
			this.maxDate = DateTime.Parse(maxDate ?? DateTime.Now.ToString());
			this.urlFormat = urlFormat;
			this.fileName = fileName;
			this.dayIncrement = dayIncrement;
			this.weekday = weekday;
			this.isHtml = isHtml;
			this.realUrlRegex = realUrlRegex;
			this.realMatchGroup = realMatchGroup;
			this.numPanels = numPanels;
		}
		public readonly string name; //{ get; set; }
		public readonly DateTime minDate; //{ get; set; }
		public readonly DateTime maxDate; //{ get; set; }
		public readonly string urlFormat; //{ get; set; }
		public readonly string fileName; //{ get; set; }

		[DefaultValue(false)]
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
		public readonly bool isHtml; //{ get; set; }

		[DefaultValue(1)]
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
		public readonly int dayIncrement; //{ get; set; }

		[DefaultValue(null)]
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public readonly int? weekday; //{ get; set; }

		[DefaultValue("")]
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public readonly string realUrlRegex; //{ get; set; }

		[DefaultValue(0)]
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public readonly int realMatchGroup; //{ get; set; }
		
		[DefaultValue(3)]
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public readonly int numPanels; //{get; set;}

		public async Task<HttpResponseMessage> fetch(string url, HttpClient http, CancellationTokenSource ctk){
			Uri fuck; //There should be a function on this this code is being used twice
			bool good = Uri.TryCreate(url, UriKind.Absolute, out fuck);
			if(good == false){
				Uri.TryCreate(Path.GetFullPath(url), UriKind.Absolute, out fuck);
			}
			HttpResponseMessage response = new HttpResponseMessage();
			response = await http.GetAsync(fuck, ctk.Token);
			response.EnsureSuccessStatusCode();
			return response; //Further processing is done below and below 
			// if(readAsString){

			// }
			// else{
			// 	Stream stream = await response.Content.ReadAsStreamAsync();
			// }
		}

		public async Task<string> fetchURL(HttpClient http, CancellationTokenSource ctk, DateTime date){ //For HTML
			string url = String.Format(this.urlFormat, date);
			if(this.isHtml){
				HttpResponseMessage response = await this.fetch(url, http, ctk);
				string htmldata = await response.Content.ReadAsStringAsync();
				Match match = Regex.Match(htmldata, this.realUrlRegex);
				return match.Groups[this.realMatchGroup].ToString();
			}
			else{
				return url;
			}
		} 
	}*/
	public class CurrentComicInfo{
		public CurrentComicInfo(Comic comic, ComicSource source){
			this.comic = comic;
			this.source = source;
		}
		public Comic comic;
		public ComicSource source;
	}
	public class WeekInfo{
		[JsonConstructor]
		public WeekInfo(int increment, int dayofweek){
			this.increment = increment;
			this.dayofweek = dayofweek;
		}
		public WeekInfo(){
			this.increment = 1;
			this.dayofweek = null;
		}
		[DefaultValue(1)]
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
		public readonly int increment;
		[DefaultValue(null)]
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
		public readonly int? dayofweek = null;
	}
	public class RegexInfo{ //i feel like a class is a bit overkill but thats fine : )
		[JsonConstructor]
		public RegexInfo(string expression, int group){
			this.expression = expression;
			this.group = group;
		}
		public readonly string expression;
		[DefaultValue(0)]
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
		public readonly int group;

		public new string ToString(){
			return String.Format("{0} {1}", this.expression, this.group);
		}//this exists solely for debugging reasons
	}
	public class ComicSource{
		[JsonConstructor]
		public ComicSource(string name, string urlFormat, string? minDate, string? maxDate, bool isHtml, RegexInfo regex){
			this.name = name;
			this.urlFormat = urlFormat;
			if(minDate is string fuck){
				this.minDate = DateTime.Parse(fuck);
			}
			if(maxDate is string fuckdos){
				this.maxDate = DateTime.Parse(fuckdos);
			}
			this.isHtml = isHtml;
			this.regex = regex;
		}
		/*public ComicSource(ComicSource dest, Comic source){ //wish therew as a better way to do this but im sticking with this for now...
			this.name = dest.name;
			this.urlFormat = dest.urlFormat;
			this.minDate = dest.minDate ?? source.minDate;
			this.maxDate = (dest.maxDate ?? source.maxDate) ?? DateTime.Now;
			this.isHtml = dest.isHtml;
			this.regex = dest.regex;
		}*/
		public readonly string name;
		public readonly string urlFormat;
		public readonly DateTime? minDate;
		public readonly DateTime? maxDate;
		[DefaultValue(false)]
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
		public readonly bool isHtml;
		public readonly RegexInfo regex;
		
		public DateTime getMinDate(Comic comic){
			return this.minDate ?? comic.minDate;
		}
		
		public DateTime getMaxDate(Comic comic){
			return (this.maxDate ?? comic.maxDate) ?? DateTime.Now;
		}

		public async Task<HttpResponseMessage> fetch(string url, HttpClient http, CancellationTokenSource ctk){
			Uri fuck; //There should be a function on this this code is being used twice
			bool good = Uri.TryCreate(url, UriKind.Absolute, out fuck);
			if(good == false){
				Uri.TryCreate(Path.GetFullPath(url), UriKind.Absolute, out fuck);
			}
			HttpResponseMessage response = new HttpResponseMessage();
			response = await http.GetAsync(fuck, ctk.Token);
			response.EnsureSuccessStatusCode();
			return response; //Further processing is done below and below 
			// if(readAsString){

			// }
			// else{
			// 	Stream stream = await response.Content.ReadAsStreamAsync();
			// }
		}

		public async Task<string> fetchURL(HttpClient http, CancellationTokenSource ctk, DateTime date){ //For HTML
			string url = String.Format(this.urlFormat, date);
			if(this.isHtml){
				HttpResponseMessage response = await this.fetch(url, http, ctk);
				string htmldata = await response.Content.ReadAsStringAsync();
				Match match = Regex.Match(htmldata, this.regex.expression);
				return match.Groups[this.regex.group].ToString();
			}
			else{
				return url;
			}
		} 
	}
	public class Comic{
		[JsonConstructor]
		public Comic(string name, int numPanels, string minDate, string? maxDate, string fileName, List<ComicSource> sources, WeekInfo? weekinfo){
			this.name = name;
			this.numPanels = numPanels;
			this.minDate = DateTime.Parse(minDate);
			this.maxDate = DateTime.Parse(maxDate ?? DateTime.Now.ToString());
			this.fileName = fileName;
			this.sources = sources;
			this.weekinfo = weekinfo ?? new WeekInfo();
		}
		public readonly string name;
		[DefaultValue(3)]
		[JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
		public readonly int numPanels;
		public readonly DateTime minDate;
		public readonly DateTime? maxDate;
		public readonly string fileName;
		public readonly List<ComicSource> sources;
		public readonly WeekInfo weekinfo;
	}
	public class Gimmick
	{
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
	public class PanelReplace : Gimmick
	{
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
	public class Gimmicks
	{
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
	public TableLayoutPanel panel;
	public TableLayoutPanel picker;
	public Button previous;
	public DateTimePicker date;
	public Button next;
	public PictureBox strip;
	public static HttpClient stripretriever;
	public ContextMenuStrip stripmenu;
	public StatusStrip status;
	public ToolStripStatusLabel statuscomic;
	public ToolStripStatusLabel statusdate;
	public ToolStripProgressBar statusprogress;
	public MenuStrip menu;
	public List<Comic> comics;
	public CurrentComicInfo currentcomic;
	public Gimmicks gimmicks;
	public string[] taglines = new string[] {
		"now with 15% more C#!",
		"just like the web verison, but standalone!",
		"featuring U.S. Acres!",
		"now with 50% less WinForms Designer!",
		"part of the WinForms without Designer challenge!",
		"now with 50% more random taglines!",
		"from the same author of HTML5 Strong Sad's Lament!",
		"because garfield.com was shot dead!",
		"watch Wade Duck tear a tag off of a pillow!",
		"because I can!",
		"now with 75% more CSC!",
		"featuring shitty code!",
		"now with Funny Ideas to spice up the comic!",
		"now with more classes and lambda expressions!"
	};
	private CancellationTokenSource ctk = new CancellationTokenSource();
	public Bitmap shittyCopy(Image bitmap){
		Bitmap bm = new Bitmap(bitmap.Width, bitmap.Height);
		using(Graphics g = Graphics.FromImage(bm)){
			g.DrawImage(bitmap, 0, 0, new RectangleF(new Point(0, 0), new Size(bitmap.Width, bitmap.Height)), GraphicsUnit.Pixel);
		};
		return bm;
	}
	ToolStripMenuItem file;
	ToolStripMenuItem comic;
	ToolStripMenuItem gimmick;
	List<ToolStripMenuItem> gimmickMenus = new List<ToolStripMenuItem>();
	ToolStripMenuItem change;
	ToolStripMenuItem save;
	ToolStripMenuItem copy;
	ToolStripMenuItem copyURL;
	ToolStripMenuItem nextstrip;
	ToolStripMenuItem previousstrip;
	ToolStripMenuItem gorando;
	ToolStripMenuItem exit;
	public Garfield()
	{
		try
		{
			string json = File.ReadAllText(@"strips.json");
			comics = JsonConvert.DeserializeObject<List<Comic>>(json);
		}
		catch (Exception suck)
		{
			MessageBox.Show(String.Format("Your strips.json is wrong.\n\n{0}\n\n...but I'll let you pass this time.", suck.ToString()), "UH OH IO!", MessageBoxButtons.OK, MessageBoxIcon.Error);
			comics = JsonConvert.DeserializeObject<List<Comic>>(@"[
				{
					'name': 'Garfield',
					'numPanels': 3,
					'minDate': '1978-06-19',
					'fileName': '{0:yyyy-MM-dd}.gif',
					'sources': [
						// These are ComicSources. It contains information of the name of the source, the URL format, whether or not it's an HTML and regex for the data.
						// maxDate and minDate will override the top-layer date settings
						{
							'name': 'GoComics',
							'urlFormat': 'https://www.gocomics.com/garfield/{0:yyyy}/{0:MM}/{0:dd}',
							'isHtml': true,
							// This is a RegexInfo. It simply contains the expression and group to tell the program how to pull the comic image URL from the HTML data.
							'regex': {
								'expression': 'item-comic-image.*data-srcset=\""(.*?) (.*?)(,|\\"")',
								'group': 1
							}
						},
						{
							'name': 'the-eye',
							'urlFormat': 'https://the-eye.eu/public/Comics/Garfield/{0:yyyy-MM-dd}.gif',
							'maxDate': '2020-07-21'
						},
						{
							'name': 'Uclick',
							'minDate': '1978-06-19',
							'urlFormat': 'http://images.ucomics.com/comics/ga/{0:yyyy}/ga{0:yyMMdd}.gif'
						},
						{
							'name': 'archive.org',
							'urlFormat': 'https://web.archive.org/web/2019id_/https://d1ejxu6vysztl5.cloudfront.net/comics/garfield/{0:yyyy}/{0:yyyy-MM-dd}.gif',
							'maxDate': '2020-07-21'
						}
						
					]
				}
			]");
		}
		file = new ToolStripMenuItem("&File");
		comic = new ToolStripMenuItem("&Comic");
		gimmick = new ToolStripMenuItem("&Gimmicks");
		gimmicks = new Gimmicks();
		foreach(Gimmick gimmick in gimmicks.gimmicks){
			ToolStripMenuItem temp = new ToolStripMenuItem(gimmick.contentlabel, null);
			temp.Click += (sender, e) => strip_gimmick(sender, e, gimmick);// Not using the EventHandler in constructor this time lol
			gimmickMenus.Add(temp);
		}
		change = new ToolStripMenuItem("&Change comic");
		save = new ToolStripMenuItem("&Save strip", null, new EventHandler(strip_save), (Keys.Control | Keys.S));
		copy = new ToolStripMenuItem("&Copy strip image to clipboard", null, new EventHandler(strip_copy), (Keys.Control | Keys.C));
		copyURL = new ToolStripMenuItem("Copy strip &URL to clipboard", null, new EventHandler(strip_copyURL), (Keys.Control | Keys.Shift | Keys.C));
		nextstrip = new ToolStripMenuItem("&Next strip", null, new EventHandler(strip_next));
		previousstrip = new ToolStripMenuItem("&Previous strip", null, new EventHandler(strip_previous));
		gorando = new ToolStripMenuItem("&Go rando", null, new EventHandler(strip_rando));
		exit = new ToolStripMenuItem("E&xit", null, new EventHandler(delegate (object sender, EventArgs e) { this.Close(); }), (Keys.Alt | Keys.F4));
		currentcomic = new CurrentComicInfo(comics[0], comics[0].sources[0]);
		this.AutoSize = true;
		this.MinimumSize = new Size(661, 480);
		this.Text = @"Garfield strip picker - " + taglines[new Random().Next(0, taglines.Length)];
		stripretriever = new HttpClient();
		System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
		stripretriever.DefaultRequestHeaders.UserAgent.TryParseAdd("Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
		// fuck you msdn you lied to me
		/*stripretriever.DownloadProgressChanged += new DownloadProgressChangedEventHandler(delegate (object sender, DownloadProgressChangedEventArgs e) {
			statusprogress.Value = e.ProgressPercentage;
		});*/
		panel = new TableLayoutPanel();
		panel.ColumnCount = 0;
		panel.RowCount = 2;
		panel.Dock = DockStyle.Fill;
		panel.RowStyles.Clear();
		panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
		panel.RowStyles.Add(new RowStyle(SizeType.Percent, 90));
		this.Controls.Add(panel);
		picker = new TableLayoutPanel();
		picker.ColumnCount = 3;
		picker.RowCount = 0;
		picker.Dock = DockStyle.Fill;
		picker.AutoSize = true;
		picker.ColumnStyles.Clear();
		picker.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 64));
		picker.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 90));
		picker.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 64));
		panel.Controls.Add(picker);
		stripmenu = new ContextMenuStrip();
		stripmenu.Opening += new System.ComponentModel.CancelEventHandler(delegate (object sender, CancelEventArgs e) {
			stripmenu.Items.Clear();
			stripmenu.Items.AddRange(new ToolStripMenuItem[]{
				save,
				copy,
				copyURL,
				nextstrip,
				previousstrip,
				gorando
			});
		});
		previous = new Button();
		previous.Dock = DockStyle.Fill;
		previous.Text = "Previous";
		previous.Click += new EventHandler(strip_previous);
		previous.Anchor = AnchorStyles.Left;
		picker.Controls.Add(previous);
		date = new DateTimePicker();
		date.MinDate = currentcomic.source.getMinDate(currentcomic.comic);
		date.MaxDate = currentcomic.source.getMaxDate(currentcomic.comic);
		date.CustomFormat = "yyyy-MM-dd";
		date.Format = DateTimePickerFormat.Custom;
		date.Dock = DockStyle.Fill;
		date.Anchor = AnchorStyles.None;
		date.ValueChanged += new EventHandler(strip_update);
		picker.Controls.Add(date);
		next = new Button();
		next.Dock = DockStyle.Fill;
		next.Text = "Next";
		next.Click += new EventHandler(strip_next);
		next.Anchor = AnchorStyles.Right;
		picker.Controls.Add(next);
		strip = new PictureBox();
		strip.SizeMode = PictureBoxSizeMode.Zoom;
		strip.Dock = DockStyle.Fill;
		strip.ContextMenuStrip = stripmenu;
		strip.MinimumSize = new Size(640, 0);
		panel.Controls.Add(strip);
		status = new StatusStrip();
		statuscomic = new ToolStripStatusLabel(String.Format("({0}) {1}", currentcomic.source.name, currentcomic.comic.name));
		statusdate = new ToolStripStatusLabel();
		statusprogress = new ToolStripProgressBar();
		statusprogress.Alignment = ToolStripItemAlignment.Right;
		statusprogress.Visible = false;
		status.Items.AddRange(new System.Windows.Forms.ToolStripItem[]{
			statuscomic,
			statusdate,
			statusprogress
		});
		this.Controls.Add(status);
		menu = new MenuStrip();
		file.DropDownOpening += new EventHandler(delegate (object sender, EventArgs e) {
			file.DropDownItems.Clear();
			file.DropDownItems.AddRange(new ToolStripMenuItem[]{
				save,
				copy,
				copyURL,
				exit
			});
		});
		comic.DropDownOpening += new EventHandler(delegate (object sender, EventArgs e) {
			comic.DropDownItems.Clear();
			comic.DropDownItems.AddRange(new ToolStripMenuItem[]{
				change,
				nextstrip,
				previousstrip,
				gorando
			});
		});
		gimmick.DropDownOpening += new EventHandler(delegate (object sender, EventArgs e) {
			gimmick.DropDownItems.Clear();
			gimmick.DropDownItems.AddRange(gimmickMenus.ToArray()); 
		});
		for (int x = 0; x < comics.Count; x++)
		{
			// im so fucked up
			Comic item = comics[x];
			ToolStripMenuItem fuck = new ToolStripMenuItem(item.name, null);//, new EventHandler((sender, e) => comic_update(sender, e)));
			for (int y = 0; y < item.sources.Count; y++){
				ComicSource source = item.sources[y];
				ToolStripMenuItem sourcemenu = new ToolStripMenuItem(source.name, null);
				sourcemenu.Click += (sender, e) => comic_update(sender, e, item, source);
				fuck.DropDownItems.Add(sourcemenu);
			}
			change.DropDownItems.Add(fuck);
		}
		menu.Items.AddRange(new ToolStripItem[]{
			file,
			comic,
			gimmick
		});
		this.Controls.Add(menu);
		strip_rando(null, null);
		strip_update(null, null);
	}
	private void strip_previous(object sender, EventArgs e)
	{
		try
		{
			date.Value = date.Value.AddDays(-currentcomic.comic.weekinfo.increment);
		}
		catch (ArgumentOutOfRangeException suck)
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
		catch (ArgumentOutOfRangeException suck)
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
		catch (ArgumentOutOfRangeException suck)
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

	private Image drawMessage(string message, int size = 96)
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
			catch (FileNotFoundException suck)
			{
				img = drawMessage("not found");
			}
			catch(IOException suck)
	        {
				img = drawMessage("i/o exception.\nyour file must be locked.", 36);
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
			catch (HttpRequestException suck)
			{
				//MessageBox.Show(suck.ToString());
				img = drawMessage(((int)(response.StatusCode)).ToString());
			}
			catch(ArgumentException suck)
			{
				img = drawMessage("an error occured while\nprocessing the image", 36);
			}
		}
		Bitmap bm = this.shittyCopy(img); // wish i could be using "using" here
		foreach(Gimmick gimmick in gimmicks.gimmicks){
			if(gimmick.enabled){
				img = gimmick.doIt(bm, img, currentcomic);
				bm = this.shittyCopy(img);
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
		ToolStripMenuItem gimmickItem = sender as ToolStripMenuItem;
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
