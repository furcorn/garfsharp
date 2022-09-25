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

public class Garfield : Form
{
	// Core stuff

	public static HttpClient stripretriever;
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
		"now with more classes and lambda expressions!",
		"now in Three Parts!",
		"now with GitHub automation!",
		"now with less UI spaghetti!",
		"now with 50% more MenuItems!"
	};
	private CancellationTokenSource ctk = new CancellationTokenSource();

	// UI - top bar

	public MainMenu menu;

	List<MenuItem> gimmickMenus = new List<MenuItem>();

	MenuItem file;
	MenuItem comic;
	MenuItem gimmick;
	MenuItem change;

	// UI - main view

	public TableLayoutPanel panel;
	public TableLayoutPanel picker;
	public Button previous;
	public DateTimePicker date;
	public Button next;
	public PictureBox strip;

	public ContextMenu stripmenu;

	// UI - status bar

	public StatusStrip status;
	public ToolStripStatusLabel statuscomic;
	public ToolStripStatusLabel statusdate;
	public ToolStripProgressBar statusprogress;
	
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

		// Core

		stripretriever = new HttpClient();
		currentcomic = new CurrentComicInfo(comics[0], comics[0].sources[0]);
		System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
		stripretriever.DefaultRequestHeaders.UserAgent.TryParseAdd("Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

		// UI - root

		this.AutoSize = true;
		this.MinimumSize = new Size(661, 480);
		this.Text = @"Garfield strip picker - " + taglines[new Random().Next(0, taglines.Length)];

		// UI - top bar

		// Why is MainMenu not supported? It's native for God's sake!!!
		// MenuStrip looks stupid and makes my app look like it came out of Office!!!

		menu = new MainMenu();

		file = new MenuItem("&File");
		comic = new MenuItem("&Comic");
		gimmick = new MenuItem("&Gimmicks");

		// File

		file.MenuItems.AddRange(new MenuItem[]{
			new MenuItem(
				"&Save strip",
				new EventHandler(strip_save),
				Shortcut.CtrlS
			),
			new MenuItem(
				"&Copy strip image to clipboard",
				new EventHandler(strip_copy),
				Shortcut.CtrlC
			),
			new MenuItem(
				"Copy strip &URL to clipboard",
				new EventHandler(strip_copyURL),
				Shortcut.CtrlShiftC
			),
			new MenuItem(
				"E&xit",
				new EventHandler(delegate (object sender, EventArgs e) { this.Close(); }),
				Shortcut.AltF4
			)
		});

		// Comic

		change = new MenuItem(
			"&Change comic"
		);

		comic.MenuItems.AddRange(new MenuItem[]{
			change,
			new MenuItem(
				"&Next strip",
				new EventHandler(strip_next)
			),
			new MenuItem(
				"&Previous strip",
				new EventHandler(strip_previous)
			),
			new MenuItem(
				"&Go rando",
				new EventHandler(strip_rando)
			)
		});

		for (int x = 0; x < comics.Count; x++)
		{
			// im so fucked up
			Comic item = comics[x];
			MenuItem fuck = new MenuItem(item.name);//, new EventHandler((sender, e) => comic_update(sender, e)));
			for (int y = 0; y < item.sources.Count; y++){
				ComicSource source = item.sources[y];
				MenuItem sourcemenu = new MenuItem(source.name);
				sourcemenu.Click += (sender, e) => comic_update(sender, e, item, source);
				fuck.MenuItems.Add(sourcemenu);
			}
			change.MenuItems.Add(fuck);
		}

		// Gimmick

		gimmicks = new Gimmicks();

		foreach(Gimmick gimmick in gimmicks.gimmicks){
			MenuItem temp = new MenuItem(gimmick.contentlabel);
			temp.Click += (sender, e) => strip_gimmick(sender, e, gimmick); // Not using the EventHandler in constructor this time lol
			gimmickMenus.Add(temp);
		}

		gimmick.MenuItems.AddRange(gimmickMenus.ToArray()); 

		menu.MenuItems.AddRange(new MenuItem[]{
			file,
			comic,
			gimmick,
		});

		// UI - Main view
		
		panel = new TableLayoutPanel();
		panel.ColumnCount = 0;
		panel.RowCount = 2;
		panel.Dock = DockStyle.Fill;
		panel.RowStyles.Clear();
		panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
		panel.RowStyles.Add(new RowStyle(SizeType.Percent, 90));

		// UI - Navbar
		
		picker = new TableLayoutPanel();
		picker.ColumnCount = 3;
		picker.RowCount = 0;
		picker.Dock = DockStyle.Fill;
		picker.AutoSize = true;
		picker.ColumnStyles.Clear();
		picker.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 64));
		picker.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 90));
		picker.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 64));

		previous = new Button();
		previous.Dock = DockStyle.Fill;
		previous.Text = "Previous";
		previous.Click += new EventHandler(strip_previous);
		previous.Anchor = AnchorStyles.Left;

		date = new DateTimePicker();
		date.MinDate = currentcomic.source.getMinDate(currentcomic.comic);
		date.MaxDate = currentcomic.source.getMaxDate(currentcomic.comic);
		date.CustomFormat = "yyyy-MM-dd";
		date.Format = DateTimePickerFormat.Custom;
		date.Dock = DockStyle.Fill;
		date.Anchor = AnchorStyles.None;
		date.ValueChanged += new EventHandler(strip_update);
		
		next = new Button();
		next.Dock = DockStyle.Fill;
		next.Text = "Next";
		next.Click += new EventHandler(strip_next);
		next.Anchor = AnchorStyles.Right;
		
		picker.Controls.Add(previous);
		picker.Controls.Add(date);
		picker.Controls.Add(next);
		panel.Controls.Add(picker);

		// UI - Context menu

		stripmenu = new ContextMenu();

		stripmenu.MenuItems.AddRange(new MenuItem[]{
			new MenuItem(
				"&Save strip",
				new EventHandler(strip_save),
				Shortcut.CtrlS
			),
			new MenuItem(
				"&Copy strip image to clipboard",
				new EventHandler(strip_copy),
				Shortcut.CtrlC
			),
			new MenuItem(
				"Copy strip &URL to clipboard",
				new EventHandler(strip_copyURL),
				Shortcut.CtrlShiftC
			),
			new MenuItem(
				"&Next strip",
				new EventHandler(strip_next)
			),
			new MenuItem(
				"&Previous strip",
				new EventHandler(strip_previous)
			),
			new MenuItem(
				"&Go rando",
				new EventHandler(strip_rando)
			)
		});

		// UI - Comic strip

		strip = new PictureBox();
		strip.SizeMode = PictureBoxSizeMode.Zoom;
		strip.Dock = DockStyle.Fill;
		strip.ContextMenu = stripmenu;
		strip.MinimumSize = new Size(640, 0);
		
		panel.Controls.Add(strip);

		// UI - Status bar

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


		this.Menu = menu;
		this.Controls.Add(panel);
		this.Controls.Add(status);

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
				img = BitmapUtils.drawMessage("not found");
			}
			catch(IOException suck)
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
			catch (HttpRequestException suck)
			{
				//MessageBox.Show(suck.ToString());
				img = BitmapUtils.drawMessage(((int)(response.StatusCode)).ToString());
			}
			catch(ArgumentException suck)
			{
				img = BitmapUtils.drawMessage("an error occured while\nprocessing the image", 36);
			}
		}
		Bitmap bm = BitmapUtils.shittyCopy(img); // wish i could be using "using" here
		foreach(Gimmick gimmick in gimmicks.gimmicks){
			if(gimmick.enabled){
				img = gimmick.doIt(bm, img, currentcomic);
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
