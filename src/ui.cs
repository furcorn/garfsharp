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

public partial class Garfield : Form
{
	// UI - top bar

	public MainMenu menu;
	List<MenuItem> gimmickMenus = new List<MenuItem>();
	MenuItem file;
	MenuItem comic;
	MenuItem gimmick;
	MenuItem change;

	// UI - main view

	public TableLayoutPanel panel;
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

	private void initialize_ui()
	{
		// UI - root

		this.AutoSize = true;
		this.MinimumSize = new Size(661, 480);
		this.Text = @"Garfield strip picker - " + taglines[new Random().Next(0, taglines.Count)];

		// UI - top bar

		// Why is MainMenu not supported? It's native for God's sake!!!
		// MenuStrip looks stupid and makes my app look like it came out of Office!!!

		menu = new MainMenu();
		file = new MenuItem("&File");
		comic = new MenuItem("&Comic");
		gimmick = new MenuItem("&Gimmicks");
		change = new MenuItem("&Change comic");

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

		// Gimmicks

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
		panel.ColumnCount = 3;
		panel.RowCount = 2;
		panel.Dock = DockStyle.Fill;
		panel.RowStyles.Clear();
		panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
		panel.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
		panel.ColumnStyles.Clear();
		panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 64));
		panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
		panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 64));

		// UI - Navbar

		previous = new Button();
		previous.Dock = DockStyle.Fill;
		previous.Text = "Previous";
		previous.Click += new EventHandler(strip_previous);
		previous.Anchor = AnchorStyles.Left;
		panel.SetColumn(previous, 0);
		panel.SetColumnSpan(previous, 1);
		panel.SetRow(previous, 0);
		panel.SetRowSpan(previous, 1);

		date = new DateTimePicker();
		date.MinDate = currentcomic.source.getMinDate(currentcomic.comic);
		date.MaxDate = currentcomic.source.getMaxDate(currentcomic.comic);
		date.CustomFormat = "yyyy-MM-dd";
		date.Format = DateTimePickerFormat.Custom;
		date.Dock = DockStyle.Fill;
		date.Anchor = AnchorStyles.None;
		date.ValueChanged += new EventHandler(strip_update);
		panel.SetColumn(date, 1);
		panel.SetColumnSpan(date, 1);
		panel.SetRow(date, 0);
		panel.SetRowSpan(date, 1);
		
		next = new Button();
		next.Dock = DockStyle.Fill;
		next.Text = "Next";
		next.Click += new EventHandler(strip_next);
		next.Anchor = AnchorStyles.Right;
		panel.SetColumn(next, 2);
		panel.SetColumnSpan(next, 1);
		panel.SetRow(next, 0);
		panel.SetRowSpan(next, 1);
		
		panel.Controls.Add(previous);
		panel.Controls.Add(date);
		panel.Controls.Add(next);

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
		panel.SetColumn(strip, 0);
		panel.SetColumnSpan(strip, 3);
		panel.SetRow(strip, 1);
		panel.SetRowSpan(strip, 1);
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
	}
}
