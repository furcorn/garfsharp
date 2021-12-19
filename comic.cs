using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using Newtonsoft.Json;

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
			return response;
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

public class RegexInfo{
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
	}
}