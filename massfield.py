#̶M̶a̶s̶s̶f̶i̶e̶l̶d, for downloading comic strips en masse
#Mass Acres
from urllib import request
from urllib.parse import urlparse
from pathlib import Path
import time
import datetime
import os
import argparse
parser = argparse.ArgumentParser(description="Massfield, for downloading comic strips en masse")
parser.add_argument("-i", "--start", type=int, help="start in days", default=0)
parser.add_argument("-c", "--cooldown", type=int, help="time in seconds between each download", default=30)
parser.add_argument("-l", "--list", type=str, help="download from a list rather than mindate/maxdate (useful with missfield)", default=30)
args = parser.parse_args()
print("""┌───────────────────────────────────────┐
│               Massfield               │
│ ───────────────────────────────────── │
│ for downloading comic strips en masse │
└───────────────────────────────────────┘""")
urlformat = 'https://web.archive.org/web/2019id_/https://d1ejxu6vysztl5.cloudfront.net/comics/usacres/{0}/usa{1}.gif'
mindate = datetime.date(year=1986,month=3,day=3)
maxdate = datetime.date(year=1989,month=5,day=7)
days = (maxdate-mindate).days
def doIt(currentdate):
	print("Getting "+currentdate.strftime("%Y-%m-%d"))
	url = urlformat.format(currentdate.year, currentdate.strftime("%Y-%m-%d"))
	req = request.Request(
		url,
		data=None,
		headers={"User-Agent": "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)"}
	)
	try:
		response = request.urlopen(req)
		content = response.read()
		filename = os.path.basename(urlparse(url).path)
		dest = str(Path() / filename)
		with open(dest, "wb") as f:
			f.write(content)
		print("Sucessefully wrote "+filename)
	except (request.HTTPError, OSError, Exception) as e:
		print("Could not get/save comic strip due to exception:\n{0}".format(e))
		print("If you'd like to stop the program now, press CTRL+C before the cooldown is over")
	if args.cooldown != 0:
		print("Waiting "+str(args.cooldown)+" seconds to avoid attack detection")
		time.sleep(args.cooldown)

if args.list is not None:
	print("list mode".center(41))
	with open(args.list) as file:
		for line in file.readlines():
			doIt(datetime.datetime.strptime(line.rstrip('\n'), "%Y-%m-%d"))
else:
	print("range mode".center(41))
	for i in range(args.start, days+1):
		doIt(mindate + datetime.timedelta(days=i))
		