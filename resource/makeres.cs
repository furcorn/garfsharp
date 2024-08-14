using System;
using System.Collections.Generic;
using System.Resources;
using System.IO;
using System.Drawing;

public class Makeres {
	public static void Main() {
		List<string> taglines = new List<string>();
		taglines.AddRange(File.ReadLines(@"./taglines.txt"));
		string defaultStrips = @"[
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
]";
		Dictionary<string, Bitmap> bitmaps = new Dictionary<string, Bitmap>();
		bitmaps.Add("pipe", new Bitmap(@"./pipe.png"));
		bitmaps.Add("deflated", new Bitmap(@"./deflated.png"));
		bitmaps.Add("window", new Bitmap(@"./window.png"));
		using (ResXResourceWriter resx = new ResXResourceWriter(@".\resources.resx")) {
			resx.AddResource("defaultStrips", defaultStrips);
			resx.AddResource("taglines", taglines);
			foreach(string k in bitmaps.Keys){
				Bitmap bitmap = null;
				bitmaps.TryGetValue(k, out bitmap);
				if(bitmap != null){
					resx.AddResource(k, bitmap);
				}
			}
		}
	}
}
